using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EveOPreview.Configuration.Implementation;
using EveOPreview.Configuration;
using EveOPreview.Services;
using EveOPreview.View;

// No package-based test runner is required. All profiles and screenshots are
// created under an isolated scratch directory; no game windows or input are used.
internal static partial class Program
{
    private static readonly Assembly App = typeof(MainForm).Assembly;
    private static readonly BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static int passed;
    private static readonly List<string> failures = new();
    private static string scratch = "";
    private static string screenshots = "";

    [STAThread]
    private static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        scratch = Path.GetFullPath(args.FirstOrDefault() ?? Path.Combine(Path.GetTempPath(), "EveO-HotkeyRegression-" + Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(scratch);
        screenshots = Path.Combine(scratch, "screenshots-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(screenshots);
        string locale = Path.Combine(AppContext.BaseDirectory, "EVE-O-Preview.locale");
        if (File.Exists(locale)) File.Copy(locale, Path.Combine(scratch, "EVE-O-Preview.locale"), true);
        Environment.CurrentDirectory = scratch;
        if (args.Contains("--settings-child")) Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if (args.Contains("--preview")) return Preview();
        if (args.Contains("--settings-child")) return SettingsLifecycleChild(args);
        if (args.Contains("--settings-only"))
        {
            Run("isolated settings lifecycle", SettingsLifecycleTests);
            return PrintResult();
        }
        Run("gesture parser", ParserTests);
        Run("legacy/profile persistence", ConfigurationTests);
        Run("real Windows hotkey transactions", TransactionTests);
        Run("fake client hotkey routing", ClientRoutingTests);
        Run("hotkey message handling", NativeHandlerTests);
        Run("settings UI controls", UiTests);
        Run("isolated settings lifecycle", SettingsLifecycleTests);
        return PrintResult();
    }

    private static int PrintResult()
    {
        Console.WriteLine($"RESULT: {passed} PASS, {failures.Count} FAIL; scratch={scratch}");
        foreach (string failure in failures) Console.WriteLine("FAIL DETAIL: " + failure);
        return failures.Count == 0 ? 0 : 1;
    }

    private static int Preview()
    {
        LocalizationExtensions.SetLanguage("zh-CN");
        using var context = new ApplicationContext();
        using var form = new MainForm(context);
        form.InitializeLanguageControls();
        form.Text += " [隔离 UI 预览]";
        Set(form, "QuickSwitchForwardHotkey", "Control+Alt+Right");
        Set(form, "QuickSwitchBackwardHotkey", "Control+Alt+Left");
        Set(form, "QuickHideHotkey", "Control+Alt+H");
        form.QuickSwitchEnabled = true;
        form.QuickHideEnabled = true;
        form.AddThumbnails(new List<IThumbnailDescription>
        {
            new TestDescription { Title = "EVE - 主力角色" },
            new TestDescription { Title = "EVE - 后勤角色" },
            new TestDescription { Title = "EVE - 侦查角色" }
        });
        form.SelectedClientChanged = title => form.SetSelectedClientHotkey("Control+Shift+K");
        form.SetSelectedClientHotkey("Control+Shift+K");
        form.QuickHotkeysApplyRequested = () => form.SetQuickHotkeyStatus("隔离预览：已应用（不注册系统热键）", true);
        form.ClientHotkeyApplyRequested = (title, key) => form.SetClientHotkeyStatus("隔离预览：已应用到 " + title, true);
        var tabs = (TabControl)form.Controls.Find("ContentTabControl", true).Single();
        tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(x => x.Name == "QuickActionsTabPage");
        Application.Run(form);
        return 0;
    }

    private static void Run(string name, Action body)
    {
        try { body(); }
        catch (Exception ex) { Fail(name + ": " + Root(ex)); }
    }

    private static void Check(bool condition, string name)
    {
        if (!condition) { Fail(name); return; }
        passed++;
        Console.WriteLine("PASS " + name);
    }
    private static void Fail(string name) { failures.Add(name); Console.WriteLine("FAIL " + name); }
    private static Exception Root(Exception ex) => ex is TargetInvocationException { InnerException: not null } tie ? Root(tie.InnerException!) : ex;
    private static Type T(string name) => App.GetType(name, true)!;
    private static object New(string name, params object?[] args) => Activator.CreateInstance(T(name), Any, null, args, null)!;
    private static object? Get(object item, string name) => item.GetType().GetProperty(name, Any)!.GetValue(item);
    private static void Set(object item, string name, object? value) => item.GetType().GetProperty(name, Any)!.SetValue(item, value);
    private static object? Call(object item, string name, params object?[] args) => item.GetType().GetMethod(name, Any)!.Invoke(item, args);
    private static void SetMember(object item, string name, object? value)
    {
        var p = item.GetType().GetProperty(name, Any);
        if (p != null) p.SetValue(item, value);
        else item.GetType().GetField(name, Any)!.SetValue(item, value);
    }

    private static (bool Success, Keys Key) Parse(string? value)
    {
        var method = T("EveOPreview.UI.Hotkeys.HotkeyGesture").GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static)!;
        object?[] args = { value, Keys.None };
        return ((bool)method.Invoke(null, args)!, (Keys)args[1]!);
    }

    private static void ParserTests()
    {
        foreach (var input in new[] { "Control+Alt+Right", "Ctrl+Alt+Right", "Shift+F14" })
            Check(Parse(input).Success, "accept legacy/valid gesture " + input);
        foreach (var input in new[] { "Control", "Alt", "Shift", "Control+Alt", "F12", "Control+F12", "LWin", "Banana", "Control+Banana", "999999", "Control+", "A+B" })
            Check(!Parse(input).Success, "reject unsafe/invalid gesture " + input);
        foreach (var input in new string?[] { null, "", "  ", "None" })
            Check(Parse(input) == (true, Keys.None), "empty gesture disables binding: " + (input ?? "null"));
        Check(Parse("Control+Alt+Right").Key == (Keys.Control | Keys.Alt | Keys.Right), "parse key preserves modifiers");
        var format = T("EveOPreview.UI.Hotkeys.HotkeyGesture").GetMethod("Format", BindingFlags.Public | BindingFlags.Static)!;
        var formatted = (string)format.Invoke(null, new object[] { Keys.Control | Keys.Alt | Keys.H })!;
        Check(Parse(formatted).Key == (Keys.Control | Keys.Alt | Keys.H), "format/parse round trip");
    }

    private static object Config() => New("EveOPreview.Configuration.Implementation.ThumbnailConfiguration");
    private static void ConfigurationTests()
    {
        var config = Config();
        var appConfig = New("EveOPreview.Configuration.Implementation.AppConfig");
        string file = Path.Combine(scratch, "isolated-profile.json");
        Set(appConfig, "ConfigFileName", file);
        var storage = New("EveOPreview.Configuration.Implementation.ConfigurationStorage", appConfig, config);
        File.WriteAllText(file, """{"Language":"zh-CN","QuickSwitchEnabled":true,"QuickSwitchForwardHotkey":"Control+Shift+F17","QuickSwitchBackwardHotkey":"Control+Shift+F18","QuickHideEnabled":true,"QuickHideHotkey":"Control+Shift+F19","ClientHotkey":{"EVE - Harness":"Control+Shift+F20"}}""");
        Call(storage, "Load");
        Check((string)Get(config, "QuickSwitchForwardHotkey")! == "Control+Shift+F17", "custom quick hotkey loads");
        Check((Keys)Call(config, "GetClientHotkey", "EVE - Harness")! == (Keys.Control | Keys.Shift | Keys.F20), "legacy ClientHotkey dictionary loads");
        Call(storage, "Save");
        var second = Config();
        var storage2 = New("EveOPreview.Configuration.Implementation.ConfigurationStorage", appConfig, second);
        Call(storage2, "Load");
        Check((string)Get(second, "QuickHideHotkey")! == "Control+Shift+F19", "saved quick hotkeys reload");
        Check((Keys)Call(second, "GetClientHotkey", "EVE - Harness")! == (Keys.Control | Keys.Shift | Keys.F20), "saved per-client hotkey reloads");
        File.WriteAllText(file, """{"Language":"zh-CN","ThumbnailSize":"192, 108"}""");
        Call(storage, "Load");
        Check((string)Get(config, "QuickSwitchForwardHotkey")! == "Control+Alt+Right", "old profile restores default forward binding");
        Check((string)Get(config, "QuickSwitchBackwardHotkey")! == "Control+Alt+Left", "old profile restores default backward binding");
        Check((string)Get(config, "QuickHideHotkey")! == "Control+Alt+H", "old profile restores default hide binding");
        Check((Keys)Call(config, "GetClientHotkey", "EVE - Harness")! == Keys.None, "old profile does not inherit prior role hotkey");
        File.WriteAllText(file, """{"QuickSwitchForwardHotkey":"nonsense","ClientHotkey":{"EVE - Bad":"not a key"}}""");
        Call(storage, "Load");
        Check((Keys)Call(config, "StringToKey", "not a key")! == Keys.None, "invalid stored quick gesture does not throw");
        Check((Keys)Call(config, "GetClientHotkey", "EVE - Bad")! == Keys.None, "invalid stored role gesture does not throw");
    }

    private static object MinimalConfig(Keys forward, Keys backward)
    {
        object cfg = Config();
        for (int group = 1; group <= 5; group++)
        {
            Set(cfg, $"CycleGroup{group}ForwardHotkeys", new List<string>());
            Set(cfg, $"CycleGroup{group}BackwardHotkeys", new List<string>());
        }
        Set(cfg, "MinimizeAllClientsHotkeys", new List<string>());
        Set(cfg, "RefreshMinimizedClientsHotkeys", new List<string>());
        Set(cfg, "QuickSwitchEnabled", true);
        Set(cfg, "QuickHideEnabled", true);
        Set(cfg, "QuickHideHotkey", "");
        Set(cfg, "QuickSwitchForwardHotkey", new KeysConverter().ConvertToInvariantString(forward));
        Set(cfg, "QuickSwitchBackwardHotkey", new KeysConverter().ConvertToInvariantString(backward));
        return cfg;
    }

    private static bool Apply(object manager, out string error)
    {
        object?[] args = { null };
        bool result = (bool)manager.GetType().GetMethod("TryApplyHotkeys", Any)!.Invoke(manager, args)!;
        error = args[0]?.ToString() ?? "";
        return result;
    }

    private static void TransactionTests()
    {
        var available = new List<Keys>();
        foreach (Keys code in new[] { Keys.F17, Keys.F18, Keys.F19, Keys.F20, Keys.F21, Keys.F22, Keys.F23, Keys.F24 })
        {
            Keys candidate = Keys.Control | Keys.Alt | Keys.Shift | code;
            using var probe = new NativeOwner(candidate);
            if (probe.Registered) available.Add(candidate);
        }
        if (available.Count < 3) throw new Exception("Need three unoccupied Ctrl+Alt+Shift+F17..F24 test shortcuts.");
        Keys first = available[0], second = available[1], busy = available[2];
        var config = MinimalConfig(first, second);
        object manager = New("EveOPreview.Services.ThumbnailManager", null, config, null, null, null);
        try
        {
            Check(!CanOwn(first) && !CanOwn(second), "manager initial quick bindings really register in Windows");
            using (var foreignOwner = new NativeOwner(busy))
            {
                Check(foreignOwner.Registered, "independent Windows thread owns conflicting gesture");
                Set(config, "QuickSwitchForwardHotkey", new KeysConverter().ConvertToInvariantString(busy));
                Check(!Apply(manager, out string error) && error.Length > 0, "OS hotkey conflict reports actionable failure");
                Check(Parse((string)Get(config, "QuickSwitchForwardHotkey")!).Key == first, "failed registration rolls configuration back");
                Check(!CanOwn(first) && !CanOwn(second), "failed registration restores both old Windows bindings");
            }
            Set(config, "QuickSwitchForwardHotkey", new KeysConverter().ConvertToInvariantString(second));
            Set(config, "QuickSwitchBackwardHotkey", new KeysConverter().ConvertToInvariantString(first));
            Check(Apply(manager, out _), "swapping this app's own gestures has no false external conflict");
            Set(config, "QuickSwitchBackwardHotkey", new KeysConverter().ConvertToInvariantString(second));
            Check(!Apply(manager, out string duplicateError) && duplicateError.Length > 0, "duplicate in-app gestures reject whole change");
            Check(Parse((string)Get(config, "QuickSwitchBackwardHotkey")!).Key == first, "duplicate rejection preserves prior configuration");
            Call(manager, "SetHotkeyCaptureActive", true);
            Check(CanOwn(first) && CanOwn(second), "capture suspension releases app shortcuts");
            Call(manager, "SetHotkeyCaptureActive", false);
            Check(!CanOwn(first) && !CanOwn(second), "ending capture restores app shortcuts");
        }
        finally { Call(manager, "SetHotkeyCaptureActive", true); Call(manager, "Stop"); }
        Check(CanOwn(first) && CanOwn(second), "manager suspension cleanup releases every registered test shortcut");
    }

    private static bool CanOwn(Keys key) { using var owner = new NativeOwner(key); return owner.Registered; }

    private static void ClientRoutingTests()
    {
        Keys forward = Keys.Control | Keys.Alt | Keys.Shift | Keys.F17;
        Keys backward = Keys.Control | Keys.Alt | Keys.Shift | Keys.F18;
        Keys roleKey = Keys.Control | Keys.Alt | Keys.Shift | Keys.F19;
        if (!CanOwn(forward) || !CanOwn(backward) || !CanOwn(roleKey))
            throw new Exception("Routing fixture needs unoccupied Ctrl+Alt+Shift+F17/F18/F19.");
        object config = MinimalConfig(forward, backward);
        Call(config, "SetClientHotkey", "EVE - Harness B", roleKey);
        var windows = new FakeWindowManager();
        object manager = New("EveOPreview.Services.ThumbnailManager", null, config, null, windows, null);
        try
        {
            Check(CanOwn(roleKey), "offline role binding does not consume a game's key");
            var clients = (Dictionary<IntPtr, IThumbnailView>)manager.GetType().GetField("_thumbnailViews", Any)!.GetValue(manager)!;
            var a = MakeView(101, "EVE - Harness A");
            var b = MakeView(102, "EVE - Harness B");
            var c = MakeView(103, "EVE - Harness C");
            clients.Add(a.Id, a); clients.Add(b.Id, b); clients.Add(c.Id, c);
            Call(manager, "SynchronizeClientHotkeys");
            Check(!CanOwn(roleKey), "online role shortcut registers in Windows");
            DispatchRegisteredBinding(manager, roleKey);
            Check(windows.Activated.SequenceEqual(new[] { b.Id }), "role shortcut activates exactly the requested fake client");
            Check(((FixtureProxy)(object)b).Calls.Contains("SetHighlight") && ((FixtureProxy)(object)b).Calls.Contains("Refresh"), "role activation updates target preview highlight and image");
            DispatchRegisteredBinding(manager, forward);
            Check(windows.Activated.Last() == c.Id, "next shortcut routes B to C");
            ExecuteBinding(manager, forward);
            Check(windows.Activated.Last() == a.Id, "next shortcut wraps last client to first");
            ExecuteBinding(manager, backward);
            Check(windows.Activated.Last() == c.Id, "previous shortcut wraps first client to last");
            ExecuteBinding(manager, backward);
            Check(windows.Activated.Last() == b.Id, "previous shortcut routes C to B");
            b.IsExcludedFromCycleGroup = true;
            ExecuteBinding(manager, forward);
            ExecuteBinding(manager, backward);
            Check(windows.Activated.Last() == a.Id, "quick cycling skips clients excluded from cycle groups");
            var customOrder = new Dictionary<string, int> { [c.Title] = 1, [a.Title] = 2 };
            Call(manager, "CycleNextClient", true, customOrder);
            Check(windows.Activated.Last() == c.Id, "advanced ordered group wraps according to configured order");
            clients.Remove(b.Id);
            Call(manager, "SynchronizeClientHotkeys");
            Check(CanOwn(roleKey), "closing role client releases its Windows shortcut");
            int activationCount = windows.Activated.Count;
            ExecuteBinding(manager, roleKey);
            Check(windows.Activated.Count == activationCount, "offline role callback does not activate a different client");
        }
        finally { Call(manager, "SetHotkeyCaptureActive", true); Call(manager, "Stop"); }
    }

    private static IThumbnailView MakeView(int id, string title)
    {
        var view = DispatchProxy.Create<IThumbnailView, FixtureProxy>();
        view.Id = (IntPtr)id;
        view.Title = title;
        return view;
    }
    private static void ExecuteBinding(object manager, Keys key)
    {
        var bindings = (System.Collections.IEnumerable)manager.GetType().GetField("_activeHotkeyBindings", Any)!.GetValue(manager)!;
        object binding = bindings.Cast<object>().Single(x => (Keys)Get(x, "Key")! == key);
        ((Action)Get(binding, "Execute")!)();
    }
    private static void DispatchRegisteredBinding(object manager, Keys key)
    {
        var handlers = (System.Collections.IEnumerable)manager.GetType().GetField("_cycleClientHotkeyHandlers", Any)!.GetValue(manager)!;
        object handler = handlers.Cast<object>().Single(x => (Keys)Get(x, "KeyCode")! == key);
        int id = (int)handler.GetType().GetField("_hotkeyId", Any)!.GetValue(handler)!;
        var wrong = Message.Create(IntPtr.Zero, 0x312, (IntPtr)(id + 10000), IntPtr.Zero);
        object[] unrelatedArgs = { wrong };
        Check(!(bool)handler.GetType().GetMethod("PreFilterMessage", Any)!.Invoke(handler, unrelatedArgs)!, "registered " + key + " ignores an unrelated hotkey message");
        var message = Message.Create(IntPtr.Zero, 0x312, (IntPtr)id, IntPtr.Zero);
        object[] args = { message };
        Check((bool)handler.GetType().GetMethod("PreFilterMessage", Any)!.Invoke(handler, args)!, "registered " + key + " handles its Windows hotkey message");
    }

    public class FixtureProxy : DispatchProxy
    {
        private readonly Dictionary<string, object?> values = new();
        public List<string> Calls { get; } = new();
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            string name = method!.Name;
            if (name.StartsWith("set_")) { values[name[4..]] = args![0]; return null; }
            if (name.StartsWith("get_") && values.TryGetValue(name[4..], out var value)) return value;
            Calls.Add(name);
            return method.ReturnType == typeof(void) ? null : method.ReturnType.IsValueType ? Activator.CreateInstance(method.ReturnType) : null;
        }
    }
    private sealed class FakeWindowManager : IWindowManager
    {
        public List<IntPtr> Activated { get; } = new();
        public bool IsCompositionEnabled => true;
        public IntPtr GetForegroundWindowHandle() => Activated.LastOrDefault();
        public void ActivateWindow(IntPtr handle, AnimationStyle animation, bool focus) => Activated.Add(handle);
        public void MinimizeWindow(IntPtr handle, AnimationStyle animation, bool enableAnimation) => throw new Exception("Unexpected minimize in routing test.");
        public void RestoreWindowInBackground(IntPtr handle) => throw new Exception("Unexpected restore in routing test.");
        public void MoveWindow(IntPtr handle, int left, int top, int width, int height) => throw new Exception("Unexpected move in routing test.");
        public void MaximizeWindow(IntPtr handle) => throw new Exception("Unexpected maximize in routing test.");
        public (int Left, int Top, int Right, int Bottom) GetWindowPosition(IntPtr handle) => (0, 0, 1920, 1080);
        public bool IsWindowMaximized(IntPtr handle) => false;
        public bool IsWindowMinimized(IntPtr handle) => false;
        public IDwmThumbnail GetLiveThumbnail(IntPtr destination, IntPtr source) => throw new Exception("Unexpected native thumbnail request.");
        public System.Drawing.Image GetStaticThumbnail(IntPtr source) => throw new Exception("Unexpected screen capture request.");
    }

