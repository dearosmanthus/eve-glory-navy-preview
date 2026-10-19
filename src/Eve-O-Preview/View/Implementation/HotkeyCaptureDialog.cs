using EveOPreview.Configuration.Implementation;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EveOPreview.View
{
	internal sealed class HotkeyCaptureDialog : Form
	{
		private readonly Label _instruction;

		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(int virtualKey);

		public string CapturedHotkey { get; private set; }

		public HotkeyCaptureDialog(string currentHotkey)
		{
			this.Name = "HotkeyCaptureDialog";
			this.Text = LocalizationExtensions.GetString("Messages.CaptureHotkeyTitle", "Set hotkey");
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ShowInTaskbar = false;
			this.AutoScaleMode = AutoScaleMode.Dpi;
			this.ClientSize = new Size(440, 180);
			this.Padding = new Padding(16);
			this.KeyPreview = true;
			var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			this._instruction = new Label
			{
				Dock = DockStyle.Fill,
				AutoSize = true,
				Text = LocalizationExtensions.GetString("Messages.CaptureHotkeyPrompt", "Press the desired key combination. Esc cancels. Changes are saved only when you click Apply.")
			};
			layout.Controls.Add(this._instruction, 0, 0);
			layout.Controls.Add(new Label
			{
				AutoSize = true,
				Dock = DockStyle.Fill,
				Padding = new Padding(0, 14, 0, 4),
				ForeColor = SystemColors.GrayText,
				Text = string.Format(LocalizationExtensions.GetString("Messages.CaptureHotkeyCurrent", "Current: {0}"), string.IsNullOrEmpty(currentHotkey) ? LocalizationExtensions.GetString("Messages.HotkeyNotAssigned", "Not assigned") : currentHotkey.Replace("Control", "Ctrl"))
			}, 0, 1);
			var cancel = new System.Windows.Forms.Button
			{
				Text = LocalizationExtensions.GetString("Messages.Cancel", "Cancel"),
				AutoSize = true,
				Anchor = AnchorStyles.Right,
				DialogResult = DialogResult.Cancel
			};
			layout.Controls.Add(cancel, 0, 2);
			this.CancelButton = cancel;
			this.Controls.Add(layout);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			// WinForms can leave a modal dialog below its topmost owner (#6190).
			// Apply this after ShowDialog has assigned Owner and created the HWND.
			this.TopMost = this.Owner?.TopMost ?? false;
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
			this.CapturedHotkey = new KeysConverter().ConvertToInvariantString(keyData);
			this.DialogResult = DialogResult.OK;
			this.Close();
			return true;
		}
	}
}
