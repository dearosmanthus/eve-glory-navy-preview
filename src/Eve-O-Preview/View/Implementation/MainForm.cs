using EveOPreview.Configuration;
using EveOPreview.Configuration.Implementation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EveOPreview.View
{
	public partial class MainForm : Form, IMainFormView
	{
		private const string GloryNavyApplicationTitle = "EVE-O-Preview 荣耀海军定制版";

		#region Private fields
		private readonly ApplicationContext _context;
		private readonly Dictionary<ViewZoomAnchor, RadioButton> _zoomAnchorMap;
		private readonly Dictionary<ViewZoomAnchor, RadioButton> _overlayLabelMap;
		private readonly Dictionary<ViewZoomAnchor, RadioButton> _cycleGroupIndicatorMap;
		private ViewZoomAnchor _cachedThumbnailZoomAnchor;
		private ViewZoomAnchor _cachedOverlayLabelAnchor;
		private ViewZoomAnchor _cachedCycleGroupIndicatorAnchor;
		private bool _suppressEvents;
		private Size _minimumSize;
		private Size _maximumSize;
		private string _iconName;
		private bool _hotkeyCaptureActive = false;
		private Dictionary<string, string> _configurationFilenames = new Dictionary<string, string>();
		private CheckBox QuickHideEnabledCheckBox;
		private CheckBox KeepMinimizedClientsLiveCheckBox;
		private NumericUpDown PerClientWidthNumericEdit;
		private NumericUpDown PerClientHeightNumericEdit;
		private Label PerClientSizeStateLabel;
		private System.Windows.Forms.ComboBox ProfilesComboBox;
		private System.Windows.Forms.TextBox ProfileNameTextBox;
		private Label ProfileStatusLabel;
		private Icon _gloryNavyIcon;
		private Image _gloryNavyLogo;
		#endregion

		public MainForm(ApplicationContext context)
		{
			this._context = context;
			this._zoomAnchorMap = new Dictionary<ViewZoomAnchor, RadioButton>();
			this._overlayLabelMap = new Dictionary<ViewZoomAnchor, RadioButton>();
			this._cycleGroupIndicatorMap = new Dictionary<ViewZoomAnchor, RadioButton>();
			this._cachedThumbnailZoomAnchor = ViewZoomAnchor.NW;
			this._suppressEvents = false;
			this._minimumSize = new Size(20, 20);
			this._maximumSize = new Size(20, 20);

			InitializeComponent();
			this.InitializeGloryNavyBranding();
			this.InitializeCustomControls();

			this.ThumbnailsList.DisplayMember = "Title";

			SetupConfigList();

			this.InitZoomAnchorMap();
			this.InitOverlayLabelMap();
			this.InitCycleGroupIndicatorMap();
			this.InitFormSize();

			this.AnimationStyleCombo.DataSource = Enum.GetValues(typeof(AnimationStyle));
			this.CaptionOnClientsStyleCombo.DataSource = Enum.GetValues(typeof(CaptionBarStyle));
		}

		public bool MinimizeToTray
		{
			get => this.MinimizeToTrayCheckBox.Checked;
			set => this.MinimizeToTrayCheckBox.Checked = value;
		}

		public string IconName
		{
			get => this._iconName;
			set
			{
				this._iconName = "GloryNavy";
				if (this._gloryNavyIcon != null)
				{
					this.Icon = this._gloryNavyIcon;
					this.NotifyIcon.Icon = this._gloryNavyIcon;
				}
			}
		}

		public string Language
		{
			get => this.LanguageCombo.Text;
			set
			{
				this.LanguageCombo.Text = value;
			}
		}

		public double ThumbnailOpacity
		{
			get => Math.Min(this.ThumbnailOpacityTrackBar.Value / 100.00, 1.00);
			set
			{
				int barValue = (int)(100.0 * value);
				if (barValue > 100)
				{
					barValue = 100;
				}
				else if (barValue < 10)
				{
					barValue = 10;
				}

				this.ThumbnailOpacityTrackBar.Value = barValue;
			}
		}

		public bool EnableClientLayoutTracking
		{
			get => this.EnableClientLayoutTrackingCheckBox.Checked;
			set => this.EnableClientLayoutTrackingCheckBox.Checked = value;
		}

		public bool HideActiveClientThumbnail
		{
			get => this.HideActiveClientThumbnailCheckBox.Checked;
			set => this.HideActiveClientThumbnailCheckBox.Checked = value;
		}

		public bool MinimizeInactiveClients
		{
			get => this.MinimizeInactiveClientsCheckBox.Checked;
			set => this.MinimizeInactiveClientsCheckBox.Checked = value;
		}
		public ViewCaptionBarStyle CaptionOnClientsStyle
		{
			get => (ViewCaptionBarStyle)this.CaptionOnClientsStyleCombo.SelectedItem;
			set => this.CaptionOnClientsStyleCombo.SelectedIndex = (int)value;
		}
		public ViewAnimationStyle WindowsAnimationStyle
		{
			get => (ViewAnimationStyle)this.AnimationStyleCombo.SelectedItem;
			set => this.AnimationStyleCombo.SelectedIndex = (int)value;
		}

		public bool ShowThumbnailsAlwaysOnTop
		{
			get => this.ShowThumbnailsAlwaysOnTopCheckBox.Checked;
			set => this.ShowThumbnailsAlwaysOnTopCheckBox.Checked = value;
		}
		public bool PreventPreviews
		{
			get => this.PreventPreviewsCheckBox.Checked;
			set => this.PreventPreviewsCheckBox.Checked = value;
		}

		public bool HideThumbnailsOnLostFocus
		{
			get => this.HideThumbnailsOnLostFocusCheckBox.Checked;
			set => this.HideThumbnailsOnLostFocusCheckBox.Checked = value;
		}

		public bool EnablePerClientThumbnailLayouts
		{
			get => this.EnablePerClientThumbnailsLayoutsCheckBox.Checked;
			set => this.EnablePerClientThumbnailsLayoutsCheckBox.Checked = value;
		}

		public bool QuickSwitchEnabled
		{
			get => this.QuickSwitchEnabledCheckBox.Checked;
			set => this.QuickSwitchEnabledCheckBox.Checked = value;
		}

		public bool QuickHideEnabled
		{
			get => this.QuickHideEnabledCheckBox.Checked;
			set => this.QuickHideEnabledCheckBox.Checked = value;
		}

		public bool KeepMinimizedClientsLive
		{
			get => this.KeepMinimizedClientsLiveCheckBox.Checked;
			set => this.KeepMinimizedClientsLiveCheckBox.Checked = value;
		}

		public Size ThumbnailSize
		{
			get => new Size((int)this.ThumbnailsWidthNumericEdit.Value, (int)this.ThumbnailsHeightNumericEdit.Value);
			set
			{
				this.ThumbnailsWidthNumericEdit.Value = value.Width;
				this.ThumbnailsHeightNumericEdit.Value = value.Height;
			}
		}

		public bool EnableThumbnailZoom
		{
			get => this.EnableThumbnailZoomCheckBox.Checked;
			set
			{
				this.EnableThumbnailZoomCheckBox.Checked = value;
				this.RefreshZoomSettings();
			}
		}

		public int ThumbnailZoomFactor
		{
			get => (int)this.ThumbnailZoomFactorNumericEdit.Value;
			set => this.ThumbnailZoomFactorNumericEdit.Value = value;
		}

		public ViewZoomAnchor ThumbnailZoomAnchor
		{
			get
			{
				if (this._zoomAnchorMap[this._cachedThumbnailZoomAnchor].Checked)
				{
					return this._cachedThumbnailZoomAnchor;
				}

				foreach (KeyValuePair<ViewZoomAnchor, RadioButton> valuePair in this._zoomAnchorMap)
				{
					if (!valuePair.Value.Checked)
					{
						continue;
					}

					this._cachedThumbnailZoomAnchor = valuePair.Key;
					return this._cachedThumbnailZoomAnchor;
				}

				// Default value
				return ViewZoomAnchor.NW;
			}
			set
			{
				this._cachedThumbnailZoomAnchor = value;
				this._zoomAnchorMap[this._cachedThumbnailZoomAnchor].Checked = true;
			}
		}

		public ViewZoomAnchor OverlayLabelAnchor
		{
			get
			{
				if (this._overlayLabelMap[this._cachedOverlayLabelAnchor].Checked)
				{
					return this._cachedOverlayLabelAnchor;
				}

				foreach (KeyValuePair<ViewZoomAnchor, RadioButton> valuePair in this._overlayLabelMap)
				{
					if (!valuePair.Value.Checked)
					{
						continue;
					}

					this._cachedOverlayLabelAnchor = valuePair.Key;
					return this._cachedOverlayLabelAnchor;
				}

				// Default Value
				return ViewZoomAnchor.NW;
			}
			set
			{
				this._cachedOverlayLabelAnchor = value;
				this._overlayLabelMap[this._cachedOverlayLabelAnchor].Checked = true;
			}
		}

		public ViewZoomAnchor CycleGroupIndicatorAnchor
		{
			get
			{
				if (this._cycleGroupIndicatorMap[this._cachedCycleGroupIndicatorAnchor].Checked)
				{
					return this._cachedCycleGroupIndicatorAnchor;
				}

				foreach (KeyValuePair<ViewZoomAnchor, RadioButton> valuePair in this._cycleGroupIndicatorMap)
				{
					if (!valuePair.Value.Checked)
					{
						continue;
					}

					this._cachedCycleGroupIndicatorAnchor = valuePair.Key;
					return this._cachedCycleGroupIndicatorAnchor;
				}

				// Default Value
				return ViewZoomAnchor.NW;
			}
			set
			{
				this._cachedCycleGroupIndicatorAnchor = value;
				this._cycleGroupIndicatorMap[this._cachedCycleGroupIndicatorAnchor].Checked = true;
			}
		}

		public bool ShowThumbnailOverlays
		{
			get => this.ShowThumbnailOverlaysCheckBox.Checked;
			set => this.ShowThumbnailOverlaysCheckBox.Checked = value;
		}

		public bool ShowThumbnailFrames
		{
			get => this.ShowThumbnailFramesCheckBox.Checked;
			set => this.ShowThumbnailFramesCheckBox.Checked = value;
		}
		public bool LockThumbnailLocation
		{
			get => this.LockThumbnailLocationCheckbox.Checked;
			set => this.LockThumbnailLocationCheckbox.Checked = value;
		}
		public bool ThumbnailSnapToGrid
		{
			get => this.ThumbnailSnapToGridCheckBox.Checked;
			set => this.ThumbnailSnapToGridCheckBox.Checked = value;
		}
		public int ThumbnailSnapToGridSizeX
		{
			get => (int)ThumbnailSnapToGridSizeXNumericEdit.Value;
			set => ThumbnailSnapToGridSizeXNumericEdit.Value = value;
		}
		public int ThumbnailSnapToGridSizeY
		{
			get => (int)ThumbnailSnapToGridSizeYNumericEdit.Value;
			set => ThumbnailSnapToGridSizeYNumericEdit.Value = value;
		}

		public bool EnableActiveClientHighlight
		{
			get => this.EnableActiveClientHighlightCheckBox.Checked;
			set => this.EnableActiveClientHighlightCheckBox.Checked = value;
		}

		public Color ActiveClientHighlightColor
		{
			get => this._activeClientHighlightColor;
			set
			{
				this._activeClientHighlightColor = value;
				this.ActiveClientHighlightColorButton.BackColor = value;
			}
		}
		private Color _activeClientHighlightColor;

		public Color PreventPreviewColor
		{
			get => this._preventPreviewColor;
			set
			{
				this._preventPreviewColor = value;
				this.PreventPreviewColorButton.BackColor = value;
			}
		}
		private Color _preventPreviewColor;

		public Color OverlayLabelColor
		{
			get => this._OverlayLabelColor;
			set
			{
				this._OverlayLabelColor = value;
				this.OverlayLabelColorButton.BackColor = value;
			}
		}
		private Color _OverlayLabelColor;

		public Font OverlayLabelFont
		{
			get => (Font)this._OverlayLabelFont;
			set
			{
				this._OverlayLabelFont = value;
				this.LabelOverlayLabelFont.Font = value;
			}
		}
		private Font _OverlayLabelFont;

		public new void Show()
		{
			// Registers the current instance as the application's Main Form
			this._context.MainForm = this;

			this._suppressEvents = true;
			this.FormActivated?.Invoke();
			this._suppressEvents = false;

			Application.Run(this._context);
		}

		public void SetThumbnailSizeLimitations(Size minimumSize, Size maximumSize)
		{
			this._minimumSize = minimumSize;
			this._maximumSize = maximumSize;

			this.ThumbnailsWidthNumericEdit.Minimum = Math.Max(20, minimumSize.Width);
			this.ThumbnailsHeightNumericEdit.Minimum = Math.Max(20, minimumSize.Height);
			this.ThumbnailsWidthNumericEdit.Maximum = Math.Max(this.ThumbnailsWidthNumericEdit.Minimum, maximumSize.Width);
			this.ThumbnailsHeightNumericEdit.Maximum = Math.Max(this.ThumbnailsHeightNumericEdit.Minimum, maximumSize.Height);
			this.PerClientWidthNumericEdit.Minimum = minimumSize.Width;
			this.PerClientWidthNumericEdit.Maximum = Math.Max(this.PerClientWidthNumericEdit.Minimum, maximumSize.Width);
			this.PerClientHeightNumericEdit.Minimum = minimumSize.Height;
			this.PerClientHeightNumericEdit.Maximum = Math.Max(this.PerClientHeightNumericEdit.Minimum, maximumSize.Height);
		}

		public void SetSelectedClientThumbnailSize(Size size, bool hasCustomSize)
		{
			this.PerClientWidthNumericEdit.Value = Math.Max(this.PerClientWidthNumericEdit.Minimum, Math.Min(this.PerClientWidthNumericEdit.Maximum, size.Width));
			this.PerClientHeightNumericEdit.Value = Math.Max(this.PerClientHeightNumericEdit.Minimum, Math.Min(this.PerClientHeightNumericEdit.Maximum, size.Height));
			this.PerClientSizeStateLabel.Text = hasCustomSize
				? LocalizationExtensions.GetString("Messages.CustomClientSize", "Custom size")
				: LocalizationExtensions.GetString("Messages.GlobalClientSize", "Using global size");
		}

		public void SetProfileStatus(string status)
		{
			this.ProfileStatusLabel.Text = status ?? string.Empty;
		}

		public void SelectProfileFilename(string filename)
		{
			KeyValuePair<string, string> match = this._configurationFilenames.FirstOrDefault(
				pair => string.Equals(Path.GetFullPath(pair.Value), Path.GetFullPath(filename), StringComparison.OrdinalIgnoreCase));
			if (!string.IsNullOrEmpty(match.Key))
			{
				this.ProfilesComboBox.SelectedItem = match.Key;
			}
		}

		public bool ConfirmProfileOverwrite(string name)
		{
			return MessageBox.Show(
				string.Format(LocalizationExtensions.GetString("Messages.ProfileOverwrite", "Profile '{0}' already exists. Replace it?"), name),
				LocalizationExtensions.GetString("Messages.Confirm", "Confirm"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question) == DialogResult.Yes;
		}

		public bool ConfirmResetAllPerClientSizes()
		{
			return MessageBox.Show(
				LocalizationExtensions.GetString("Messages.ResetAllClientSizesConfirm", "Reset every custom client size?"),
				LocalizationExtensions.GetString("Messages.Confirm", "Confirm"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question) == DialogResult.Yes;
		}

		public void Minimize()
		{
			this.WindowState = FormWindowState.Minimized;
		}

		public void SetVersionInfo(string version)
		{
			this.VersionLabel.Text = version;
		}

		public void SetDocumentationUrl(string url)
		{
			this.DocumentationLink.Text = url;
		}

		public void AddThumbnails(IList<IThumbnailDescription> thumbnails)
		{
			this.ThumbnailsList.BeginUpdate();

			foreach (IThumbnailDescription view in thumbnails)
			{
				this.ThumbnailsList.SetItemChecked(this.ThumbnailsList.Items.Add(view), view.IsDisabled);

				if (!this.HotkeysClientsList.Items.Contains(view.Title)) this.HotkeysClientsList.Items.Add(view.Title, false);

			}

			this.ThumbnailsList.EndUpdate();
			if (this.ThumbnailsList.SelectedIndex < 0 && this.ThumbnailsList.Items.Count > 0)
			{
				this.ThumbnailsList.SelectedIndex = 0;
			}
		}

		public void RemoveThumbnails(IList<IThumbnailDescription> thumbnails)
		{
			this.ThumbnailsList.BeginUpdate();

			foreach (IThumbnailDescription view in thumbnails)
			{
				this.ThumbnailsList.Items.Remove(view);
			}

			this.ThumbnailsList.EndUpdate();
		}

		public void RefreshZoomSettings()
		{
			bool enableControls = this.EnableThumbnailZoom;
			this.ThumbnailZoomFactorNumericEdit.Enabled = enableControls;
			this.ZoomAnchorPanel.Enabled = enableControls;
		}

		public Action ApplicationExitRequested { get; set; }
		public Action<string> LoadNewSettings { get; set; }
		public Action SaveSettings { get; set; }

		public Action FormActivated { get; set; }

		public Action FormMinimized { get; set; }

		public Action<ViewCloseRequest> FormCloseRequested { get; set; }

		public Action ApplicationSettingsChanged { get; set; }

		public Action ThumbnailsSizeChanged { get; set; }

		public Action<string> ThumbnailStateChanged { get; set; }
		public Action<string> SelectedClientChanged { get; set; }
		public Action<string, Size> PerClientSizeApplyRequested { get; set; }
		public Action<string> PerClientSizeResetRequested { get; set; }
		public Action ResetAllPerClientSizesRequested { get; set; }
		public Action<string> SaveProfileRequested { get; set; }

		public Action DocumentationLinkActivated { get; set; }
		public Action SelectedCycleGroupChanged { get; set; }

		#region UI events
		private void ContentTabControl_DrawItem(object sender, DrawItemEventArgs e)
		{
			TabControl control = (TabControl)sender;
			TabPage page = control.TabPages[e.Index];
			Rectangle bounds = control.GetTabRect(e.Index);

			Graphics graphics = e.Graphics;

			Brush textBrush = new SolidBrush(SystemColors.ActiveCaptionText);
			Brush backgroundBrush = (e.State == DrawItemState.Selected)
										? new SolidBrush(SystemColors.Control)
										: new SolidBrush(SystemColors.ControlDark);
			graphics.FillRectangle(backgroundBrush, e.Bounds);

			// Use our own font
			Font font = new Font("Arial", this.Font.Size * 1.5f, FontStyle.Bold, GraphicsUnit.Pixel);

			// Draw string and center the text
			StringFormat stringFlags = new StringFormat();
			stringFlags.Alignment = StringAlignment.Center;
			stringFlags.LineAlignment = StringAlignment.Center;

			graphics.DrawString(page.Text, font, textBrush, bounds, stringFlags);
		}

		private void OptionChanged_Handler(object sender, EventArgs e)
		{
			if (this._suppressEvents)
			{
				return;
			}

			this.ApplicationSettingsChanged?.Invoke();
		}

		public int SelectedCycleGroup
		{
			get => (this.CycleGroupSelectorComboBox?.SelectedIndex ?? 0) + 1;
			set
			{
				int idx = Math.Max(0, Math.Min(4, value - 1));
				if (this.CycleGroupSelectorComboBox != null)
				{
					this.CycleGroupSelectorComboBox.SelectedIndex = idx;
				}
			}
		}

		public string CycleGroupForwardHotkeysText
		{
			get => this.HotkeysForwardListBox != null ? string.Join(",", this.HotkeysForwardListBox.Items.Cast<object>().Select(i => i.ToString())) : string.Empty;
			set
			{
				if (this.HotkeysForwardListBox == null) return;
				this.HotkeysForwardListBox.Items.Clear();
				if (string.IsNullOrWhiteSpace(value)) return;
				foreach (var part in value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
				{
					this.HotkeysForwardListBox.Items.Add(part);
				}
			}
		}

		public string CycleGroupBackwardHotkeysText
		{
			get => this.HotkeysBackwardListBox != null ? string.Join(",", this.HotkeysBackwardListBox.Items.Cast<object>().Select(i => i.ToString())) : string.Empty;
			set
			{
				if (this.HotkeysBackwardListBox == null) return;
				this.HotkeysBackwardListBox.Items.Clear();
				if (string.IsNullOrWhiteSpace(value)) return;
				foreach (var part in value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
				{
					this.HotkeysBackwardListBox.Items.Add(part);
				}
			}
		}

		public void SetAvailableClients(IList<string> clients)
		{
			if (this.HotkeysClientsList == null) return;
			this.HotkeysClientsList.Items.Clear();
			foreach (var c in clients)
			{
				this.HotkeysClientsList.Items.Add(c, false);
			}
		}

		public IList<string> GetSelectedClientsForCurrentGroup()
		{
			if (this.HotkeysClientsList == null) return new List<string>();
			var ordered = new List<string>();
			for (int i = 0; i < this.HotkeysClientsList.Items.Count; i++)
			{
				if (this.HotkeysClientsList.GetItemChecked(i))
				{
					ordered.Add(this.HotkeysClientsList.Items[i].ToString());
				}
			}
			return ordered;
		}

		public void SetSelectedClientsForCurrentGroup(IList<string> orderedClients)
		{
			if (this.HotkeysClientsList == null) return;
			// Reorder items so orderedClients appear first in the given order, others follow
			var all = this.HotkeysClientsList.Items.Cast<object>().Select(o => o.ToString()).ToList();
			var newOrder = new List<string>();
			if (orderedClients != null)
			{
				foreach (var s in orderedClients)
				{
					if (all.Contains(s) && !newOrder.Contains(s)) newOrder.Add(s);
				}
			}
			foreach (var a in all)
			{
				if (!newOrder.Contains(a)) newOrder.Add(a);
			}
			this.HotkeysClientsList.Items.Clear();
			foreach (var it in newOrder)
			{
				this.HotkeysClientsList.Items.Add(it, orderedClients != null && orderedClients.Contains(it));
			}
		}

		private void CycleGroupSelectorComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Notify presenter that the selected group changed
			this.SelectedCycleGroupChanged?.Invoke();
		}

		private void HotkeysForwardAddButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(this.HotkeyCaptureTextBox?.Text)) return;
			var keyText = this.HotkeyCaptureTextBox.Text.Trim();
			if (this.ValidateAndMaybeWarnHotkey(keyText))
			{
				if (!this.HotkeysForwardListBox.Items.Contains(keyText))
				{
					this.HotkeysForwardListBox.Items.Add(keyText);
					this.ApplicationSettingsChanged?.Invoke();
				}
			}
		}

		private void HotkeysForwardRemoveButton_Click(object sender, EventArgs e)
		{
			if (this.HotkeysForwardListBox.SelectedIndex < 0) return;
			this.HotkeysForwardListBox.Items.RemoveAt(this.HotkeysForwardListBox.SelectedIndex);
			this.ApplicationSettingsChanged?.Invoke();
		}

		private void HotkeysBackwardAddButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(this.HotkeyCaptureTextBox?.Text)) return;
			var keyText = this.HotkeyCaptureTextBox.Text.Trim();
			if (this.ValidateAndMaybeWarnHotkey(keyText))
			{
				if (!this.HotkeysBackwardListBox.Items.Contains(keyText))
				{
					this.HotkeysBackwardListBox.Items.Add(keyText);
					this.ApplicationSettingsChanged?.Invoke();
				}
			}
		}

		private void HotkeysBackwardRemoveButton_Click(object sender, EventArgs e)
		{
			if (this.HotkeysBackwardListBox.SelectedIndex < 0) return;
			this.HotkeysBackwardListBox.Items.RemoveAt(this.HotkeysBackwardListBox.SelectedIndex);
			this.ApplicationSettingsChanged?.Invoke();
		}

		private void HotkeyCaptureTextBox_Enter(object sender, EventArgs e)
		{
			this.HotkeyCaptureTextBox.Text = LocalizationExtensions.GetString("MainForm.ContentTabControl.CycleGroupTabPage.HotkeyCaptureButton", "Capture");
			this.HotkeyCaptureTextBox.SelectAll();
		}

		private void HotkeyCaptureTextBox_Leave(object sender, EventArgs e)
		{
			// clear placeholder if left unchanged
			if (this.HotkeyCaptureTextBox.Text == LocalizationExtensions.GetString("MainForm.ContentTabControl.CycleGroupTabPage.HotkeyCaptureButton", "Capture")) this.HotkeyCaptureTextBox.Text = string.Empty;
		}

		private void HotkeysClientUpButton_Click(object sender, EventArgs e)
		{
			int idx = this.HotkeysClientsList.SelectedIndex;
			if (idx <= 0) return;
			var item = this.HotkeysClientsList.Items[idx];
			var checkedState = this.HotkeysClientsList.GetItemChecked(idx);
			this.HotkeysClientsList.Items.RemoveAt(idx);
			this.HotkeysClientsList.Items.Insert(idx - 1, item);
			this.HotkeysClientsList.SetItemChecked(idx - 1, checkedState);
			this.HotkeysClientsList.SelectedIndex = idx - 1;
			this.ApplicationSettingsChanged?.Invoke();
		}

		private void HotkeysClientDownButton_Click(object sender, EventArgs e)
		{
			int idx = this.HotkeysClientsList.SelectedIndex;
			if (idx < 0 || idx >= this.HotkeysClientsList.Items.Count - 1) return;
			var item = this.HotkeysClientsList.Items[idx];
			var checkedState = this.HotkeysClientsList.GetItemChecked(idx);
			this.HotkeysClientsList.Items.RemoveAt(idx);
			this.HotkeysClientsList.Items.Insert(idx + 1, item);
			this.HotkeysClientsList.SetItemChecked(idx + 1, checkedState);
			this.HotkeysClientsList.SelectedIndex = idx + 1;
			this.ApplicationSettingsChanged?.Invoke();
		}

		private void HotkeySaveButton_Click(object sender, EventArgs e)
		{
			this.ApplicationSettingsChanged?.Invoke();
		}

		/// <summary>
		/// Convert the hotkey string to Keys and attempt to register/unregister to verify its validity; if invalid, display a pop-up message.		/// </summary>
		/// <param name="keyText">For example, "Control+F14"</param>
		// <returns>Returns true if valid, otherwise false</returns>
		private bool ValidateAndMaybeWarnHotkey(string keyText)
		{
			Keys parsed = Keys.None;
			try
			{
				var conv = new KeysConverter();
				var obj = conv.ConvertFromInvariantString(keyText);
				if (obj is Keys k)
				{
					parsed = k;
				}
			}
			catch
			{
				parsed = Keys.None;
			}

			// Filtering invalid values ??and modifying only keys
			if (parsed == Keys.None || parsed == Keys.ControlKey || parsed == Keys.ShiftKey || parsed == Keys.Menu || parsed == Keys.ProcessKey)
			{
				MessageBox.Show(LocalizationExtensions.GetString("Messages.InvalidHotkey", "Invalid hotkey"), LocalizationExtensions.GetString("Messages.Error", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			// Attempt to register and verify usage
			EveOPreview.UI.Hotkeys.HotkeyHandler tester = null;
			try
			{
				tester = new EveOPreview.UI.Hotkeys.HotkeyHandler(default(IntPtr), parsed);
				if (!tester.CanRegister())
				{
					MessageBox.Show(LocalizationExtensions.GetString("Messages.HotkeyAlreadyInUse", "Hotkey Already in use"), LocalizationExtensions.GetString("Messages.Error", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				}
			}
			finally
			{
				tester?.Dispose();
			}

			return true;
		}


		private void ThumbnailSizeChanged_Handler(object sender, EventArgs e)
		{
			if (this._suppressEvents)
			{
				return;
			}

			// Perform some View work that is not properly done in the Control
			this._suppressEvents = true;
			Size thumbnailSize = this.ThumbnailSize;
			thumbnailSize.Width = Math.Min(Math.Max(thumbnailSize.Width, this._minimumSize.Width), this._maximumSize.Width);
			thumbnailSize.Height = Math.Min(Math.Max(thumbnailSize.Height, this._minimumSize.Height), this._maximumSize.Height);
			this.ThumbnailSize = thumbnailSize;
			this._suppressEvents = false;

			this.ThumbnailsSizeChanged?.Invoke();
		}

		private void ApplyThumbnailPreset(int width, int height)
		{
			this._suppressEvents = true;
			this.ThumbnailSize = new Size(
				Math.Min(Math.Max(width, this._minimumSize.Width), this._maximumSize.Width),
				Math.Min(Math.Max(height, this._minimumSize.Height), this._maximumSize.Height));
			this._suppressEvents = false;
			this.ThumbnailsSizeChanged?.Invoke();
		}

		private void ThumbnailPresetTinyButton_Click(object sender, EventArgs e)
		{
			this.ApplyThumbnailPreset(96, 54);
		}

		private void ThumbnailPresetCompactButton_Click(object sender, EventArgs e)
		{
			this.ApplyThumbnailPreset(128, 72);
		}

		private void ThumbnailPresetStandardButton_Click(object sender, EventArgs e)
		{
			this.ApplyThumbnailPreset(192, 108);
		}

		private void ActiveClientHighlightColorButton_Click(object sender, EventArgs e)
		{
			using (ColorDialog dialog = new ColorDialog())
			{
				dialog.Color = this.ActiveClientHighlightColor;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				this.ActiveClientHighlightColor = dialog.Color;
			}

			this.OptionChanged_Handler(sender, e);
		}

		private void OverlayLabelColorButton_Click(object sender, EventArgs e)
		{
			using (ColorDialog dialog = new ColorDialog())
			{
				dialog.Color = this.OverlayLabelColor;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				this.OverlayLabelColor = dialog.Color;
			}

			this.OptionChanged_Handler(sender, e);
		}

		private void ThumbnailsList_ItemCheck_Handler(object sender, ItemCheckEventArgs e)
		{
			if (!(this.ThumbnailsList.Items[e.Index] is IThumbnailDescription selectedItem))
			{
				return;
			}

			selectedItem.IsDisabled = (e.NewValue == CheckState.Checked);

			this.ThumbnailStateChanged?.Invoke(selectedItem.Title);
		}

		private void DocumentationLinkClicked_Handler(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.DocumentationLinkActivated?.Invoke();
		}

		private void MainFormResize_Handler(object sender, EventArgs e)
		{
			if (this.WindowState != FormWindowState.Minimized)
			{
				return;
			}

			this.FormMinimized?.Invoke();
		}

		private void MainFormClosing_Handler(object sender, FormClosingEventArgs e)
		{
			ViewCloseRequest request = new ViewCloseRequest();

			this.FormCloseRequested?.Invoke(request);

			e.Cancel = !request.Allow;
		}

		private void RestoreMainForm_Handler(object sender, EventArgs e)
		{
			// This is form's GUI lifecycle event that is invariant to the Form data
			base.Show();
			this.WindowState = FormWindowState.Normal;
			this.BringToFront();
		}

		private void ExitMenuItemClick_Handler(object sender, EventArgs e)
		{
			this.ApplicationExitRequested?.Invoke();
		}
		#endregion

		private void InitializeGloryNavyBranding()
		{
			System.Reflection.Assembly assembly = typeof(MainForm).Assembly;
			using (Stream iconStream = assembly.GetManifestResourceStream("EveOPreview.Assets.GloryNavyIcon.ico"))
			{
				if (iconStream != null)
				{
					using (Icon loadedIcon = new Icon(iconStream))
					{
						this._gloryNavyIcon = (Icon)loadedIcon.Clone();
					}
					this.Icon = this._gloryNavyIcon;
					this.NotifyIcon.Icon = this._gloryNavyIcon;
				}
			}

			using (Stream logoStream = assembly.GetManifestResourceStream("EveOPreview.Assets.GloryNavyIcon.png"))
			{
				if (logoStream != null)
				{
					using (Image loadedLogo = Image.FromStream(logoStream))
					{
						this._gloryNavyLogo = new Bitmap(loadedLogo);
					}
				}
			}

			Panel aboutPanel = this.Controls.Find("AboutPanel", true).OfType<Panel>().FirstOrDefault();
			if (aboutPanel != null && this._gloryNavyLogo != null)
			{
				PictureBox logoPicture = new PictureBox
				{
					Name = "GloryNavyLogoPictureBox",
					Image = this._gloryNavyLogo,
					Location = new Point(14, 14),
					Size = new Size(72, 72),
					SizeMode = PictureBoxSizeMode.Zoom,
					BackColor = Color.Black
				};
				aboutPanel.Controls.Add(logoPicture);

				Label nameLabel = this.Controls.Find("NameLabel", true).OfType<Label>().FirstOrDefault();
				if (nameLabel != null)
				{
					nameLabel.Location = new Point(98, 14);
					nameLabel.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
				}

				Label corporationLabel = new Label
				{
					Name = "GloryNavyCorporationLabel",
					Text = "Glory Navy / 荣耀海军",
					AutoSize = true,
					Location = new Point(98, 45),
					Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
					ForeColor = Color.DarkGoldenrod
				};
				aboutPanel.Controls.Add(corporationLabel);

				this.VersionLabel.Location = new Point(98, 72);
				this.VersionLabel.Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Regular);

				Label descriptionLabel = this.Controls.Find("DescriptionLabel", true).OfType<Label>().FirstOrDefault();
				if (descriptionLabel != null)
				{
					descriptionLabel.Location = new Point(0, 96);
					descriptionLabel.Size = new Size(434, 179);
					descriptionLabel.Text =
						"荣耀海军军团专用的 EVE 多客户端预览与快捷切换工具。\r\n\r\n" +
						"本程序不会修改 EVE 客户端界面，不会广播键鼠输入；" +
						"只负责显示预览、调整布局以及把选定客户端切换到前台。";
				}
			}

			this.ApplyGloryNavyBrandText();
		}

		private void ApplyGloryNavyBrandText()
		{
			this.Text = MainForm.GloryNavyApplicationTitle;
			this.NotifyIcon.Text = MainForm.GloryNavyApplicationTitle;

			ToolStripMenuItem titleMenuItem = this.TrayMenu.Items
				.OfType<ToolStripMenuItem>()
				.FirstOrDefault(item => item.Name == "TitleMenuItem");
			if (titleMenuItem != null)
			{
				titleMenuItem.Text = MainForm.GloryNavyApplicationTitle;
			}

			Label nameLabel = this.Controls.Find("NameLabel", true).OfType<Label>().FirstOrDefault();
			if (nameLabel != null)
			{
				nameLabel.Text = MainForm.GloryNavyApplicationTitle;
			}
		}

		private void InitializeCustomControls()
		{
			TabControl contentTabs = this.Controls.Find("ContentTabControl", true).OfType<TabControl>().First();

			// Keep the most frequently used actions together and move the v1 quick
			// switch control out of the crowded general page.
			TabPage quickActionsPage = new TabPage
			{
				Name = "QuickActionsTabPage",
				Text = "Quick actions",
				BackColor = SystemColors.Control
			};
			Panel quickActionsPanel = new Panel
			{
				Name = "QuickActionsPanel",
				Dock = DockStyle.Fill,
				BorderStyle = BorderStyle.FixedSingle
			};
			quickActionsPage.Controls.Add(quickActionsPanel);

			Label quickSwitchHint = this.Controls.Find("QuickSwitchHintLabel", true).OfType<Label>().First();
			this.QuickSwitchEnabledCheckBox.Location = new Point(18, 24);
			quickSwitchHint.Location = new Point(40, 59);
			quickActionsPanel.Controls.Add(this.QuickSwitchEnabledCheckBox);
			quickActionsPanel.Controls.Add(quickSwitchHint);

			this.QuickHideEnabledCheckBox = new CheckBox
			{
				Name = "QuickHideEnabledCheckBox",
				Text = "Quick hide/show previews (Ctrl+Alt+H)",
				AutoSize = true,
				Location = new Point(18, 108),
				Checked = true
			};
			this.QuickHideEnabledCheckBox.CheckedChanged += OptionChanged_Handler;
			Label quickHideHint = new Label
			{
				Name = "QuickHideHintLabel",
				Text = "Press once to hide all previews; press again to restore them.",
				AutoSize = true,
				ForeColor = SystemColors.GrayText,
				Location = new Point(40, 143)
			};

			this.KeepMinimizedClientsLiveCheckBox = new CheckBox
			{
				Name = "KeepMinimizedClientsLiveCheckBox",
				Text = "Experimental: keep minimized client previews live",
				AutoSize = true,
				Location = new Point(18, 205)
			};
			this.KeepMinimizedClientsLiveCheckBox.CheckedChanged += KeepMinimizedClientsLiveCheckBox_CheckedChanged;
			Label keepLiveHint = new Label
			{
				Name = "KeepMinimizedClientsLiveHintLabel",
				Text = "Restores minimized clients behind other windows; may increase GPU use.",
				AutoSize = true,
				MaximumSize = new Size(500, 0),
				ForeColor = SystemColors.GrayText,
				Location = new Point(40, 240)
			};

			quickActionsPanel.Controls.Add(this.QuickHideEnabledCheckBox);
			quickActionsPanel.Controls.Add(quickHideHint);
			quickActionsPanel.Controls.Add(this.KeepMinimizedClientsLiveCheckBox);
			quickActionsPanel.Controls.Add(keepLiveHint);
			contentTabs.TabPages.Insert(1, quickActionsPage);

			this.InitializePerClientSizeControls();
			this.InitializeProfilesTab(contentTabs);

			this.RenameTab(contentTabs, "GeneralTabPage", "Running");
			this.RenameTab(contentTabs, "ThumbnailTabPage", "Size & layout");
			this.RenameTab(contentTabs, "ZoomTabPage", "Hover zoom");
			this.RenameTab(contentTabs, "OverlayTabPage", "Appearance");
			this.RenameTab(contentTabs, "CycleGroupTabPage", "Advanced hotkeys");
			this.RenameTab(contentTabs, "ClientsTabPage", "Clients");
		}

		private void InitializePerClientSizeControls()
		{
			Panel clientsPanel = this.Controls.Find("ClientsPanel", true).OfType<Panel>().First();
			this.ThumbnailsList.Dock = DockStyle.None;
			this.ThumbnailsList.Location = new Point(0, 174);
			this.ThumbnailsList.Size = new Size(clientsPanel.ClientSize.Width, Math.Max(100, clientsPanel.ClientSize.Height - 174));
			this.ThumbnailsList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Label sizeLabel = new Label
			{
				Name = "PerClientSizeLabel",
				Text = "Selected client size",
				AutoSize = true,
				Location = new Point(13, 56)
			};

			this.PerClientWidthNumericEdit = new NumericUpDown
			{
				Name = "PerClientWidthNumericEdit",
				Minimum = 64,
				Maximum = 960,
				Increment = 1,
				Value = 192,
				Size = new Size(78, 31),
				Location = new Point(164, 51),
				Enabled = false
			};
			Label multiplyLabel = new Label
			{
				Name = "PerClientMultiplyLabel",
				Text = "×",
				AutoSize = true,
				Location = new Point(249, 56)
			};
			this.PerClientHeightNumericEdit = new NumericUpDown
			{
				Name = "PerClientHeightNumericEdit",
				Minimum = 36,
				Maximum = 540,
				Increment = 1,
				Value = 108,
				Size = new Size(78, 31),
				Location = new Point(273, 51),
				Enabled = false
			};

			System.Windows.Forms.Button applyButton = new System.Windows.Forms.Button
			{
				Name = "PerClientSizeApplyButton",
				Text = "Apply",
				Size = new Size(84, 34),
				Location = new Point(13, 93),
				Enabled = false
			};
			applyButton.Click += PerClientSizeApplyButton_Click;
			System.Windows.Forms.Button resetButton = new System.Windows.Forms.Button
			{
				Name = "PerClientSizeResetButton",
				Text = "Use global",
				Size = new Size(100, 34),
				Location = new Point(105, 93),
				Enabled = false
			};
			resetButton.Click += PerClientSizeResetButton_Click;

			this.PerClientSizeStateLabel = new Label
			{
				Name = "PerClientSizeStateLabel",
				Text = "Select a client below",
				AutoSize = true,
				ForeColor = SystemColors.GrayText,
				Location = new Point(13, 138)
			};
			System.Windows.Forms.Button resetAllButton = new System.Windows.Forms.Button
			{
				Name = "PerClientSizeResetAllButton",
				Text = "Reset all custom sizes",
				AutoSize = true,
				Location = new Point(214, 93)
			};
			resetAllButton.Click += (sender, e) => this.ResetAllPerClientSizesRequested?.Invoke();

			clientsPanel.Controls.Add(sizeLabel);
			clientsPanel.Controls.Add(this.PerClientWidthNumericEdit);
			clientsPanel.Controls.Add(multiplyLabel);
			clientsPanel.Controls.Add(this.PerClientHeightNumericEdit);
			clientsPanel.Controls.Add(applyButton);
			clientsPanel.Controls.Add(resetButton);
			clientsPanel.Controls.Add(this.PerClientSizeStateLabel);
			clientsPanel.Controls.Add(resetAllButton);
			this.ThumbnailsList.SelectedIndexChanged += ThumbnailsList_SelectedIndexChanged;
		}

		private void InitializeProfilesTab(TabControl contentTabs)
		{
			TabPage profilesPage = new TabPage
			{
				Name = "ProfilesTabPage",
				Text = "Layout profiles",
				BackColor = SystemColors.Control
			};
			Panel profilesPanel = new Panel
			{
				Name = "ProfilesPanel",
				Dock = DockStyle.Fill,
				BorderStyle = BorderStyle.FixedSingle
			};
			profilesPage.Controls.Add(profilesPanel);

			Label profileListLabel = new Label
			{
				Name = "ProfileListLabel",
				Text = "Saved profiles",
				AutoSize = true,
				Location = new Point(18, 24)
			};
			this.ProfilesComboBox = new System.Windows.Forms.ComboBox
			{
				Name = "ProfilesComboBox",
				DropDownStyle = ComboBoxStyle.DropDownList,
				Size = new Size(260, 33),
				Location = new Point(18, 57)
			};
			System.Windows.Forms.Button loadButton = new System.Windows.Forms.Button
			{
				Name = "ProfileLoadButton",
				Text = "Load",
				Size = new Size(76, 35),
				Location = new Point(285, 56)
			};
			loadButton.Click += ProfileLoadButton_Click;

			Label saveAsLabel = new Label
			{
				Name = "ProfileSaveAsLabel",
				Text = "Save current settings as",
				AutoSize = true,
				Location = new Point(18, 129)
			};
			this.ProfileNameTextBox = new System.Windows.Forms.TextBox
			{
				Name = "ProfileNameTextBox",
				Size = new Size(260, 31),
				Location = new Point(18, 162)
			};
			System.Windows.Forms.Button saveButton = new System.Windows.Forms.Button
			{
				Name = "ProfileSaveButton",
				Text = "Save as",
				Size = new Size(76, 35),
				Location = new Point(285, 160)
			};
			saveButton.Click += ProfileSaveButton_Click;

			Label profileHint = new Label
			{
				Name = "ProfileHintLabel",
				Text = "Profiles include preview positions, sizes, appearance and hotkeys.",
				AutoSize = true,
				MaximumSize = new Size(520, 0),
				ForeColor = SystemColors.GrayText,
				Location = new Point(18, 225)
			};
			this.ProfileStatusLabel = new Label
			{
				Name = "ProfileStatusLabel",
				AutoSize = true,
				ForeColor = SystemColors.Highlight,
				Location = new Point(18, 300)
			};

			profilesPanel.Controls.Add(profileListLabel);
			profilesPanel.Controls.Add(this.ProfilesComboBox);
			profilesPanel.Controls.Add(loadButton);
			profilesPanel.Controls.Add(saveAsLabel);
			profilesPanel.Controls.Add(this.ProfileNameTextBox);
			profilesPanel.Controls.Add(saveButton);
			profilesPanel.Controls.Add(profileHint);
			profilesPanel.Controls.Add(this.ProfileStatusLabel);

			int clientsIndex = contentTabs.TabPages.IndexOfKey("ClientsTabPage");
			contentTabs.TabPages.Insert(clientsIndex + 1, profilesPage);
		}

		private void RenameTab(TabControl tabs, string name, string text)
		{
			TabPage page = tabs.TabPages.Cast<TabPage>().FirstOrDefault(tab => tab.Name == name);
			if (page != null)
			{
				page.Text = text;
			}
		}

		private string GetSelectedClientTitle()
		{
			return (this.ThumbnailsList.SelectedItem as IThumbnailDescription)?.Title;
		}

		private void KeepMinimizedClientsLiveCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this.KeepMinimizedClientsLiveCheckBox.Checked)
			{
				this.MinimizeInactiveClientsCheckBox.Checked = false;
				this.MinimizeInactiveClientsCheckBox.Enabled = false;
			}
			else
			{
				this.MinimizeInactiveClientsCheckBox.Enabled = true;
			}

			this.OptionChanged_Handler(sender, e);
		}

		private void ThumbnailsList_SelectedIndexChanged(object sender, EventArgs e)
		{
			string title = this.GetSelectedClientTitle();
			bool enabled = !string.IsNullOrEmpty(title);
			this.PerClientWidthNumericEdit.Enabled = enabled;
			this.PerClientHeightNumericEdit.Enabled = enabled;
			foreach (Control control in this.Controls.Find("PerClientSizeApplyButton", true).Concat(this.Controls.Find("PerClientSizeResetButton", true)))
			{
				control.Enabled = enabled;
			}

			if (enabled)
			{
				this.SelectedClientChanged?.Invoke(title);
			}
		}

		private void PerClientSizeApplyButton_Click(object sender, EventArgs e)
		{
			string title = this.GetSelectedClientTitle();
			if (!string.IsNullOrEmpty(title))
			{
				this.PerClientSizeApplyRequested?.Invoke(title, new Size((int)this.PerClientWidthNumericEdit.Value, (int)this.PerClientHeightNumericEdit.Value));
			}
		}

		private void PerClientSizeResetButton_Click(object sender, EventArgs e)
		{
			string title = this.GetSelectedClientTitle();
			if (!string.IsNullOrEmpty(title))
			{
				this.PerClientSizeResetRequested?.Invoke(title);
			}
		}

		private void ProfileLoadButton_Click(object sender, EventArgs e)
		{
			string displayName = this.ProfilesComboBox.SelectedItem?.ToString();
			if (!string.IsNullOrEmpty(displayName) && this._configurationFilenames.TryGetValue(displayName, out string filename))
			{
				this.LoadNewSettings?.Invoke(filename);
				this.SetProfileStatus(string.Format(
					LocalizationExtensions.GetString("Messages.ProfileLoaded", "Loaded profile: {0}"),
					displayName));
			}
		}

		private void ProfileSaveButton_Click(object sender, EventArgs e)
		{
			this.SaveProfileRequested?.Invoke(this.ProfileNameTextBox.Text);
		}

		private void InitZoomAnchorMap()
		{
			this._zoomAnchorMap[ViewZoomAnchor.NW] = this.ZoomAanchorNWRadioButton;
			this._zoomAnchorMap[ViewZoomAnchor.N] = this.ZoomAanchorNRadioButton;
			this._zoomAnchorMap[ViewZoomAnchor.NE] = this.ZoomAanchorNERadioButton;
			this._zoomAnchorMap[ViewZoomAnchor.W] = this.ZoomAanchorWRadioButton;
			this._zoomAnchorMap[ViewZoomAnchor.C] = this.ZoomAanchorCRadioButton;
			this._zoomAnchorMap[ViewZoomAnchor.E] = this.ZoomAanchorERadioButton;
			this._zoomAnchorMap[ViewZoomAnchor.SW] = this.ZoomAanchorSWRadioButton;
			this._zoomAnchorMap[ViewZoomAnchor.S] = this.ZoomAanchorSRadioButton;
			this._zoomAnchorMap[ViewZoomAnchor.SE] = this.ZoomAanchorSERadioButton;
		}
		private void InitOverlayLabelMap()
		{
			this._overlayLabelMap[ViewZoomAnchor.NW] = this.OverlayLabelNWRadioButton;
			this._overlayLabelMap[ViewZoomAnchor.N] = this.OverlayLabelNRadioButton;
			this._overlayLabelMap[ViewZoomAnchor.NE] = this.OverlayLabelNERadioButton;
			this._overlayLabelMap[ViewZoomAnchor.W] = this.OverlayLabelWRadioButton;
			this._overlayLabelMap[ViewZoomAnchor.C] = this.OverlayLabelCRadioButton;
			this._overlayLabelMap[ViewZoomAnchor.E] = this.OverlayLabelERadioButton;
			this._overlayLabelMap[ViewZoomAnchor.SW] = this.OverlayLabelSWRadioButton;
			this._overlayLabelMap[ViewZoomAnchor.S] = this.OverlayLabelSRadioButton;
			this._overlayLabelMap[ViewZoomAnchor.SE] = this.OverlayLabelSERadioButton;
		}
		private void InitCycleGroupIndicatorMap()
		{
			this._cycleGroupIndicatorMap[ViewZoomAnchor.NW] = this.CycleGroupIndicatorNWRadioButton;
			this._cycleGroupIndicatorMap[ViewZoomAnchor.N] = this.CycleGroupIndicatorNRadioButton;
			this._cycleGroupIndicatorMap[ViewZoomAnchor.NE] = this.CycleGroupIndicatorNERadioButton;
			this._cycleGroupIndicatorMap[ViewZoomAnchor.W] = this.CycleGroupIndicatorWRadioButton;
			this._cycleGroupIndicatorMap[ViewZoomAnchor.C] = this.CycleGroupIndicatorCRadioButton;
			this._cycleGroupIndicatorMap[ViewZoomAnchor.E] = this.CycleGroupIndicatorERadioButton;
			this._cycleGroupIndicatorMap[ViewZoomAnchor.SW] = this.CycleGroupIndicatorSWRadioButton;
			this._cycleGroupIndicatorMap[ViewZoomAnchor.S] = this.CycleGroupIndicatorSRadioButton;
			this._cycleGroupIndicatorMap[ViewZoomAnchor.SE] = this.CycleGroupIndicatorSERadioButton;
		}

		private void InitFormSize()
		{
			const int BUFFER_PIXEL_AMOUNT = 8;
			// resize form height based on tabbed control item height
			var tabControl = (System.Windows.Forms.TabControl)this.Controls.Find("ContentTabControl", false).First();
			if (tabControl != null)
			{
				var furnitureSize = this.Height - tabControl.Height;
				var calculatedHeight = (tabControl.ItemSize.Width * tabControl.Controls.Count) + furnitureSize + BUFFER_PIXEL_AMOUNT;
				if (this.Height < calculatedHeight)
				{
					this.Height = calculatedHeight;
				}
			}
		}

		private void btnLabelFont_Click(object sender, EventArgs e)
		{
			FontDialog fontSelector = new FontDialog();
			fontSelector.Font = OverlayLabelFont;
			fontSelector.ShowColor = false;
			fontSelector.ShowApply = false;
			fontSelector.ShowHelp = false;
			if (fontSelector.ShowDialog() != DialogResult.Cancel)
			{
				OverlayLabelFont = fontSelector.Font;
				LabelOverlayLabelFont.Font = fontSelector.Font;
				this.OptionChanged_Handler(sender, e);
			}
		}

		private void PreventPreviewColorButton_Click(object sender, EventArgs e)
		{
			using (ColorDialog dialog = new ColorDialog())
			{
				dialog.Color = this.PreventPreviewColor;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				this.PreventPreviewColor = dialog.Color;
			}

			this.OptionChanged_Handler(sender, e);

		}

		public void InitializeLanguageControls()
		{
			if (LanguageCombo.Items.Count == 0)
			{
				foreach (var l in LocalizationExtensions.GetLanguages())
				{
					LanguageCombo.Items.Add(l);
				}
			}

			LocalizationExtensions.ApplyLocalization(this);
			this.NotifyIcon.Text = LocalizationExtensions.GetString($"{this.Name}.NotifyIcon", this.NotifyIcon.Text);
			foreach (var v in this.TrayMenu.Items)
			{
				try
				{
					ToolStripMenuItem f = (ToolStripMenuItem)v;
					f.Text = LocalizationExtensions.GetString($"{this.Name}.{f.Name}", f.Text);
				}
				catch
				{
				}
			}
			this.ApplyGloryNavyBrandText();
		}

		private void GeneralSettingsPanel_Paint(object sender, PaintEventArgs e)
		{

		}

		private void LanguageCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this._suppressEvents)
			{
				return;
			}
			this.ApplicationSettingsChanged?.Invoke();
			LocalizationExtensions.SetLanguage(Language);
			InitializeLanguageControls();
		}

		private void LanguageTabPage_Click(object sender, EventArgs e)
		{

		}

		private void HotkeyCaptureButton_Click(object sender, EventArgs e)
		{
			this._hotkeyCaptureActive = true;
			if (this.HotkeyCaptureButton != null)
			{
				// if you rename the object - adjust this string
				this.HotkeyCaptureButton.Text = LocalizationExtensions.GetString("MainForm.ContentTabControl.CycleGroupTabPage.HotkeyCaptureButton_PressKey", "Press key...");
			}
			if (this.HotkeyCaptureTextBox != null)
			{
				this.HotkeyCaptureTextBox.Text = string.Empty;
				this.HotkeyCaptureTextBox.Focus();
			}
		}

		private void HotkeyCaptureTextBox_MouseDown(object sender, MouseEventArgs e)
		{
			this.HotkeyCaptureTextBox.Focus();
		}

		private void HotkeyCaptureTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (!this._hotkeyCaptureActive)
			{
				// not currently capturing
				return;
			}
			e.SuppressKeyPress = true;
			e.Handled = true;

			// Filter only modifier keys and unrecognized ones. ProcessKey
			var baseKey = e.KeyCode;
			if (baseKey == Keys.ControlKey || baseKey == Keys.ShiftKey || baseKey == Keys.Menu || baseKey == Keys.ProcessKey)
			{
				return;
			}

			// Uses Microsoft's official KeysConverter to output canonical strings (invariant regions), compatible with Keys Enum.
			var combined = e.KeyData; // Includes modifier keys

			string keyText = new KeysConverter().ConvertToInvariantString(combined);
			if (string.IsNullOrWhiteSpace(keyText))
			{
				return;
			}
			this.HotkeyCaptureTextBox.Text = keyText;

			// Done
			this._hotkeyCaptureActive = false;
			if (this.HotkeyCaptureButton != null)
			{
				// if you rename the object - adjust this string
				this.HotkeyCaptureButton.Text = LocalizationExtensions.GetString("MainForm.ContentTabControl.CycleGroupTabPage.HotkeyCaptureButton", "Capture");
			}
		}

		public void BeginUpdateUI()
		{
			this._suppressEvents = true;
		}

		public void EndUpdateUI()
		{
			this._suppressEvents = false;
		}

		public void SetupConfigList()
		{
			string previousSelection = this.ProfilesComboBox.SelectedItem?.ToString();
			this._configurationFilenames.Clear();
			this.ProfilesComboBox.Items.Clear();
			this.MenuConfigurationFile.DropDownItems.Clear();
			this.MenuConfigurationFile.DropDownItems.Add(LocalizationExtensions.GetString("MainForm.MenuConfigurationFile.Reload", "Reload Configuration"));
			this.MenuConfigurationFile.DropDownItems.Add(new ToolStripSeparator());

			string defaultDisplayName = LocalizationExtensions.GetString("MainForm.MenuConfigurationFile.DEFALT", "*DEFAULT*");
			this.AddConfigurationChoice(defaultDisplayName, ConfigurationStorage.CONFIGURATION_FILE_NAME, true);

			foreach (var filename in Directory.GetFiles(".", "Eve-O-Preview*.json"))
			{
				string cleanFilename = filename.Replace(".//", "").Replace("./", "").Replace(".\\", "");
				string displayName = Path.GetFileName(cleanFilename);
				if (displayName.Equals(ConfigurationStorage.CONFIGURATION_FILE_NAME, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (!displayName.StartsWith("Eve-O-Preview-", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				displayName = displayName.Substring("Eve-O-Preview-".Length);
				displayName = Path.GetFileNameWithoutExtension(displayName);
				this.AddConfigurationChoice(displayName, cleanFilename, false);
			}

			if (!string.IsNullOrEmpty(previousSelection) && this.ProfilesComboBox.Items.Contains(previousSelection))
			{
				this.ProfilesComboBox.SelectedItem = previousSelection;
			}
			else if (this.ProfilesComboBox.Items.Count > 0)
			{
				this.ProfilesComboBox.SelectedIndex = 0;
			}
		}

		private void AddConfigurationChoice(string displayName, string filename, bool isDefault)
		{
			this._configurationFilenames[displayName] = filename;
			this.ProfilesComboBox.Items.Add(displayName);
			this.MenuConfigurationFile.DropDownItems.Add(new ToolStripMenuItem
			{
				Text = displayName,
				Checked = isDefault
			});
		}

		private void MenuConfigurationFile_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if ( e.ClickedItem.Text.Equals(LocalizationExtensions.GetString("MainForm.MenuConfigurationFile.Reload", "Reload Configuration")))
			{
				this.LoadNewSettings?.Invoke(null);
				return;
			}

			if (_configurationFilenames.ContainsKey(e.ClickedItem.Text))
			{
				var _configurationFilename = _configurationFilenames[e.ClickedItem.Text];

				foreach (var mi in this.MenuConfigurationFile.DropDownItems)
				{

					if ( mi.GetType()  == typeof(ToolStripMenuItem) )
					{
						ToolStripMenuItem menuItem = (ToolStripMenuItem)mi;
						if (menuItem.Text.Length > 0 && menuItem.Text != LocalizationExtensions.GetString("MainForm.MenuConfigurationFile.Reload", "Reload Configuration"))
						{
							menuItem.Checked = (menuItem.Text == e.ClickedItem.Text) ? true : false;
						}
					}
				}
				this.LoadNewSettings?.Invoke(_configurationFilename);
			}
		}
	}
}
