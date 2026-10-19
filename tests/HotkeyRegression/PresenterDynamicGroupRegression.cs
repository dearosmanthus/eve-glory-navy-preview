using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using EveOPreview;
using EveOPreview.Configuration;
using EveOPreview.Presenters;
using EveOPreview.Services;
using EveOPreview.View;
using MediatR;
using Newtonsoft.Json;

internal static partial class Program
{
    private static void PresenterDynamicGroupTests()
    {
        // Use the production Show/activation and modal message loops. Only the
        // monitor, window manager, mediator and persistence boundary are fake.
        // AddThumbnails/RemoveThumbnails are the presenter's public notification
        // entry points; no private presenter state or method is manipulated.
        using var context = new ApplicationContext();
        using var form = new MainForm(context)
        {
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-10000, -10000),
            TopMost = true
        };
        var config = (IThumbnailConfiguration)MinimalConfig(Keys.None, Keys.None);
        Check(!config.AutoAddUngroupedClients, "presenter fixture starts with automatic collection disabled by default");
        config.MinimizeToTray = false;
        config.Language = "zh-CN";
        config.CycleGroups = new()
        {
            new() { Id = "presenter-first", Name = "主力测试组", ClientsOrder = new() { ["EVE - Persisted offline"] = 0 } },
            new() { Id = "presenter-second", Name = "后勤测试组", ClientsOrder = new() { ["EVE - Other offline"] = 0 } }
        };
        var storage = new LifecycleStorage();
        var mediator = DispatchProxy.Create<IMediator, LifecycleMediator>();
        object manager = New("EveOPreview.Services.ThumbnailManager", mediator, config,
            new LifecycleProcessMonitor(), new FakeWindowManager(), null);
        ((LifecycleMediator)(object)mediator).Manager = (IThumbnailManager)manager;
        var controller = DispatchProxy.Create<IApplicationController, LifecycleController>();
        ((LifecycleController)(object)controller).Manager = (IThumbnailManager)manager;
        var presenter = new MainFormPresenter(controller, form, mediator, config, storage);
        Exception? failure = null;
        bool completed = false;
        using var timer = new System.Windows.Forms.Timer { Interval = 80 };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            try
            {
                ExercisePresenterDynamicGroups(form, presenter, config, storage, manager);
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
        try { presenter.Run(); }
        finally
        {
            timer.Stop();
            Call(manager, "SetHotkeyCaptureActive", true);
            Call(manager, "Stop");
        }
        if (failure != null) throw failure;
        Check(completed && storage.Loads == 1, "dynamic-group presenter integration completes its real startup/message loop without reloading settings");
    }

