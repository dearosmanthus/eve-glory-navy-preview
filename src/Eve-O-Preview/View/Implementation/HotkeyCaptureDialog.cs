using EveOPreview.Configuration.Implementation;
using EveOPreview.UI.Hotkeys;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EveOPreview.View
{
	internal sealed class HotkeyCaptureDialog : Form
	{
		private readonly Label _instruction;
		private readonly Label _mousePreview;
		private readonly Label _mouseStatus;
		private readonly ComboBox _mousePreset;
		private readonly CheckBox _controlModifier;
		private readonly CheckBox _altModifier;
		private readonly CheckBox _shiftModifier;
		private readonly bool _chinese;
		private readonly bool _enableNativeMouseCapture;
		private MouseInputListener _mouseListener;
		private Keys _mouseCandidate;
		private bool _updatingPreset;
		private volatile bool _mouseCaptureActive;
		private static readonly Keys[] MousePresets = { Keys.XButton1, Keys.XButton2, Keys.MButton, Keys.F13, Keys.F14 };

		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(int virtualKey);

		public string CapturedHotkey { get; private set; }

		public HotkeyCaptureDialog(string currentHotkey) : this(currentHotkey, true)
		{
		}

		// UI regression can exercise this modal without installing a global hook.
		internal HotkeyCaptureDialog(string currentHotkey, bool enableNativeMouseCapture)
		{
			this._enableNativeMouseCapture = enableNativeMouseCapture;
			this._chinese = LocalizationExtensions.GetString("Messages.Cancel", "Cancel") == "取消";
			this.Name = "HotkeyCaptureDialog";
			this.Text = LocalizationExtensions.GetString("Messages.CaptureHotkeyTitle", "Set hotkey");
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ShowInTaskbar = false;
			this.AutoScaleMode = AutoScaleMode.Dpi;
			this.ClientSize = new Size(620, 392);
			this.Padding = new Padding(16);
			this.KeyPreview = true;
			var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Margin = Padding.Empty };
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			this._instruction = new Label
			{
				Dock = DockStyle.Fill,
				AutoSize = true,
				Text = this.LocalText(
					"键盘：直接按下组合键即可录入。鼠标：按侧键/滚轮键，或选择下方预设，再确认。\r\nEsc 取消。录入后仍需在主界面点击“应用”保存。",
					"Keyboard: press a key combination to record it. Mouse: press a side/wheel button or choose a preset, then confirm.\r\nEsc cancels. Use Apply in the main window to save the recorded hotkey.")
			};
			layout.Controls.Add(this._instruction, 0, 0);
			layout.Controls.Add(new Label
			{
				AutoSize = true,
				Dock = DockStyle.Fill,
				Padding = new Padding(0, 8, 0, 8),
				ForeColor = SystemColors.GrayText,
				Text = string.Format(LocalizationExtensions.GetString("Messages.CaptureHotkeyCurrent", "Current: {0}"), string.IsNullOrEmpty(currentHotkey) ? LocalizationExtensions.GetString("Messages.HotkeyNotAssigned", "Not assigned") : currentHotkey.Replace("Control", "Ctrl"))
			}, 0, 1);

			var mouseGroup = new GroupBox
			{
				Name = "MouseHotkeyPresetGroup", Text = this.LocalText("鼠标按键 / 驱动映射", "Mouse buttons / driver mappings"),
				Dock = DockStyle.Fill, Padding = new Padding(10, 16, 10, 8), Margin = new Padding(0, 0, 0, 8)
			};
			var mouseLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Margin = Padding.Empty };
			mouseLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			for (int row = 0; row < 4; row++) mouseLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			this._mousePreset = new ComboBox
			{
				Name = "MouseHotkeyPresetCombo", Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList,
				IntegralHeight = false, DropDownHeight = 180, Margin = new Padding(0, 0, 0, 8)
			};
			this._mousePreset.Items.AddRange(new object[]
			{
				this.LocalText("侧键 1 — XButton1（原生）", "Side button 1 — XButton1 (native)"),
				this.LocalText("侧键 2 — XButton2（原生）", "Side button 2 — XButton2 (native)"),
				this.LocalText("滚轮按下 — MButton（原生，非滚动）", "Wheel press — MButton (native, not scrolling)"),
				this.LocalText("侧键 3 — 需鼠标驱动映射为 F13", "Side button 3 — requires mouse driver mapping to F13"),
				this.LocalText("侧键 4 — 需鼠标驱动映射为 F14", "Side button 4 — requires mouse driver mapping to F14")
			});
			mouseLayout.Controls.Add(this._mousePreset, 0, 0);
			var modifiers = new FlowLayoutPanel
			{
				Name = "MouseHotkeyModifiers", Dock = DockStyle.Top, AutoSize = true, WrapContents = false,
				Margin = new Padding(0, 0, 0, 6)
			};
			this._controlModifier = new CheckBox { Name = "MouseHotkeyControlCheckBox", Text = "Ctrl", AutoSize = true };
			this._altModifier = new CheckBox { Name = "MouseHotkeyAltCheckBox", Text = "Alt", AutoSize = true };
			this._shiftModifier = new CheckBox { Name = "MouseHotkeyShiftCheckBox", Text = "Shift", AutoSize = true };
			modifiers.Controls.AddRange(new Control[] { this._controlModifier, this._altModifier, this._shiftModifier });
			mouseLayout.Controls.Add(modifiers, 0, 1);
			mouseLayout.Controls.Add(new Label
			{
				AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8), ForeColor = SystemColors.GrayText,
				Text = this.LocalText("Windows 只提供两个原生侧键；侧键 3/4 须先在鼠标驱动中映射 F13/F14。左右键和滚轮滚动不支持。",
					"Windows exposes two native side buttons. Map side buttons 3/4 to F13/F14 in your mouse driver first. Left/right clicks and wheel scrolling are not supported.")
			}, 0, 2);
			this._mousePreview = new Label { Name = "MouseHotkeyPreviewLabel", AutoSize = true, Dock = DockStyle.Fill, Margin = Padding.Empty };
			mouseLayout.Controls.Add(this._mousePreview, 0, 3);
			mouseGroup.Controls.Add(mouseLayout);
			layout.Controls.Add(mouseGroup, 0, 2);
			this._mouseStatus = new Label
			{
				Name = "MouseCaptureStatusLabel", Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 0, 0, 8),
				Text = this.LocalText("选择预设不会提交；请确认下方组合。", "Choosing a preset does not submit it; confirm the combination below.")
			};
			layout.Controls.Add(this._mouseStatus, 0, 3);
			var buttons = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Margin = Padding.Empty
			};
			var cancel = new System.Windows.Forms.Button
			{
				Name = "MouseCaptureCancelButton",
				Text = LocalizationExtensions.GetString("Messages.Cancel", "Cancel"),
				AutoSize = true,
				Anchor = AnchorStyles.Right,
				DialogResult = DialogResult.Cancel
			};
			var confirm = new System.Windows.Forms.Button
			{
				Name = "MouseHotkeyConfirmButton", AutoSize = true, Anchor = AnchorStyles.Right,
				Text = this.LocalText("确认此组合", "Confirm combination")
			};
			confirm.Click += (sender, args) => this.AcceptMouseCandidate();
			buttons.Controls.Add(cancel);
			buttons.Controls.Add(confirm);
			layout.Controls.Add(buttons, 0, 4);
			this.CancelButton = cancel;
			this.Controls.Add(layout);
			this._mousePreset.SelectedIndexChanged += (sender, args) => this.UpdateMousePreset();
			this._controlModifier.CheckedChanged += (sender, args) => this.UpdateMousePreset();
			this._altModifier.CheckedChanged += (sender, args) => this.UpdateMousePreset();
			this._shiftModifier.CheckedChanged += (sender, args) => this.UpdateMousePreset();
			this._mousePreset.SelectedIndex = 0;
		}

		private string LocalText(string chinese, string english) => this._chinese ? chinese : english;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			// WinForms can leave a modal dialog below its topmost owner (#6190).
			// Apply this after ShowDialog has assigned Owner and created the HWND.
			this.TopMost = this.Owner?.TopMost ?? false;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (!this._enableNativeMouseCapture || this.IsDisposed || !this.Visible || this._mouseListener != null) return;
			this._mouseCaptureActive = this.ContainsFocus;
			try
			{
				this._mouseListener = new MouseInputListener(this.AcceptsMouseGesture, this.RecordMouseGesture);
				if (!this._mouseListener.Start(out int error))
				{
					this.StopMouseCapture();
					this.ReportMouseCaptureFailure(error);
				}
			}
			catch (Exception exception) when (exception is System.ComponentModel.Win32Exception || exception is InvalidOperationException || exception is ExternalException)
			{
				this.StopMouseCapture();
				this.ReportMouseCaptureFailure(exception is System.ComponentModel.Win32Exception native ? native.NativeErrorCode : exception.HResult);
			}
		}

		private bool AcceptsMouseGesture(Keys gesture)
		{
			// The native hook thread must never inspect or invoke WinForms controls.
			return this._mouseCaptureActive && HotkeyGesture.IsMouseGesture(gesture) && HotkeyGesture.IsValid(gesture);
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			this._mouseCaptureActive = true;
		}

		protected override void OnDeactivate(EventArgs e)
		{
			this._mouseCaptureActive = false;
			this._mouseListener?.InvalidatePending();
			base.OnDeactivate(e);
		}

		private void RecordMouseGesture(Keys gesture)
		{
			if (this.IsDisposed || (this._enableNativeMouseCapture && !this._mouseCaptureActive)
				|| !HotkeyGesture.IsMouseGesture(gesture) || !HotkeyGesture.IsValid(gesture)) return;
			Keys code = gesture & Keys.KeyCode;
			int preset = Array.IndexOf(MousePresets, code);
			if (preset < 0) return;
			this._updatingPreset = true;
			try
			{
				this._mousePreset.SelectedIndex = preset;
				this._controlModifier.Checked = (gesture & Keys.Control) != 0;
				this._altModifier.Checked = (gesture & Keys.Alt) != 0;
				this._shiftModifier.Checked = (gesture & Keys.Shift) != 0;
			}
			finally { this._updatingPreset = false; }
			this.UpdateMousePreset();
		}

		private void UpdateMousePreset()
		{
			if (this._updatingPreset || this._mousePreset.SelectedIndex < 0) return;
			this._mouseCandidate = MousePresets[this._mousePreset.SelectedIndex]
				| (this._controlModifier.Checked ? Keys.Control : Keys.None)
				| (this._altModifier.Checked ? Keys.Alt : Keys.None)
				| (this._shiftModifier.Checked ? Keys.Shift : Keys.None);
			this._mousePreview.Text = this.LocalText("待确认：", "To confirm: ") + HotkeyGesture.Format(this._mouseCandidate);
		}

		private void AcceptMouseCandidate()
		{
			if (this._mouseCandidate == Keys.None || !HotkeyGesture.IsValid(this._mouseCandidate)) return;
			this.CapturedHotkey = HotkeyGesture.Format(this._mouseCandidate);
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void ReportMouseCaptureFailure(int error)
		{
			this._mouseStatus.ForeColor = Color.Firebrick;
			this._mouseStatus.Text = string.Format(this.LocalText(
				"鼠标直接录制不可用（错误 {0}）。仍可选择预设或使用键盘录制。",
				"Direct mouse recording is unavailable (error {0}). Presets and keyboard recording still work."), error);
		}

		private void StopMouseCapture()
		{
			this._mouseCaptureActive = false;
			MouseInputListener listener = this._mouseListener;
			this._mouseListener = null;
			listener?.Dispose();
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			this.StopMouseCapture();
			base.OnFormClosed(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) this.StopMouseCapture();
			base.Dispose(disposing);
		}

		internal static bool IsSupportedKey(Keys keyData)
		{
			Keys key = keyData & Keys.KeyCode;
			return Enum.IsDefined(typeof(Keys), key) && key >= Keys.Back && key <= Keys.OemClear
				&& key != Keys.Escape && key != Keys.F12 && key != Keys.ControlKey && key != Keys.ShiftKey
				&& key != Keys.Menu && key != Keys.LControlKey && key != Keys.RControlKey
				&& key != Keys.LShiftKey && key != Keys.RShiftKey && key != Keys.LMenu && key != Keys.RMenu
				&& key != Keys.LWin && key != Keys.RWin && key != Keys.ProcessKey && key != Keys.Packet;
		}

		protected override bool ProcessCmdKey(ref Message message, Keys keyData)
		{
			Keys key = keyData & Keys.KeyCode;
			if (key == Keys.Escape)
			{
				this.DialogResult = DialogResult.Cancel;
				this.Close();
				return true;
			}
			if (key == Keys.LWin || key == Keys.RWin || key == Keys.F12
				|| (GetAsyncKeyState((int)Keys.LWin) & 0x8000) != 0 || (GetAsyncKeyState((int)Keys.RWin) & 0x8000) != 0)
			{
				this._instruction.Text = LocalizationExtensions.GetString("Messages.CaptureHotkeyReserved", "Windows-key combinations and F12 are reserved. Please choose another key.");
				return true;
			}
			if (!IsSupportedKey(keyData)) return true;
			this.CapturedHotkey = HotkeyGesture.Format(keyData);
			this.DialogResult = DialogResult.OK;
			this.Close();
			return true;
		}
	}
}