    private static void NativeHandlerTests()
    {
        Type native = T("EveOPreview.UI.Hotkeys.HotkeyHandlerNativeMethods");
        Check(Convert.ToUInt32(native.GetField("MOD_NOREPEAT", BindingFlags.Public | BindingFlags.Static)!.GetRawConstantValue()) == 0x4000, "MOD_NOREPEAT Windows constant is correct");
        Type handler = T("EveOPreview.UI.Hotkeys.HotkeyHandler");
        byte[] il = handler.GetMethod("Register", Any)!.GetMethodBody()!.GetILAsByteArray()!;
        Check(Contains(il, new byte[] { 0x20, 0x00, 0x40, 0x00, 0x00 }), "Register includes MOD_NOREPEAT in native modifiers");
        var invalid = New(handler.FullName!, IntPtr.Zero, Keys.Control | Keys.Alt);
        try { Check(!(bool)Call(invalid, "Register")!, "native handler rejects pure-modifier gesture"); }
        finally { ((IDisposable)invalid).Dispose(); }
    }
    private static bool Contains(byte[] source, byte[] pattern) => Enumerable.Range(0, Math.Max(0, source.Length - pattern.Length + 1)).Any(i => source.AsSpan(i, pattern.Length).SequenceEqual(pattern));

    private static void UiTests()
    {
        using var context = new ApplicationContext();
        using var form = new MainForm(context);
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new System.Drawing.Point(-10000, -10000);
        ((Form)form).Show();
        Application.DoEvents();
        Check(form.Text.Contains("荣耀海军"), "form retains Glory Navy branding");
        Check(form.Controls.Find("ContentTabControl", true).Length == 1, "settings tab control available");
        Set(form, "QuickSwitchForwardHotkey", "Control+Alt+Right");
        Set(form, "QuickSwitchBackwardHotkey", "Control+Alt+Left");
        Set(form, "QuickHideHotkey", "Control+Alt+H");
        Check(Parse((string)Get(form, "QuickHideHotkey")!).Key == (Keys.Control | Keys.Alt | Keys.H), "UI hotkey property roundtrip");
        Check(form.Controls.Find("QuickHotkeysApplyButton", true).Length == 1, "quick apply control exists");
        int applied = 0;
        SetMember(form, "QuickHotkeysApplyRequested", (Action)(() => applied++));
        Click(form, "QuickHotkeysApplyButton");
        Check(applied == 1, "quick apply invokes presenter callback once");
        foreach (var item in new[]
        {
            ("QuickForward", "QuickSwitchForwardHotkey", "Control+Alt+Right"),
            ("QuickBackward", "QuickSwitchBackwardHotkey", "Control+Alt+Left"),
            ("QuickHide", "QuickHideHotkey", "Control+Alt+H")
        })
        {
            Click(form, item.Item1 + "ClearButton");
            Check((string)Get(form, item.Item2)! == "", item.Item1 + " clear stages empty binding");
            Check(applied == 1, item.Item1 + " clear does not apply before confirmation");
            Click(form, item.Item1 + "ResetButton");
            Check((string)Get(form, item.Item2)! == item.Item3, item.Item1 + " default restores expected gesture");
        }
        var list = (CheckedListBox)form.Controls.Find("ThumbnailsList", true).Single();
        Check(!list.CheckOnClick, "selecting a role does not automatically toggle preview visibility");
        Check(!form.Controls.Find("ClientHotkeyTextBox", true).Single().Enabled, "role hotkey editing disabled without client selection");
        list.Items.Add(new TestDescription { Title = "EVE - Harness role", IsDisabled = false });
        list.SelectedIndex = 0;
        Check(!list.GetItemChecked(0), "first selection leaves the client visibility checkbox unchanged");
        Check(form.Controls.Find("ClientHotkeyTextBox", true).Single().Enabled, "role hotkey editing enabled for selected client");
        string? role = null, roleKey = null;
        SetMember(form, "ClientHotkeyApplyRequested", (Action<string, string>)((title, key) => { role = title; roleKey = key; }));
        Call(form, "SetSelectedClientHotkey", "Control+Shift+F20");
        Click(form, "ClientHotkeyApplyButton");
        Check(role == "EVE - Harness role" && Parse(roleKey).Key == (Keys.Control | Keys.Shift | Keys.F20), "role apply targets selected client and staged gesture");
        Click(form, "ClientHotkeyClearButton");
        Click(form, "ClientHotkeyApplyButton");
        Check(role == "EVE - Harness role" && roleKey == "", "role clear applies empty binding");
        Call(form, "SetHotkeyStatus", "Harness conflict status", false);
        var status = (Label)form.Controls.Find("QuickHotkeyStatusLabel", true).Single();
        Check(status.Text == "Harness conflict status" && status.ForeColor == System.Drawing.Color.Firebrick, "failed hotkey status is readable and styled as error");
        Call(form, "SetQuickHotkeyStatus", "quick pending", true);
        Call(form, "SetClientHotkeyStatus", "client saved", true);
        Check(status.Text == "quick pending", "saving role binding preserves pending quick hotkey status");
        Call(form, "SetQuickHotkeyStatus", "quick saved", true);
        Check(((Label)form.Controls.Find("ClientHotkeyStatusLabel", true).Single()).Text == "client saved", "saving quick bindings preserves role hotkey status");
        var captureStates = new List<bool>();
        form.HotkeyCaptureStateChanged = active => captureStates.Add(active);
        bool changedByCancel = false;
        using (var timer = new System.Windows.Forms.Timer { Interval = 40 })
        {
            timer.Tick += (_, _) =>
            {
                Form? dialog = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name == "HotkeyCaptureDialog");
                if (dialog == null) return;
                timer.Stop();
                PressDialogKey(dialog, Keys.Escape);
            };
            timer.Start();
            Call(form, "CaptureHotkey", "Control+Alt+Right", (Action<string>)(_ => changedByCancel = true));
        }
        Check(captureStates.SequenceEqual(new[] { true, false }), "capture cancellation suspends then restores global hotkeys exactly once");
        Check(!changedByCancel && form.QuickSwitchForwardHotkey == "Control+Alt+Right", "cancelling modal capture keeps staged hotkey unchanged");

