using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

internal static partial class Program
{
    private static int ForegroundFixture()
    {
        using var form = new Form { Text = "EVE-O 自有切屏测试（非游戏）", StartPosition = FormStartPosition.Manual,
            Location = new Point(-10000, -10000), Size = new Size(300, 150) };
        using var lifetime = new System.Windows.Forms.Timer { Interval = 20000 };
        lifetime.Tick += (_, _) => form.Close();
        form.Shown += (_, _) => { Console.WriteLine("FIXTURE_HANDLE=" + form.Handle.ToInt64()); Console.Out.Flush(); lifetime.Start(); };
        Application.Run(form);
        return 0;
    }

    private static void NativeForegroundFixtureTests()
    {
        using Process a = StartForegroundFixture("a", out IntPtr first);
        using Process b = StartForegroundFixture("b", out IntPtr second);
        try
        {
            object native = T("EveOPreview.Services.Implementation.ForegroundActivationNativeCalls")
                .GetMethod("CreateWindows", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, null)!;
            object NativeField(string name) => native.GetType().GetField(name, Any)!.GetValue(native)!;
            var foreground = (Func<IntPtr>)NativeField("GetForegroundWindow");
            var setForeground = (Func<IntPtr, bool>)NativeField("SetForegroundWindow");
            var threadId = (Func<IntPtr, uint>)NativeField("GetWindowThreadId");
            var attach = (Func<uint, uint, bool, bool>)NativeField("AttachThreadInput");
            // Never attach to the player's game, Codex, or any window outside
            // the two processes that this test has just created.
            setForeground(first);
            var waiting = Stopwatch.StartNew();
            while (foreground() != first && waiting.ElapsedMilliseconds < 250) Thread.Sleep(5);
            if (foreground() != first)
            {
                Console.WriteLine("NOT RUN native fixture: Windows did not allow the isolated first window to obtain foreground; no foreign thread attached.");
                return;
            }
            uint firstThread = threadId(first), secondThread = threadId(second);
            int attempts = 0;
            var attachments = new List<bool>();
            bool ScopedSet(IntPtr target)
            {
                if (target != second) throw new Exception("Activation escaped fixture target.");
                if (Interlocked.Increment(ref attempts) == 1) return false;
                return setForeground(target);
            }
            bool ScopedAttach(uint from, uint to, bool enabled)
            {
                if (to != firstThread && to != secondThread) throw new Exception("Attachment escaped fixture processes.");
                attachments.Add(enabled);
                return attach(from, to, enabled);
            }
            object scopedNative = New("EveOPreview.Services.Implementation.ForegroundActivationNativeCalls",
                NativeField("IsWindow"), NativeField("GetForegroundWindow"), NativeField("GetWindowThreadId"),
                NativeField("GetCurrentThreadId"), NativeField("EnsureMessageQueue"), NativeField("PingWindow"),
                (Func<IntPtr, bool>)ScopedSet, (Func<uint, uint, bool, bool>)ScopedAttach, NativeField("IsOwnProcessWindow"));
            using var worker = (IDisposable)New("EveOPreview.Services.Implementation.ForegroundActivationWorker", scopedNative, 400, 320);
            bool success = ActivateFixture(worker, second, out string reason);
            Check(success && foreground() == second, "real Windows fixture fallback confirms foreground on the second independent process: " + reason);
            Check(attempts == 2 && attachments.SequenceEqual(new[] { true, false }),
                "fault-injected first refusal exercises real native attach, one retry and detach only on fixture threads");
            Console.WriteLine("NATIVE FIXTURE: first refusal injected; subsequent USER32 calls real; no physical-input or EVE acceptance claimed.");
        }
        finally
        {
            CloseForegroundFixture(a);
            CloseForegroundFixture(b);
        }
    }

    private static Process StartForegroundFixture(string name, out IntPtr handle)
    {
        var start = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false, RedirectStandardOutput = true,
            RedirectStandardError = true, CreateNoWindow = true };
        start.ArgumentList.Add(Path.Combine(scratch, "foreground-fixture-" + name));
        start.ArgumentList.Add("--foreground-fixture");
        Process process = Process.Start(start)!;
        try
        {
            string? line = process.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            if (line == null || !line.StartsWith("FIXTURE_HANDLE=")) throw new Exception("Fixture failed to announce its own window.");
            handle = new IntPtr(long.Parse(line[15..]));
            return process;
        }
        catch { CloseForegroundFixture(process); process.Dispose(); throw; }
    }

    private static void CloseForegroundFixture(Process process)
    {
        if (process.HasExited) return;
        process.Refresh();
        process.CloseMainWindow();
        if (!process.WaitForExit(1000)) { process.Kill(entireProcessTree: false); process.WaitForExit(1000); }
    }
}
