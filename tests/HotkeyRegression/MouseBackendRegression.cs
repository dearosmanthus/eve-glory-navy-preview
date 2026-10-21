using System.Reflection;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

internal static partial class Program
{
    private static void MouseBackendTests()
    {
        MouseGestureContractTests();
        MouseDecodeAndStateTests();
        MouseDispatchBoundaryTests();
        MouseSharedRegistrationTests();
    }

    private static void MouseGestureContractTests()
    {
        Type gestures = T("EveOPreview.UI.Hotkeys.HotkeyGesture");
        var format = gestures.GetMethod("Format", BindingFlags.Public | BindingFlags.Static)!;
        var mouse = gestures.GetMethod("IsMouseGesture", BindingFlags.Public | BindingFlags.Static)!;
        foreach (Keys button in new[] { Keys.MButton, Keys.XButton1, Keys.XButton2 })
        {
            foreach (Keys modifiers in new[] { Keys.None, Keys.Control, Keys.Alt, Keys.Shift,
                Keys.Control | Keys.Alt, Keys.Control | Keys.Shift, Keys.Alt | Keys.Shift,
                Keys.Control | Keys.Alt | Keys.Shift })
            {
                Keys expected = button | modifiers;
                string text = (string)format.Invoke(null, new object[] { expected })!;
                Check(Parse(text) == (true, expected), "mouse gesture persists and round trips " + expected);
                Check((bool)mouse.Invoke(null, new object[] { expected })!, "mouse route selected " + expected);
            }
        }
        foreach (string invalid in new[] { "LButton", "RButton", "Control+LButton", "Alt+RButton", "LWin", "RWin", "Control+F12" })
            Check(!Parse(invalid).Success, "mouse extension preserves rejected gesture " + invalid);
        foreach (Keys keyboard in new[] { Keys.F13, Keys.F14, Keys.Control | Keys.F13, Keys.Alt | Keys.F14 })
        {
            string text = (string)format.Invoke(null, new object[] { keyboard })!;
            Check(Parse(text).Key == keyboard, "driver-mapped extra button remains keyboard " + keyboard);
            Check(!(bool)mouse.Invoke(null, new object[] { keyboard })!, "driver-mapped extra button avoids mouse route " + keyboard);
        }
    }

    private static void MouseDecodeAndStateTests()
    {
        Type decoder = T("EveOPreview.UI.Hotkeys.MouseInputDecoder");
        MethodInfo decode = decoder.GetMethod("TryDecode", BindingFlags.Static | BindingFlags.NonPublic)!;
        (bool Success, Keys Button, bool Down) Decode(int message, uint data)
        {
            object?[] args = { message, data, Keys.None, false };
            bool success = (bool)decode.Invoke(null, args)!;
            return (success, (Keys)args[2]!, (bool)args[3]!);
        }
        Check(Decode(0x0207, 0) == (true, Keys.MButton, true), "decode native middle down");
        Check(Decode(0x0208, 0) == (true, Keys.MButton, false), "decode native middle up");
        Check(Decode(0x020B, 1u << 16) == (true, Keys.XButton1, true), "decode native first side button");
        Check(Decode(0x020C, 2u << 16) == (true, Keys.XButton2, false), "decode native second side release");
        foreach (int message in new[] { 0x0200, 0x0201, 0x0202, 0x0204, 0x0205, 0x020A, 0x020E })
            Check(!Decode(message, 1u << 16).Success, "unrelated native mouse input passes through " + message);
        foreach (uint xbutton in new uint[] { 0, 3, 4, 0xFFFF })
            Check(!Decode(0x020B, xbutton << 16).Success, "reject nonstandard side button " + xbutton);

        object state = New("EveOPreview.UI.Hotkeys.MouseInputState");
        MethodInfo process = state.GetType().GetMethod("Process", Any)!;
        int acceptsCalls = 0;
        Func<Keys, bool> accepts = gesture => { acceptsCalls++; return gesture == (Keys.Control | Keys.XButton1) || gesture == Keys.MButton; };
        (bool Suppress, Keys Fired) Input(Keys button, bool down, Keys modifiers = Keys.None, bool windows = false)
        {
            object?[] args = { button, down, modifiers, windows, accepts, Keys.None };
            bool suppress = (bool)process.Invoke(state, args)!;
            return (suppress, (Keys)args[5]!);
        }
        Check(Input(Keys.XButton1, true, Keys.Control) == (true, Keys.Control | Keys.XButton1), "matching first mouse down fires once");
        Check(Input(Keys.XButton1, true, Keys.Alt) == (true, Keys.None), "held mouse repeat does not fire when modifiers change");
        Check(acceptsCalls == 1, "held mouse repeats never re-evaluate acceptance");
        Check(Input(Keys.XButton1, false) == (true, Keys.None), "accepted mouse release stays paired after modifier release");
        Check(Input(Keys.XButton1, false) == (false, Keys.None), "orphan release passes through");
        Check(Input(Keys.XButton1, true) == (false, Keys.None), "unbound mouse down passes through");
        Check(Input(Keys.XButton1, true, Keys.Control) == (false, Keys.None), "holding an unmatched button cannot acquire a hotkey");
        Check(Input(Keys.XButton1, false, Keys.Control) == (false, Keys.None), "unmatched mouse release passes through after modifier press");
        Check(Input(Keys.XButton1, true, Keys.Control, true) == (false, Keys.None), "Windows modifier excludes mouse binding");
        Input(Keys.XButton1, false);
        Check(Input(Keys.MButton, true) == (true, Keys.MButton), "independent middle button can fire");
        Check(Input(Keys.XButton1, true, Keys.Control) == (true, Keys.Control | Keys.XButton1), "different held buttons are independent");
        Check(Input(Keys.MButton, false, Keys.Alt, true) == (true, Keys.None), "paired release ignores later Windows and Alt state");
        Check(Input(Keys.XButton1, false, Keys.Shift) == (true, Keys.None), "each accepted button retains its own release pair");
        foreach (Keys forbidden in new[] { Keys.LButton, Keys.RButton, Keys.F13, Keys.None })
            Check(Input(forbidden, true) == (false, Keys.None), "state machine ignores unsupported input " + forbidden);
    }

