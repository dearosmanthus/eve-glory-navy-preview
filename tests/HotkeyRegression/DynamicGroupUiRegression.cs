using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using EveOPreview.Configuration;
using EveOPreview.Configuration.Implementation;
using EveOPreview.View;

internal static partial class Program
{
    // These tests construct only our own settings form. Inputs are dispatched
    // directly to our controls/dialogs; no global input or EVE window is used.
    private static void DynamicGroupUiTests()
    {
        LocalizationExtensions.SetLanguage("zh-CN");
        using var context = new ApplicationContext();
        using var form = new MainForm(context);
        form.FormCloseRequested = request => request.Allow = true;
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(-10000, -10000);
        form.Size = new Size(509, 397);
        form.TopMost = true;
        form.BeginUpdateUI();
        try { form.Language = "zh-CN"; form.InitializeLanguageControls(); }
        finally { form.EndUpdateUI(); }
        var groups = new List<CycleGroupDefinition>
        {
            new() { Id = "ui-main", Name = "主力组", ForwardHotkeys = new() { "Control+Shift+F17" }, ClientsOrder = new() { ["EVE - Alpha"] = 0, ["EVE - Beta"] = 1, ["EVE - Gamma"] = 2 } },
            new() { Id = "ui-support", Name = "后勤组", ClientsOrder = new() { ["EVE - Delta"] = 0 } }
        };
        form.SetCycleGroups(groups);
        form.RefreshCycleGroupClients(new[] { "EVE - Alpha", "EVE - Gamma", "EVE - Delta" },
            new[] { "EVE - Alpha", "EVE - Beta", "EVE - Gamma", "EVE - Delta", "EVE - Echo" }, new[] { "EVE - Gamma" });
        var tabs = (TabControl)form.Controls.Find("ContentTabControl", true).Single();
        tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(page => page.Name == "CycleGroupTabPage");
        ((Form)form).Show();
        Application.DoEvents();
        var panel = (UserControl)form.Controls.Find("DynamicCycleGroupsPanel", true).Single();
        var groupList = (ListBox)form.Controls.Find("DynamicCycleGroupList", true).Single();
        var members = (ListView)form.Controls.Find("DynamicCycleGroupMembersList", true).Single();
        int changes = 0;
        form.CycleGroupsChanged = () => changes++;
        try
        {
            Check(form.SelectedDynamicCycleGroupId == "ui-main", "dynamic UI selects stable first group identity");
            groups[0].Name = "externally changed";
            groups[0].ForwardHotkeys.Add("Alt+F18");
            groups[0].ClientsOrder.Clear();
            Check(form.GetCycleGroups()[0].Name == "主力组" && UiMemberOrder(form, 0).Count == 3
                    && form.GetCycleGroups()[0].ForwardHotkeys.Count == 1,
                "dynamic UI clones incoming group names, keys and member order");
            var outgoing = form.GetCycleGroups();
            outgoing[0].Name = "output mutated";
            outgoing[0].ClientsOrder.Clear();
            outgoing[0].ForwardHotkeys.Clear();
            Check(form.GetCycleGroups()[0].Name == "主力组" && UiMemberOrder(form, 0).Count == 3
                    && form.GetCycleGroups()[0].ForwardHotkeys.Count == 1,
                "dynamic UI returns deep independent drafts");
            Check(members.Items.Cast<ListViewItem>().Single(item => (string)item.Tag! == "EVE - Beta").SubItems[1].Text.Contains("离线"),
                "dynamic UI retains and labels offline members");
            Check(members.Items.Cast<ListViewItem>().Single(item => (string)item.Tag! == "EVE - Gamma").SubItems[1].Text.Contains("隐藏"),
                "dynamic UI shows hidden state independently of membership");
            Check(members.Items.Cast<ListViewItem>().All(item => item.ToolTipText == (string)item.Tag!)
                    && members.Items.Cast<ListViewItem>().Single(item => (string)item.Tag! == "EVE - Alpha").Text == "Alpha",
                "member display strips title prefix but preserves original configuration key");

            form.AutoAddUngroupedClients = true;
            Check(changes == 0, "loading automatic grouping does not emit a user change");
            var autoAdd = (CheckBox)form.Controls.Find("DynamicGroupAutoAddCheckBox", true).Single();
            autoAdd.Checked = false;
            Check(changes == 1 && !form.AutoAddUngroupedClients, "automatic grouping user toggle emits one change");
            int beforeRefresh = changes;
            groupList.SelectedIndex = 1;
            form.RefreshCycleGroupClients(new[] { "EVE - Delta" }, new[] { "EVE - Echo" }, Array.Empty<string>());
            Check(form.SelectedDynamicCycleGroupId == "ui-support" && changes == beforeRefresh,
                "online refresh preserves selection without persisting a new draft");
            form.RefreshCycleGroupClients(new[] { "EVE - Alpha", "EVE - Gamma", "EVE - Delta" },
                new[] { "EVE - Alpha", "EVE - Beta", "EVE - Gamma", "EVE - Delta", "EVE - Echo" }, new[] { "EVE - Gamma" });
            groupList.SelectedIndex = 0;

            SelectUiMember(members, "EVE - Beta");
            Click(form, "DynamicGroupMemberUpButton");
            Check(UiMemberOrder(form, 0).SequenceEqual(new[] { "EVE - Beta", "EVE - Alpha", "EVE - Gamma" }), "member Up changes draft cycle order");
            Click(form, "DynamicGroupMemberDownButton");
            Check(UiMemberOrder(form, 0).SequenceEqual(new[] { "EVE - Alpha", "EVE - Beta", "EVE - Gamma" }), "member Down restores draft order");

            var drag = NewUiMemberDrag("ui-main", "EVE - Gamma");
            var data = new DataObject();
            data.SetData(drag.GetType(), drag);
            Point overFirst = members.PointToScreen(new Point(5, members.Items[0].Bounds.Top + 4));
            Call(panel, "MemberDragDrop", members, new DragEventArgs(data, 0, overFirst.X, overFirst.Y, DragDropEffects.Move, DragDropEffects.Move));
            Check(UiMemberOrder(form, 0).SequenceEqual(new[] { "EVE - Gamma", "EVE - Alpha", "EVE - Beta" }), "in-list drag reorders a member");
            Point overSupport = groupList.PointToScreen(new Point(4, groupList.GetItemRectangle(1).Top + 3));
            Call(panel, "GroupDragDrop", groupList, new DragEventArgs(data, 0, overSupport.X, overSupport.Y, DragDropEffects.Move, DragDropEffects.Move));
            Check(!UiMemberOrder(form, 0).Contains("EVE - Gamma") && UiMemberOrder(form, 1).SequenceEqual(new[] { "EVE - Delta", "EVE - Gamma" }),
                "cross-group drag moves instead of duplicating a member");
            Check(form.SelectedDynamicCycleGroupId == "ui-support", "cross-group drag selects the destination group");

            string? previewTitle = null, syncTitle = null;
            form.ClientPreviewToggleRequested = title => previewTitle = title;
            form.ClientConfigSyncRequested = title => syncTitle = title;
            SelectUiMember(members, "EVE - Gamma");
            Click(form, "DynamicGroupPreviewToggleButton");
            Click(form, "DynamicGroupConfigSyncButton");
            Check(previewTitle == "EVE - Gamma" && syncTitle == "EVE - Gamma", "preview and config-copy actions pass original title");

            RunOwnUiModal(form, "CycleGroupMembersDialog", () => Click(form, "DynamicGroupMemberAddButton"), dialog =>
            {
                var available = (CheckedListBox)dialog.Controls.Find("DynamicGroupAvailableClientsList", true).Single();
                Check(available.Items.Cast<object>().All(item => !item.ToString()!.StartsWith("Gamma —") && !item.ToString()!.StartsWith("Delta —")),
                    "add-members picker excludes duplicate membership in the current group");
                int alpha = available.Items.Cast<object>().Select((item, index) => (item, index)).Single(pair => pair.item.ToString()!.StartsWith("Alpha —")).index;
                int echo = available.Items.Cast<object>().Select((item, index) => (item, index)).Single(pair => pair.item.ToString()!.StartsWith("Echo —")).index;
                available.SetItemChecked(alpha, true);
                available.SetItemChecked(echo, true);
                Click(dialog, "DynamicGroupMembersApplyButton");
            });
            Check(UiMemberOrder(form, 0).Contains("EVE - Alpha") && UiMemberOrder(form, 1).Contains("EVE - Alpha")
                    && UiMemberOrder(form, 1).Contains("EVE - Echo"),
                "manual addition supports several groups and known offline clients");
            SelectUiMember(members, "EVE - Echo");
            Click(form, "DynamicGroupMemberRemoveButton");
            Check(!UiMemberOrder(form, 1).Contains("EVE - Echo")
                    && form.ConsumeRemovedCycleGroupClients().SequenceEqual(new[] { "EVE - Echo" })
                    && form.ConsumeRemovedCycleGroupClients().Count == 0,
                "manual removal emits a one-shot automatic-collection exclusion");

            RunOwnUiModal(form, "CycleGroupTextDialog", () => Click(form, "DynamicGroupAddButton"), dialog =>
            {
                dialog.Controls.Find("DynamicGroupNameTextBox", true).Single().Text = "侦察组";
                Click(dialog, "DynamicGroupNameAcceptButton");
            });
            string createdId = form.SelectedDynamicCycleGroupId;
            Check(form.GetCycleGroups().Count == 3 && form.GetCycleGroups().Last().Name == "侦察组" && createdId.Length > 0,
                "new-group modal creates and selects a stable identity");
            RunOwnUiModal(form, "CycleGroupTextDialog", () => Click(form, "DynamicGroupRenameButton"), dialog =>
            {
                dialog.Controls.Find("DynamicGroupNameTextBox", true).Single().Text = "远程侦察组";
                Click(dialog, "DynamicGroupNameAcceptButton");
            });
            Check(form.SelectedDynamicCycleGroupId == createdId && form.GetCycleGroups().Last().Name == "远程侦察组", "renaming preserves group identity");
            RunOwnUiModal(form, "CycleGroupTextDialog", () => Click(form, "DynamicGroupRenameButton"), dialog =>
            {
                dialog.Controls.Find("DynamicGroupNameTextBox", true).Single().Text = "cancelled name";
                Click(dialog, "DynamicGroupNameCancelButton");
            });
            Check(form.GetCycleGroups().Last().Name == "远程侦察组", "name-dialog Cancel leaves draft unchanged");
            ConfirmOwnGroupDeletion(form, () => Click(form, "DynamicGroupDeleteButton"));
            Check(form.GetCycleGroups().Count == 2 && form.GetCycleGroups().All(group => group.Id != createdId), "confirmed deletion removes only the selected group");

            groupList.SelectedIndex = 0;
            ExerciseNestedGroupKeyModal(form, save: true);
            Check(form.GetCycleGroups()[0].ForwardHotkeys.Any(key => Parse(key) == (true, Keys.Control | Keys.Shift | Keys.K)),
                "group-key modal applies a recorded gesture");
            var savedKeys = form.GetCycleGroups()[0].ForwardHotkeys.ToList();
            ExerciseNestedGroupKeyModal(form, save: false);
            Check(form.GetCycleGroups()[0].ForwardHotkeys.SequenceEqual(savedKeys), "outer group-key Cancel discards its recorded changes");

            foreach (string language in new[] { "zh-CN", "en-US" })
            {
                LocalizationExtensions.SetLanguage(language);
                form.BeginUpdateUI();
                try { form.Language = language; form.InitializeLanguageControls(); }
                finally { form.EndUpdateUI(); }
                panel.AutoScrollPosition = Point.Empty;
                form.PerformLayout();
                Application.DoEvents();
                SaveLifecycleScreenshot(form, "dynamic-groups-" + language + "-509x397-top");
                foreach (string button in new[] { "DynamicGroupAddButton", "DynamicGroupRenameButton", "DynamicGroupDeleteButton", "DynamicGroupHotkeysButton", "DynamicGroupMemberAddButton", "DynamicGroupMemberRemoveButton", "DynamicGroupMemberUpButton", "DynamicGroupMemberDownButton", "DynamicGroupPreviewToggleButton", "DynamicGroupConfigSyncButton" })
                {
                    Control target = form.Controls.Find(button, true).Single();
                    panel.ScrollControlIntoView(target);
                    Application.DoEvents();
                    Point position = panel.PointToClient(target.PointToScreen(Point.Empty));
                    Check(target.Visible && position.X >= 0 && position.Y >= 0
                            && position.X + target.Width <= panel.ClientSize.Width && position.Y + target.Height <= panel.ClientSize.Height,
                        language + " dynamic group button is reachable in the real settings viewport: " + button);
                }
                panel.ScrollControlIntoView(form.Controls.Find("DynamicGroupHintLabel", true).Single());
                Application.DoEvents();
                SaveLifecycleScreenshot(form, "dynamic-groups-" + language + "-509x397-bottom");
                Check(!panel.HorizontalScroll.Visible, language + " dynamic grouping needs no horizontal page scroll");
            }
        }
        finally { form.Close(); }
    }

