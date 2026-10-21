using System.Diagnostics;
using System.Reflection;
using System.Threading;

internal static partial class Program
{
    // Fake-native contract checks exercise the production worker and scheduling
    // algorithm. They never generate input, attach real queues, or activate EVE.
    private static void ForegroundActivationTests()
    {
        Check(App.GetType("EveOPreview.Services.Implementation.ForegroundWindowActivator") != null,
            "foreground activation uses an explicit bounded-result helper");
        if (App.GetType("EveOPreview.Services.Implementation.ForegroundWindowActivator") == null) return;
        int callerThread = Environment.CurrentManagedThreadId;
        var normal = new ForegroundFake { AcceptPlain = true };
        using (var worker = NewForegroundWorker(normal))
        {
            Check(ActivateFixture(worker, normal.Target, out _), "normal native activation verifies actual foreground");
            Check(normal.SetCalls == 1 && normal.Attachments.Count == 0, "successful plain activation never attaches input queues");
            Check(normal.CallbackThreads.Count > 0 && normal.CallbackThreads.All(id => id != callerThread)
                && normal.CallbackThreads.Distinct().Count() == 1, "all native activation operations stay on one non-UI worker");
        }
        var asynchronous = new ForegroundFake { AcceptPlain = true, DelayObservation = true };
        using (var worker = NewForegroundWorker(asynchronous))
        {
            Check(ActivateFixture(worker, asynchronous.Target, out _), "accepted activation waits for asynchronous foreground observation");
            Check(asynchronous.SetCalls == 1 && asynchronous.Attachments.Count == 0, "asynchronous success neither attaches nor retries activation");
        }
        var fallback = new ForegroundFake { AcceptAttached = true };
        using (var worker = NewForegroundWorker(fallback))
        {
            Check(ActivateFixture(worker, fallback.Target, out _), "plain refusal can retry on an isolated attached worker");
            Check(fallback.SetCalls == 2 && fallback.Attachments.SequenceEqual(new[] { true, false }),
                "fallback attaches once and always detaches after its single retry");
            Check(fallback.AttachmentSources.All(id => id == (uint)fallback.CallbackThreads[0])
                && fallback.AttachmentTargets.All(id => id == 9000), "only the dedicated worker attaches to the original foreground thread");
        }
        var attachmentDenied = new ForegroundFake { AllowAttach = false };
        using (var worker = NewForegroundWorker(attachmentDenied))
            Check(!ActivateFixture(worker, attachmentDenied.Target, out string reason) && reason == "input-attach-denied"
                && attachmentDenied.SetCalls == 1, "denied attachment reports failure without an unsafe retry");
        var exception = new ForegroundFake { ThrowOnAttachedSet = true };
        using (var worker = NewForegroundWorker(exception))
            Check(!ActivateFixture(worker, exception.Target, out string reason) && reason.Contains("activation-native-error")
                && exception.Attachments.SequenceEqual(new[] { true, false }), "native retry exception still detaches and returns a failure");
        var ownForeground = new ForegroundFake { OwnForeground = true };
        using (var worker = NewForegroundWorker(ownForeground))
            Check(!ActivateFixture(worker, ownForeground.Target, out string reason) && reason == "own-process-input-not-attached"
                && ownForeground.Attachments.Count == 0, "worker never attaches input to its own application UI or hook threads");
        var badDetach = new ForegroundFake { AcceptAttached = true, AllowDetach = false };
        using (var worker = NewForegroundWorker(badDetach))
        {
            Check(!ActivateFixture(worker, badDetach.Target, out string reason) && reason == "input-detach-failed",
                "failed detach is reported rather than hidden by successful foreground change");
            var retiredThread = (Thread)worker.GetType().GetField("_thread", Any)!.GetValue(worker)!;
            Check(retiredThread.Join(1000) && !retiredThread.IsAlive,
                "failed detach naturally exits its worker instead of leaving an attached queue parked");
            int attempts = badDetach.SetCalls;
            Check(!ActivateFixture(worker, badDetach.Target, out reason) && reason == "input-detach-failed" && attempts == badDetach.SetCalls,
                "potentially attached worker is retired and never reused for another activation");
        }
        var noObservation = new ForegroundFake { AcceptPlain = true, NeverObserve = true };
        using (var worker = NewForegroundWorker(noObservation, 120, 80))
            Check(!ActivateFixture(worker, noObservation.Target, out _) && noObservation.SetCalls == 1
                && noObservation.Attachments.Count == 0, "native success flag alone never claims foreground success or triggers fallback");
        var changed = new ForegroundFake { ChangeForegroundOnPlainSet = true };
        using (var worker = NewForegroundWorker(changed))
            Check(!ActivateFixture(worker, changed.Target, out string reason) && reason == "foreground-changed"
                && changed.Attachments.Count == 0, "foreground change during the plain attempt cancels fallback");
        var hung = new ForegroundFake { Responsive = false };
        using (var worker = NewForegroundWorker(hung))
            Check(!ActivateFixture(worker, hung.Target, out string reason) && reason == "foreground-not-responding"
                && hung.SetCalls == 1 && hung.Attachments.Count == 0, "unresponsive foreground fails at a bounded probe before any attachment");
        var slowBackground = new ForegroundFake { Responsive = false, AcceptPlain = true };
        using (var worker = NewForegroundWorker(slowBackground))
            Check(ActivateFixture(worker, slowBackground.Target, out _) && slowBackground.SetCalls == 1
                && slowBackground.Attachments.Count == 0, "normal asynchronous activation is not rejected by a short background-game responsiveness probe");
        var invalid = new ForegroundFake { ValidTarget = false };
        using (var worker = NewForegroundWorker(invalid))
            Check(!ActivateFixture(worker, invalid.Target, out string reason) && reason == "invalid-target"
                && invalid.SetCalls == 0, "invalid target cannot be treated as an activation");

        using var release = new ManualResetEventSlim(false);
        var blocked = new ForegroundFake { BlockPlainSet = release };
        using (var worker = NewForegroundWorker(blocked, 120, 80))
        {
            var watch = Stopwatch.StartNew();
            Check(!ActivateFixture(worker, blocked.Target, out string reason) && reason == "activation-timeout"
                && watch.Elapsed < TimeSpan.FromSeconds(1), "stuck native call cannot keep its caller waiting indefinitely");
            watch.Restart();
            Check(!ActivateFixture(worker, blocked.Target, out reason) && reason == "activation-worker-busy"
                && watch.Elapsed < TimeSpan.FromMilliseconds(100), "busy worker rejects new requests without spawning or queueing them");
            release.Set();
            var untilIdle = Stopwatch.StartNew();
            while ((int)worker.GetType().GetField("_inFlight", Any)!.GetValue(worker)! != 0 && untilIdle.ElapsedMilliseconds < 1000) Thread.Sleep(1);
            Check(blocked.SetCalls == 1 && blocked.Attachments.Count == 0,
                "expired native request cannot retry or attach after its blocked call returns");
            blocked.BlockPlainSet = null;
            blocked.AcceptPlain = true;
            Check(ActivateFixture(worker, blocked.Target, out _) && blocked.CallbackThreads.Distinct().Count() == 1,
                "the same worker can accept a fresh request after the old native call finally returns");
        }
        var stopped = new ForegroundFake();
        var stoppedWorker = NewForegroundWorker(stopped);
        stoppedWorker.Dispose();
        Check(!ActivateFixture(stoppedWorker, stopped.Target, out string stoppedReason) && stoppedReason == "activation-worker-stopped",
            "disposed test worker rejects new activation without restarting");
        stoppedWorker.Dispose();
    }

