using EveOPreview.Configuration;
using EveOPreview.Configuration.Implementation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EveOPreview.View
{
	internal abstract class CycleGroupOwnedDialog : Form
	{
		protected CycleGroupOwnedDialog()
		{
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ShowInTaskbar = false;
			this.AutoScaleMode = AutoScaleMode.Dpi;
			this.Padding = new Padding(12);
		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.TopMost = this.Owner?.TopMost ?? false;
		}
		protected static string TextFor(bool chinese, string key, string zh, string en) => LocalizationExtensions.GetString("Messages.DynamicGroups." + key, chinese ? zh : en);
		protected static FlowLayoutPanel ButtonFlow() => new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.RightToLeft, WrapContents = true, Margin = Padding.Empty };
		protected static System.Windows.Forms.Button DialogButton(string name, string text, DialogResult result = DialogResult.None) => new System.Windows.Forms.Button { Name = name, Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(75, 30), Margin = new Padding(3), DialogResult = result };
	}

	internal sealed class CycleGroupTextDialog : CycleGroupOwnedDialog
	{
		private readonly System.Windows.Forms.TextBox _value;
		private readonly Label _status;
		internal string Value => this._value.Text.Trim();
		internal CycleGroupTextDialog(string title, string label, string value, bool chinese)
		{
			this.Name = "CycleGroupTextDialog";
			this.Text = title;
			this.ClientSize = new Size(380, 160);
			var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.Controls.Add(new Label { Text = label, AutoSize = true, Dock = DockStyle.Fill }, 0, 0);
			this._value = new System.Windows.Forms.TextBox { Name = "DynamicGroupNameTextBox", Text = value ?? string.Empty, MaxLength = 80, Dock = DockStyle.Fill };
			layout.Controls.Add(this._value, 0, 1);
			this._status = new Label { Name = "DynamicGroupNameStatusLabel", ForeColor = Color.Firebrick, AutoSize = true, Dock = DockStyle.Fill };
			layout.Controls.Add(this._status, 0, 2);
			var buttons = ButtonFlow();
			var cancel = DialogButton("DynamicGroupNameCancelButton", TextFor(chinese, "Cancel", "取消", "Cancel"), DialogResult.Cancel);
			var accept = DialogButton("DynamicGroupNameAcceptButton", TextFor(chinese, "Save", "保存", "Save"));
			accept.Click += (sender, e) =>
			{
				if (this.Value.Length == 0) { this._status.Text = TextFor(chinese, "NameRequired", "请输入组名。", "Enter a group name."); this._value.Focus(); return; }
				this.DialogResult = DialogResult.OK;
				this.Close();
			};
			buttons.Controls.Add(cancel);
			buttons.Controls.Add(accept);
			layout.Controls.Add(buttons, 0, 3);
			this.CancelButton = cancel;
			this.AcceptButton = accept;
			this.Controls.Add(layout);
			this.Shown += (sender, e) => { this._value.Focus(); this._value.SelectAll(); };
		}
	}

	internal sealed class CycleGroupHotkeysDialog : CycleGroupOwnedDialog
	{
		private readonly ListBox _forward;
		private readonly ListBox _backward;
		internal List<string> ForwardHotkeys => this._forward.Items.Cast<string>().ToList();
		internal List<string> BackwardHotkeys => this._backward.Items.Cast<string>().ToList();
		internal CycleGroupHotkeysDialog(CycleGroupDefinition group, Action<string, Action<string>> captureHotkey, bool chinese)
		{
			this.Name = "CycleGroupHotkeysDialog";
			this.Text = TextFor(chinese, "GroupHotkeysTitle", "分组快捷键", "Group hotkeys") + " — " + group.Name;
			this.ClientSize = new Size(500, 340);
			var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4 };
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			this._forward = this.AddKeyList(layout, true, group.ForwardHotkeys, captureHotkey, chinese);
			this._backward = this.AddKeyList(layout, false, group.BackwardHotkeys, captureHotkey, chinese);
			var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
			footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			var hint = new Label { AutoSize = true, Dock = DockStyle.Fill, ForeColor = SystemColors.GrayText, Margin = new Padding(3, 8, 3, 5), Text = TextFor(chinese, "GroupHotkeyHint", "支持多个按键。保存时检查重复与系统占用；取消不会改变原设置。", "Several keys may be assigned. Save validates conflicts; Cancel keeps the previous bindings.") };
			footer.Controls.Add(hint, 0, 0);
			var buttons = ButtonFlow();
			var cancel = DialogButton("DynamicGroupHotkeysCancelButton", TextFor(chinese, "Cancel", "取消", "Cancel"), DialogResult.Cancel);
			var accept = DialogButton("DynamicGroupHotkeysApplyButton", TextFor(chinese, "Save", "保存", "Save"), DialogResult.OK);
			buttons.Controls.Add(cancel);
			buttons.Controls.Add(accept);
			footer.Controls.Add(buttons, 0, 1);
			layout.Controls.Add(footer, 0, 3);
			layout.SetColumnSpan(footer, 2);
			this.CancelButton = cancel;
			this.AcceptButton = accept;
			this.Controls.Add(layout);
		}
		private ListBox AddKeyList(TableLayoutPanel layout, bool forward, IEnumerable<string> keys, Action<string, Action<string>> captureHotkey, bool chinese)
		{
			string prefix = forward ? "DynamicGroupForward" : "DynamicGroupBackward";
			int column = forward ? 0 : 1;
			layout.Controls.Add(new Label { Name = prefix + "Label", AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(3, 4, 3, 7), Text = forward ? TextFor(chinese, "NextAction", "下一角色", "Next client") : TextFor(chinese, "PreviousAction", "上一角色", "Previous client") }, column, 0);
			var list = new ListBox { Name = prefix + "List", Dock = DockStyle.Fill, IntegralHeight = false };
			foreach (string key in keys ?? Array.Empty<string>()) list.Items.Add(key);
			layout.Controls.Add(list, column, 1);
			var buttons = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill, WrapContents = true, Margin = Padding.Empty };
			var add = DialogButton(prefix + "AddButton", TextFor(chinese, "AddHotkey", "录入按键…", "Record key…"));
			add.Click += (sender, e) => captureHotkey(string.Empty, key => { if (!list.Items.Cast<string>().Contains(key, StringComparer.OrdinalIgnoreCase)) { list.Items.Add(key); list.SelectedIndex = list.Items.Count - 1; } });
			var remove = DialogButton(prefix + "RemoveButton", TextFor(chinese, "RemoveHotkey", "移除", "Remove"));
			remove.Click += (sender, e) => { if (list.SelectedIndex >= 0) list.Items.RemoveAt(list.SelectedIndex); };
			buttons.Controls.Add(add);
			buttons.Controls.Add(remove);
			layout.Controls.Add(buttons, column, 2);
			return list;
		}
	}

	internal sealed class CycleGroupMembersDialog : CycleGroupOwnedDialog
	{
		private sealed class ClientItem
		{
			internal string Title;
			internal string Caption;
			public override string ToString() => this.Caption;
		}
		private readonly CheckedListBox _clients;
		internal List<string> SelectedTitles => this._clients.CheckedItems.Cast<ClientItem>().Select(item => item.Title).ToList();
		internal CycleGroupMembersDialog(IEnumerable<string> available, ISet<string> online, bool chinese)
		{
			this.Name = "CycleGroupMembersDialog";
			this.Text = TextFor(chinese, "AddMembersTitle", "添加已知角色", "Add known clients");
			this.ClientSize = new Size(460, 340);
			var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.Controls.Add(new Label { AutoSize = true, Dock = DockStyle.Fill, Text = TextFor(chinese, "AddMembersHint", "勾选要加入本组的角色；不会移出其原有分组。", "Select clients to add. Existing memberships will be kept."), Margin = new Padding(3, 3, 3, 8) }, 0, 0);
			this._clients = new CheckedListBox { Name = "DynamicGroupAvailableClientsList", Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false, HorizontalScrollbar = true };
			foreach (string title in available ?? Array.Empty<string>()) this._clients.Items.Add(new ClientItem { Title = title, Caption = ClientTitleFormatter.GetCharacterName(title) + " — " + (online.Contains(title) ? TextFor(chinese, "Online", "在线", "Online") : TextFor(chinese, "Offline", "离线", "Offline")) });
			layout.Controls.Add(this._clients, 0, 1);
			var buttons = ButtonFlow();
			var cancel = DialogButton("DynamicGroupMembersCancelButton", TextFor(chinese, "Cancel", "取消", "Cancel"), DialogResult.Cancel);
			var accept = DialogButton("DynamicGroupMembersApplyButton", TextFor(chinese, "AddSelected", "添加所选", "Add selected"), DialogResult.OK);
			var all = DialogButton("DynamicGroupMembersSelectAllButton", TextFor(chinese, "SelectAll", "全选", "Select all"));
			all.Click += (sender, e) => { for (int i = 0; i < this._clients.Items.Count; i++) this._clients.SetItemChecked(i, true); };
			buttons.Controls.Add(cancel);
			buttons.Controls.Add(accept);
			buttons.Controls.Add(all);
			layout.Controls.Add(buttons, 0, 2);
			this.CancelButton = cancel;
			this.AcceptButton = accept;
			this.Controls.Add(layout);
		}
	}
}
