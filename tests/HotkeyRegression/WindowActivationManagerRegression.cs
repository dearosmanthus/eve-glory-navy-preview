using System.Drawing;
using System.Windows.Forms;
using EveOPreview.Configuration;
using EveOPreview.Services;
using EveOPreview.View;

internal static partial class Program
{
    private static void WindowActivationManagerTests()
    {
        var config = (IThumbnailConfiguration)MinimalConfig(Keys.None, Keys.None);
        config.MinimizeInactiveClients = false;
        config.KeepMinimizedClientsLive = false;
        var native = new RejectingActivationWindowManager();
        object manager = New("EveOPreview.Services.ThumbnailManager", null, config, null, native, null);
        try
        {
            var views = (Dictionary<IntPtr, IThumbnailView>)manager.GetType().GetField("_thumbnailViews", Any)!.GetValue(manager)!;
            IThumbnailView a = MakeView(1101, "EVE - Focus A"), b = MakeView(1102, "EVE - Focus B");
            views.Add(a.Id, a); views.Add(b.Id, b);
            Call(manager, "SetActive", new KeyValuePair<IntPtr, IThumbnailView>(a.Id, a));
            Check(ReferenceEquals(Call(manager, "GetActiveClient"), a), "activation fixture initially selects first client");
            ((FixtureProxy)(object)a).Calls.Clear();
            ((FixtureProxy)(object)b).Calls.Clear();
            config.MinimizeInactiveClients = true;
            native.Reject = true;
            Call(manager, "SetActive", new KeyValuePair<IntPtr, IThumbnailView>(b.Id, b));
            Check(ReferenceEquals(Call(manager, "GetActiveClient"), a) && native.Foreground == a.Id,
                "Windows rejection preserves actual and internal current client");
            Check(native.Minimized.Count == 0, "failed foreground switch never minimizes the original client");
            Check(!((FixtureProxy)(object)a).Calls.Contains("ClearBorder") && !((FixtureProxy)(object)b).Calls.Contains("SetHighlight"),
                "failed foreground switch never advertises a false highlight success");
            native.Reject = false;
            Call(manager, "SetActive", new KeyValuePair<IntPtr, IThumbnailView>(b.Id, b));
            Check(ReferenceEquals(Call(manager, "GetActiveClient"), b) && native.Foreground == b.Id && native.Minimized.SequenceEqual(new[] { a.Id }),
                "successful activation commits selection before minimizing the old client once");
            // Same internally selected role must still be brought back after
            // the user interacts with a different application/game window.
            native.Foreground = (IntPtr)1199;
            int attempts = native.Attempts;
            Call(manager, "SetActive", new KeyValuePair<IntPtr, IThumbnailView>(b.Id, b));
            Check(native.Attempts == attempts + 1 && native.Foreground == b.Id,
                "selected role shortcut reacquires foreground after external interaction");
            native.Foreground = (IntPtr)1199;
            attempts = native.Attempts;
            bool stale = (bool)Call(manager, "SwitchActiveClient", b.Id, b.Title, false)!;
            Check(!stale && native.Attempts == attempts && native.Foreground == (IntPtr)1199,
                "passive thumbnail refresh never reclaims focus from a stale foreground snapshot");
        }
        finally { Call(manager, "Stop"); }

        var emptyConfig = (IThumbnailConfiguration)MinimalConfig(Keys.None, Keys.None);
        var refusing = new RejectingActivationWindowManager { Reject = true };
        object empty = New("EveOPreview.Services.ThumbnailManager", null, emptyConfig, null, refusing, null);
        try
        {
            var views = (Dictionary<IntPtr, IThumbnailView>)empty.GetType().GetField("_thumbnailViews", Any)!.GetValue(empty)!;
            IThumbnailView first = MakeView(1201, "EVE - First rejected");
            views.Add(first.Id, first);
            Call(empty, "CycleNextClient", true, new Dictionary<string, int> { [first.Title] = 1 });
            Check(Call(empty, "GetActiveClient") == null,
                "first-cycle fallback cannot overwrite a rejected activation result");
        }
        finally { Call(empty, "Stop"); }

        // This hidden window belongs to the calling UI thread. It must never
        // be pinged by a worker while this same thread waits for that worker.
        using var ownWindow = new Form { ShowInTaskbar = false };
        _ = ownWindow.Handle;
        object nativeWorker = T("EveOPreview.Services.Implementation.ForegroundWindowActivator")
            .GetField("Worker", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!.GetValue(null)!;
        bool existed = (bool)Get(nativeWorker, "IsValueCreated")!;
        var realWindows = new EveOPreview.Services.Implementation.WindowManager((IThumbnailConfiguration)Config());
        realWindows.TryActivateWindow(ownWindow.Handle, AnimationStyle.OriginalAnimation, true);
        Check((bool)Get(nativeWorker, "IsValueCreated")! == existed,
            "same-thread own-window activation does not start a worker that would ping its waiting UI");
    }

    private sealed class RejectingActivationWindowManager : IWindowManager
    {
        public bool Reject { get; set; }
        public IntPtr Foreground { get; set; }
        public int Attempts { get; private set; }
        public List<IntPtr> Minimized { get; } = new();
        public bool IsCompositionEnabled => true;
        public IntPtr GetForegroundWindowHandle() => Foreground;
        public void ActivateWindow(IntPtr handle, AnimationStyle animation, bool focus)
        {
            Attempts++;
            if (!Reject && focus) Foreground = handle;
        }
        public bool TryActivateWindow(IntPtr handle, AnimationStyle animation, bool focus)
        {
            ActivateWindow(handle, animation, focus);
            return !Reject;
        }
        public void MinimizeWindow(IntPtr handle, AnimationStyle animation, bool enableAnimation) => Minimized.Add(handle);
        public void RestoreWindowInBackground(IntPtr handle) => throw new Exception("Unexpected restore");
        public void MoveWindow(IntPtr handle, int left, int top, int width, int height) => throw new Exception("Unexpected move");
        public void MaximizeWindow(IntPtr handle) => throw new Exception("Unexpected maximize");
        public (int Left, int Top, int Right, int Bottom) GetWindowPosition(IntPtr handle) => (0, 0, 1920, 1080);
        public bool IsWindowMaximized(IntPtr handle) => false;
        public bool IsWindowMinimized(IntPtr handle) => false;
        public IDwmThumbnail GetLiveThumbnail(IntPtr destination, IntPtr source) => throw new Exception("Unexpected thumbnail");
        public Image GetStaticThumbnail(IntPtr source) => throw new Exception("Unexpected capture");
    }
}