    private static IDisposable NewForegroundWorker(ForegroundFake fake, int callerTimeout = 400, int workBudget = 320)
    {
        object native = New("EveOPreview.Services.Implementation.ForegroundActivationNativeCalls",
            (Func<IntPtr, bool>)fake.IsWindow, (Func<IntPtr>)fake.GetForeground,
            (Func<IntPtr, uint>)fake.GetWindowThread, (Func<uint>)fake.GetCurrentThread,
            (Action)fake.EnsureQueue, (Func<IntPtr, uint, bool>)fake.Ping,
            (Func<IntPtr, bool>)fake.SetForeground, (Func<uint, uint, bool, bool>)fake.Attach,
            (Func<IntPtr, bool>)fake.IsOwnProcessWindow);
        return (IDisposable)New("EveOPreview.Services.Implementation.ForegroundActivationWorker", native, callerTimeout, workBudget);
    }

    private static bool ActivateFixture(object worker, IntPtr target, out string reason)
    {
        object?[] arguments = { target, null };
        bool success = (bool)worker.GetType().GetMethod("TryActivate", Any)!.Invoke(worker, arguments)!;
        reason = (string)arguments[1]!;
        return success;
    }

    private sealed class ForegroundFake
    {
        internal readonly IntPtr Target = new IntPtr(100);
        internal IntPtr Foreground = new IntPtr(200);
        internal bool AcceptPlain, AcceptAttached, DelayObservation, NeverObserve, ChangeForegroundOnPlainSet, ThrowOnAttachedSet;
        internal bool AllowAttach = true, AllowDetach = true, Responsive = true, ValidTarget = true, OwnForeground;
        internal ManualResetEventSlim? BlockPlainSet;
        internal int SetCalls;
        internal readonly List<bool> Attachments = new();
        internal readonly List<uint> AttachmentSources = new(), AttachmentTargets = new();
        internal readonly List<int> CallbackThreads = new();
        private bool attached, pending;
        private int observations;

        private void Record() => CallbackThreads.Add(Environment.CurrentManagedThreadId);
        internal bool IsWindow(IntPtr window) { Record(); return ValidTarget && window != IntPtr.Zero; }
        internal IntPtr GetForeground()
        {
            Record();
            if (pending && !NeverObserve && ++observations >= 3) Foreground = Target;
            return Foreground;
        }
        internal uint GetWindowThread(IntPtr window) { Record(); return 9000; }
        internal uint GetCurrentThread() { Record(); return (uint)Environment.CurrentManagedThreadId; }
        internal void EnsureQueue() => Record();
        internal bool IsOwnProcessWindow(IntPtr window) { Record(); return OwnForeground; }
        internal bool Ping(IntPtr window, uint timeout) { Record(); return Responsive && timeout <= 100; }
        internal bool SetForeground(IntPtr window)
        {
            Record();
            SetCalls++;
            if (!attached) BlockPlainSet?.Wait();
            if (attached && ThrowOnAttachedSet) throw new InvalidOperationException("fixture native failure");
            if (!attached && ChangeForegroundOnPlainSet) Foreground = new IntPtr(300);
            bool accept = attached ? AcceptAttached : AcceptPlain;
            if (accept)
            {
                if (DelayObservation || NeverObserve) pending = true;
                else Foreground = Target;
            }
            return accept;
        }
        internal bool Attach(uint from, uint to, bool enable)
        {
            Record();
            Attachments.Add(enable); AttachmentSources.Add(from); AttachmentTargets.Add(to);
            if (enable && !AllowAttach) return false;
            if (!enable && !AllowDetach) return false;
            attached = enable;
            return true;
        }
    }
}
