using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EveOPreview.Configuration.Implementation;

internal static partial class Program
{
    // Run in the dedicated watchdog child. Every input below calls this
    // dialog's own controls/handlers; no physical mouse or global input is used.
    private static void MouseCaptureUiTests()
    {
        Type captureType = T("EveOPreview.View.HotkeyCaptureDialog");
        bool hasIsolatedConstructor = captureType.GetConstructor(Any, null, new[] { typeof(string), typeof(bool) }, null) != null;
        Check(hasIsolatedConstructor, "mouse capture has an isolated UI constructor without a native hook");
        if (!hasIsolatedConstructor) return; // Original keyboard-only dialog fails here.

        LocalizationExtensions.SetLanguage("zh-CN");
        using var owner = new Form { Name = "MouseCaptureUiOwner", Text = "Isolated mouse capture owner", TopMost = true,
            StartPosition = FormStartPosition.Manual, Location = new Point(-10000, -10000) };
        owner.Show();
        Application.DoEvents();
        try
        {
            using (var dialog = NewMouseCapture(false))
            {
                ShowMouseCapture(owner, dialog, () =>
                {
                    var presets = (ComboBox)dialog.Controls.Find("MouseHotkeyPresetCombo", true).Single();
                    Check(presets.Items.Count == 5, "mouse capture offers two native side buttons, wheel press and two driver mappings");
                    Check(presets.Items[3]!.ToString()!.Contains("F13") && presets.Items[4]!.ToString()!.Contains("F14"), "side buttons three and four explicitly require F13/F14 driver mappings");
                    Check(dialog.Owner == owner && dialog.TopMost && !MouseUiIsWindowEnabled(owner.Handle), "mouse capture modal owns and natively disables its topmost owner");
                    Check(MouseCaptureListener(dialog) == null, "isolated mouse capture never installs a native listener");
                    for (int index = 0; index < presets.Items.Count; index++)
                    {
                        presets.SelectedIndex = index;
                        Check(dialog.DialogResult == DialogResult.None && Get(dialog, "CapturedHotkey") == null, "selecting mouse preset " + index + " does not submit a gesture");
                    }
                    presets.SelectedIndex = 0;
                    ((CheckBox)dialog.Controls.Find("MouseHotkeyControlCheckBox", true).Single()).Checked = true;
                    ((CheckBox)dialog.Controls.Find("MouseHotkeyShiftCheckBox", true).Single()).Checked = true;
                    Check(dialog.Controls.Find("MouseHotkeyPreviewLabel", true).Single().Text.Contains("XButton1"), "mouse preset preview uses a recognizable native button name");
                    Click(dialog, "MouseHotkeyConfirmButton");
                });
                Check(dialog.DialogResult == DialogResult.OK && Parse((string)Get(dialog, "CapturedHotkey")!).Key == (Keys.Control | Keys.Shift | Keys.XButton1), "explicit confirmation records Ctrl+Shift+side button one");
            }

            foreach (var item in new[] { (1, Keys.XButton2), (2, Keys.MButton), (3, Keys.F13), (4, Keys.F14) })
            {
                using var dialog = NewMouseCapture(false);
                ShowMouseCapture(owner, dialog, () =>
                {
                    ((ComboBox)dialog.Controls.Find("MouseHotkeyPresetCombo", true).Single()).SelectedIndex = item.Item1;
                    ((CheckBox)dialog.Controls.Find("MouseHotkeyAltCheckBox", true).Single()).Checked = true;
                    Click(dialog, "MouseHotkeyConfirmButton");
                });
                Check(dialog.DialogResult == DialogResult.OK && Parse((string)Get(dialog, "CapturedHotkey")!).Key == (Keys.Alt | item.Item2), "mouse preset confirms the exact native/driver gesture " + item.Item2);
            }

            using (var dialog = NewMouseCapture(false))
            {
                ShowMouseCapture(owner, dialog, () =>
                {
                    Call(dialog, "RecordMouseGesture", Keys.Control | Keys.MButton);
                    Check(dialog.DialogResult == DialogResult.None && Get(dialog, "CapturedHotkey") == null, "direct mouse recording waits for explicit confirmation");
                    Check(dialog.Controls.Find("MouseHotkeyPreviewLabel", true).Single().Text.Contains("MButton"), "direct wheel press updates the candidate preview");
                    PressDialogKey(dialog, Keys.Escape);
                });
                Check(dialog.DialogResult == DialogResult.Cancel && Get(dialog, "CapturedHotkey") == null, "cancelling a recorded mouse candidate preserves the prior binding");
            }

            using (var dialog = NewMouseCapture(false))
            {
                ShowMouseCapture(owner, dialog, () =>
                {
                    Call(dialog, "ReportMouseCaptureFailure", 5);
                    string failure = dialog.Controls.Find("MouseCaptureStatusLabel", true).Single().Text;
                    Check(failure.Contains("5") && failure.Contains("键盘"), "native capture failure shows its error code and keyboard fallback");
                    PressDialogKey(dialog, Keys.F12);
                    Check(dialog.DialogResult == DialogResult.None && Get(dialog, "CapturedHotkey") == null, "F12 remains reserved after mouse capture failure");
                    PressDialogKey(dialog, Keys.Control | Keys.K);
                });
                Check(dialog.DialogResult == DialogResult.OK && Parse((string)Get(dialog, "CapturedHotkey")!).Key == (Keys.Control | Keys.K), "keyboard recording remains usable after native mouse failure");
            }

            using (var dialog = NewMouseCapture(false))
            {
                ShowMouseCapture(owner, dialog, () =>
                {
                    foreach (Keys invalid in new[] { Keys.LButton, Keys.RButton, Keys.F13, Keys.None }) Call(dialog, "RecordMouseGesture", invalid);
                    Check(Get(dialog, "CapturedHotkey") == null && dialog.DialogResult == DialogResult.None, "direct recording rejects left/right mouse and non-mouse inputs");
                    Click(dialog, "MouseCaptureCancelButton");
                });
                Check(dialog.DialogResult == DialogResult.Cancel && Get(dialog, "CapturedHotkey") == null, "Cancel button never commits a mouse preset");
            }

            foreach (string language in new[] { "zh-CN", "en-US" })
            foreach (float scale in new[] { 1f, 1.25f, 1.5f, 2f })
            {
                LocalizationExtensions.SetLanguage(language);
                using var dialog = NewMouseCapture(false);
                dialog.Scale(new SizeF(scale, scale));
                ShowMouseCapture(owner, dialog, () =>
                {
                    foreach (string name in new[] { "MouseHotkeyPresetCombo", "MouseHotkeyConfirmButton", "MouseCaptureCancelButton", "MouseCaptureStatusLabel" })
                    {
                        Control control = dialog.Controls.Find(name, true).Single();
                        Point origin = dialog.PointToClient(control.PointToScreen(Point.Empty));
                        Check(origin.X >= 0 && origin.Y >= 0 && origin.X + control.Width <= dialog.ClientSize.Width && origin.Y + control.Height <= dialog.ClientSize.Height,
                            $"{language} mouse capture {name} fits at {scale * 100:0}% layout simulation");
                    }
                    using var bitmap = new Bitmap(dialog.Width, dialog.Height);
                    dialog.DrawToBitmap(bitmap, new Rectangle(Point.Empty, dialog.Size));
                    string path = Path.Combine(screenshots, $"mouse-capture-{language}-{scale * 100:0}pct.png");
                    bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                    Console.WriteLine("SCREENSHOT " + path);
                    PressDialogKey(dialog, Keys.Escape);
                });
            }

            // This one uses the actual default constructor and briefly starts
            // the real hook. It dispatches no input and verifies close cleanup.
            using (var dialog = (Form)New("EveOPreview.View.HotkeyCaptureDialog", "Control+Alt+Right"))
            {
                ShowMouseCapture(owner, dialog, () =>
                {
                    string status = dialog.Controls.Find("MouseCaptureStatusLabel", true).Single().Text;
                    Check(MouseCaptureListener(dialog) != null || status.Contains("error", StringComparison.OrdinalIgnoreCase) || status.Contains("错误"), "default modal starts the listener or reports an actionable install failure");
                    PressDialogKey(dialog, Keys.Escape);
                });
                Check(MouseCaptureListener(dialog) == null, "closing default capture releases the native mouse listener");
            }
            Check(MouseUiIsWindowEnabled(owner.Handle), "closing mouse capture restores its native owner input state");
        }
        finally { owner.Close(); }
    }