    private static List<string> UiMemberOrder(MainForm form, int group) => form.GetCycleGroups()[group].ClientsOrder.OrderBy(pair => pair.Value).Select(pair => pair.Key).ToList();
    private static void SelectUiMember(ListView list, string title)
    {
        foreach (ListViewItem item in list.Items) item.Selected = (string)item.Tag! == title;
        Application.DoEvents();
    }
    private static object NewUiMemberDrag(string group, string title)
    {
        object drag = New("EveOPreview.View.DynamicCycleGroupsPanel+MemberDrag");
        SetMember(drag, "GroupId", group);
        SetMember(drag, "Title", title);
        return drag;
    }

    private static void RunOwnUiModal(MainForm owner, string name, Action open, Action<Form> act)
    {
        Exception? failure = null;
        bool seen = false;
        using var timer = new System.Windows.Forms.Timer { Interval = 40 };
        timer.Tick += (_, _) =>
        {
            Form? dialog = Application.OpenForms.Cast<Form>().FirstOrDefault(form => form.Name == name);
            if (dialog == null) return;
            timer.Stop();
            seen = true;
            try
            {
                Check(dialog.Visible && dialog.Enabled && dialog.Owner == owner && IsWindowEnabled(dialog.Handle)
                        && !IsWindowEnabled(owner.Handle), name + " is visible and natively blocks its settings owner");
                Check(!owner.TopMost || dialog.TopMost, name + " inherits the settings topmost band");
                act(dialog);
            }
            catch (Exception ex) { failure = Root(ex); dialog.DialogResult = DialogResult.Cancel; dialog.Close(); }
        };
        timer.Start();
        open();
        timer.Stop();
        if (failure != null) throw failure;
        Check(seen && owner.Enabled && IsWindowEnabled(owner.Handle), name + " returns control to settings");
    }