    private static void ExercisePresenterDynamicGroups(MainForm form, MainFormPresenter presenter,
        IThumbnailConfiguration config, LifecycleStorage storage, object manager)
    {
        const string a = "EVE - Integration Alpha", b = "EVE - Integration Beta", c = "EVE - Integration Gamma";
        var tabs = (TabControl)form.Controls.Find("ContentTabControl", true).Single();
        tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(page => page.Name == "CycleGroupTabPage");
        var groupList = (ListBox)form.Controls.Find("DynamicCycleGroupList", true).Single();
        var members = (ListView)form.Controls.Find("DynamicCycleGroupMembersList", true).Single();
        var legacy = (CheckedListBox)form.Controls.Find("ThumbnailsList", true).Single();
        var autoAdd = (CheckBox)form.Controls.Find("DynamicGroupAutoAddCheckBox", true).Single();
        Check(!autoAdd.Checked && !form.AutoAddUngroupedClients,
            "real presenter loads automatic collection OFF into the real checkbox");
        presenter.AddThumbnails(new[] { a });
        Check(presenter.GetActiveClients().Contains(a) && !PresenterHasGroupMember(config, a)
                && !form.GetCycleGroups().Any(group => group.ClientsOrder.ContainsKey(a)),
            "online notification does not change group membership while automatic collection is OFF");

        int savesBeforeEnable = storage.Saves;
        autoAdd.Checked = true; // Real control event -> MainForm -> presenter -> manager -> storage.
        Application.DoEvents();
        Check(config.AutoAddUngroupedClients && storage.Saves > savesBeforeEnable
                && config.CycleGroups[0].ClientsOrder.ContainsKey(a)
                && !config.CycleGroups[1].ClientsOrder.ContainsKey(a),
            "enabling automatic collection persists the option and collects an already-online ungrouped role into the first group");
        presenter.AddThumbnails(new[] { b });
        Check(config.CycleGroups[0].ClientsOrder.OrderBy(item => item.Value).Select(item => item.Key)
                    .SequenceEqual(new[] { "EVE - Persisted offline", a, b })
                && form.GetCycleGroups()[0].ClientsOrder.ContainsKey(b),
            "new online role appends after existing first-group members in configuration and UI");

        groupList.SelectedIndex = 0;
        SelectUiMember(members, a);
        Click(form, "DynamicGroupMemberRemoveButton");
        Check(!PresenterHasGroupMember(config, a) && !form.GetCycleGroups()[0].ClientsOrder.ContainsKey(a)
                && config.CycleGroupAutoAddExclusions.Contains(a),
            "real Remove action persists exclusion and does not immediately auto-add the online role back");
        presenter.RemoveThumbnails(new[] { a });
        presenter.AddThumbnails(new[] { a, c });
        Check(!PresenterHasGroupMember(config, a) && config.CycleGroupAutoAddExclusions.Contains(a)
                && config.CycleGroups[0].ClientsOrder.ContainsKey(c),
            "removed role stays excluded across offline/online notifications while other new roles still auto-collect");

        RunOwnUiModal(form, "CycleGroupMembersDialog", () => Click(form, "DynamicGroupMemberAddButton"), dialog =>
        {
            var available = (CheckedListBox)dialog.Controls.Find("DynamicGroupAvailableClientsList", true).Single();
            int index = available.Items.Cast<object>().Select((item, index) => (item, index))
                .Single(pair => pair.item.ToString()!.StartsWith("Integration Alpha —", StringComparison.Ordinal)).index;
            available.SetItemChecked(index, true);
            Click(dialog, "DynamicGroupMembersApplyButton");
        });
        Application.DoEvents();
        Check(config.CycleGroups[0].ClientsOrder.ContainsKey(a) && !config.CycleGroupAutoAddExclusions.Contains(a)
                && form.GetCycleGroups()[0].ClientsOrder.ContainsKey(a),
            "manual Add dialog returns role to the group and clears its automatic-collection exclusion");

        const string duringModal = "EVE - Integration During modal";
        RunOwnUiModal(form, "CycleGroupMembersDialog", () => Click(form, "DynamicGroupMemberAddButton"), dialog =>
        {
            presenter.AddThumbnails(new[] { duringModal });
            Check(!PresenterHasGroupMember(config, duringModal),
                "online notification during an outer settings dialog does not overwrite the active group draft");
            Click(dialog, "DynamicGroupMembersCancelButton");
        });
        Application.DoEvents();
        Check(config.CycleGroups[0].ClientsOrder.ContainsKey(duringModal),
            "deferred automatic collection resumes after the outer settings dialog closes");

        presenter.RemoveThumbnails(new[] { b });
        Check(config.CycleGroups[0].ClientsOrder.ContainsKey(b)
                && !presenter.GetActiveClients().Contains(b)
                && !legacy.Items.Cast<IThumbnailDescription>().Any(item => item.Title == b)
                && members.Items.Cast<ListViewItem>().Single(item => (string)item.Tag! == b).SubItems[1].Text.Contains("离线"),
            "offline notification removes the active preview row but retains and labels the configured group member");

        SelectUiMember(members, a);
        int savesBeforePreview = storage.Saves;
        Click(form, "DynamicGroupPreviewToggleButton");
        int legacyIndex = PresenterLegacyIndex(legacy, a);
        Check(config.IsThumbnailDisabled(a) && legacy.GetItemChecked(legacyIndex)
                && ((IThumbnailDescription)legacy.Items[legacyIndex]).IsDisabled
                && PresenterMemberHidden(members, a) && storage.Saves > savesBeforePreview,
            "group preview Hide persists and synchronizes the legacy checkbox, description, and group status");
        Click(form, "DynamicGroupPreviewToggleButton");
        Check(!config.IsThumbnailDisabled(a) && !legacy.GetItemChecked(legacyIndex) && !PresenterMemberHidden(members, a),
            "group preview Show reverses the same persistent and UI states");
        legacy.SetItemChecked(legacyIndex, true);
        Application.DoEvents();
        Check(config.IsThumbnailDisabled(a) && PresenterMemberHidden(members, a),
            "legacy preview checkbox changes flow through the real presenter back to the dynamic group status");
        legacy.SetItemChecked(legacyIndex, false);
        Application.DoEvents();
        Check(!config.IsThumbnailDisabled(a) && !PresenterMemberHidden(members, a),
            "legacy preview uncheck clears the dynamic group hidden state without altering membership");

        ExercisePresenterGroupConflict(form, config, storage);
        SaveLifecycleScreenshot(form, "presenter-dynamic-groups-integrated");
    }

