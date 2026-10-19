using EveOPreview.Configuration;
using EveOPreview.Configuration.Implementation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EveOPreview.View
{
	/// <summary>Edits independent group drafts. Only the presenter may commit them to configuration.</summary>
	internal sealed class DynamicCycleGroupsPanel : UserControl
	{
		private readonly Action<string, Action<string>> _captureHotkey;
		private readonly Func<Form, DialogResult> _showOwnedDialog;
		private readonly ListBox _groupList;
		private readonly ListView _members;
		private readonly CheckBox _autoAdd;
		private readonly Label _keySummary;
		private readonly Dictionary<Control, (string Key, string Zh, string En)> _texts = new Dictionary<Control, (string, string, string)>();
		private readonly List<Control> _needsGroup = new List<Control>();
		private readonly List<Control> _needsMember = new List<Control>();
		private List<CycleGroupDefinition> _groups = new List<CycleGroupDefinition>();
		private readonly HashSet<string> _online = new HashSet<string>(StringComparer.Ordinal);
		private readonly HashSet<string> _known = new HashSet<string>(StringComparer.Ordinal);
		private readonly HashSet<string> _hidden = new HashSet<string>(StringComparer.Ordinal);
		private readonly HashSet<string> _removed = new HashSet<string>(StringComparer.Ordinal);
		private bool _syncing;
		private bool _chinese = true;

		internal event Action GroupsChanged;
		internal event Action<string> PreviewToggleRequested;
		internal event Action<string> ConfigSyncRequested;
		internal string SelectedGroupId => this.SelectedGroup?.Id ?? string.Empty;
		private CycleGroupDefinition SelectedGroup => this._groupList.SelectedItem as CycleGroupDefinition;
		private string SelectedTitle => this._members.SelectedItems.Count == 1 ? this._members.SelectedItems[0].Tag as string : null;
		internal bool AutoAddUngroupedClients
		{
			get => this._autoAdd.Checked;
			set { bool old = this._syncing; this._syncing = true; this._autoAdd.Checked = value; this._syncing = old; }
		}

		internal DynamicCycleGroupsPanel(Action<string, Action<string>> captureHotkey, Func<Form, DialogResult> showOwnedDialog)
		{
			this._captureHotkey = captureHotkey ?? throw new ArgumentNullException(nameof(captureHotkey));
			this._showOwnedDialog = showOwnedDialog ?? throw new ArgumentNullException(nameof(showOwnedDialog));
			this.AutoScaleMode = AutoScaleMode.Dpi;
			this.AutoScroll = true;
			this.Padding = new Padding(6);
			var layout = new TableLayoutPanel { Name = "DynamicCycleGroupLayout", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Padding = new Padding(2), Margin = Padding.Empty };
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			this.Controls.Add(layout);
			var tools = Flow();
			tools.Controls.Add(this.Button("DynamicGroupAddButton", "Add", "新建组", "New group", this.AddGroup));
			tools.Controls.Add(this.Button("DynamicGroupRenameButton", "Rename", "改名", "Rename", this.RenameGroup, group: true));
			tools.Controls.Add(this.Button("DynamicGroupDeleteButton", "Delete", "删组", "Delete", this.DeleteGroup, group: true));
			tools.Controls.Add(this.Button("DynamicGroupHotkeysButton", "Hotkeys", "组快捷键…", "Group keys…", this.EditHotkeys, group: true));
			AddRow(layout, tools);
			var headings = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2, Margin = Padding.Empty };
			headings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
			headings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
			headings.Controls.Add(this.Label("DynamicGroupListLabel", "Groups", "分组", "Groups"), 0, 0);
			headings.Controls.Add(this.Label("DynamicGroupMembersLabel", "Members", "角色及状态", "Members / state"), 1, 0);
			AddRow(layout, headings);
			var lists = new TableLayoutPanel { Name = "DynamicCycleGroupLists", Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Height = 160, Margin = Padding.Empty };
			lists.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
			lists.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
			lists.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			this._groupList = new ListBox { Name = "DynamicCycleGroupList", Dock = DockStyle.Fill, IntegralHeight = false, DisplayMember = nameof(CycleGroupDefinition.Name), AllowDrop = true, Margin = new Padding(2) };
			this._members = new ListView { Name = "DynamicCycleGroupMembersList", Dock = DockStyle.Fill, View = System.Windows.Forms.View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false, AllowDrop = true, ShowItemToolTips = true, Margin = new Padding(2) };
			this._members.Columns.Add("", 130);
			this._members.Columns.Add("", 78);
			this._members.Resize += (sender, e) => this.ResizeMemberColumns();
			this._groupList.SelectedIndexChanged += (sender, e) => { if (!this._syncing) this.RenderMembers(); };
			this._members.SelectedIndexChanged += (sender, e) => this.UpdateEnabled();
			this._members.ItemDrag += (sender, e) => { if (this.SelectedTitle != null) this._members.DoDragDrop(new MemberDrag { GroupId = this.SelectedGroupId, Title = this.SelectedTitle }, DragDropEffects.Move); };
			this._groupList.DragOver += this.GroupDragOver;
			this._groupList.DragDrop += this.GroupDragDrop;
			this._members.DragOver += this.MemberDragOver;
			this._members.DragDrop += this.MemberDragDrop;
			lists.Controls.Add(this._groupList, 0, 0);
			lists.Controls.Add(this._members, 1, 0);
			AddRow(layout, lists, 160);
			var memberTools = Flow();
			memberTools.Controls.Add(this.Button("DynamicGroupMemberAddButton", "AddMembers", "添加角色…", "Add clients…", this.AddMembers, group: true));
			memberTools.Controls.Add(this.Button("DynamicGroupMemberRemoveButton", "RemoveMember", "移出", "Remove", this.RemoveMember, member: true));
			memberTools.Controls.Add(this.Button("DynamicGroupMemberUpButton", "MoveUp", "上移", "Up", () => this.MoveMember(-1), member: true));
			memberTools.Controls.Add(this.Button("DynamicGroupMemberDownButton", "MoveDown", "下移", "Down", () => this.MoveMember(1), member: true));
			memberTools.Controls.Add(this.Button("DynamicGroupPreviewToggleButton", "Preview", "显示/隐藏", "Show / hide", () => { if (this.SelectedTitle != null) this.PreviewToggleRequested?.Invoke(this.SelectedTitle); }, member: true));
			memberTools.Controls.Add(this.Button("DynamicGroupConfigSyncButton", "Sync", "复制角色配置…", "Copy settings…", () => { if (this.SelectedTitle != null) this.ConfigSyncRequested?.Invoke(this.SelectedTitle); }, member: true));
			AddRow(layout, memberTools);
			this._keySummary = new Label { Name = "DynamicGroupHotkeySummaryLabel", AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(3, 5, 3, 5) };
			AddRow(layout, this._keySummary);
			this._autoAdd = new CheckBox { Name = "DynamicGroupAutoAddCheckBox", AutoSize = false, Height = 42, Dock = DockStyle.Top, Margin = new Padding(3, 5, 3, 5) };
			this.RegisterText(this._autoAdd, "AutoAdd", "自动将新上线的未分组角色加入第一组", "Add new ungrouped online clients to the first group");
			this._autoAdd.CheckedChanged += (sender, e) => { if (!this._syncing) this.GroupsChanged?.Invoke(); };
			AddRow(layout, this._autoAdd);
			var hint = this.Label("DynamicGroupHintLabel", "Hint", "可拖动角色调序，拖到左侧另一组则移动。添加角色可让同一角色加入多组；空组循环全部在线角色。", "Drag members to reorder; drop on another group to move. Add allows membership in several groups. An empty group cycles all online clients.");
			hint.ForeColor = SystemColors.GrayText;
			AddRow(layout, hint);
			this.ApplyLanguage("zh-CN");
			this.UpdateEnabled();
		}

		internal void ApplyLanguage(string language)
		{
			this._chinese = !string.Equals(language, "en-US", StringComparison.OrdinalIgnoreCase);
			foreach (var pair in this._texts) pair.Key.Text = this.TextFor(pair.Value.Key, pair.Value.Zh, pair.Value.En);
			this._members.Columns[0].Text = this.TextFor("Character", "角色", "Character");
			this._members.Columns[1].Text = this.TextFor("Status", "状态", "Status");
			this.RenderMembers();
		}

		internal void SetGroups(IList<CycleGroupDefinition> groups)
		{
			string selected = this.SelectedGroupId;
			this._syncing = true;
			try
			{
				this._groups = (groups ?? Array.Empty<CycleGroupDefinition>()).Where(group => group != null).Select(group => group.DeepClone()).ToList();
				this._groupList.Items.Clear();
				foreach (var group in this._groups) this._groupList.Items.Add(group);
				this._groupList.SelectedIndex = this._groups.FindIndex(group => group.Id == selected);
				if (this._groupList.SelectedIndex < 0 && this._groups.Count > 0) this._groupList.SelectedIndex = 0;
			}
			finally { this._syncing = false; }
			this.RenderMembers();
		}

		internal List<CycleGroupDefinition> GetGroups() => this._groups.Select(group => group.DeepClone()).ToList();
		internal List<string> ConsumeRemovedClients() { var result = this._removed.ToList(); this._removed.Clear(); return result; }

		internal void RefreshClients(IList<string> online, IList<string> known, IList<string> hidden)
		{
			ReplaceSet(this._online, online);
			ReplaceSet(this._known, known);
			ReplaceSet(this._hidden, hidden);
			this.RenderMembers();
		}

		private static void ReplaceSet(HashSet<string> set, IEnumerable<string> values)
		{
			set.Clear();
			foreach (string value in values ?? Array.Empty<string>()) if (!string.IsNullOrWhiteSpace(value)) set.Add(value);
		}

		private string TextFor(string key, string chinese, string english) => LocalizationExtensions.GetString("Messages.DynamicGroups." + key, this._chinese ? chinese : english);
		private void RegisterText(Control control, string key, string chinese, string english) { this._texts.Add(control, (key, chinese, english)); control.Text = this.TextFor(key, chinese, english); }
		private Label Label(string name, string key, string chinese, string english)
		{
			var label = new Label { Name = name, AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(3, 3, 3, 3) };
			this.RegisterText(label, key, chinese, english);
			return label;
		}
		private System.Windows.Forms.Button Button(string name, string key, string chinese, string english, Action action, bool group = false, bool member = false)
		{
			var button = new System.Windows.Forms.Button { Name = name, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(48, 28), Margin = new Padding(2) };
			this.RegisterText(button, key, chinese, english);
			button.Click += (sender, e) => action();
			if (group) this._needsGroup.Add(button);
			if (member) this._needsMember.Add(button);
			return button;
		}
		private static FlowLayoutPanel Flow() => new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Size = Size.Empty, WrapContents = true, Margin = Padding.Empty };
		private static void AddRow(TableLayoutPanel layout, Control control, int height = 0)
		{
			int row = layout.RowCount++;
			layout.RowStyles.Add(height > 0 ? new RowStyle(SizeType.Absolute, height) : new RowStyle(SizeType.AutoSize));
			layout.Controls.Add(control, 0, row);
		}
		private void ResizeMemberColumns()
		{
			int width = Math.Max(40, this._members.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 2);
			this._members.Columns[1].Width = Math.Min(this._chinese ? 78 : 95, width / 2);
			this._members.Columns[0].Width = width - this._members.Columns[1].Width;
		}
		private void RenderMembers(string select = null)
		{
			select ??= this.SelectedTitle;
			this._members.BeginUpdate();
			try
			{
				this._members.Items.Clear();
				CycleGroupDefinition group = this.SelectedGroup;
				if (group != null)
				{
					foreach (string title in OrderedTitles(group))
					{
						string state = this._online.Contains(title) ? this.TextFor("Online", "在线", "Online") : this.TextFor("Offline", "离线", "Offline");
						if (this._hidden.Contains(title)) state += this.TextFor("HiddenSuffix", "·隐藏", " / hidden");
						var item = new ListViewItem(ClientTitleFormatter.GetCharacterName(title)) { Tag = title, ToolTipText = title, ForeColor = this._online.Contains(title) ? SystemColors.WindowText : SystemColors.GrayText };
						item.SubItems.Add(state);
						this._members.Items.Add(item);
						if (title == select) item.Selected = true;
					}
				}
				string forward = string.Join(" / ", group?.ForwardHotkeys ?? new List<string>());
				string backward = string.Join(" / ", group?.BackwardHotkeys ?? new List<string>());
				string none = this.TextFor("None", "未设置", "Not assigned");
				this._keySummary.Text = this.TextFor("NextKeys", "下一角色：", "Next: ") + (forward.Length > 0 ? forward.Replace("Control", "Ctrl") : none)
					+ Environment.NewLine + this.TextFor("PreviousKeys", "上一角色：", "Previous: ") + (backward.Length > 0 ? backward.Replace("Control", "Ctrl") : none);
			}
			finally { this._members.EndUpdate(); }
			this.ResizeMemberColumns();
			this.UpdateEnabled();
		}
		private void UpdateEnabled()
		{
			foreach (Control control in this._needsGroup) control.Enabled = this.SelectedGroup != null;
			foreach (Control control in this._needsMember) control.Enabled = this.SelectedTitle != null;
		}
		private static List<string> OrderedTitles(CycleGroupDefinition group) => (group.ClientsOrder ?? new Dictionary<string, int>()).OrderBy(pair => pair.Value).ThenBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Key).ToList();
		private static void WriteOrder(CycleGroupDefinition group, IEnumerable<string> titles) => group.ClientsOrder = titles.Distinct(StringComparer.Ordinal).Select((title, index) => new { title, index }).ToDictionary(item => item.title, item => item.index, StringComparer.Ordinal);
		private void NotifyChanged(string selectedTitle = null)
		{
			this.RenderMembers(selectedTitle);
			this.GroupsChanged?.Invoke();
		}

		private void AddGroup()
		{
			string name = this.TextFor("DefaultGroup", "分组 ", "Group ") + (this._groups.Count + 1);
			using (var dialog = new CycleGroupTextDialog(this.TextFor("NewTitle", "新建循环组", "Create cycle group"), this.TextFor("GroupName", "组名", "Group name"), name, this._chinese))
			{
				if (this._showOwnedDialog(dialog) != DialogResult.OK) return;
				var group = new CycleGroupDefinition { Name = dialog.Value };
				this._groups.Add(group);
				this._groupList.Items.Add(group);
				this._groupList.SelectedItem = group;
				this.NotifyChanged();
			}
		}
		private void RenameGroup()
		{
			var group = this.SelectedGroup;
			if (group == null) return;
			using (var dialog = new CycleGroupTextDialog(this.TextFor("RenameTitle", "修改组名", "Rename group"), this.TextFor("GroupName", "组名", "Group name"), group.Name, this._chinese))
			{
				if (this._showOwnedDialog(dialog) != DialogResult.OK) return;
				group.Name = dialog.Value;
				int index = this._groupList.SelectedIndex;
				this._groupList.Items[index] = group;
				this.NotifyChanged();
			}
		}
		private void DeleteGroup()
		{
			var group = this.SelectedGroup;
			if (group == null) return;
			if (this._groups.Count <= 1)
			{
				MessageBox.Show(this.FindForm(), this.TextFor("KeepOneGroup", "请至少保留一个循环组。可移出全部角色，让空组循环所有在线角色。", "Keep at least one cycle group. Remove its members to cycle all online clients."), this.TextFor("Delete", "删组", "Delete"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			string prompt = string.Format(this.TextFor("DeleteConfirm", "删除分组“{0}”？只删除分组，不影响角色和预览设置。", "Delete group '{0}'? Client and preview settings will not be removed."), group.Name);
			if (MessageBox.Show(this.FindForm(), prompt, this.TextFor("Delete", "删组", "Delete"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
			foreach (string title in OrderedTitles(group)) this._removed.Add(title);
			int index = this._groupList.SelectedIndex;
			this._groups.Remove(group);
			this._groupList.Items.RemoveAt(index);
			this._groupList.SelectedIndex = Math.Min(index, this._groups.Count - 1);
			this.NotifyChanged();
		}
		private void EditHotkeys()
		{
			var group = this.SelectedGroup;
			if (group == null) return;
			using (var dialog = new CycleGroupHotkeysDialog(group, this._captureHotkey, this._chinese))
			{
				if (this._showOwnedDialog(dialog) != DialogResult.OK) return;
				group.ForwardHotkeys = dialog.ForwardHotkeys;
				group.BackwardHotkeys = dialog.BackwardHotkeys;
				this.NotifyChanged();
			}
		}
		private void AddMembers()
		{
			var group = this.SelectedGroup;
			if (group == null) return;
			var available = this._known.Concat(this._online).Concat(this._groups.SelectMany(item => OrderedTitles(item))).Distinct(StringComparer.Ordinal).Where(title => !group.ClientsOrder.ContainsKey(title)).OrderBy(title => title, StringComparer.Ordinal).ToList();
			using (var dialog = new CycleGroupMembersDialog(available, this._online, this._chinese))
			{
				if (this._showOwnedDialog(dialog) != DialogResult.OK) return;
				var titles = OrderedTitles(group);
				foreach (string title in dialog.SelectedTitles) { titles.Add(title); this._removed.Remove(title); }
				WriteOrder(group, titles);
				this.NotifyChanged(dialog.SelectedTitles.FirstOrDefault());
			}
		}
		private void RemoveMember()
		{
			var group = this.SelectedGroup;
			string title = this.SelectedTitle;
			if (group == null || title == null) return;
			group.ClientsOrder.Remove(title);
			this._removed.Add(title);
			WriteOrder(group, OrderedTitles(group));
			this.NotifyChanged();
		}
		private void MoveMember(int offset)
		{
			var group = this.SelectedGroup;
			string title = this.SelectedTitle;
			if (group == null || title == null) return;
			var titles = OrderedTitles(group);
			int index = titles.IndexOf(title), target = index + offset;
			if (index < 0 || target < 0 || target >= titles.Count) return;
			titles.RemoveAt(index);
			titles.Insert(target, title);
			WriteOrder(group, titles);
			this.NotifyChanged(title);
		}
		private sealed class MemberDrag { public string GroupId; public string Title; }
		private MemberDrag GetDrag(DragEventArgs e) => e.Data?.GetData(typeof(MemberDrag)) as MemberDrag;
		private void GroupDragOver(object sender, DragEventArgs e)
		{
			var drag = this.GetDrag(e);
			int index = this._groupList.IndexFromPoint(this._groupList.PointToClient(new Point(e.X, e.Y)));
			e.Effect = drag != null && index >= 0 && this._groups[index].Id != drag.GroupId ? DragDropEffects.Move : DragDropEffects.None;
		}
		private void GroupDragDrop(object sender, DragEventArgs e)
		{
			var drag = this.GetDrag(e);
			int index = this._groupList.IndexFromPoint(this._groupList.PointToClient(new Point(e.X, e.Y)));
			if (drag == null || index < 0) return;
			var source = this._groups.FirstOrDefault(group => group.Id == drag.GroupId);
			var target = this._groups[index];
			if (source == null || source == target || !source.ClientsOrder.Remove(drag.Title)) return;
			WriteOrder(source, OrderedTitles(source));
			WriteOrder(target, OrderedTitles(target).Concat(new[] { drag.Title }));
			this._groupList.SelectedIndex = index;
			this.NotifyChanged(drag.Title);
		}
		private void MemberDragOver(object sender, DragEventArgs e)
		{
			var drag = this.GetDrag(e);
			e.Effect = drag != null && drag.GroupId == this.SelectedGroupId ? DragDropEffects.Move : DragDropEffects.None;
		}
		private void MemberDragDrop(object sender, DragEventArgs e)
		{
			var drag = this.GetDrag(e);
			var group = this.SelectedGroup;
			if (drag == null || group == null || drag.GroupId != group.Id) return;
			var target = this._members.GetItemAt(this._members.PointToClient(new Point(e.X, e.Y)).X, this._members.PointToClient(new Point(e.X, e.Y)).Y);
			var titles = OrderedTitles(group);
			int targetIndex = target == null ? titles.Count - 1 : titles.IndexOf((string)target.Tag);
			if (!titles.Remove(drag.Title)) return;
			titles.Insert(Math.Max(0, Math.Min(targetIndex, titles.Count)), drag.Title);
			WriteOrder(group, titles);
			this.NotifyChanged(drag.Title);
		}
	}
}