    private static void ConfirmOwnGroupDeletion(MainForm owner, Action open)
    {
        bool seen = false;
        using var timer = new System.Windows.Forms.Timer { Interval = 40 };
        timer.Tick += (_, _) => EnumThreadWindows(GetCurrentThreadId(), (window, _) =>
        {
            var className = new StringBuilder(100);
            GetClassName(window, className, className.Capacity);
            if (className.ToString() != "#32770" || GetWindow(window, 4) != owner.Handle || !IsWindowVisible(window)) return true;
            timer.Stop();
            seen = true;
            EndDialog(window, (IntPtr)6); // IDYES, only this process/thread's owned confirmation.
            return false;
        }, IntPtr.Zero);
        timer.Start();
        open();
        timer.Stop();
        Check(seen && owner.Enabled, "group deletion uses an owned confirmation and returns control");
    }

    private static void ExerciseNestedGroupKeyModal(MainForm owner, bool save, bool cancelCapture = false)
    {
        var states = new List<bool>();
        Action<bool>? previousCaptureState = owner.HotkeyCaptureStateChanged;
        owner.HotkeyCaptureStateChanged = active => { states.Add(active); previousCaptureState?.Invoke(active); };
        bool captureSeen = false, outerSeen = false;
        Form? observedOuter = null;
        int outerModalTicks = 0;
        Exception? failure = null;
        using var outerTimer = new System.Windows.Forms.Timer { Interval = 40 };
        outerTimer.Tick += (_, _) =>
        {
            Form? outer = Application.OpenForms.Cast<Form>().FirstOrDefault(form => form.Name == "CycleGroupHotkeysDialog");
            if (outer == null || !outer.Visible || !outer.Modal) return;
            if (observedOuter != outer) { observedOuter = outer; outerModalTicks = 0; }
            // Observe on a second WM_TIMER turn, after the visible modal has had
            // a complete pump iteration; do not sample from Load/Shown callbacks.
            if (++outerModalTicks < 2) return;
            outerTimer.Stop();
            outerSeen = true;
            try
            {
                Check(states.SequenceEqual(new[] { true }), "group-key editor pauses global bindings before any nested capture");
                Console.WriteLine($"GROUP MODAL outer={outer.Name}/0x{outer.Handle.ToInt64():x}, managedOwner={outer.Owner?.Name ?? "null"}/0x{outer.Owner?.Handle.ToInt64() ?? 0:x}, nativeOwner=0x{GetWindow(outer.Handle, 4).ToInt64():x}, settings={owner.Name}/0x{owner.Handle.ToInt64():x}, outerEnabled={outer.Enabled}, settingsEnabled={owner.Enabled}, nativeOuterEnabled={IsWindowEnabled(outer.Handle)}, nativeSettingsEnabled={IsWindowEnabled(owner.Handle)}, modal={outer.Modal}, settledTicks={outerModalTicks}");
                Check(outer.Owner == owner && GetWindow(outer.Handle, 4) == owner.Handle && outer.TopMost
                        && (GetWindowLong(outer.Handle, -20) & 8) != 0 && IsAboveOwner(outer.Handle, owner.Handle),
                    "group-key editor stays natively owned by and above its topmost settings owner");
                Check(IsWindowEnabled(outer.Handle) && !IsWindowEnabled(owner.Handle),
                    "group-key editor accepts native input while the settings window is blocked");
                Form? observedCapture = null;
                int captureModalTicks = 0;
                using var captureTimer = new System.Windows.Forms.Timer { Interval = 40 };
                captureTimer.Tick += (_, _) =>
                {
                    Form? capture = Application.OpenForms.Cast<Form>().FirstOrDefault(form => form.Name == "HotkeyCaptureDialog");
                    if (capture == null || !capture.Visible || !capture.Modal) return;
                    if (observedCapture != capture) { observedCapture = capture; captureModalTicks = 0; }
                    if (++captureModalTicks < 2) return;
                    captureTimer.Stop();
                    captureSeen = true;
                    try
                    {
                        Console.WriteLine($"NESTED MODAL capture={capture.Name}/0x{capture.Handle.ToInt64():x}, managedOwner={capture.Owner?.Name ?? "null"}/0x{capture.Owner?.Handle.ToInt64() ?? 0:x}, nativeOwner=0x{GetWindow(capture.Handle, 4).ToInt64():x}, expectedOuter={outer.Name}/0x{outer.Handle.ToInt64():x}, captureEnabled={capture.Enabled}, captureVisible={capture.Visible}, outerEnabled={outer.Enabled}, settingsEnabled={owner.Enabled}, nativeCaptureEnabled={IsWindowEnabled(capture.Handle)}, nativeOuterEnabled={IsWindowEnabled(outer.Handle)}, nativeSettingsEnabled={IsWindowEnabled(owner.Handle)}, modal={capture.Modal}, settledTicks={captureModalTicks}, states={string.Join(",", states)}");
                        // WinForms Modal ThreadWindows calls EnableWindow directly,
                        // while Control.Enabled reads only its managed state bits.
                        // Native input availability is the property under test.
                        Check(capture.Owner == outer && GetWindow(capture.Handle, 4) == outer.Handle
                                && !IsWindowEnabled(outer.Handle) && !IsWindowEnabled(owner.Handle)
                                && capture.Visible && capture.Enabled && IsWindowEnabled(capture.Handle),
                            "nested recorder belongs to the deepest settings dialog");
                        Check(capture.TopMost && (GetWindowLong(capture.Handle, -20) & 8) != 0 && IsAboveOwner(capture.Handle, outer.Handle),
                            "nested recorder remains in the native topmost band above the group editor");
                        Check(states.SequenceEqual(new[] { true }), "nested recording does not add an early resume notification");
                        SaveLifecycleScreenshot(capture, "dynamic-group-nested-capture-" + owner.Language + "-" + save + "-" + cancelCapture);
                        PressDialogKey(capture, cancelCapture ? Keys.Escape : save ? Keys.Control | Keys.Shift | Keys.K : Keys.Control | Keys.Shift | Keys.J);
                        Console.WriteLine($"NESTED INPUT result={capture.DialogResult}, captured={Get(capture, "CapturedHotkey") ?? "null"}");
                    }
                    catch (Exception ex) { failure = Root(ex); capture.DialogResult = DialogResult.Cancel; capture.Close(); }
                };
                captureTimer.Start();
                Click(outer, "DynamicGroupForwardAddButton");
                captureTimer.Stop();
                Console.WriteLine($"NESTED RETURN captureSeen={captureSeen}, outer={outer.Name}/0x{outer.Handle.ToInt64():x}, outerEnabled={outer.Enabled}, settingsEnabled={owner.Enabled}, nativeOuterEnabled={IsWindowEnabled(outer.Handle)}, nativeSettingsEnabled={IsWindowEnabled(owner.Handle)}, states={string.Join(",", states)}");
                Check(captureSeen && outer.Enabled && IsWindowEnabled(outer.Handle) && !IsWindowEnabled(owner.Handle),
                    "nested capture returns native input only to the group editor");
                Check(states.SequenceEqual(new[] { true }), "global bindings stay paused until the outer editor closes");
                SaveLifecycleScreenshot(outer, "dynamic-group-hotkeys-" + owner.Language + "-" + save + "-" + cancelCapture);
                Click(outer, save ? "DynamicGroupHotkeysApplyButton" : "DynamicGroupHotkeysCancelButton");
            }
            catch (Exception ex) { failure = Root(ex); outer.DialogResult = DialogResult.Cancel; outer.Close(); }
        };
        try
        {
            outerTimer.Start();
            Click(owner, "DynamicGroupHotkeysButton");
            outerTimer.Stop();
            if (failure != null) throw failure;
            Check(outerSeen && captureSeen && owner.Enabled && IsWindowEnabled(owner.Handle) && states.SequenceEqual(new[] { true, false }),
                "closing outer group editor restores bindings exactly once (save=" + save + ")");
        }
        finally { owner.HotkeyCaptureStateChanged = previousCaptureState; }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowEnabled(IntPtr window);
}