    private static bool PresenterHasGroupMember(IThumbnailConfiguration config, string title) =>
        config.CycleGroups.Any(group => group.ClientsOrder.ContainsKey(title));
    private static bool PresenterMemberHidden(ListView list, string title) =>
        list.Items.Cast<ListViewItem>().Single(item => (string)item.Tag! == title).SubItems[1].Text.Contains("隐藏");
    private static int PresenterLegacyIndex(CheckedListBox list, string title) =>
        list.Items.Cast<IThumbnailDescription>().Select((item, index) => (item, index)).Single(pair => pair.item.Title == title).index;

    private static void ExercisePresenterGroupConflict(MainForm form, IThumbnailConfiguration config, LifecycleStorage storage)
    {
        // The existing nested-recorder fixture records Ctrl+Shift+K using only
        // the recorder's own command handler, never global keyboard input.
        Keys busy = Keys.Control | Keys.Shift | Keys.K;
        if (!CanOwn(busy)) throw new Exception("Presenter conflict fixture needs unoccupied Ctrl+Shift+K before acquiring its own foreign registration.");
        string originalGroups = JsonConvert.SerializeObject(config.CycleGroups);
        string originalExclusions = JsonConvert.SerializeObject(config.CycleGroupAutoAddExclusions);
        bool originalAutoAdd = config.AutoAddUngroupedClients;
        int originalSaves = storage.Saves;
        bool warningSeen = false;
        using (var foreign = new NativeOwner(busy))
        using (var dismissWarning = new System.Windows.Forms.Timer { Interval = 40 })
        {
            Check(foreign.Registered, "presenter conflict fixture acquires a real Windows shortcut on an independent thread");
            dismissWarning.Tick += (_, _) => EnumThreadWindows(GetCurrentThreadId(), (window, _) =>
            {
                var className = new StringBuilder(100);
                GetClassName(window, className, className.Capacity);
                if (className.ToString() != "#32770" || GetWindow(window, 4) != form.Handle || !IsWindowVisible(window)) return true;
                warningSeen = true;
                dismissWarning.Stop();
                // This enumerates only our own UI thread and only our owned
                // warning; no input is sent to any unrelated app or game.
                EndDialog(window, (IntPtr)1);
                return false;
            }, IntPtr.Zero);
            dismissWarning.Start();
            ExerciseNestedGroupKeyModal(form, save: true);
            dismissWarning.Stop();
            Application.DoEvents();
            Check(warningSeen, "saving a recorded occupied group gesture reaches a visible real presenter conflict warning");
            Check(JsonConvert.SerializeObject(config.CycleGroups) == originalGroups
                    && JsonConvert.SerializeObject(form.GetCycleGroups()) == originalGroups,
                "failed group edit restores the applied collection in both configuration and real settings UI");
            Check(config.AutoAddUngroupedClients == originalAutoAdd
                    && JsonConvert.SerializeObject(config.CycleGroupAutoAddExclusions) == originalExclusions
                    && storage.Saves == originalSaves,
                "presenter group conflict preserves auto-collection policy and does not persist the rejected draft");
            var status = (Label)form.Controls.Find("QuickHotkeyStatusLabel", true).Single();
            Check(status.ForeColor == Color.Firebrick && status.Text.Contains("主力测试组") && form.Enabled,
                "group conflict identifies the named group and returns enabled settings after the warning");
        }
        Check(CanOwn(busy), "presenter conflict does not retain the foreign or rejected shortcut after fixture cleanup");
    }
}
