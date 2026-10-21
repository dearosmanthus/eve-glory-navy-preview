using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using EveOPreview.Configuration;
using EveOPreview.Services;
using EveOPreview.View;
using MediatR;

internal static partial class Program
{
    // Calls the real hook-to-UI seam, never SendInput or a game window.
    private static object? RuntimeMouseListener() => T("EveOPreview.UI.Hotkeys.MouseHotkeyRegistry")
        .GetField("_listener", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null);

    private static bool QueueRuntimeMouse(Keys gesture)
    {
        object? listener = RuntimeMouseListener();
        if (listener == null) return false;
        bool consumed = (bool)Call(listener, "ProcessInput", gesture & Keys.KeyCode, true, gesture & Keys.Modifiers, false)!;
        Call(listener, "ProcessInput", gesture & Keys.KeyCode, false, Keys.None, false);
        return consumed;
    }

    private static void MouseIntegrationTests()
    {
        const Keys mods = Keys.Control | Keys.Alt | Keys.Shift;
        Keys forward = mods | Keys.XButton1, backward = mods | Keys.XButton2;
        Keys hide = mods | Keys.MButton, role = Keys.Control | Keys.Shift | Keys.MButton;
        Keys groupKey = Keys.Control | Keys.Alt | Keys.MButton;
        Keys busy = mods | Keys.F24;
        if (!CanOwn(busy)) throw new Exception("Mixed fixture needs an unoccupied Ctrl+Alt+Shift+F24.");
        var config = (IThumbnailConfiguration)MinimalConfig(forward, backward);
        config.QuickHideHotkey = new KeysConverter().ConvertToInvariantString(hide)!;
        config.SetClientHotkey("EVE - Mouse B", role);
        config.CycleGroups = new List<CycleGroupDefinition>
        {
            new() { Id = "mouse-group", Name = "鼠标分组", ForwardHotkeys = new() { new KeysConverter().ConvertToInvariantString(groupKey)! },
                ClientsOrder = new() { ["EVE - Mouse C"] = 1, ["EVE - Mouse A"] = 2 } }
        };
        config.ThumbnailSize = new Size(211, 119);
        var windows = new FakeWindowManager();
        var mediator = DispatchProxy.Create<IMediator, LifecycleMediator>();
        object manager = New("EveOPreview.Services.ThumbnailManager", mediator, config,
            new LifecycleProcessMonitor(), windows, null);
        ((LifecycleMediator)(object)mediator).Manager = (IThumbnailManager)manager;
        try
        {
            Check(RuntimeMouseListener() != null, "manager starts real shared mouse hook for configured buttons");
            Check(!QueueRuntimeMouse(role), "offline mouse role does not consume its gesture");
            var views = (Dictionary<IntPtr, IThumbnailView>)manager.GetType().GetField("_thumbnailViews", Any)!.GetValue(manager)!;
            IThumbnailView a = MakeView(901, "EVE - Mouse A"), b = MakeView(902, "EVE - Mouse B"), c = MakeView(903, "EVE - Mouse C");
            views.Add(a.Id, a); views.Add(b.Id, b); views.Add(c.Id, c);
            Call(manager, "SynchronizeClientHotkeys");
            Check(QueueRuntimeMouse(role) && windows.Activated.Count == 0, "online mouse role queues without synchronous action");
            Application.DoEvents();
            Check(windows.Activated.SequenceEqual(new[] { b.Id }), "real mouse dispatch activates exactly its fake role");
            Check(QueueRuntimeMouse(forward), "bound next side key is consumed");
            Application.DoEvents();
            Check(windows.Activated.Last() == c.Id, "side key next routes B to C");
            QueueRuntimeMouse(backward); Application.DoEvents();
            Check(windows.Activated.Last() == b.Id, "second side key previous routes C to B");
            QueueRuntimeMouse(groupKey); Application.DoEvents();
            Check(windows.Activated.Last() == c.Id, "mouse group hotkey respects named group member order");
            QueueRuntimeMouse(hide); Application.DoEvents();
            Check((bool)manager.GetType().GetField("_quickHideActive", Any)!.GetValue(manager)!, "wheel press toggles quick hide through real dispatch");
            QueueRuntimeMouse(hide); Application.DoEvents();
            Check(!(bool)manager.GetType().GetField("_quickHideActive", Any)!.GetValue(manager)!, "second wheel press restores previews");
            views.Remove(b.Id); Call(manager, "SynchronizeClientHotkeys");
            Check(!QueueRuntimeMouse(role), "closing role releases only that mouse gesture");

            int before = windows.Activated.Count;
            QueueRuntimeMouse(forward);
            Call(manager, "SetHotkeyCaptureActive", true);
            Application.DoEvents();
            Check(RuntimeMouseListener() == null && windows.Activated.Count == before, "capture suspension releases mouse hook and cancels pending switch");
            Call(manager, "SetHotkeyCaptureActive", false);
            Check(RuntimeMouseListener() != null, "ending capture restores applied mouse bindings");

            using (var foreign = new NativeOwner(busy))
            {
                Check(foreign.Registered, "mixed transaction has a genuine foreign keyboard conflict");
                QueueRuntimeMouse(forward);
                config.QuickSwitchBackwardHotkey = new KeysConverter().ConvertToInvariantString(busy)!;
                config.CycleGroups[0].Name = "未保存的名称";
                Check(!Apply(manager, out string error) && error.Length > 0, "keyboard failure rolls back an already registered mouse binding");
                Application.DoEvents();
                Check(windows.Activated.Count == before && Parse(config.QuickSwitchBackwardHotkey).Key == backward
                    && config.CycleGroups[0].Name == "鼠标分组" && config.ThumbnailSize == new Size(211, 119),
                    "mixed rollback cancels old queues, restores full hotkey snapshot and preserves unrelated size");
                Check(QueueRuntimeMouse(backward), "previous mouse bindings remain usable after keyboard conflict");
                Application.DoEvents();
            }
            config.CycleGroups[0].BackwardHotkeys.Add(config.QuickSwitchForwardHotkey);
            Check(!Apply(manager, out string duplicate) && duplicate.Length > 0 && config.CycleGroups[0].BackwardHotkeys.Count == 0,
                "duplicate between quick mouse key and group rejects complete edit");

            // Existing JSON strings need no migration for native or driver keys.
            config.SetClientHotkey("EVE - Side 3", Keys.F13);
            config.SetClientHotkey("EVE - Side 4", Keys.F14);
            var appConfig = New("EveOPreview.Configuration.Implementation.AppConfig");
            Set(appConfig, "ConfigFileName", Path.Combine(scratch, "mixed-mouse-profile.json"));
            Call(New("EveOPreview.Configuration.Implementation.ConfigurationStorage", appConfig, config), "Save");
            var reloaded = (IThumbnailConfiguration)Config();
            Call(New("EveOPreview.Configuration.Implementation.ConfigurationStorage", appConfig, reloaded), "Load");
            Check(Parse(reloaded.QuickSwitchForwardHotkey).Key == forward && Parse(reloaded.QuickHideHotkey).Key == hide
                && reloaded.GetClientHotkey("EVE - Side 3") == Keys.F13 && reloaded.GetClientHotkey("EVE - Side 4") == Keys.F14,
                "JSON round trip preserves native mouse and F13/F14 driver mapping strings");
            Check(reloaded.CycleGroups.Single().Id == "mouse-group" && reloaded.ThumbnailSize == config.ThumbnailSize,
                "mouse persistence preserves named group and existing thumbnail dimensions");

            before = windows.Activated.Count;
            QueueRuntimeMouse(forward);
            Call(manager, "Stop");
            Application.DoEvents();
            Check(RuntimeMouseListener() == null && windows.Activated.Count == before, "Stop releases final hook and cancels queued switching");
            Call(manager, "SetHotkeyCaptureActive", true); Call(manager, "SetHotkeyCaptureActive", false);
            Call(manager, "SynchronizeClientHotkeys");
            config.QuickSwitchForwardHotkey = "F13";
            Check(!Apply(manager, out _) && Parse(config.QuickSwitchForwardHotkey).Key == forward && RuntimeMouseListener() == null,
                "stopped manager cannot rearm through capture, synchronization or Apply");
            Call(manager, "Start");
            Check(RuntimeMouseListener() != null && QueueRuntimeMouse(forward), "Start restores previously applied mouse bindings");
            Application.DoEvents();
            Check(windows.Activated.Count == before + 1, "restored manager dispatches exactly once after restart");
        }
        finally { Call(manager, "Stop"); }
        Check(RuntimeMouseListener() == null && CanOwn(busy), "mixed manager cleanup leaves no test listener or keyboard claim");
    }
}
