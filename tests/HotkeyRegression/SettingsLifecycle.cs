using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using EveOPreview;
using EveOPreview.Configuration;
using EveOPreview.Configuration.Implementation;
using EveOPreview.Presenters;
using EveOPreview.Services;
using EveOPreview.View;
using MediatR;

internal static partial class Program
{
    // Child processes isolate synchronous WinForms layout hangs. They construct the
    // real MainForm and presenter, but replace the mediator/service boundary so no
    // process monitor, real EVE window, or game input can be reached.
    private static void SettingsLifecycleTests()
    {
        foreach (string scale in new[] { "100", "125", "150", "200" })
        {
            RunIsolatedUiChild("--settings-child", "settings-scale-" + scale,
                $"real presenter settings lifecycle completes at {scale}% simulated layout scale", scale);
        }
        RunIsolatedUiChild("--sync-ui-child", "character-copy-ui", "isolated character-copy dialog lifecycle completes");
        RunIsolatedUiChild("--presenter-groups-child", "dynamic-group-presenter", "real dynamic-group presenter integration completes");
        RunIsolatedUiChild("--mouse-backend-child", "mouse-backend", "mouse input and mixed-manager regression completes");
        RunIsolatedUiChild("--mouse-ui-child", "mouse-capture-ui", "mouse capture modal regression completes");
    }

    private static void RunIsolatedUiChild(string mode, string directory, string label, string? argument = null)
    {
            string childScratch = Path.Combine(scratch, directory);
            using var child = new Process();
            child.StartInfo = new ProcessStartInfo(Environment.ProcessPath!)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            child.StartInfo.ArgumentList.Add(childScratch);
            child.StartInfo.ArgumentList.Add(mode);
            if (argument != null) child.StartInfo.ArgumentList.Add(argument);
            child.Start();
            Task<string> stdout = child.StandardOutput.ReadToEndAsync();
            Task<string> stderr = child.StandardError.ReadToEndAsync();
            bool exited = child.WaitForExit(45000);
            if (!exited)
            {
                // This Process was created here with our own test executable and
                // unique scratch argument. Never terminate another application.
                child.Kill(entireProcessTree: false);
                child.WaitForExit(5000);
            }
            string output = stdout.GetAwaiter().GetResult();
            string errors = stderr.GetAwaiter().GetResult();
            Directory.CreateDirectory(childScratch);
            File.WriteAllText(Path.Combine(childScratch, "lifecycle-output.txt"), output + errors);
            Console.Write(output);
            if (errors.Length > 0) Console.Write(errors);
            Check(exited && child.ExitCode == 0,
                label +
                (exited ? $" (exit {child.ExitCode})" : " (45-second watchdog timeout)"));
    }

    private static int SettingsLifecycleChild(string[] args)
    {
        int childIndex = Array.IndexOf(args, "--settings-child");
        int percent = int.Parse(args[childIndex + 1]);
        Run("real settings lifecycle", () => ExerciseSettingsLifecycle(percent));
        return PrintResult();
    }

    private static void Progress(string name)
    {
        Console.WriteLine("STEP " + name);
        Console.Out.Flush();
    }