    private static void MouseDispatchBoundaryTests()
    {
        int ownerThread = Environment.CurrentManagedThreadId;
        int callbackThread = 0;
        int calls = 0;
        object listener = New("EveOPreview.UI.Hotkeys.MouseInputListener", (Func<Keys, bool>)(_ => true),
            (Action<Keys>)(_ => { calls++; callbackThread = Environment.CurrentManagedThreadId; }));
        var disposable = (IDisposable)listener;
        bool Process(Keys button, bool down) => (bool)Call(listener, "ProcessInput", button, down, Keys.None, false)!;
        try
        {
            // Exercise the very same async seam used by HookCallback, without any native input.
            Exception? backgroundFailure = null;
            var sender = new Thread(() =>
            {
                try { Process(Keys.XButton1, true); }
                catch (Exception exception) { backgroundFailure = exception; }
            });
            sender.Start();
            sender.Join();
            Check(backgroundFailure == null && calls == 0, "mouse action never runs synchronously on input thread");
            Application.DoEvents();
            Check(calls == 1 && callbackThread == ownerThread, "mouse action is posted to creator UI thread");
            Process(Keys.XButton1, false);
            Process(Keys.XButton1, true);
            Call(listener, "InvalidatePending", new object?[] { null });
            Application.DoEvents();
            Check(calls == 1, "binding mutation cancels already queued mouse action");
            Process(Keys.XButton1, false);
            Process(Keys.MButton, true);
            disposable.Dispose();
            Application.DoEvents();
            Check(calls == 1, "disposing listener cancels already queued recording action");
            Check(!Process(Keys.XButton2, true), "disposed listener no longer accepts input");
            object?[] args = { 0 };
            bool started = (bool)listener.GetType().GetMethod("Start", Any)!.Invoke(listener, args)!;
            Check(!started && (int)args[0]! != 0, "disposed listener reports native startup failure explicitly");
            disposable.Dispose();
            Check(true, "mouse listener repeated Dispose is harmless");
        }
        finally { disposable.Dispose(); }

        // A re-recording owns a new dispatch queue; callbacks from the old listener stay canceled.
        int replacementCalls = 0;
        using var replacement = (IDisposable)New("EveOPreview.UI.Hotkeys.MouseInputListener", (Func<Keys, bool>)(_ => true),
            (Action<Keys>)(_ => replacementCalls++));
        Call(replacement, "ProcessInput", Keys.XButton2, true, Keys.Shift, false);
        Application.DoEvents();
        Check(calls == 1 && replacementCalls == 1, "new recording is isolated from canceled old recording actions");
        // Final removal does not retain a global release-only hook.
        replacement.Dispose();
        Check(!(bool)Call(replacement, "ProcessInput", Keys.XButton2, false, Keys.None, false)!,
            "release after final listener teardown passes through without reviving an action");
        int restartedCalls = 0;
        using var restarted = (IDisposable)New("EveOPreview.UI.Hotkeys.MouseInputListener", (Func<Keys, bool>)(_ => true),
            (Action<Keys>)(_ => restartedCalls++));
        Check(!(bool)Call(restarted, "ProcessInput", Keys.XButton2, false, Keys.None, false)!,
            "replacement listener does not consume an orphan release from its predecessor");
        Call(restarted, "ProcessInput", Keys.XButton2, true, Keys.None, false);
        Call(restarted, "ProcessInput", Keys.XButton2, false, Keys.None, false);
        Application.DoEvents();
        Check(restartedCalls == 1 && replacementCalls == 1,
            "fresh down after held-button teardown dispatches once and never invokes the old listener");
    }

