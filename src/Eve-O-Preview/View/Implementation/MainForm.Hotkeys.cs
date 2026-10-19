using EveOPreview.Configuration.Implementation;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EveOPreview.View
{
	public partial class MainForm
	{
		private System.Windows.Forms.TextBox _quickForwardTextBox;
		private System.Windows.Forms.TextBox _quickBackwardTextBox;
		private System.Windows.Forms.TextBox _quickHideTextBox;
		private System.Windows.Forms.TextBox _clientHotkeyTextBox;
		private FlowLayoutPanel _clientHotkeyButtons;
		private Label _quickHotkeyStatusLabel;
		private Label _clientHotkeyStatusLabel;
		private string _quickForwardHotkey = string.Empty;
		private string _quickBackwardHotkey = string.Empty;
		private string _quickHideHotkey = string.Empty;
		private string _selectedClientHotkey = string.Empty;

		public string QuickSwitchForwardHotkey
		{
			get => this._quickForwardHotkey;
			set { this._quickForwardHotkey = value ?? string.Empty; this.RefreshHotkeyDisplayText(); }
		}

		public string QuickSwitchBackwardHotkey
		{
			get => this._quickBackwardHotkey;
			set { this._quickBackwardHotkey = value ?? string.Empty; this.RefreshHotkeyDisplayText(); }
		}

		public string QuickHideHotkey
		{
			get => this._quickHideHotkey;
			set { this._quickHideHotkey = value ?? string.Empty; this.RefreshHotkeyDisplayText(); }
		}

		public Action QuickHotkeysApplyRequested { get; set; }
		public Action<string, string> ClientHotkeyApplyRequested { get; set; }
		public Action<bool> HotkeyCaptureStateChanged { get; set; }

		public void SetSelectedClientHotkey(string hotkey)
		{
			this._selectedClientHotkey = hotkey ?? string.Empty;
			if (this._clientHotkeyTextBox != null) this._clientHotkeyTextBox.Text = DisplayHotkey(this._selectedClientHotkey);
		}

		public void SetHotkeyStatus(string text, bool success)
		{
			this.SetQuickHotkeyStatus(text, success);
			this.SetClientHotkeyStatus(text, success);
		}

		public void SetQuickHotkeyStatus(string text, bool success)
		{
			SetHotkeyStatusLabel(this._quickHotkeyStatusLabel, text, success);
		}

		public void SetClientHotkeyStatus(string text, bool success)
		{
			SetHotkeyStatusLabel(this._clientHotkeyStatusLabel, text, success);
		}

		private static void SetHotkeyStatusLabel(Label label, string text, bool success)
		{
			if (label == null) return;
			label.Text = text ?? string.Empty;
			label.ForeColor = success ? SystemColors.Highlight : Color.Firebrick;
		}

		public void ShowHotkeyError(string message)
		{
			this.SetHotkeyStatus(message, false);
			MessageBox.Show(this, message, LocalizationExtensions.GetString("Messages.Error", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		private static string DisplayHotkey(string hotkey)
		{
			return string.IsNullOrWhiteSpace(hotkey)
				? LocalizationExtensions.GetString("Messages.HotkeyNotAssigned", "Not assigned")
				: hotkey.Replace("Control", "Ctrl");
		}

		private void RefreshHotkeyDisplayText()
		{
			if (this._quickForwardTextBox != null) this._quickForwardTextBox.Text = DisplayHotkey(this._quickForwardHotkey);
			if (this._quickBackwardTextBox != null) this._quickBackwardTextBox.Text = DisplayHotkey(this._quickBackwardHotkey);
			if (this._quickHideTextBox != null) this._quickHideTextBox.Text = DisplayHotkey(this._quickHideHotkey);
			if (this._clientHotkeyTextBox != null) this._clientHotkeyTextBox.Text = DisplayHotkey(this._selectedClientHotkey);
		}

		private static System.Windows.Forms.Button HotkeyButton(string name, string text, Action click)
		{
			var button = new System.Windows.Forms.Button
			{
				Name = name,
				Text = text,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				MinimumSize = new Size(62, 32),
				Margin = new Padding(3)
			};
			button.Click += (sender, e) => click();
			return button;
		}

		private static Label HotkeyLabel(string name, string text, bool hint = false)
		{
			return new Label
			{
				Name = name,
				Text = text,
				AutoSize = true,
				Dock = DockStyle.Fill,
				ForeColor = hint ? SystemColors.GrayText : SystemColors.ControlText,
				Margin = new Padding(3, 5, 3, 5)
			};
		}

		private static TableLayoutPanel HotkeyVerticalLayout()
		{
			var layout = new TableLayoutPanel
			{
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				ColumnCount = 1,
				Dock = DockStyle.Top,
				Padding = new Padding(12),
				Margin = Padding.Empty
			};
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			return layout;
		}

		private static void AddHotkeyLayoutRow(TableLayoutPanel layout, Control control)
		{
			int row = layout.RowCount++;
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.Controls.Add(control, 0, row);
		}

		private void InitializeQuickHotkeyControls(Panel panel)
		{
			panel.AutoScroll = false;
			var host = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty };
			host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			host.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			panel.Controls.Add(host);
			var scroll = new Panel { Name = "QuickActionsScrollPanel", Dock = DockStyle.Fill, AutoScroll = true };
			scroll.Margin = Padding.Empty;
			host.Controls.Add(scroll, 0, 0);
			var layout = HotkeyVerticalLayout();
			scroll.Controls.Add(layout);
			Label switchHint = this.Controls.Find("QuickSwitchHintLabel", true).OfType<Label>().First();
			this.QuickSwitchEnabledCheckBox.Margin = new Padding(3, 5, 3, 3);
			this.QuickSwitchEnabledCheckBox.Text = "Enable quick switch";
			AddHotkeyLayoutRow(layout, this.QuickSwitchEnabledCheckBox);
			switchHint.Dock = DockStyle.Fill;
			switchHint.Margin = new Padding(3, 0, 3, 8);
			AddHotkeyLayoutRow(layout, switchHint);
			AddHotkeyLayoutRow(layout, this.CreateQuickHotkeyRow("QuickBackward", "Previous client", () => this.QuickSwitchBackwardHotkey, value => this.QuickSwitchBackwardHotkey = value, "Control+Alt+Left", out this._quickBackwardTextBox));
			AddHotkeyLayoutRow(layout, this.CreateQuickHotkeyRow("QuickForward", "Next client", () => this.QuickSwitchForwardHotkey, value => this.QuickSwitchForwardHotkey = value, "Control+Alt+Right", out this._quickForwardTextBox));

			this.QuickHideEnabledCheckBox.Text = "Enable quick hide/show";
			this.QuickHideEnabledCheckBox.Margin = new Padding(3, 12, 3, 3);
			AddHotkeyLayoutRow(layout, this.QuickHideEnabledCheckBox);
			AddHotkeyLayoutRow(layout, HotkeyLabel("QuickHideHintLabel", "Press once to hide all previews; press again to restore them.", true));
			AddHotkeyLayoutRow(layout, this.CreateQuickHotkeyRow("QuickHide", "Hide / show previews", () => this.QuickHideHotkey, value => this.QuickHideHotkey = value, "Control+Alt+H", out this._quickHideTextBox));
			this.KeepMinimizedClientsLiveCheckBox.Margin = new Padding(3, 16, 3, 3);
			AddHotkeyLayoutRow(layout, this.KeepMinimizedClientsLiveCheckBox);
			AddHotkeyLayoutRow(layout, HotkeyLabel("KeepMinimizedClientsLiveHintLabel", "Restores minimized clients behind other windows; may increase GPU use.", true));

			var footer = HotkeyVerticalLayout();
			footer.Name = "QuickHotkeysFooter";
			footer.Dock = DockStyle.Fill;
			footer.Padding = new Padding(12, 6, 12, 8);
			AddHotkeyLayoutRow(footer, HotkeyButton("QuickHotkeysApplyButton", "Apply hotkeys", () => this.QuickHotkeysApplyRequested?.Invoke()));
			AddHotkeyLayoutRow(footer, HotkeyLabel("QuickHotkeysApplyHintLabel", "Set, clear or reset keys, then click Apply. Empty keys disable that action.", true));
			this._quickHotkeyStatusLabel = HotkeyLabel("QuickHotkeyStatusLabel", string.Empty);
			this._quickHotkeyStatusLabel.MaximumSize = new Size(0, 60);
			this._quickHotkeyStatusLabel.AutoEllipsis = true;
			AddHotkeyLayoutRow(footer, this._quickHotkeyStatusLabel);
			host.Controls.Add(footer, 0, 1);
		}

		private Control CreateQuickHotkeyRow(string name, string title, Func<string> getValue, Action<string> setValue, string defaultValue, out System.Windows.Forms.TextBox textBox)
		{
			var row = new TableLayoutPanel
			{
				ColumnCount = 1,
				RowCount = 3,
				Dock = DockStyle.Top,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				Size = Size.Empty,
				Margin = new Padding(0, 3, 0, 3)
			};
			row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			for (int i = 0; i < 3; i++) row.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			Label label = HotkeyLabel(name + "Label", title);
			row.Controls.Add(label, 0, 0);
			textBox = new System.Windows.Forms.TextBox
			{
				Name = name + "TextBox",
				ReadOnly = true,
				Dock = DockStyle.Fill,
				MinimumSize = new Size(115, 0),
				Margin = new Padding(3, 6, 6, 3)
			};
			row.Controls.Add(textBox, 0, 1);
			Action<string> stage = value =>
			{
				setValue(value);
				this.SetQuickHotkeyStatus(LocalizationExtensions.GetString("Messages.HotkeysPending", "Changes pending. Click Apply hotkeys to save."), true);
			};
			var buttons = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Size = Size.Empty, Dock = DockStyle.Top, WrapContents = false, Margin = Padding.Empty };
			buttons.Controls.Add(HotkeyButton(name + "SetButton", "Set...", () => this.CaptureHotkey(getValue(), stage)));
			buttons.Controls.Add(HotkeyButton(name + "ClearButton", "Clear", () => stage(string.Empty)));
			buttons.Controls.Add(HotkeyButton(name + "ResetButton", "Default", () => stage(defaultValue)));
			row.Controls.Add(buttons, 0, 2);
			return row;
		}

		private void InitializeClientHotkeyControls()
		{
			Panel panel = this.Controls.Find("ClientsPanel", true).OfType<Panel>().First();
			panel.AutoScroll = true;
			this.ThumbnailsList.CheckOnClick = false;
			var layout = HotkeyVerticalLayout();
			layout.Padding = new Padding(10);
			var sizeInputs = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill, WrapContents = true, Margin = Padding.Empty };
			foreach (string name in new[] { "PerClientSizeLabel", "PerClientWidthNumericEdit", "PerClientMultiplyLabel", "PerClientHeightNumericEdit" })
			{
				Control control = panel.Controls.Find(name, true).First();
				control.Margin = new Padding(3, 5, 3, 3);
				sizeInputs.Controls.Add(control);
			}
			var sizeButtons = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill, WrapContents = true, Margin = Padding.Empty };
			foreach (string name in new[] { "PerClientSizeApplyButton", "PerClientSizeResetButton", "PerClientSizeResetAllButton" })
			{
				Control control = panel.Controls.Find(name, true).First();
				control.AutoSize = true;
				control.Margin = new Padding(3);
				sizeButtons.Controls.Add(control);
			}
			AddHotkeyLayoutRow(layout, sizeInputs);
			AddHotkeyLayoutRow(layout, sizeButtons);
			this.PerClientSizeStateLabel.Dock = DockStyle.Fill;
			this.PerClientSizeStateLabel.Margin = new Padding(3, 2, 3, 6);
			AddHotkeyLayoutRow(layout, this.PerClientSizeStateLabel);
			AddHotkeyLayoutRow(layout, HotkeyLabel("ClientHotkeyLabel", "Hotkey for selected client"));
			this._clientHotkeyTextBox = new System.Windows.Forms.TextBox { Name = "ClientHotkeyTextBox", ReadOnly = true, Dock = DockStyle.Fill, Margin = new Padding(3) };
			AddHotkeyLayoutRow(layout, this._clientHotkeyTextBox);
			this._clientHotkeyButtons = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill, WrapContents = true, Margin = Padding.Empty };
			this._clientHotkeyButtons.Controls.Add(HotkeyButton("ClientHotkeySetButton", "Set...", () => this.CaptureHotkey(this._selectedClientHotkey, value => this.StageClientHotkey(value))));
			this._clientHotkeyButtons.Controls.Add(HotkeyButton("ClientHotkeyClearButton", "Clear", () => this.StageClientHotkey(string.Empty)));
			this._clientHotkeyButtons.Controls.Add(HotkeyButton("ClientHotkeyApplyButton", "Apply client hotkey", () =>
			{
				string title = this.GetSelectedClientTitle();
				if (!string.IsNullOrEmpty(title)) this.ClientHotkeyApplyRequested?.Invoke(title, this._selectedClientHotkey);
			}));
			AddHotkeyLayoutRow(layout, this._clientHotkeyButtons);
			this._clientHotkeyStatusLabel = HotkeyLabel("ClientHotkeyStatusLabel", string.Empty);
			AddHotkeyLayoutRow(layout, this._clientHotkeyStatusLabel);
			Label listLabel = panel.Controls.Find("ThumbnailsListLabel", true).OfType<Label>().First();
			listLabel.Dock = DockStyle.Fill;
			listLabel.Margin = new Padding(3, 8, 3, 3);
			AddHotkeyLayoutRow(layout, listLabel);
			int listRow = layout.RowCount++;
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
			this.ThumbnailsList.Dock = DockStyle.Fill;
			this.ThumbnailsList.MinimumSize = new Size(0, 100);
			this.ThumbnailsList.Margin = new Padding(3);
			layout.Controls.Add(this.ThumbnailsList, 0, listRow);
			panel.Controls.Add(layout);
			layout.BringToFront();
			this.SetClientHotkeyControlsEnabled(false);
		}

		private void SetClientHotkeyControlsEnabled(bool enabled)
		{
			if (this._clientHotkeyTextBox != null) this._clientHotkeyTextBox.Enabled = enabled;
			if (this._clientHotkeyButtons != null) this._clientHotkeyButtons.Enabled = enabled;
		}

		private void StageClientHotkey(string hotkey)
		{
			this.SetSelectedClientHotkey(hotkey);
			this.SetClientHotkeyStatus(LocalizationExtensions.GetString("Messages.ClientHotkeyPending", "Changes pending. Click Apply client hotkey to save."), true);
		}

		private void CaptureHotkey(string currentHotkey, Action<string> accepted)
		{
			try
			{
				this.HotkeyCaptureStateChanged?.Invoke(true);
				using (var dialog = new HotkeyCaptureDialog(currentHotkey))
				{
					if (dialog.ShowDialog(this) == DialogResult.OK) accepted(dialog.CapturedHotkey);
				}
			}
			finally
			{
				this.HotkeyCaptureStateChanged?.Invoke(false);
			}
		}
	}
}