    private static void ExerciseSettingsLifecycle(int percent)
    {
        Progress("create real MainForm + MainFormPresenter");
        using var context = new ApplicationContext();
        using var form = new MainForm(context);
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(-10000, -10000);
        var config = (IThumbnailConfiguration)MinimalConfig(Keys.None, Keys.None);
        config.MinimizeToTray = true;
        config.Language = "zh-CN";
        config.ThumbnailRefreshPeriod = 50;
        var storage = new LifecycleStorage();
        var mediator = DispatchProxy.Create<IMediator, LifecycleMediator>();
        var processMonitor = new LifecycleProcessMonitor();
        object manager = New("EveOPreview.Services.ThumbnailManager", mediator, config, processMonitor, new FakeWindowManager(), null);
        ((LifecycleMediator)(object)mediator).Manager = (IThumbnailManager)manager;
        var controller = DispatchProxy.Create<IApplicationController, LifecycleController>();
        ((LifecycleController)(object)controller).Manager = (IThumbnailManager)manager;
        var presenter = new MainFormPresenter(controller, form, mediator, config, storage);

        // Explicit Control.Scale explores layout stability at the sizes used by
        // common DPI settings. It does not alter or certify real monitor DPI.
        if (percent != 100)
        {
            Progress("scale " + percent + "%");
            form.Scale(new SizeF(percent / 100f, percent / 100f));
        }
        bool completed = false;
        Exception? failure = null;
        using var timer = new System.Windows.Forms.Timer { Interval = 100 };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            try
            {
                Progress("restore Settings from tray path");
                Call(form, "RestoreMainForm_Handler", null, EventArgs.Empty);
                Application.DoEvents();
                Check(form.Visible && form.WindowState == FormWindowState.Normal, "tray Settings restore displays the real form");
                var tabs = (TabControl)form.Controls.Find("ContentTabControl", true).Single();
                foreach (string language in new[] { "zh-CN", "en-US" })
                {
                    Progress("localize " + language);
                    LocalizationExtensions.SetLanguage(language);
                    form.BeginUpdateUI();
                    try { form.Language = language; form.InitializeLanguageControls(); }
                    finally { form.EndUpdateUI(); }
                    foreach (TabPage page in tabs.TabPages)
                    {
                        Progress("open " + language + " " + page.Name);
                        var watch = Stopwatch.StartNew();
                        tabs.SelectedTab = page;
                        form.PerformLayout();
                        Application.DoEvents();
                        Check(watch.Elapsed < TimeSpan.FromSeconds(2), $"{language} {page.Name} opens responsively");
                    }
                    tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(x => x.Name == "QuickActionsTabPage");
                    form.SetQuickHotkeyStatus("", true);
                    int ticksBeforeModal = processMonitor.UpdateCount;
                    ExerciseCaptureButton(form, "QuickForwardSetButton", Keys.Escape, 650);
                    Check(processMonitor.UpdateCount > ticksBeforeModal, "thumbnail DispatcherTimer continues while the recording dialog is modal");
                    ExerciseCaptureButton(form, "QuickBackwardSetButton", Keys.Control | Keys.Shift | Keys.K);
                    ExerciseCaptureButton(form, "QuickHideSetButton", Keys.Escape);
                    Set(form, "QuickSwitchBackwardHotkey", "");
                    Click(form, "QuickHotkeysApplyButton");
                    Check(storage.Saves > 0, "quick Apply reaches the real presenter persistence boundary");
                    SaveLifecycleScreenshot(form, "quick-" + language + "-" + percent);

                    form.AddThumbnails(new List<IThumbnailDescription> { new TestDescription { Title = "EVE - Lifecycle fake" } });
                    tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(x => x.Name == "ClientsTabPage");
                    ((CheckedListBox)form.Controls.Find("ThumbnailsList", true).Single()).SelectedIndex = 0;
                    ExerciseCaptureButton(form, "ClientHotkeySetButton", Keys.Escape);
                    ExerciseCaptureButton(form, "ClientHotkeySetButton", Keys.Control | Keys.Shift | Keys.K);
                    Click(form, "ClientHotkeyApplyButton");
                    Check(config.GetClientHotkey("EVE - Lifecycle fake") == (Keys.Control | Keys.Shift | Keys.K), "role Set + Apply reaches the real presenter/configuration");
                    // Disable the role before the next language iteration to avoid
                    // creating an intentional duplicate with its quick test value.
                    Click(form, "ClientHotkeyClearButton");
                    Click(form, "ClientHotkeyApplyButton");
                    tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(x => x.Name == "CycleGroupTabPage");
                    ExerciseNestedGroupKeyModal(form, save: false, cancelCapture: true);
                    SaveLifecycleScreenshot(form, "advanced-" + language + "-" + percent);
                }
                for (int repeat = 0; repeat < 3; repeat++)
                {
                    Progress("hide/restore repeat " + repeat);
                    form.Hide();
                    Call(form, "RestoreMainForm_Handler", null, EventArgs.Empty);
                    Application.DoEvents();
                    Check(form.Visible, "repeated Settings restoration remains responsive " + repeat);
                }
                if (percent == 100) ExerciseCaptureRestoreConflict(form, config, manager);
                completed = true;
            }
            catch (Exception ex) { failure = Root(ex); }
            finally
            {
                Call(manager, "SetHotkeyCaptureActive", true);
                Call(manager, "Stop");
                form.FormCloseRequested = request => request.Allow = true;
                form.Close();
                context.ExitThread();
            }
        };
        timer.Start();
        Progress("run real MainForm.Show message loop");
        presenter.Run();
        if (failure != null) throw failure;
        Check(completed, "real MainForm.Show startup and modal message loops complete");
        Check(storage.Loads == 1, "Settings restoration does not reload/restart the application");
    }

    private static void ExerciseCaptureButton(MainForm form, string buttonName, Keys result, int modalDuration = 100)
    {
        Progress("click " + buttonName + " => " + result);
        if (form.WindowState == FormWindowState.Minimized || !form.Visible)
        {
            Progress($"restore isolated fixture before recording: state={form.WindowState}; visible={form.Visible}");
            Call(form, "RestoreMainForm_Handler", null, EventArgs.Empty);
            Application.DoEvents();
        }
        Console.WriteLine($"CAPTURE PRECONDITION {buttonName}: state={form.WindowState}; visible={form.Visible}; enabled={form.Enabled}");
        bool dialogSeen = false;
        var elapsed = Stopwatch.StartNew();
        using var modalTimer = new System.Windows.Forms.Timer { Interval = 40 };
        modalTimer.Tick += (_, _) =>
        {
            Form? dialog = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name == "HotkeyCaptureDialog");
            if (dialog == null) return;
            if (elapsed.ElapsedMilliseconds < modalDuration) return;
            modalTimer.Stop();
            dialogSeen = true;
            Console.WriteLine($"MODAL owner={GetWindow(dialog.Handle, 4) == form.Handle}, visible={dialog.Visible}, enabled={dialog.Enabled}, managedTopmost={dialog.TopMost}, nativeTopmost={(GetWindowLong(dialog.Handle, -20) & 8) != 0}, ownerTopmost={form.TopMost}");
            Check(GetWindow(dialog.Handle, 4) == form.Handle && dialog.Visible && dialog.Enabled,
                buttonName + " capture dialog is visible, enabled and owned by settings");
            Check(IsWindowEnabled(dialog.Handle) && !IsWindowEnabled(form.Handle),
                buttonName + " recording dialog natively blocks input to settings");
            Check(!form.TopMost || (GetWindowLong(dialog.Handle, -20) & 8) != 0,
                buttonName + " capture dialog preserves the settings topmost band");
            Check(IsAboveOwner(dialog.Handle, form.Handle), buttonName + " capture dialog is above its settings owner in native Z-order");
            PressDialogKey(dialog, result);
        };
        modalTimer.Start();
        Click(form, buttonName);
        Application.DoEvents();
        Console.WriteLine($"MODAL RETURN seen={dialogSeen}, settingsEnabled={form.Enabled}, state={form.WindowState}, visible={form.Visible}");
        Check(dialogSeen && form.Enabled && IsWindowEnabled(form.Handle), buttonName + " modal accepts/cancels and returns control to settings");
    }

    private static void ExerciseCaptureRestoreConflict(MainForm form, IThumbnailConfiguration config, object manager)
    {
        Progress("capture restore with independently acquired conflicting shortcut");
        Keys key = Keys.Control | Keys.Alt | Keys.Shift | Keys.F23;
        if (!CanOwn(key)) throw new Exception("Conflict restoration fixture needs unoccupied Ctrl+Alt+Shift+F23.");
        config.QuickSwitchForwardHotkey = new KeysConverter().ConvertToInvariantString(key)!;
        Check(Apply(manager, out _), "fixture registers the original shortcut before recording");
        NativeOwner? foreign = null;
        bool warningSeen = false;
        bool warningOwned = false;
        bool warningTopmost = false;
        using var timer = new System.Windows.Forms.Timer { Interval = 40 };
        timer.Tick += (_, _) =>
        {
            Form? capture = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name == "HotkeyCaptureDialog");
            if (capture != null && foreign == null)
            {
                foreign = new NativeOwner(key);
                Check(foreign.Registered, "foreign thread acquires released shortcut during recording");
                PressDialogKey(capture, Keys.Escape);
                return;
            }
            EnumThreadWindows(GetCurrentThreadId(), (window, _) =>
            {
                var className = new StringBuilder(100);
                GetClassName(window, className, className.Capacity);
                var title = new StringBuilder(200);
                GetWindowText(window, title, title.Capacity);
                if (className.ToString() != "#32770" || !IsWindowVisible(window) ||
                    (title.ToString() != "快捷键设置" && title.ToString() != "Hotkey settings")) return true;
                warningSeen = true;
                warningOwned = GetWindow(window, 4) == form.Handle;
                warningTopmost = (GetWindowLong(window, -20) & 8) != 0;
                Console.WriteLine($"RESTORE WARNING title={title}, ownerIsSettings={warningOwned}, nativeTopmost={warningTopmost}, settingsTopmost={form.TopMost}, settingsEnabled={form.Enabled}");
                // Only this process's thread windows are enumerated. Dismiss our
                // own message box directly, never send global keyboard input.
                timer.Stop();
                EndDialog(window, (IntPtr)1);
                return false;
            }, IntPtr.Zero);
        };
        try
        {
            timer.Start();
            Click(form, "QuickForwardSetButton");
            timer.Stop();
            Check(warningSeen, "failed shortcut restoration shows a warning rather than silently dropping it");
            Check(warningOwned && (!form.TopMost || warningTopmost), "restore warning stays owned by and above the topmost settings window");
            Application.DoEvents();
            Check(form.Enabled, "settings accepts input after conflict warning is dismissed");
        }
        finally
        {
            timer.Stop();
            foreign?.Dispose();
            config.QuickSwitchForwardHotkey = "";
            Apply(manager, out _);
        }
    }

    private delegate bool EnumThreadWindowProc(IntPtr window, IntPtr parameter);
    private static bool IsAboveOwner(IntPtr dialog, IntPtr owner)
    {
        // Traversal observes only handle order, without reading or operating any
        // unrelated window. It stops as soon as either of our two handles occurs.
        for (IntPtr window = GetWindow(dialog, 0); window != IntPtr.Zero; window = GetWindow(window, 2))
        {
            if (window == dialog) return true;
            if (window == owner) return false;
        }
        return false;
    }
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")] private static extern bool EnumThreadWindows(uint threadId, EnumThreadWindowProc callback, IntPtr parameter);
    [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr window, uint command);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr window, StringBuilder name, int maximum);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr window, StringBuilder name, int maximum);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr window, int index);
    [DllImport("user32.dll")] private static extern bool EndDialog(IntPtr dialog, IntPtr result);

    private static void SaveLifecycleScreenshot(Form form, string name)
    {
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
        string path = Path.Combine(screenshots, name + ".png");
        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        Console.WriteLine("SCREENSHOT " + path);
    }

    private sealed class LifecycleStorage : IConfigurationStorage
    {
        public int Loads { get; private set; }
        public int Saves { get; private set; }
        public void Load() => Loads++;
        public void Save() => Saves++;
        public void SetConfigurationFilename(string filename) { }
    }

    public class LifecycleController : DispatchProxy
    {
        public IThumbnailManager Manager { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method!.Name == "Create" && method.GetGenericArguments()[0] == typeof(IThumbnailManager)) return Manager;
            throw new Exception("Unexpected controller call in isolated settings fixture: " + method.Name);
        }
    }

    public class LifecycleMediator : DispatchProxy
    {
        public IThumbnailManager Manager { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method!.Name == "Send" && args?[0]?.GetType().Name == "StartService") Manager.Start();
            Type type = method!.ReturnType;
            if (type == typeof(Task)) return Task.CompletedTask;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                Type valueType = type.GetGenericArguments()[0];
                object? value = valueType.IsValueType ? Activator.CreateInstance(valueType) : null;
                return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(valueType).Invoke(null, new[] { value });
            }
            throw new Exception("Unexpected mediator call in isolated settings fixture: " + method.Name);
        }
    }

    private sealed class LifecycleProcessMonitor : IProcessMonitor
    {
        public int UpdateCount { get; private set; }
        public IProcessInfo GetMainProcess() => new LifecycleProcessInfo();
        public ICollection<IProcessInfo> GetAllProcesses() => new List<IProcessInfo>();
        public void ClearAllProcesses() { }
        public void GetUpdatedProcesses(out ICollection<IProcessInfo> addedProcesses, out ICollection<IProcessInfo> updatedProcesses, out ICollection<IProcessInfo> removedProcesses)
        {
            UpdateCount++;
            addedProcesses = new List<IProcessInfo>();
            updatedProcesses = new List<IProcessInfo>();
            removedProcesses = new List<IProcessInfo>();
        }
    }
    private sealed class LifecycleProcessInfo : IProcessInfo
    {
        public IntPtr Handle => IntPtr.Zero;
        public string Title => "Isolated lifecycle fixture";
    }
}