        LocalizationExtensions.SetLanguage("zh-CN");
        using (var cancel = (Form)New("EveOPreview.View.HotkeyCaptureDialog", "Control+Alt+Right"))
        {
            cancel.StartPosition = FormStartPosition.Manual;
            cancel.Location = new System.Drawing.Point(-10000, -10000);
            cancel.Show();
            Application.DoEvents();
            Control cancelButton = (Control)cancel.CancelButton!;
            var cancelPosition = cancel.PointToClient(cancelButton.PointToScreen(System.Drawing.Point.Empty));
            Check(cancelPosition.X >= 0 && cancelPosition.X + cancelButton.Width <= cancel.ClientSize.Width && cancelPosition.Y + cancelButton.Height <= cancel.ClientSize.Height, "capture Cancel button fits visible dialog viewport");
            using (var bitmap = new System.Drawing.Bitmap(cancel.Width, cancel.Height))
            {
                cancel.DrawToBitmap(bitmap, new System.Drawing.Rectangle(System.Drawing.Point.Empty, cancel.Size));
                string image = Path.Combine(screenshots, "capture-zh-CN-96dpi.png");
                bitmap.Save(image, System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine("SCREENSHOT " + image);
            }
            PressDialogKey(cancel, Keys.Escape);
            Check(cancel.DialogResult == DialogResult.Cancel && Get(cancel, "CapturedHotkey") == null, "Esc cancels capture without an accepted gesture");
        }
        using (var capture = (Form)New("EveOPreview.View.HotkeyCaptureDialog", "Control+Alt+Right"))
        {
            PressDialogKey(capture, Keys.ControlKey);
            Check(capture.DialogResult == DialogResult.None && Get(capture, "CapturedHotkey") == null, "pure modifier leaves capture pending");
            PressDialogKey(capture, Keys.F12);
            Check(capture.DialogResult == DialogResult.None && Get(capture, "CapturedHotkey") == null, "reserved F12 leaves capture pending");
            PressDialogKey(capture, Keys.Control | Keys.Shift | Keys.K);
            Check(capture.DialogResult == DialogResult.OK && Parse((string)Get(capture, "CapturedHotkey")!).Key == (Keys.Control | Keys.Shift | Keys.K), "valid gesture accepts capture without sending OS input");
        }
        // Render the real controls in-process without displaying/activating a form.
        foreach (string language in new[] { "zh-CN", "en-US" })
        {
            LocalizationExtensions.SetLanguage(language);
            form.InitializeLanguageControls();
            var tabs = (TabControl)form.Controls.Find("ContentTabControl", true).Single();
            foreach (string pageName in new[] { "QuickActionsTabPage", "ClientsTabPage" })
            {
                TabPage page = tabs.TabPages.Cast<TabPage>().Single(x => x.Name == pageName);
                tabs.SelectedTab = page;
                form.PerformLayout();
                Application.DoEvents();
                string path = Path.Combine(screenshots, $"settings-{language}-{pageName}-96dpi.png");
                using var bitmap = new System.Drawing.Bitmap(form.Width, form.Height);
                form.DrawToBitmap(bitmap, new System.Drawing.Rectangle(System.Drawing.Point.Empty, form.Size));
                bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine("SCREENSHOT " + path);
                if (pageName == "QuickActionsTabPage")
                {
                    Control scroll = form.Controls.Find("QuickActionsScrollPanel", true).Single();
                    Control footer = form.Controls.Find("QuickHotkeysFooter", true).Single();
                    Check(scroll.Parent == footer.Parent && scroll.Bottom <= footer.Top, $"{language} fixed footer does not cover scroll content");
                    Control apply = form.Controls.Find("QuickHotkeysApplyButton", true).Single();
                    var applyPosition = page.PointToClient(apply.PointToScreen(System.Drawing.Point.Empty));
                    Check(applyPosition.X >= 0 && applyPosition.Y >= 0 && applyPosition.X + apply.Width <= page.ClientSize.Width && applyPosition.Y + apply.Height <= page.ClientSize.Height, $"{language} Apply hotkeys remains visible in fixed footer");
                    foreach (string prefix in new[] { "QuickForward", "QuickBackward", "QuickHide" })
                    {
                        Control reset = form.Controls.Find(prefix + "ResetButton", true).Single();
                        var resetPosition = page.PointToClient(reset.PointToScreen(System.Drawing.Point.Empty));
                        Check(resetPosition.X >= 0 && resetPosition.X + reset.Width <= page.ClientSize.Width - 2, $"{language} {prefix} buttons fit visible settings viewport");
                    }
                }
                if (pageName == "ClientsTabPage")
                {
                    Control textbox = form.Controls.Find("ClientHotkeyTextBox", true).Single();
                    var position = page.PointToClient(textbox.PointToScreen(System.Drawing.Point.Empty));
                    Check(position.X >= 0 && position.X + textbox.Width <= page.ClientSize.Width - 2, $"{language} role hotkey field fits visible settings viewport");
                }
            }
        }
    }
    private static void PressDialogKey(Form dialog, Keys key)
    {
        object[] args = { Message.Create(IntPtr.Zero, 0x100, (IntPtr)(int)(key & Keys.KeyCode), IntPtr.Zero), key };
        dialog.GetType().GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(dialog, args);
    }