    private static void MouseSharedRegistrationTests()
    {
        Type registry = T("EveOPreview.UI.Hotkeys.MouseHotkeyRegistry");
        FieldInfo listenerField = registry.GetField("_listener", BindingFlags.Static | BindingFlags.NonPublic)!;
        Check(listenerField.GetValue(null) == null, "empty mouse bindings do not install a native hook");
        object first = New("EveOPreview.UI.Hotkeys.HotkeyHandler", IntPtr.Zero, Keys.Control | Keys.Alt | Keys.Shift | Keys.XButton1);
        object second = New("EveOPreview.UI.Hotkeys.HotkeyHandler", IntPtr.Zero, Keys.Control | Keys.Alt | Keys.Shift | Keys.XButton2);
        object duplicate = New("EveOPreview.UI.Hotkeys.HotkeyHandler", IntPtr.Zero, Keys.Control | Keys.Alt | Keys.Shift | Keys.XButton1);
        try
        {
            bool firstRegistered = (bool)Call(first, "Register")!;
            Check(firstRegistered && (bool)Get(first, "IsRegistered")!, "native mouse gesture registers without RegisterHotKey");
            if (!firstRegistered)
            {
                Fail("native mouse listener startup error " + Get(first, "LastRegistrationError"));
                return;
            }
            object? shared = listenerField.GetValue(null);
            Check((bool)Call(second, "Register")! && ReferenceEquals(shared, listenerField.GetValue(null)), "all runtime mouse gestures share one native listener");
            object?[] startArgs = { 0 };
            Check((bool)shared!.GetType().GetMethod("Start", Any)!.Invoke(shared, startArgs)! && (int)startArgs[0]! == 0,
                "starting an active listener again preserves its native hook");
            Check(!(bool)Call(duplicate, "Register")! && (int)Get(duplicate, "LastRegistrationError")! == 1409,
                "duplicate mouse registration reports existing-binding error");
            // A mouse handler must not react to a forged keyboard WM_HOTKEY.
            int id = (int)first.GetType().GetField("_hotkeyId", Any)!.GetValue(first)!;
            Message keyboardMessage = Message.Create(IntPtr.Zero, 0x0312, new IntPtr(id), IntPtr.Zero);
            object?[] messageArgs = { keyboardMessage };
            Check(!(bool)first.GetType().GetMethod("PreFilterMessage", Any)!.Invoke(first, messageArgs)!, "mouse route cannot be triggered by WM_HOTKEY");
            int oldActions = 0;
            int replacementActions = 0;
            HandledEventHandler oldPressed = (_, args) => { oldActions++; args.Handled = true; };
            HandledEventHandler replacementPressed = (_, args) => { replacementActions++; args.Handled = true; };
            first.GetType().GetEvent("Pressed", Any)!.AddEventHandler(first, oldPressed);
            duplicate.GetType().GetEvent("Pressed", Any)!.AddEventHandler(duplicate, replacementPressed);
            Call(shared!, "ProcessInput", Keys.XButton1, true, Keys.Control | Keys.Alt | Keys.Shift, false);
            Call(first, "Unregister");
            Check(ReferenceEquals(shared, listenerField.GetValue(null)), "removing one binding preserves listener for remaining mouse binding");
            Check((bool)Call(duplicate, "Register")!, "a removed mouse gesture can be rebound while the shared listener runs");
            Application.DoEvents();
            Check(oldActions == 0 && replacementActions == 0, "queued old mouse action cannot reach a replacement binding");
            Call(shared!, "ProcessInput", Keys.XButton1, false, Keys.None, false);
            Call(shared!, "ProcessInput", Keys.XButton1, true, Keys.Control | Keys.Alt | Keys.Shift, false);
            Application.DoEvents();
            Check(oldActions == 0 && replacementActions == 1, "fresh mouse action reaches only the current handler");
            Call(shared!, "ProcessInput", Keys.XButton1, false, Keys.None, false);
            Call(duplicate, "Unregister");
            Call(second, "Unregister");
            Check(listenerField.GetValue(null) == null, "removing final mouse binding releases native hook");
            Check((bool)Call(duplicate, "Register")!, "released mouse binding can be registered again");
        }
        finally
        {
            ((IDisposable)first).Dispose();
            ((IDisposable)second).Dispose();
            ((IDisposable)duplicate).Dispose();
        }
        Check(listenerField.GetValue(null) == null, "mouse registration cleanup leaves no native listener");
    }
}