    private static Form NewMouseCapture(bool enableNativeMouseCapture) =>
        (Form)Activator.CreateInstance(T("EveOPreview.View.HotkeyCaptureDialog"), Any, null, new object[] { "Control+Alt+Right", enableNativeMouseCapture }, null)!;

    private static object? MouseCaptureListener(Form dialog) => dialog.GetType().GetField("_mouseListener", Any)!.GetValue(dialog);

    private static void ShowMouseCapture(Form owner, Form dialog, Action action)
    {
        Exception? error = null;
        dialog.StartPosition = FormStartPosition.Manual;
        dialog.Location = new Point(-10000, -10000);
        dialog.Shown += (_, _) => dialog.BeginInvoke((Action)(() =>
        {
            try { action(); }
            catch (Exception exception) { error = exception; }
            finally
            {
                // ShowDialog consumes a non-None result when control returns
                // to its message loop; Close need not make Visible false here.
                // Never turn a correctly accepted OK into test-cleanup Cancel.
                Console.WriteLine($"MOUSE MODAL action completed: result={dialog.DialogResult}, visible={dialog.Visible}, captured={Get(dialog, "CapturedHotkey") ?? "null"}");
                if (dialog.Visible && dialog.DialogResult == DialogResult.None)
                {
                    dialog.DialogResult = DialogResult.Cancel;
                    dialog.Close();
                }
            }
        }));
        dialog.ShowDialog(owner);
        if (error != null) throw error;
    }

    [DllImport("user32.dll", EntryPoint = "IsWindowEnabled")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MouseUiIsWindowEnabled(IntPtr window);
}