    private sealed class TestDescription : IThumbnailDescription
    {
        public string Title { get; set; } = "";
        public bool IsDisabled { get; set; }
        public override string ToString() => Title;
    }
    private static void Click(Control form, string name)
    {
        Control? control = form.Controls.Find(name, true).SingleOrDefault();
        if (control is not Button button) throw new Exception("Button missing: " + name);
        // Invoke the event without needing the current tab to be visible or stealing focus.
        typeof(Control).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(button, new object[] { EventArgs.Empty });
    }

    private sealed class NativeOwner : IDisposable
    {
        private readonly ManualResetEventSlim ready = new();
        private readonly ManualResetEventSlim stop = new();
        private readonly Thread worker;
        public bool Registered { get; private set; }
        public NativeOwner(Keys key)
        {
            worker = new Thread(() =>
            {
                const int id = 0x6A51;
                uint modifiers = 0x4000 | (key.HasFlag(Keys.Control) ? 2u : 0) | (key.HasFlag(Keys.Alt) ? 1u : 0) | (key.HasFlag(Keys.Shift) ? 4u : 0);
                Registered = RegisterHotKey(IntPtr.Zero, id, modifiers, (uint)(key & Keys.KeyCode));
                ready.Set();
                stop.Wait();
                if (Registered) UnregisterHotKey(IntPtr.Zero, id);
            }) { IsBackground = true, Name = "EveO-HotkeyRegression-owner" };
            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();
            if (!ready.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Native shortcut probe did not start.");
        }
        public void Dispose() { stop.Set(); worker.Join(TimeSpan.FromSeconds(5)); ready.Dispose(); stop.Dispose(); }
    }
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
