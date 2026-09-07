using System.Drawing;
using System.Windows.Forms;

namespace EveOPreview.View
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>s
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			ToolStripMenuItem RestoreWindowMenuItem;
			ToolStripMenuItem ExitMenuItem;
			ToolStripMenuItem TitleMenuItem;
			ToolStripSeparator SeparatorMenuItem;
			TabControl ContentTabControl;
			TabPage GeneralTabPage;
			Panel GeneralSettingsPanel;
			Label captionBarStyleLabel;
			Label animationStyleLabel;
			Label QuickSwitchHintLabel;
			TabPage ThumbnailTabPage;
			Panel ThumbnailSettingsPanel;
			Label HeigthLabel;
			Label WidthLabel;
			Label OpacityLabel;
			Label ThumbnailPresetLabel;
			Panel ZoomSettingsPanel;
			Label ZoomFactorLabel;
			Label ZoomAnchorLabel;
			TabPage OverlayTabPage;
			Panel OverlaySettingsPanel;
			Label CycleGroupLabel;
			TabPage ClientsTabPage;
			Panel ClientsPanel;
			Label ThumbnailsListLabel;
			TabPage AboutTabPage;
			Panel AboutPanel;
			Label CreditMaintLabel;
			Label DocumentationLinkLabel;
			Label DescriptionLabel;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			Label NameLabel;
			CaptionOnClientsStyleCombo = new ComboBox();
			AnimationStyleCombo = new ComboBox();
			MinimizeInactiveClientsCheckBox = new CheckBox();
			EnableClientLayoutTrackingCheckBox = new CheckBox();
			HideActiveClientThumbnailCheckBox = new CheckBox();
			ShowThumbnailsAlwaysOnTopCheckBox = new CheckBox();
			HideThumbnailsOnLostFocusCheckBox = new CheckBox();
			EnablePerClientThumbnailsLayoutsCheckBox = new CheckBox();
			MinimizeToTrayCheckBox = new CheckBox();
			QuickSwitchEnabledCheckBox = new CheckBox();
			QuickSwitchHintLabel = new Label();
			DoNotDisplayPreviewColour = new Label();
			PreventPreviewColorButton = new Panel();
			PreventPreviewsCheckBox = new CheckBox();
			ThumbnailSnapToGridCheckBox = new CheckBox();
			ThumbnailSnapToGridSizeYNumericEdit = new NumericUpDown();
			SnapYLabel = new Label();
			ThumbnailSnapToGridSizeXNumericEdit = new NumericUpDown();
			SnapXLabel = new Label();
			LockThumbnailLocationCheckbox = new CheckBox();
			ThumbnailsWidthNumericEdit = new NumericUpDown();
			ThumbnailsHeightNumericEdit = new NumericUpDown();
			ThumbnailPresetLabel = new Label();
			ThumbnailPresetTinyButton = new Button();
			ThumbnailPresetCompactButton = new Button();
			ThumbnailPresetStandardButton = new Button();
			ThumbnailOpacityTrackBar = new TrackBar();
			ZoomTabPage = new TabPage();
			ZoomAnchorPanel = new Panel();
			ZoomAanchorNWRadioButton = new RadioButton();
			ZoomAanchorNRadioButton = new RadioButton();
			ZoomAanchorNERadioButton = new RadioButton();
			ZoomAanchorWRadioButton = new RadioButton();
			ZoomAanchorSERadioButton = new RadioButton();
			ZoomAanchorCRadioButton = new RadioButton();
			ZoomAanchorSRadioButton = new RadioButton();
			ZoomAanchorERadioButton = new RadioButton();
			ZoomAanchorSWRadioButton = new RadioButton();
			EnableThumbnailZoomCheckBox = new CheckBox();
			ThumbnailZoomFactorNumericEdit = new NumericUpDown();
			CycleGroupIndicatorPositionLabel = new Label();
			panel2 = new Panel();
			CycleGroupIndicatorNWRadioButton = new RadioButton();
			CycleGroupIndicatorNRadioButton = new RadioButton();
			CycleGroupIndicatorNERadioButton = new RadioButton();
			CycleGroupIndicatorWRadioButton = new RadioButton();
			CycleGroupIndicatorSERadioButton = new RadioButton();
			CycleGroupIndicatorCRadioButton = new RadioButton();
			CycleGroupIndicatorSRadioButton = new RadioButton();
			CycleGroupIndicatorERadioButton = new RadioButton();
			CycleGroupIndicatorSWRadioButton = new RadioButton();
			LabelOverlayLabelFont = new Label();
			btnLabelFont = new Button();
			OverlayPositionLabel = new Label();
			OverlayLabelColourLabel = new Label();
			OverlayLabelColorButton = new Panel();
			panel1 = new Panel();
			OverlayLabelNWRadioButton = new RadioButton();
			OverlayLabelNRadioButton = new RadioButton();
			OverlayLabelNERadioButton = new RadioButton();
			OverlayLabelWRadioButton = new RadioButton();
			OverlayLabelSERadioButton = new RadioButton();
			OverlayLabelCRadioButton = new RadioButton();
			OverlayLabelSRadioButton = new RadioButton();
			OverlayLabelERadioButton = new RadioButton();
			OverlayLabelSWRadioButton = new RadioButton();
			HighlightColorLabel = new Label();
			ActiveClientHighlightColorButton = new Panel();
			EnableActiveClientHighlightCheckBox = new CheckBox();
			ShowThumbnailOverlaysCheckBox = new CheckBox();
			ShowThumbnailFramesCheckBox = new CheckBox();
			CycleGroupTabPage = new TabPage();
			CycleGroupSelectorComboBox = new ComboBox();
			HotkeysForwardLabel = new Label();
			HotkeysForwardListBox = new ListBox();
			HotkeysForwardAddButton = new Button();
			HotkeysForwardRemoveButton = new Button();
			HotkeysBackwardLabel = new Label();
			HotkeysBackwardListBox = new ListBox();
			HotkeysBackwardAddButton = new Button();
			HotkeysBackwardRemoveButton = new Button();
			HotkeysClientsList = new CheckedListBox();
			HotkeysClientUpButton = new Button();
			HotkeysClientDownButton = new Button();
			HotkeyReadmeTextBox = new TextBox();
			HotkeySaveButton = new Button();
			HotkeyCaptureTextBox = new TextBox();
			HotkeyCaptureButton = new Button();
			ThumbnailsList = new CheckedListBox();
			LanguageTabPage = new TabPage();
			LanguageLabel = new Label();
			LanguageCombo = new ComboBox();
			VersionLabel = new Label();
			DocumentationLink = new LinkLabel();
			NotifyIcon = new NotifyIcon(components);
			TrayMenu = new ContextMenuStrip(components);
			MenuConfigurationFile = new ToolStripMenuItem();
			SeparatorMenuItem2 = new ToolStripSeparator();
			defaultoolStripMenuItem = new ToolStripMenuItem();
			RestoreWindowMenuItem = new ToolStripMenuItem();
			ExitMenuItem = new ToolStripMenuItem();
			TitleMenuItem = new ToolStripMenuItem();
			SeparatorMenuItem = new ToolStripSeparator();
			ContentTabControl = new TabControl();
			GeneralTabPage = new TabPage();
			GeneralSettingsPanel = new Panel();
			captionBarStyleLabel = new Label();
			animationStyleLabel = new Label();
			ThumbnailTabPage = new TabPage();
			ThumbnailSettingsPanel = new Panel();
			HeigthLabel = new Label();
			WidthLabel = new Label();
			OpacityLabel = new Label();
			ZoomSettingsPanel = new Panel();
			ZoomFactorLabel = new Label();
			ZoomAnchorLabel = new Label();
			OverlayTabPage = new TabPage();
			OverlaySettingsPanel = new Panel();
			CycleGroupLabel = new Label();
			ClientsTabPage = new TabPage();
			ClientsPanel = new Panel();
			ThumbnailsListLabel = new Label();
			AboutTabPage = new TabPage();
			AboutPanel = new Panel();
			CreditMaintLabel = new Label();
			DocumentationLinkLabel = new Label();
			DescriptionLabel = new Label();
			NameLabel = new Label();
			ContentTabControl.SuspendLayout();
			GeneralTabPage.SuspendLayout();
			GeneralSettingsPanel.SuspendLayout();
			ThumbnailTabPage.SuspendLayout();
			ThumbnailSettingsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)ThumbnailSnapToGridSizeYNumericEdit).BeginInit();
			((System.ComponentModel.ISupportInitialize)ThumbnailSnapToGridSizeXNumericEdit).BeginInit();
			((System.ComponentModel.ISupportInitialize)ThumbnailsWidthNumericEdit).BeginInit();
			((System.ComponentModel.ISupportInitialize)ThumbnailsHeightNumericEdit).BeginInit();
			((System.ComponentModel.ISupportInitialize)ThumbnailOpacityTrackBar).BeginInit();
			ZoomTabPage.SuspendLayout();
			ZoomSettingsPanel.SuspendLayout();
			ZoomAnchorPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)ThumbnailZoomFactorNumericEdit).BeginInit();
			OverlayTabPage.SuspendLayout();
			OverlaySettingsPanel.SuspendLayout();
			panel2.SuspendLayout();
			panel1.SuspendLayout();
			CycleGroupTabPage.SuspendLayout();
			ClientsTabPage.SuspendLayout();
			ClientsPanel.SuspendLayout();
			LanguageTabPage.SuspendLayout();
			AboutTabPage.SuspendLayout();
			AboutPanel.SuspendLayout();
			TrayMenu.SuspendLayout();
			SuspendLayout();
			// 
			// RestoreWindowMenuItem
			// 
			RestoreWindowMenuItem.Name = "RestoreWindowMenuItem";
			RestoreWindowMenuItem.Size = new Size(224, 32);
			RestoreWindowMenuItem.Text = "Restore";
			RestoreWindowMenuItem.Click += RestoreMainForm_Handler;
			// 
			// ExitMenuItem
			// 
			ExitMenuItem.Name = "ExitMenuItem";
			ExitMenuItem.Size = new Size(224, 32);
			ExitMenuItem.Text = "Exit";
			ExitMenuItem.Click += ExitMenuItemClick_Handler;
			// 
			// TitleMenuItem
			// 
			TitleMenuItem.Enabled = false;
			TitleMenuItem.Name = "TitleMenuItem";
			TitleMenuItem.Size = new Size(224, 32);
			TitleMenuItem.Text = "EVE-O-Preview";
			// 
			// SeparatorMenuItem
			// 
			SeparatorMenuItem.Name = "SeparatorMenuItem";
			SeparatorMenuItem.Size = new Size(221, 6);
			// 
			// ContentTabControl
			// 
			ContentTabControl.Alignment = TabAlignment.Left;
			ContentTabControl.Controls.Add(GeneralTabPage);
			ContentTabControl.Controls.Add(ThumbnailTabPage);
			ContentTabControl.Controls.Add(ZoomTabPage);
			ContentTabControl.Controls.Add(OverlayTabPage);
			ContentTabControl.Controls.Add(CycleGroupTabPage);
			ContentTabControl.Controls.Add(ClientsTabPage);
			ContentTabControl.Controls.Add(LanguageTabPage);
			ContentTabControl.Controls.Add(AboutTabPage);
			ContentTabControl.Dock = DockStyle.Fill;
			ContentTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
			ContentTabControl.ItemSize = new Size(35, 120);
			ContentTabControl.Location = new Point(0, 0);
			ContentTabControl.Margin = new Padding(6, 7, 6, 7);
			ContentTabControl.Multiline = true;
			ContentTabControl.Name = "ContentTabControl";
			ContentTabControl.SelectedIndex = 0;
			ContentTabControl.Size = new Size(704, 480);
			ContentTabControl.SizeMode = TabSizeMode.Fixed;
			ContentTabControl.TabIndex = 6;
			ContentTabControl.DrawItem += ContentTabControl_DrawItem;
			// 
			// GeneralTabPage
			// 
			GeneralTabPage.BackColor = SystemColors.Control;
			GeneralTabPage.Controls.Add(GeneralSettingsPanel);
			GeneralTabPage.Location = new Point(124, 4);
			GeneralTabPage.Margin = new Padding(6, 7, 6, 7);
			GeneralTabPage.Name = "GeneralTabPage";
			GeneralTabPage.Padding = new Padding(6, 7, 6, 7);
			GeneralTabPage.Size = new Size(576, 472);
			GeneralTabPage.TabIndex = 0;
			GeneralTabPage.Text = "General";
			// 
			// GeneralSettingsPanel
			// 
			GeneralSettingsPanel.BorderStyle = BorderStyle.FixedSingle;
			GeneralSettingsPanel.Controls.Add(captionBarStyleLabel);
			GeneralSettingsPanel.Controls.Add(CaptionOnClientsStyleCombo);
			GeneralSettingsPanel.Controls.Add(animationStyleLabel);
			GeneralSettingsPanel.Controls.Add(AnimationStyleCombo);
			GeneralSettingsPanel.Controls.Add(MinimizeInactiveClientsCheckBox);
			GeneralSettingsPanel.Controls.Add(EnableClientLayoutTrackingCheckBox);
			GeneralSettingsPanel.Controls.Add(HideActiveClientThumbnailCheckBox);
			GeneralSettingsPanel.Controls.Add(ShowThumbnailsAlwaysOnTopCheckBox);
			GeneralSettingsPanel.Controls.Add(HideThumbnailsOnLostFocusCheckBox);
			GeneralSettingsPanel.Controls.Add(EnablePerClientThumbnailsLayoutsCheckBox);
			GeneralSettingsPanel.Controls.Add(MinimizeToTrayCheckBox);
			GeneralSettingsPanel.Controls.Add(QuickSwitchEnabledCheckBox);
			GeneralSettingsPanel.Controls.Add(QuickSwitchHintLabel);
			GeneralSettingsPanel.Dock = DockStyle.Fill;
			GeneralSettingsPanel.Location = new Point(6, 7);
			GeneralSettingsPanel.Margin = new Padding(6, 7, 6, 7);
			GeneralSettingsPanel.Name = "GeneralSettingsPanel";
			GeneralSettingsPanel.Size = new Size(564, 458);
			GeneralSettingsPanel.TabIndex = 18;
			GeneralSettingsPanel.Paint += GeneralSettingsPanel_Paint;
			// 
			// captionBarStyleLabel
			// 
			captionBarStyleLabel.AutoSize = true;
			captionBarStyleLabel.Location = new Point(9, 205);
			captionBarStyleLabel.Margin = new Padding(6, 0, 6, 0);
			captionBarStyleLabel.Name = "captionBarStyleLabel";
			captionBarStyleLabel.Size = new Size(142, 25);
			captionBarStyleLabel.TabIndex = 30;
			captionBarStyleLabel.Text = "Captionbar Style";
			// 
			// CaptionOnClientsStyleCombo
			// 
			CaptionOnClientsStyleCombo.DropDownStyle = ComboBoxStyle.DropDownList;
			CaptionOnClientsStyleCombo.FormattingEnabled = true;
			CaptionOnClientsStyleCombo.Location = new Point(244, 200);
			CaptionOnClientsStyleCombo.Margin = new Padding(6, 7, 6, 7);
			CaptionOnClientsStyleCombo.Name = "CaptionOnClientsStyleCombo";
			CaptionOnClientsStyleCombo.Size = new Size(201, 33);
			CaptionOnClientsStyleCombo.TabIndex = 29;
			CaptionOnClientsStyleCombo.SelectedIndexChanged += OptionChanged_Handler;
			// 
			// animationStyleLabel
			// 
			animationStyleLabel.AutoSize = true;
			animationStyleLabel.Location = new Point(9, 160);
			animationStyleLabel.Margin = new Padding(6, 0, 6, 0);
			animationStyleLabel.Name = "animationStyleLabel";
			animationStyleLabel.Size = new Size(136, 25);
			animationStyleLabel.TabIndex = 27;
			animationStyleLabel.Text = "Animation Style";
			// 
			// AnimationStyleCombo
			// 
			AnimationStyleCombo.DropDownStyle = ComboBoxStyle.DropDownList;
			AnimationStyleCombo.FormattingEnabled = true;
			AnimationStyleCombo.Location = new Point(244, 155);
			AnimationStyleCombo.Margin = new Padding(6, 7, 6, 7);
			AnimationStyleCombo.Name = "AnimationStyleCombo";
			AnimationStyleCombo.Size = new Size(201, 33);
			AnimationStyleCombo.TabIndex = 26;
			AnimationStyleCombo.SelectedIndexChanged += OptionChanged_Handler;
			// 
			// MinimizeInactiveClientsCheckBox
			// 
			MinimizeInactiveClientsCheckBox.AutoSize = true;
			MinimizeInactiveClientsCheckBox.Location = new Point(13, 122);
			MinimizeInactiveClientsCheckBox.Margin = new Padding(6, 7, 6, 7);
			MinimizeInactiveClientsCheckBox.Name = "MinimizeInactiveClientsCheckBox";
			MinimizeInactiveClientsCheckBox.Size = new Size(261, 29);
			MinimizeInactiveClientsCheckBox.TabIndex = 24;
			MinimizeInactiveClientsCheckBox.Text = "Minimize inactive EVE clients";
			MinimizeInactiveClientsCheckBox.UseVisualStyleBackColor = true;
			MinimizeInactiveClientsCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// EnableClientLayoutTrackingCheckBox
			// 
			EnableClientLayoutTrackingCheckBox.AutoSize = true;
			EnableClientLayoutTrackingCheckBox.Location = new Point(13, 50);
			EnableClientLayoutTrackingCheckBox.Margin = new Padding(6, 7, 6, 7);
			EnableClientLayoutTrackingCheckBox.Name = "EnableClientLayoutTrackingCheckBox";
			EnableClientLayoutTrackingCheckBox.Size = new Size(199, 29);
			EnableClientLayoutTrackingCheckBox.TabIndex = 19;
			EnableClientLayoutTrackingCheckBox.Text = "Track client locations";
			EnableClientLayoutTrackingCheckBox.UseVisualStyleBackColor = true;
			EnableClientLayoutTrackingCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// HideActiveClientThumbnailCheckBox
			// 
			HideActiveClientThumbnailCheckBox.AutoSize = true;
			HideActiveClientThumbnailCheckBox.Checked = true;
			HideActiveClientThumbnailCheckBox.CheckState = CheckState.Checked;
			HideActiveClientThumbnailCheckBox.Location = new Point(13, 87);
			HideActiveClientThumbnailCheckBox.Margin = new Padding(6, 7, 6, 7);
			HideActiveClientThumbnailCheckBox.Name = "HideActiveClientThumbnailCheckBox";
			HideActiveClientThumbnailCheckBox.Size = new Size(293, 29);
			HideActiveClientThumbnailCheckBox.TabIndex = 20;
			HideActiveClientThumbnailCheckBox.Text = "Hide preview of active EVE client";
			HideActiveClientThumbnailCheckBox.UseVisualStyleBackColor = true;
			HideActiveClientThumbnailCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// ShowThumbnailsAlwaysOnTopCheckBox
			// 
			ShowThumbnailsAlwaysOnTopCheckBox.AutoSize = true;
			ShowThumbnailsAlwaysOnTopCheckBox.Checked = true;
			ShowThumbnailsAlwaysOnTopCheckBox.CheckState = CheckState.Checked;
			ShowThumbnailsAlwaysOnTopCheckBox.Location = new Point(13, 237);
			ShowThumbnailsAlwaysOnTopCheckBox.Margin = new Padding(6, 7, 6, 7);
			ShowThumbnailsAlwaysOnTopCheckBox.Name = "ShowThumbnailsAlwaysOnTopCheckBox";
			ShowThumbnailsAlwaysOnTopCheckBox.RightToLeft = RightToLeft.No;
			ShowThumbnailsAlwaysOnTopCheckBox.Size = new Size(222, 29);
			ShowThumbnailsAlwaysOnTopCheckBox.TabIndex = 21;
			ShowThumbnailsAlwaysOnTopCheckBox.Text = "Previews always on top";
			ShowThumbnailsAlwaysOnTopCheckBox.UseVisualStyleBackColor = true;
			ShowThumbnailsAlwaysOnTopCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// HideThumbnailsOnLostFocusCheckBox
			// 
			HideThumbnailsOnLostFocusCheckBox.AutoSize = true;
			HideThumbnailsOnLostFocusCheckBox.Checked = true;
			HideThumbnailsOnLostFocusCheckBox.CheckState = CheckState.Checked;
			HideThumbnailsOnLostFocusCheckBox.Location = new Point(13, 272);
			HideThumbnailsOnLostFocusCheckBox.Margin = new Padding(6, 7, 6, 7);
			HideThumbnailsOnLostFocusCheckBox.Name = "HideThumbnailsOnLostFocusCheckBox";
			HideThumbnailsOnLostFocusCheckBox.Size = new Size(375, 29);
			HideThumbnailsOnLostFocusCheckBox.TabIndex = 22;
			HideThumbnailsOnLostFocusCheckBox.Text = "Hide previews when EVE client is not active";
			HideThumbnailsOnLostFocusCheckBox.UseVisualStyleBackColor = true;
			HideThumbnailsOnLostFocusCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// EnablePerClientThumbnailsLayoutsCheckBox
			// 
			EnablePerClientThumbnailsLayoutsCheckBox.AutoSize = true;
			EnablePerClientThumbnailsLayoutsCheckBox.Checked = true;
			EnablePerClientThumbnailsLayoutsCheckBox.CheckState = CheckState.Checked;
			EnablePerClientThumbnailsLayoutsCheckBox.Location = new Point(13, 308);
			EnablePerClientThumbnailsLayoutsCheckBox.Margin = new Padding(6, 7, 6, 7);
			EnablePerClientThumbnailsLayoutsCheckBox.Name = "EnablePerClientThumbnailsLayoutsCheckBox";
			EnablePerClientThumbnailsLayoutsCheckBox.Size = new Size(297, 29);
			EnablePerClientThumbnailsLayoutsCheckBox.TabIndex = 23;
			EnablePerClientThumbnailsLayoutsCheckBox.Text = "Unique layout for each EVE client";
			EnablePerClientThumbnailsLayoutsCheckBox.UseVisualStyleBackColor = true;
			EnablePerClientThumbnailsLayoutsCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// MinimizeToTrayCheckBox
			// 
			MinimizeToTrayCheckBox.AutoSize = true;
			MinimizeToTrayCheckBox.Location = new Point(13, 13);
			MinimizeToTrayCheckBox.Margin = new Padding(6, 7, 6, 7);
			MinimizeToTrayCheckBox.Name = "MinimizeToTrayCheckBox";
			MinimizeToTrayCheckBox.Size = new Size(229, 29);
			MinimizeToTrayCheckBox.TabIndex = 18;
			MinimizeToTrayCheckBox.Text = "Minimize to System Tray";
			MinimizeToTrayCheckBox.UseVisualStyleBackColor = true;
			MinimizeToTrayCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// QuickSwitchEnabledCheckBox
			// 
			QuickSwitchEnabledCheckBox.AutoSize = true;
			QuickSwitchEnabledCheckBox.Checked = true;
			QuickSwitchEnabledCheckBox.CheckState = CheckState.Checked;
			QuickSwitchEnabledCheckBox.Location = new Point(13, 351);
			QuickSwitchEnabledCheckBox.Margin = new Padding(6, 7, 6, 7);
			QuickSwitchEnabledCheckBox.Name = "QuickSwitchEnabledCheckBox";
			QuickSwitchEnabledCheckBox.Size = new Size(338, 29);
			QuickSwitchEnabledCheckBox.TabIndex = 31;
			QuickSwitchEnabledCheckBox.Text = "Quick switch (Ctrl+Alt+Left / Right)";
			QuickSwitchEnabledCheckBox.UseVisualStyleBackColor = true;
			QuickSwitchEnabledCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// QuickSwitchHintLabel
			// 
			QuickSwitchHintLabel.AutoSize = true;
			QuickSwitchHintLabel.ForeColor = SystemColors.GrayText;
			QuickSwitchHintLabel.Location = new Point(35, 388);
			QuickSwitchHintLabel.Margin = new Padding(6, 0, 6, 0);
			QuickSwitchHintLabel.Name = "QuickSwitchHintLabel";
			QuickSwitchHintLabel.Size = new Size(387, 25);
			QuickSwitchHintLabel.TabIndex = 32;
			QuickSwitchHintLabel.Text = "Switches the foreground client only; no input broadcast.";
			// 
			// ThumbnailTabPage
			// 
			ThumbnailTabPage.BackColor = SystemColors.Control;
			ThumbnailTabPage.Controls.Add(ThumbnailSettingsPanel);
			ThumbnailTabPage.Location = new Point(124, 4);
			ThumbnailTabPage.Margin = new Padding(6, 7, 6, 7);
			ThumbnailTabPage.Name = "ThumbnailTabPage";
			ThumbnailTabPage.Padding = new Padding(6, 7, 6, 7);
			ThumbnailTabPage.Size = new Size(576, 472);
			ThumbnailTabPage.TabIndex = 1;
			ThumbnailTabPage.Text = "Thumbnail";
			// 
			// ThumbnailSettingsPanel
			// 
			ThumbnailSettingsPanel.BorderStyle = BorderStyle.FixedSingle;
			ThumbnailSettingsPanel.Controls.Add(DoNotDisplayPreviewColour);
			ThumbnailSettingsPanel.Controls.Add(PreventPreviewColorButton);
			ThumbnailSettingsPanel.Controls.Add(PreventPreviewsCheckBox);
			ThumbnailSettingsPanel.Controls.Add(ThumbnailSnapToGridCheckBox);
			ThumbnailSettingsPanel.Controls.Add(ThumbnailSnapToGridSizeYNumericEdit);
			ThumbnailSettingsPanel.Controls.Add(SnapYLabel);
			ThumbnailSettingsPanel.Controls.Add(ThumbnailSnapToGridSizeXNumericEdit);
			ThumbnailSettingsPanel.Controls.Add(SnapXLabel);
			ThumbnailSettingsPanel.Controls.Add(LockThumbnailLocationCheckbox);
			ThumbnailSettingsPanel.Controls.Add(HeigthLabel);
			ThumbnailSettingsPanel.Controls.Add(WidthLabel);
			ThumbnailSettingsPanel.Controls.Add(ThumbnailsWidthNumericEdit);
			ThumbnailSettingsPanel.Controls.Add(ThumbnailsHeightNumericEdit);
			ThumbnailSettingsPanel.Controls.Add(ThumbnailPresetLabel);
			ThumbnailSettingsPanel.Controls.Add(ThumbnailPresetTinyButton);
			ThumbnailSettingsPanel.Controls.Add(ThumbnailPresetCompactButton);
			ThumbnailSettingsPanel.Controls.Add(ThumbnailPresetStandardButton);
			ThumbnailSettingsPanel.Controls.Add(ThumbnailOpacityTrackBar);
			ThumbnailSettingsPanel.Controls.Add(OpacityLabel);
			ThumbnailSettingsPanel.Dock = DockStyle.Fill;
			ThumbnailSettingsPanel.Location = new Point(6, 7);
			ThumbnailSettingsPanel.Margin = new Padding(6, 7, 6, 7);
			ThumbnailSettingsPanel.Name = "ThumbnailSettingsPanel";
			ThumbnailSettingsPanel.Size = new Size(564, 458);
			ThumbnailSettingsPanel.TabIndex = 19;
			// 
			// DoNotDisplayPreviewColour
			// 
			DoNotDisplayPreviewColour.AutoSize = true;
			DoNotDisplayPreviewColour.Location = new Point(250, 325);
			DoNotDisplayPreviewColour.Margin = new Padding(6, 0, 6, 0);
			DoNotDisplayPreviewColour.Name = "DoNotDisplayPreviewColour";
			DoNotDisplayPreviewColour.Size = new Size(55, 25);
			DoNotDisplayPreviewColour.TabIndex = 35;
			DoNotDisplayPreviewColour.Text = "Color";
			// 
			// PreventPreviewColorButton
			// 
			PreventPreviewColorButton.BorderStyle = BorderStyle.FixedSingle;
			PreventPreviewColorButton.Location = new Point(359, 323);
			PreventPreviewColorButton.Margin = new Padding(6, 7, 6, 7);
			PreventPreviewColorButton.Name = "PreventPreviewColorButton";
			PreventPreviewColorButton.Size = new Size(82, 30);
			PreventPreviewColorButton.TabIndex = 34;
			PreventPreviewColorButton.Click += PreventPreviewColorButton_Click;
			// 
			// PreventPreviewsCheckBox
			// 
			PreventPreviewsCheckBox.AutoSize = true;
			PreventPreviewsCheckBox.Location = new Point(19, 323);
			PreventPreviewsCheckBox.Margin = new Padding(6, 7, 6, 7);
			PreventPreviewsCheckBox.Name = "PreventPreviewsCheckBox";
			PreventPreviewsCheckBox.Size = new Size(229, 29);
			PreventPreviewsCheckBox.TabIndex = 33;
			PreventPreviewsCheckBox.Text = "Do not display previews";
			PreventPreviewsCheckBox.UseVisualStyleBackColor = true;
			PreventPreviewsCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// ThumbnailSnapToGridCheckBox
			// 
			ThumbnailSnapToGridCheckBox.AutoSize = true;
			ThumbnailSnapToGridCheckBox.Location = new Point(19, 243);
			ThumbnailSnapToGridCheckBox.Margin = new Padding(6, 7, 6, 7);
			ThumbnailSnapToGridCheckBox.Name = "ThumbnailSnapToGridCheckBox";
			ThumbnailSnapToGridCheckBox.Size = new Size(226, 29);
			ThumbnailSnapToGridCheckBox.TabIndex = 32;
			ThumbnailSnapToGridCheckBox.Text = "Thumbnail Snap to Grid";
			ThumbnailSnapToGridCheckBox.UseVisualStyleBackColor = true;
			ThumbnailSnapToGridCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// ThumbnailSnapToGridSizeYNumericEdit
			// 
			ThumbnailSnapToGridSizeYNumericEdit.BackColor = SystemColors.Window;
			ThumbnailSnapToGridSizeYNumericEdit.BorderStyle = BorderStyle.FixedSingle;
			ThumbnailSnapToGridSizeYNumericEdit.CausesValidation = false;
			ThumbnailSnapToGridSizeYNumericEdit.Increment = new decimal(new int[] { 10, 0, 0, 0 });
			ThumbnailSnapToGridSizeYNumericEdit.Location = new Point(247, 278);
			ThumbnailSnapToGridSizeYNumericEdit.Margin = new Padding(6, 7, 6, 7);
			ThumbnailSnapToGridSizeYNumericEdit.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
			ThumbnailSnapToGridSizeYNumericEdit.Name = "ThumbnailSnapToGridSizeYNumericEdit";
			ThumbnailSnapToGridSizeYNumericEdit.Size = new Size(80, 31);
			ThumbnailSnapToGridSizeYNumericEdit.TabIndex = 31;
			ThumbnailSnapToGridSizeYNumericEdit.Value = new decimal(new int[] { 100, 0, 0, 0 });
			ThumbnailSnapToGridSizeYNumericEdit.ValueChanged += OptionChanged_Handler;
			// 
			// SnapYLabel
			// 
			SnapYLabel.AutoSize = true;
			SnapYLabel.Location = new Point(213, 281);
			SnapYLabel.Margin = new Padding(6, 0, 6, 0);
			SnapYLabel.Name = "SnapYLabel";
			SnapYLabel.Size = new Size(22, 25);
			SnapYLabel.TabIndex = 30;
			SnapYLabel.Text = "Y";
			// 
			// ThumbnailSnapToGridSizeXNumericEdit
			// 
			ThumbnailSnapToGridSizeXNumericEdit.BackColor = SystemColors.Window;
			ThumbnailSnapToGridSizeXNumericEdit.BorderStyle = BorderStyle.FixedSingle;
			ThumbnailSnapToGridSizeXNumericEdit.CausesValidation = false;
			ThumbnailSnapToGridSizeXNumericEdit.Increment = new decimal(new int[] { 10, 0, 0, 0 });
			ThumbnailSnapToGridSizeXNumericEdit.Location = new Point(123, 278);
			ThumbnailSnapToGridSizeXNumericEdit.Margin = new Padding(6, 7, 6, 7);
			ThumbnailSnapToGridSizeXNumericEdit.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
			ThumbnailSnapToGridSizeXNumericEdit.Name = "ThumbnailSnapToGridSizeXNumericEdit";
			ThumbnailSnapToGridSizeXNumericEdit.Size = new Size(80, 31);
			ThumbnailSnapToGridSizeXNumericEdit.TabIndex = 29;
			ThumbnailSnapToGridSizeXNumericEdit.Value = new decimal(new int[] { 100, 0, 0, 0 });
			ThumbnailSnapToGridSizeXNumericEdit.ValueChanged += OptionChanged_Handler;
			// 
			// SnapXLabel
			// 
			SnapXLabel.AutoSize = true;
			SnapXLabel.Location = new Point(13, 281);
			SnapXLabel.Margin = new Padding(6, 0, 6, 0);
			SnapXLabel.Name = "SnapXLabel";
			SnapXLabel.Size = new Size(68, 25);
			SnapXLabel.TabIndex = 28;
			SnapXLabel.Text = "Snap X";
			// 
			// LockThumbnailLocationCheckbox
			// 
			LockThumbnailLocationCheckbox.AutoSize = true;
			LockThumbnailLocationCheckbox.Location = new Point(19, 200);
			LockThumbnailLocationCheckbox.Margin = new Padding(6, 7, 6, 7);
			LockThumbnailLocationCheckbox.Name = "LockThumbnailLocationCheckbox";
			LockThumbnailLocationCheckbox.Size = new Size(234, 29);
			LockThumbnailLocationCheckbox.TabIndex = 26;
			LockThumbnailLocationCheckbox.Text = "Lock Thumbnail Location";
			LockThumbnailLocationCheckbox.UseVisualStyleBackColor = true;
			LockThumbnailLocationCheckbox.CheckedChanged += OptionChanged_Handler;
			// 
			// HeigthLabel
			// 
			HeigthLabel.AutoSize = true;
			HeigthLabel.Location = new Point(13, 110);
			HeigthLabel.Margin = new Padding(6, 0, 6, 0);
			HeigthLabel.Name = "HeigthLabel";
			HeigthLabel.Size = new Size(153, 25);
			HeigthLabel.TabIndex = 24;
			HeigthLabel.Text = "Thumbnail Height";
			// 
			// WidthLabel
			// 
			WidthLabel.AutoSize = true;
			WidthLabel.Location = new Point(13, 63);
			WidthLabel.Margin = new Padding(6, 0, 6, 0);
			WidthLabel.Name = "WidthLabel";
			WidthLabel.Size = new Size(148, 25);
			WidthLabel.TabIndex = 23;
			WidthLabel.Text = "Thumbnail Width";
			// 
			// ThumbnailsWidthNumericEdit
			// 
			ThumbnailsWidthNumericEdit.BackColor = SystemColors.Window;
			ThumbnailsWidthNumericEdit.BorderStyle = BorderStyle.FixedSingle;
			ThumbnailsWidthNumericEdit.CausesValidation = false;
			ThumbnailsWidthNumericEdit.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			ThumbnailsWidthNumericEdit.Location = new Point(197, 60);
			ThumbnailsWidthNumericEdit.Margin = new Padding(6, 7, 6, 7);
			ThumbnailsWidthNumericEdit.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
			ThumbnailsWidthNumericEdit.Name = "ThumbnailsWidthNumericEdit";
			ThumbnailsWidthNumericEdit.Size = new Size(80, 31);
			ThumbnailsWidthNumericEdit.TabIndex = 21;
			ThumbnailsWidthNumericEdit.Value = new decimal(new int[] { 100, 0, 0, 0 });
			ThumbnailsWidthNumericEdit.ValueChanged += ThumbnailSizeChanged_Handler;
			// 
			// ThumbnailsHeightNumericEdit
			// 
			ThumbnailsHeightNumericEdit.BackColor = SystemColors.Window;
			ThumbnailsHeightNumericEdit.BorderStyle = BorderStyle.FixedSingle;
			ThumbnailsHeightNumericEdit.CausesValidation = false;
			ThumbnailsHeightNumericEdit.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			ThumbnailsHeightNumericEdit.Location = new Point(197, 107);
			ThumbnailsHeightNumericEdit.Margin = new Padding(6, 7, 6, 7);
			ThumbnailsHeightNumericEdit.Maximum = new decimal(new int[] { 99999999, 0, 0, 0 });
			ThumbnailsHeightNumericEdit.Name = "ThumbnailsHeightNumericEdit";
			ThumbnailsHeightNumericEdit.Size = new Size(80, 31);
			ThumbnailsHeightNumericEdit.TabIndex = 22;
			ThumbnailsHeightNumericEdit.Value = new decimal(new int[] { 70, 0, 0, 0 });
			ThumbnailsHeightNumericEdit.ValueChanged += ThumbnailSizeChanged_Handler;
			// 
			// ThumbnailPresetLabel
			// 
			ThumbnailPresetLabel.AutoSize = true;
			ThumbnailPresetLabel.Location = new Point(13, 157);
			ThumbnailPresetLabel.Margin = new Padding(6, 0, 6, 0);
			ThumbnailPresetLabel.Name = "ThumbnailPresetLabel";
			ThumbnailPresetLabel.Size = new Size(72, 25);
			ThumbnailPresetLabel.TabIndex = 36;
			ThumbnailPresetLabel.Text = "Presets";
			// 
			// ThumbnailPresetTinyButton
			// 
			ThumbnailPresetTinyButton.Location = new Point(100, 149);
			ThumbnailPresetTinyButton.Name = "ThumbnailPresetTinyButton";
			ThumbnailPresetTinyButton.Size = new Size(105, 37);
			ThumbnailPresetTinyButton.TabIndex = 37;
			ThumbnailPresetTinyButton.Text = "96x54";
			ThumbnailPresetTinyButton.UseVisualStyleBackColor = true;
			ThumbnailPresetTinyButton.Click += ThumbnailPresetTinyButton_Click;
			// 
			// ThumbnailPresetCompactButton
			// 
			ThumbnailPresetCompactButton.Location = new Point(215, 149);
			ThumbnailPresetCompactButton.Name = "ThumbnailPresetCompactButton";
			ThumbnailPresetCompactButton.Size = new Size(115, 37);
			ThumbnailPresetCompactButton.TabIndex = 38;
			ThumbnailPresetCompactButton.Text = "128x72";
			ThumbnailPresetCompactButton.UseVisualStyleBackColor = true;
			ThumbnailPresetCompactButton.Click += ThumbnailPresetCompactButton_Click;
			// 
			// ThumbnailPresetStandardButton
			// 
			ThumbnailPresetStandardButton.Location = new Point(340, 149);
			ThumbnailPresetStandardButton.Name = "ThumbnailPresetStandardButton";
			ThumbnailPresetStandardButton.Size = new Size(120, 37);
			ThumbnailPresetStandardButton.TabIndex = 39;
			ThumbnailPresetStandardButton.Text = "192x108";
			ThumbnailPresetStandardButton.UseVisualStyleBackColor = true;
			ThumbnailPresetStandardButton.Click += ThumbnailPresetStandardButton_Click;
			// 
			// ThumbnailOpacityTrackBar
			// 
			ThumbnailOpacityTrackBar.AutoSize = false;
			ThumbnailOpacityTrackBar.LargeChange = 10;
			ThumbnailOpacityTrackBar.Location = new Point(101, 12);
			ThumbnailOpacityTrackBar.Margin = new Padding(6, 7, 6, 7);
			ThumbnailOpacityTrackBar.Maximum = 100;
			ThumbnailOpacityTrackBar.Minimum = 20;
			ThumbnailOpacityTrackBar.Name = "ThumbnailOpacityTrackBar";
			ThumbnailOpacityTrackBar.Size = new Size(319, 42);
			ThumbnailOpacityTrackBar.TabIndex = 20;
			ThumbnailOpacityTrackBar.TickFrequency = 10;
			ThumbnailOpacityTrackBar.Value = 20;
			ThumbnailOpacityTrackBar.ValueChanged += OptionChanged_Handler;
			// 
			// OpacityLabel
			// 
			OpacityLabel.AutoSize = true;
			OpacityLabel.Location = new Point(13, 17);
			OpacityLabel.Margin = new Padding(6, 0, 6, 0);
			OpacityLabel.Name = "OpacityLabel";
			OpacityLabel.Size = new Size(73, 25);
			OpacityLabel.TabIndex = 19;
			OpacityLabel.Text = "Opacity";
			// 
			// ZoomTabPage
			// 
			ZoomTabPage.BackColor = SystemColors.Control;
			ZoomTabPage.Controls.Add(ZoomSettingsPanel);
			ZoomTabPage.Location = new Point(124, 4);
			ZoomTabPage.Margin = new Padding(6, 7, 6, 7);
			ZoomTabPage.Name = "ZoomTabPage";
			ZoomTabPage.Size = new Size(576, 472);
			ZoomTabPage.TabIndex = 2;
			ZoomTabPage.Text = "Zoom";
			// 
			// ZoomSettingsPanel
			// 
			ZoomSettingsPanel.BorderStyle = BorderStyle.FixedSingle;
			ZoomSettingsPanel.Controls.Add(ZoomFactorLabel);
			ZoomSettingsPanel.Controls.Add(ZoomAnchorPanel);
			ZoomSettingsPanel.Controls.Add(ZoomAnchorLabel);
			ZoomSettingsPanel.Controls.Add(EnableThumbnailZoomCheckBox);
			ZoomSettingsPanel.Controls.Add(ThumbnailZoomFactorNumericEdit);
			ZoomSettingsPanel.Dock = DockStyle.Fill;
			ZoomSettingsPanel.Location = new Point(0, 0);
			ZoomSettingsPanel.Margin = new Padding(6, 7, 6, 7);
			ZoomSettingsPanel.Name = "ZoomSettingsPanel";
			ZoomSettingsPanel.Size = new Size(576, 472);
			ZoomSettingsPanel.TabIndex = 36;
			// 
			// ZoomFactorLabel
			// 
			ZoomFactorLabel.AutoSize = true;
			ZoomFactorLabel.Location = new Point(13, 63);
			ZoomFactorLabel.Margin = new Padding(6, 0, 6, 0);
			ZoomFactorLabel.Name = "ZoomFactorLabel";
			ZoomFactorLabel.Size = new Size(113, 25);
			ZoomFactorLabel.TabIndex = 39;
			ZoomFactorLabel.Text = "Zoom Factor";
			// 
			// ZoomAnchorPanel
			// 
			ZoomAnchorPanel.BorderStyle = BorderStyle.FixedSingle;
			ZoomAnchorPanel.Controls.Add(ZoomAanchorNWRadioButton);
			ZoomAnchorPanel.Controls.Add(ZoomAanchorNRadioButton);
			ZoomAnchorPanel.Controls.Add(ZoomAanchorNERadioButton);
			ZoomAnchorPanel.Controls.Add(ZoomAanchorWRadioButton);
			ZoomAnchorPanel.Controls.Add(ZoomAanchorSERadioButton);
			ZoomAnchorPanel.Controls.Add(ZoomAanchorCRadioButton);
			ZoomAnchorPanel.Controls.Add(ZoomAanchorSRadioButton);
			ZoomAnchorPanel.Controls.Add(ZoomAanchorERadioButton);
			ZoomAnchorPanel.Controls.Add(ZoomAanchorSWRadioButton);
			ZoomAnchorPanel.Location = new Point(160, 103);
			ZoomAnchorPanel.Margin = new Padding(6, 7, 6, 7);
			ZoomAnchorPanel.Name = "ZoomAnchorPanel";
			ZoomAnchorPanel.Size = new Size(128, 139);
			ZoomAnchorPanel.TabIndex = 38;
			// 
			// ZoomAanchorNWRadioButton
			// 
			ZoomAanchorNWRadioButton.AutoSize = true;
			ZoomAanchorNWRadioButton.Location = new Point(6, 7);
			ZoomAanchorNWRadioButton.Margin = new Padding(6, 7, 6, 7);
			ZoomAanchorNWRadioButton.Name = "ZoomAanchorNWRadioButton";
			ZoomAanchorNWRadioButton.Size = new Size(21, 20);
			ZoomAanchorNWRadioButton.TabIndex = 0;
			ZoomAanchorNWRadioButton.TabStop = true;
			ZoomAanchorNWRadioButton.UseVisualStyleBackColor = true;
			ZoomAanchorNWRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// ZoomAanchorNRadioButton
			// 
			ZoomAanchorNRadioButton.AutoSize = true;
			ZoomAanchorNRadioButton.Location = new Point(51, 7);
			ZoomAanchorNRadioButton.Margin = new Padding(6, 7, 6, 7);
			ZoomAanchorNRadioButton.Name = "ZoomAanchorNRadioButton";
			ZoomAanchorNRadioButton.Size = new Size(21, 20);
			ZoomAanchorNRadioButton.TabIndex = 1;
			ZoomAanchorNRadioButton.TabStop = true;
			ZoomAanchorNRadioButton.UseVisualStyleBackColor = true;
			ZoomAanchorNRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// ZoomAanchorNERadioButton
			// 
			ZoomAanchorNERadioButton.AutoSize = true;
			ZoomAanchorNERadioButton.Location = new Point(99, 7);
			ZoomAanchorNERadioButton.Margin = new Padding(6, 7, 6, 7);
			ZoomAanchorNERadioButton.Name = "ZoomAanchorNERadioButton";
			ZoomAanchorNERadioButton.Size = new Size(21, 20);
			ZoomAanchorNERadioButton.TabIndex = 2;
			ZoomAanchorNERadioButton.TabStop = true;
			ZoomAanchorNERadioButton.UseVisualStyleBackColor = true;
			ZoomAanchorNERadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// ZoomAanchorWRadioButton
			// 
			ZoomAanchorWRadioButton.AutoSize = true;
			ZoomAanchorWRadioButton.Location = new Point(6, 57);
			ZoomAanchorWRadioButton.Margin = new Padding(6, 7, 6, 7);
			ZoomAanchorWRadioButton.Name = "ZoomAanchorWRadioButton";
			ZoomAanchorWRadioButton.Size = new Size(21, 20);
			ZoomAanchorWRadioButton.TabIndex = 3;
			ZoomAanchorWRadioButton.TabStop = true;
			ZoomAanchorWRadioButton.UseVisualStyleBackColor = true;
			ZoomAanchorWRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// ZoomAanchorSERadioButton
			// 
			ZoomAanchorSERadioButton.AutoSize = true;
			ZoomAanchorSERadioButton.Location = new Point(99, 107);
			ZoomAanchorSERadioButton.Margin = new Padding(6, 7, 6, 7);
			ZoomAanchorSERadioButton.Name = "ZoomAanchorSERadioButton";
			ZoomAanchorSERadioButton.Size = new Size(21, 20);
			ZoomAanchorSERadioButton.TabIndex = 8;
			ZoomAanchorSERadioButton.TabStop = true;
			ZoomAanchorSERadioButton.UseVisualStyleBackColor = true;
			ZoomAanchorSERadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// ZoomAanchorCRadioButton
			// 
			ZoomAanchorCRadioButton.AutoSize = true;
			ZoomAanchorCRadioButton.Location = new Point(51, 57);
			ZoomAanchorCRadioButton.Margin = new Padding(6, 7, 6, 7);
			ZoomAanchorCRadioButton.Name = "ZoomAanchorCRadioButton";
			ZoomAanchorCRadioButton.Size = new Size(21, 20);
			ZoomAanchorCRadioButton.TabIndex = 4;
			ZoomAanchorCRadioButton.TabStop = true;
			ZoomAanchorCRadioButton.UseVisualStyleBackColor = true;
			ZoomAanchorCRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// ZoomAanchorSRadioButton
			// 
			ZoomAanchorSRadioButton.AutoSize = true;
			ZoomAanchorSRadioButton.Location = new Point(51, 107);
			ZoomAanchorSRadioButton.Margin = new Padding(6, 7, 6, 7);
			ZoomAanchorSRadioButton.Name = "ZoomAanchorSRadioButton";
			ZoomAanchorSRadioButton.Size = new Size(21, 20);
			ZoomAanchorSRadioButton.TabIndex = 7;
			ZoomAanchorSRadioButton.TabStop = true;
			ZoomAanchorSRadioButton.UseVisualStyleBackColor = true;
			ZoomAanchorSRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// ZoomAanchorERadioButton
			// 
			ZoomAanchorERadioButton.AutoSize = true;
			ZoomAanchorERadioButton.Location = new Point(99, 57);
			ZoomAanchorERadioButton.Margin = new Padding(6, 7, 6, 7);
			ZoomAanchorERadioButton.Name = "ZoomAanchorERadioButton";
			ZoomAanchorERadioButton.Size = new Size(21, 20);
			ZoomAanchorERadioButton.TabIndex = 5;
			ZoomAanchorERadioButton.TabStop = true;
			ZoomAanchorERadioButton.UseVisualStyleBackColor = true;
			ZoomAanchorERadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// ZoomAanchorSWRadioButton
			// 
			ZoomAanchorSWRadioButton.AutoSize = true;
			ZoomAanchorSWRadioButton.Location = new Point(6, 107);
			ZoomAanchorSWRadioButton.Margin = new Padding(6, 7, 6, 7);
			ZoomAanchorSWRadioButton.Name = "ZoomAanchorSWRadioButton";
			ZoomAanchorSWRadioButton.Size = new Size(21, 20);
			ZoomAanchorSWRadioButton.TabIndex = 6;
			ZoomAanchorSWRadioButton.TabStop = true;
			ZoomAanchorSWRadioButton.UseVisualStyleBackColor = true;
			ZoomAanchorSWRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// ZoomAnchorLabel
			// 
			ZoomAnchorLabel.AutoSize = true;
			ZoomAnchorLabel.Location = new Point(13, 110);
			ZoomAnchorLabel.Margin = new Padding(6, 0, 6, 0);
			ZoomAnchorLabel.Name = "ZoomAnchorLabel";
			ZoomAnchorLabel.Size = new Size(69, 25);
			ZoomAnchorLabel.TabIndex = 40;
			ZoomAnchorLabel.Text = "Anchor";
			// 
			// EnableThumbnailZoomCheckBox
			// 
			EnableThumbnailZoomCheckBox.AutoSize = true;
			EnableThumbnailZoomCheckBox.Checked = true;
			EnableThumbnailZoomCheckBox.CheckState = CheckState.Checked;
			EnableThumbnailZoomCheckBox.Location = new Point(13, 13);
			EnableThumbnailZoomCheckBox.Margin = new Padding(6, 7, 6, 7);
			EnableThumbnailZoomCheckBox.Name = "EnableThumbnailZoomCheckBox";
			EnableThumbnailZoomCheckBox.RightToLeft = RightToLeft.No;
			EnableThumbnailZoomCheckBox.Size = new Size(162, 29);
			EnableThumbnailZoomCheckBox.TabIndex = 36;
			EnableThumbnailZoomCheckBox.Text = "Zoom on hover";
			EnableThumbnailZoomCheckBox.UseVisualStyleBackColor = true;
			EnableThumbnailZoomCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// ThumbnailZoomFactorNumericEdit
			// 
			ThumbnailZoomFactorNumericEdit.BackColor = SystemColors.Window;
			ThumbnailZoomFactorNumericEdit.BorderStyle = BorderStyle.FixedSingle;
			ThumbnailZoomFactorNumericEdit.Location = new Point(227, 60);
			ThumbnailZoomFactorNumericEdit.Margin = new Padding(6, 7, 6, 7);
			ThumbnailZoomFactorNumericEdit.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
			ThumbnailZoomFactorNumericEdit.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
			ThumbnailZoomFactorNumericEdit.Name = "ThumbnailZoomFactorNumericEdit";
			ThumbnailZoomFactorNumericEdit.Size = new Size(63, 31);
			ThumbnailZoomFactorNumericEdit.TabIndex = 37;
			ThumbnailZoomFactorNumericEdit.Value = new decimal(new int[] { 2, 0, 0, 0 });
			ThumbnailZoomFactorNumericEdit.ValueChanged += OptionChanged_Handler;
			// 
			// OverlayTabPage
			// 
			OverlayTabPage.BackColor = SystemColors.Control;
			OverlayTabPage.Controls.Add(OverlaySettingsPanel);
			OverlayTabPage.Location = new Point(124, 4);
			OverlayTabPage.Margin = new Padding(6, 7, 6, 7);
			OverlayTabPage.Name = "OverlayTabPage";
			OverlayTabPage.Size = new Size(576, 472);
			OverlayTabPage.TabIndex = 3;
			OverlayTabPage.Text = "Overlay";
			// 
			// OverlaySettingsPanel
			// 
			OverlaySettingsPanel.BorderStyle = BorderStyle.FixedSingle;
			OverlaySettingsPanel.Controls.Add(CycleGroupIndicatorPositionLabel);
			OverlaySettingsPanel.Controls.Add(panel2);
			OverlaySettingsPanel.Controls.Add(LabelOverlayLabelFont);
			OverlaySettingsPanel.Controls.Add(btnLabelFont);
			OverlaySettingsPanel.Controls.Add(OverlayPositionLabel);
			OverlaySettingsPanel.Controls.Add(OverlayLabelColourLabel);
			OverlaySettingsPanel.Controls.Add(OverlayLabelColorButton);
			OverlaySettingsPanel.Controls.Add(panel1);
			OverlaySettingsPanel.Controls.Add(HighlightColorLabel);
			OverlaySettingsPanel.Controls.Add(ActiveClientHighlightColorButton);
			OverlaySettingsPanel.Controls.Add(EnableActiveClientHighlightCheckBox);
			OverlaySettingsPanel.Controls.Add(ShowThumbnailOverlaysCheckBox);
			OverlaySettingsPanel.Controls.Add(ShowThumbnailFramesCheckBox);
			OverlaySettingsPanel.Dock = DockStyle.Fill;
			OverlaySettingsPanel.Location = new Point(0, 0);
			OverlaySettingsPanel.Margin = new Padding(6, 7, 6, 7);
			OverlaySettingsPanel.Name = "OverlaySettingsPanel";
			OverlaySettingsPanel.Size = new Size(576, 472);
			OverlaySettingsPanel.TabIndex = 25;
			// 
			// CycleGroupIndicatorPositionLabel
			// 
			CycleGroupIndicatorPositionLabel.AutoSize = true;
			CycleGroupIndicatorPositionLabel.Location = new Point(213, 52);
			CycleGroupIndicatorPositionLabel.Margin = new Padding(6, 0, 6, 0);
			CycleGroupIndicatorPositionLabel.Name = "CycleGroupIndicatorPositionLabel";
			CycleGroupIndicatorPositionLabel.Size = new Size(251, 25);
			CycleGroupIndicatorPositionLabel.TabIndex = 47;
			CycleGroupIndicatorPositionLabel.Text = "Cycle Group Indicator Position";
			CycleGroupIndicatorPositionLabel.TextAlign = ContentAlignment.MiddleRight;
			// 
			// panel2
			// 
			panel2.BorderStyle = BorderStyle.FixedSingle;
			panel2.Controls.Add(CycleGroupIndicatorNWRadioButton);
			panel2.Controls.Add(CycleGroupIndicatorNRadioButton);
			panel2.Controls.Add(CycleGroupIndicatorNERadioButton);
			panel2.Controls.Add(CycleGroupIndicatorWRadioButton);
			panel2.Controls.Add(CycleGroupIndicatorSERadioButton);
			panel2.Controls.Add(CycleGroupIndicatorCRadioButton);
			panel2.Controls.Add(CycleGroupIndicatorSRadioButton);
			panel2.Controls.Add(CycleGroupIndicatorERadioButton);
			panel2.Controls.Add(CycleGroupIndicatorSWRadioButton);
			panel2.Location = new Point(306, 83);
			panel2.Margin = new Padding(6, 7, 6, 7);
			panel2.Name = "panel2";
			panel2.Size = new Size(103, 109);
			panel2.TabIndex = 46;
			// 
			// CycleGroupIndicatorNWRadioButton
			// 
			CycleGroupIndicatorNWRadioButton.AutoSize = true;
			CycleGroupIndicatorNWRadioButton.Location = new Point(6, 7);
			CycleGroupIndicatorNWRadioButton.Margin = new Padding(6, 7, 6, 7);
			CycleGroupIndicatorNWRadioButton.Name = "CycleGroupIndicatorNWRadioButton";
			CycleGroupIndicatorNWRadioButton.Size = new Size(21, 20);
			CycleGroupIndicatorNWRadioButton.TabIndex = 0;
			CycleGroupIndicatorNWRadioButton.TabStop = true;
			CycleGroupIndicatorNWRadioButton.UseVisualStyleBackColor = true;
			CycleGroupIndicatorNWRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// CycleGroupIndicatorNRadioButton
			// 
			CycleGroupIndicatorNRadioButton.AutoSize = true;
			CycleGroupIndicatorNRadioButton.Location = new Point(39, 7);
			CycleGroupIndicatorNRadioButton.Margin = new Padding(6, 7, 6, 7);
			CycleGroupIndicatorNRadioButton.Name = "CycleGroupIndicatorNRadioButton";
			CycleGroupIndicatorNRadioButton.Size = new Size(21, 20);
			CycleGroupIndicatorNRadioButton.TabIndex = 1;
			CycleGroupIndicatorNRadioButton.TabStop = true;
			CycleGroupIndicatorNRadioButton.UseVisualStyleBackColor = true;
			CycleGroupIndicatorNRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// CycleGroupIndicatorNERadioButton
			// 
			CycleGroupIndicatorNERadioButton.AutoSize = true;
			CycleGroupIndicatorNERadioButton.Location = new Point(71, 7);
			CycleGroupIndicatorNERadioButton.Margin = new Padding(6, 7, 6, 7);
			CycleGroupIndicatorNERadioButton.Name = "CycleGroupIndicatorNERadioButton";
			CycleGroupIndicatorNERadioButton.Size = new Size(21, 20);
			CycleGroupIndicatorNERadioButton.TabIndex = 2;
			CycleGroupIndicatorNERadioButton.TabStop = true;
			CycleGroupIndicatorNERadioButton.UseVisualStyleBackColor = true;
			CycleGroupIndicatorNERadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// CycleGroupIndicatorWRadioButton
			// 
			CycleGroupIndicatorWRadioButton.AutoSize = true;
			CycleGroupIndicatorWRadioButton.Location = new Point(6, 42);
			CycleGroupIndicatorWRadioButton.Margin = new Padding(6, 7, 6, 7);
			CycleGroupIndicatorWRadioButton.Name = "CycleGroupIndicatorWRadioButton";
			CycleGroupIndicatorWRadioButton.Size = new Size(21, 20);
			CycleGroupIndicatorWRadioButton.TabIndex = 3;
			CycleGroupIndicatorWRadioButton.TabStop = true;
			CycleGroupIndicatorWRadioButton.UseVisualStyleBackColor = true;
			CycleGroupIndicatorWRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// CycleGroupIndicatorSERadioButton
			// 
			CycleGroupIndicatorSERadioButton.AutoSize = true;
			CycleGroupIndicatorSERadioButton.Location = new Point(71, 77);
			CycleGroupIndicatorSERadioButton.Margin = new Padding(6, 7, 6, 7);
			CycleGroupIndicatorSERadioButton.Name = "CycleGroupIndicatorSERadioButton";
			CycleGroupIndicatorSERadioButton.Size = new Size(21, 20);
			CycleGroupIndicatorSERadioButton.TabIndex = 8;
			CycleGroupIndicatorSERadioButton.TabStop = true;
			CycleGroupIndicatorSERadioButton.UseVisualStyleBackColor = true;
			CycleGroupIndicatorSERadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// CycleGroupIndicatorCRadioButton
			// 
			CycleGroupIndicatorCRadioButton.AutoSize = true;
			CycleGroupIndicatorCRadioButton.Location = new Point(39, 42);
			CycleGroupIndicatorCRadioButton.Margin = new Padding(6, 7, 6, 7);
			CycleGroupIndicatorCRadioButton.Name = "CycleGroupIndicatorCRadioButton";
			CycleGroupIndicatorCRadioButton.Size = new Size(21, 20);
			CycleGroupIndicatorCRadioButton.TabIndex = 4;
			CycleGroupIndicatorCRadioButton.TabStop = true;
			CycleGroupIndicatorCRadioButton.UseVisualStyleBackColor = true;
			CycleGroupIndicatorCRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// CycleGroupIndicatorSRadioButton
			// 
			CycleGroupIndicatorSRadioButton.AutoSize = true;
			CycleGroupIndicatorSRadioButton.Location = new Point(39, 77);
			CycleGroupIndicatorSRadioButton.Margin = new Padding(6, 7, 6, 7);
			CycleGroupIndicatorSRadioButton.Name = "CycleGroupIndicatorSRadioButton";
			CycleGroupIndicatorSRadioButton.Size = new Size(21, 20);
			CycleGroupIndicatorSRadioButton.TabIndex = 7;
			CycleGroupIndicatorSRadioButton.TabStop = true;
			CycleGroupIndicatorSRadioButton.UseVisualStyleBackColor = true;
			CycleGroupIndicatorSRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// CycleGroupIndicatorERadioButton
			// 
			CycleGroupIndicatorERadioButton.AutoSize = true;
			CycleGroupIndicatorERadioButton.Location = new Point(71, 42);
			CycleGroupIndicatorERadioButton.Margin = new Padding(6, 7, 6, 7);
			CycleGroupIndicatorERadioButton.Name = "CycleGroupIndicatorERadioButton";
			CycleGroupIndicatorERadioButton.Size = new Size(21, 20);
			CycleGroupIndicatorERadioButton.TabIndex = 5;
			CycleGroupIndicatorERadioButton.TabStop = true;
			CycleGroupIndicatorERadioButton.UseVisualStyleBackColor = true;
			CycleGroupIndicatorERadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// CycleGroupIndicatorSWRadioButton
			// 
			CycleGroupIndicatorSWRadioButton.AutoSize = true;
			CycleGroupIndicatorSWRadioButton.Location = new Point(6, 77);
			CycleGroupIndicatorSWRadioButton.Margin = new Padding(6, 7, 6, 7);
			CycleGroupIndicatorSWRadioButton.Name = "CycleGroupIndicatorSWRadioButton";
			CycleGroupIndicatorSWRadioButton.Size = new Size(21, 20);
			CycleGroupIndicatorSWRadioButton.TabIndex = 6;
			CycleGroupIndicatorSWRadioButton.TabStop = true;
			CycleGroupIndicatorSWRadioButton.UseVisualStyleBackColor = true;
			CycleGroupIndicatorSWRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// LabelOverlayLabelFont
			// 
			LabelOverlayLabelFont.AutoSize = true;
			LabelOverlayLabelFont.Location = new Point(13, 237);
			LabelOverlayLabelFont.Margin = new Padding(6, 0, 6, 0);
			LabelOverlayLabelFont.Name = "LabelOverlayLabelFont";
			LabelOverlayLabelFont.Size = new Size(72, 25);
			LabelOverlayLabelFont.TabIndex = 45;
			LabelOverlayLabelFont.Text = "Overlay";
			// 
			// btnLabelFont
			// 
			btnLabelFont.Location = new Point(9, 190);
			btnLabelFont.Name = "btnLabelFont";
			btnLabelFont.Size = new Size(124, 43);
			btnLabelFont.TabIndex = 44;
			btnLabelFont.Text = "Label Font";
			btnLabelFont.UseVisualStyleBackColor = true;
			btnLabelFont.Click += btnLabelFont_Click;
			// 
			// OverlayPositionLabel
			// 
			OverlayPositionLabel.AutoSize = true;
			OverlayPositionLabel.Location = new Point(337, 237);
			OverlayPositionLabel.Margin = new Padding(6, 0, 6, 0);
			OverlayPositionLabel.Name = "OverlayPositionLabel";
			OverlayPositionLabel.Size = new Size(75, 25);
			OverlayPositionLabel.TabIndex = 43;
			OverlayPositionLabel.Text = "Position";
			// 
			// OverlayLabelColourLabel
			// 
			OverlayLabelColourLabel.AutoSize = true;
			OverlayLabelColourLabel.Location = new Point(9, 345);
			OverlayLabelColourLabel.Margin = new Padding(6, 0, 6, 0);
			OverlayLabelColourLabel.Name = "OverlayLabelColourLabel";
			OverlayLabelColourLabel.Size = new Size(55, 25);
			OverlayLabelColourLabel.TabIndex = 42;
			OverlayLabelColourLabel.Text = "Color";
			// 
			// OverlayLabelColorButton
			// 
			OverlayLabelColorButton.BorderStyle = BorderStyle.FixedSingle;
			OverlayLabelColorButton.Location = new Point(103, 343);
			OverlayLabelColorButton.Margin = new Padding(6, 7, 6, 7);
			OverlayLabelColorButton.Name = "OverlayLabelColorButton";
			OverlayLabelColorButton.Size = new Size(121, 30);
			OverlayLabelColorButton.TabIndex = 41;
			OverlayLabelColorButton.Click += OverlayLabelColorButton_Click;
			// 
			// panel1
			// 
			panel1.BorderStyle = BorderStyle.FixedSingle;
			panel1.Controls.Add(OverlayLabelNWRadioButton);
			panel1.Controls.Add(OverlayLabelNRadioButton);
			panel1.Controls.Add(OverlayLabelNERadioButton);
			panel1.Controls.Add(OverlayLabelWRadioButton);
			panel1.Controls.Add(OverlayLabelSERadioButton);
			panel1.Controls.Add(OverlayLabelCRadioButton);
			panel1.Controls.Add(OverlayLabelSRadioButton);
			panel1.Controls.Add(OverlayLabelERadioButton);
			panel1.Controls.Add(OverlayLabelSWRadioButton);
			panel1.Location = new Point(309, 265);
			panel1.Margin = new Padding(6, 7, 6, 7);
			panel1.Name = "panel1";
			panel1.Size = new Size(103, 109);
			panel1.TabIndex = 39;
			// 
			// OverlayLabelNWRadioButton
			// 
			OverlayLabelNWRadioButton.AutoSize = true;
			OverlayLabelNWRadioButton.Location = new Point(6, 7);
			OverlayLabelNWRadioButton.Margin = new Padding(6, 7, 6, 7);
			OverlayLabelNWRadioButton.Name = "OverlayLabelNWRadioButton";
			OverlayLabelNWRadioButton.Size = new Size(21, 20);
			OverlayLabelNWRadioButton.TabIndex = 0;
			OverlayLabelNWRadioButton.TabStop = true;
			OverlayLabelNWRadioButton.UseVisualStyleBackColor = true;
			OverlayLabelNWRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// OverlayLabelNRadioButton
			// 
			OverlayLabelNRadioButton.AutoSize = true;
			OverlayLabelNRadioButton.Location = new Point(39, 7);
			OverlayLabelNRadioButton.Margin = new Padding(6, 7, 6, 7);
			OverlayLabelNRadioButton.Name = "OverlayLabelNRadioButton";
			OverlayLabelNRadioButton.Size = new Size(21, 20);
			OverlayLabelNRadioButton.TabIndex = 1;
			OverlayLabelNRadioButton.TabStop = true;
			OverlayLabelNRadioButton.UseVisualStyleBackColor = true;
			OverlayLabelNRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// OverlayLabelNERadioButton
			// 
			OverlayLabelNERadioButton.AutoSize = true;
			OverlayLabelNERadioButton.Location = new Point(71, 7);
			OverlayLabelNERadioButton.Margin = new Padding(6, 7, 6, 7);
			OverlayLabelNERadioButton.Name = "OverlayLabelNERadioButton";
			OverlayLabelNERadioButton.Size = new Size(21, 20);
			OverlayLabelNERadioButton.TabIndex = 2;
			OverlayLabelNERadioButton.TabStop = true;
			OverlayLabelNERadioButton.UseVisualStyleBackColor = true;
			OverlayLabelNERadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// OverlayLabelWRadioButton
			// 
			OverlayLabelWRadioButton.AutoSize = true;
			OverlayLabelWRadioButton.Location = new Point(6, 42);
			OverlayLabelWRadioButton.Margin = new Padding(6, 7, 6, 7);
			OverlayLabelWRadioButton.Name = "OverlayLabelWRadioButton";
			OverlayLabelWRadioButton.Size = new Size(21, 20);
			OverlayLabelWRadioButton.TabIndex = 3;
			OverlayLabelWRadioButton.TabStop = true;
			OverlayLabelWRadioButton.UseVisualStyleBackColor = true;
			OverlayLabelWRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// OverlayLabelSERadioButton
			// 
			OverlayLabelSERadioButton.AutoSize = true;
			OverlayLabelSERadioButton.Location = new Point(71, 77);
			OverlayLabelSERadioButton.Margin = new Padding(6, 7, 6, 7);
			OverlayLabelSERadioButton.Name = "OverlayLabelSERadioButton";
			OverlayLabelSERadioButton.Size = new Size(21, 20);
			OverlayLabelSERadioButton.TabIndex = 8;
			OverlayLabelSERadioButton.TabStop = true;
			OverlayLabelSERadioButton.UseVisualStyleBackColor = true;
			OverlayLabelSERadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// OverlayLabelCRadioButton
			// 
			OverlayLabelCRadioButton.AutoSize = true;
			OverlayLabelCRadioButton.Location = new Point(39, 42);
			OverlayLabelCRadioButton.Margin = new Padding(6, 7, 6, 7);
			OverlayLabelCRadioButton.Name = "OverlayLabelCRadioButton";
			OverlayLabelCRadioButton.Size = new Size(21, 20);
			OverlayLabelCRadioButton.TabIndex = 4;
			OverlayLabelCRadioButton.TabStop = true;
			OverlayLabelCRadioButton.UseVisualStyleBackColor = true;
			OverlayLabelCRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// OverlayLabelSRadioButton
			// 
			OverlayLabelSRadioButton.AutoSize = true;
			OverlayLabelSRadioButton.Location = new Point(39, 77);
			OverlayLabelSRadioButton.Margin = new Padding(6, 7, 6, 7);
			OverlayLabelSRadioButton.Name = "OverlayLabelSRadioButton";
			OverlayLabelSRadioButton.Size = new Size(21, 20);
			OverlayLabelSRadioButton.TabIndex = 7;
			OverlayLabelSRadioButton.TabStop = true;
			OverlayLabelSRadioButton.UseVisualStyleBackColor = true;
			OverlayLabelSRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// OverlayLabelERadioButton
			// 
			OverlayLabelERadioButton.AutoSize = true;
			OverlayLabelERadioButton.Location = new Point(71, 42);
			OverlayLabelERadioButton.Margin = new Padding(6, 7, 6, 7);
			OverlayLabelERadioButton.Name = "OverlayLabelERadioButton";
			OverlayLabelERadioButton.Size = new Size(21, 20);
			OverlayLabelERadioButton.TabIndex = 5;
			OverlayLabelERadioButton.TabStop = true;
			OverlayLabelERadioButton.UseVisualStyleBackColor = true;
			OverlayLabelERadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// OverlayLabelSWRadioButton
			// 
			OverlayLabelSWRadioButton.AutoSize = true;
			OverlayLabelSWRadioButton.Location = new Point(6, 77);
			OverlayLabelSWRadioButton.Margin = new Padding(6, 7, 6, 7);
			OverlayLabelSWRadioButton.Name = "OverlayLabelSWRadioButton";
			OverlayLabelSWRadioButton.Size = new Size(21, 20);
			OverlayLabelSWRadioButton.TabIndex = 6;
			OverlayLabelSWRadioButton.TabStop = true;
			OverlayLabelSWRadioButton.UseVisualStyleBackColor = true;
			OverlayLabelSWRadioButton.CheckedChanged += OptionChanged_Handler;
			// 
			// HighlightColorLabel
			// 
			HighlightColorLabel.AutoSize = true;
			HighlightColorLabel.Location = new Point(9, 150);
			HighlightColorLabel.Margin = new Padding(6, 0, 6, 0);
			HighlightColorLabel.Name = "HighlightColorLabel";
			HighlightColorLabel.Size = new Size(55, 25);
			HighlightColorLabel.TabIndex = 29;
			HighlightColorLabel.Text = "Color";
			// 
			// ActiveClientHighlightColorButton
			// 
			ActiveClientHighlightColorButton.BorderStyle = BorderStyle.FixedSingle;
			ActiveClientHighlightColorButton.Location = new Point(103, 148);
			ActiveClientHighlightColorButton.Margin = new Padding(6, 7, 6, 7);
			ActiveClientHighlightColorButton.Name = "ActiveClientHighlightColorButton";
			ActiveClientHighlightColorButton.Size = new Size(121, 30);
			ActiveClientHighlightColorButton.TabIndex = 28;
			ActiveClientHighlightColorButton.Click += ActiveClientHighlightColorButton_Click;
			// 
			// EnableActiveClientHighlightCheckBox
			// 
			EnableActiveClientHighlightCheckBox.AutoSize = true;
			EnableActiveClientHighlightCheckBox.Checked = true;
			EnableActiveClientHighlightCheckBox.CheckState = CheckState.Checked;
			EnableActiveClientHighlightCheckBox.Location = new Point(13, 107);
			EnableActiveClientHighlightCheckBox.Margin = new Padding(6, 7, 6, 7);
			EnableActiveClientHighlightCheckBox.Name = "EnableActiveClientHighlightCheckBox";
			EnableActiveClientHighlightCheckBox.RightToLeft = RightToLeft.No;
			EnableActiveClientHighlightCheckBox.Size = new Size(207, 29);
			EnableActiveClientHighlightCheckBox.TabIndex = 27;
			EnableActiveClientHighlightCheckBox.Text = "Highlight active client";
			EnableActiveClientHighlightCheckBox.UseVisualStyleBackColor = true;
			EnableActiveClientHighlightCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// ShowThumbnailOverlaysCheckBox
			// 
			ShowThumbnailOverlaysCheckBox.AutoSize = true;
			ShowThumbnailOverlaysCheckBox.Checked = true;
			ShowThumbnailOverlaysCheckBox.CheckState = CheckState.Checked;
			ShowThumbnailOverlaysCheckBox.Location = new Point(13, 13);
			ShowThumbnailOverlaysCheckBox.Margin = new Padding(6, 7, 6, 7);
			ShowThumbnailOverlaysCheckBox.Name = "ShowThumbnailOverlaysCheckBox";
			ShowThumbnailOverlaysCheckBox.RightToLeft = RightToLeft.No;
			ShowThumbnailOverlaysCheckBox.Size = new Size(144, 29);
			ShowThumbnailOverlaysCheckBox.TabIndex = 25;
			ShowThumbnailOverlaysCheckBox.Text = "Show overlay";
			ShowThumbnailOverlaysCheckBox.UseVisualStyleBackColor = true;
			ShowThumbnailOverlaysCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// ShowThumbnailFramesCheckBox
			// 
			ShowThumbnailFramesCheckBox.AutoSize = true;
			ShowThumbnailFramesCheckBox.Checked = true;
			ShowThumbnailFramesCheckBox.CheckState = CheckState.Checked;
			ShowThumbnailFramesCheckBox.Location = new Point(13, 60);
			ShowThumbnailFramesCheckBox.Margin = new Padding(6, 7, 6, 7);
			ShowThumbnailFramesCheckBox.Name = "ShowThumbnailFramesCheckBox";
			ShowThumbnailFramesCheckBox.RightToLeft = RightToLeft.No;
			ShowThumbnailFramesCheckBox.Size = new Size(141, 29);
			ShowThumbnailFramesCheckBox.TabIndex = 26;
			ShowThumbnailFramesCheckBox.Text = "Show frames";
			ShowThumbnailFramesCheckBox.UseVisualStyleBackColor = true;
			ShowThumbnailFramesCheckBox.CheckedChanged += OptionChanged_Handler;
			// 
			// CycleGroupTabPage
			// 
			CycleGroupTabPage.Controls.Add(CycleGroupLabel);
			CycleGroupTabPage.Controls.Add(CycleGroupSelectorComboBox);
			CycleGroupTabPage.Controls.Add(HotkeysForwardLabel);
			CycleGroupTabPage.Controls.Add(HotkeysForwardListBox);
			CycleGroupTabPage.Controls.Add(HotkeysForwardAddButton);
			CycleGroupTabPage.Controls.Add(HotkeysForwardRemoveButton);
			CycleGroupTabPage.Controls.Add(HotkeysBackwardLabel);
			CycleGroupTabPage.Controls.Add(HotkeysBackwardListBox);
			CycleGroupTabPage.Controls.Add(HotkeysBackwardAddButton);
			CycleGroupTabPage.Controls.Add(HotkeysBackwardRemoveButton);
			CycleGroupTabPage.Controls.Add(HotkeysClientsList);
			CycleGroupTabPage.Controls.Add(HotkeysClientUpButton);
			CycleGroupTabPage.Controls.Add(HotkeysClientDownButton);
			CycleGroupTabPage.Controls.Add(HotkeyReadmeTextBox);
			CycleGroupTabPage.Controls.Add(HotkeySaveButton);
			CycleGroupTabPage.Controls.Add(HotkeyCaptureTextBox);
			CycleGroupTabPage.Controls.Add(HotkeyCaptureButton);
			CycleGroupTabPage.Location = new Point(124, 4);
			CycleGroupTabPage.Margin = new Padding(4, 5, 4, 5);
			CycleGroupTabPage.Name = "CycleGroupTabPage";
			CycleGroupTabPage.Padding = new Padding(4, 5, 4, 5);
			CycleGroupTabPage.Size = new Size(576, 472);
			CycleGroupTabPage.TabIndex = 7;
			CycleGroupTabPage.Text = "Cycle Groups";
			CycleGroupTabPage.UseVisualStyleBackColor = true;
			// 
			// CycleGroupLabel
			// 
			CycleGroupLabel.AutoSize = true;
			CycleGroupLabel.Location = new Point(10, 18);
			CycleGroupLabel.Margin = new Padding(6, 0, 6, 0);
			CycleGroupLabel.Name = "CycleGroupLabel";
			CycleGroupLabel.Size = new Size(62, 25);
			CycleGroupLabel.TabIndex = 30;
			CycleGroupLabel.Text = "Group";
			// 
			// CycleGroupSelectorComboBox
			// 
			CycleGroupSelectorComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			CycleGroupSelectorComboBox.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
			CycleGroupSelectorComboBox.Location = new Point(91, 13);
			CycleGroupSelectorComboBox.Name = "CycleGroupSelectorComboBox";
			CycleGroupSelectorComboBox.Size = new Size(60, 33);
			CycleGroupSelectorComboBox.TabIndex = 16;
			CycleGroupSelectorComboBox.SelectedIndexChanged += CycleGroupSelectorComboBox_SelectedIndexChanged;
			// 
			// HotkeysForwardLabel
			// 
			HotkeysForwardLabel.AutoSize = true;
			HotkeysForwardLabel.Location = new Point(4, 82);
			HotkeysForwardLabel.Name = "HotkeysForwardLabel";
			HotkeysForwardLabel.Size = new Size(48, 25);
			HotkeysForwardLabel.TabIndex = 17;
			HotkeysForwardLabel.Text = ">>>";
			// 
			// HotkeysForwardListBox
			// 
			HotkeysForwardListBox.ItemHeight = 25;
			HotkeysForwardListBox.Location = new Point(20, 115);
			HotkeysForwardListBox.Name = "HotkeysForwardListBox";
			HotkeysForwardListBox.Size = new Size(144, 104);
			HotkeysForwardListBox.TabIndex = 18;
			// 
			// HotkeysForwardAddButton
			// 
			HotkeysForwardAddButton.Location = new Point(54, 78);
			HotkeysForwardAddButton.Name = "HotkeysForwardAddButton";
			HotkeysForwardAddButton.Size = new Size(60, 37);
			HotkeysForwardAddButton.TabIndex = 19;
			HotkeysForwardAddButton.Text = "Add";
			HotkeysForwardAddButton.Click += HotkeysForwardAddButton_Click;
			// 
			// HotkeysForwardRemoveButton
			// 
			HotkeysForwardRemoveButton.Location = new Point(111, 78);
			HotkeysForwardRemoveButton.Name = "HotkeysForwardRemoveButton";
			HotkeysForwardRemoveButton.Size = new Size(60, 37);
			HotkeysForwardRemoveButton.TabIndex = 20;
			HotkeysForwardRemoveButton.Text = "Del";
			HotkeysForwardRemoveButton.Click += HotkeysForwardRemoveButton_Click;
			// 
			// HotkeysBackwardLabel
			// 
			HotkeysBackwardLabel.AutoSize = true;
			HotkeysBackwardLabel.Location = new Point(4, 230);
			HotkeysBackwardLabel.Name = "HotkeysBackwardLabel";
			HotkeysBackwardLabel.Size = new Size(48, 25);
			HotkeysBackwardLabel.TabIndex = 21;
			HotkeysBackwardLabel.Text = "<<<";
			// 
			// HotkeysBackwardListBox
			// 
			HotkeysBackwardListBox.ItemHeight = 25;
			HotkeysBackwardListBox.Location = new Point(20, 267);
			HotkeysBackwardListBox.Name = "HotkeysBackwardListBox";
			HotkeysBackwardListBox.Size = new Size(144, 104);
			HotkeysBackwardListBox.TabIndex = 22;
			// 
			// HotkeysBackwardAddButton
			// 
			HotkeysBackwardAddButton.Location = new Point(54, 228);
			HotkeysBackwardAddButton.Name = "HotkeysBackwardAddButton";
			HotkeysBackwardAddButton.Size = new Size(60, 37);
			HotkeysBackwardAddButton.TabIndex = 23;
			HotkeysBackwardAddButton.Text = "Add";
			HotkeysBackwardAddButton.Click += HotkeysBackwardAddButton_Click;
			// 
			// HotkeysBackwardRemoveButton
			// 
			HotkeysBackwardRemoveButton.Location = new Point(111, 228);
			HotkeysBackwardRemoveButton.Name = "HotkeysBackwardRemoveButton";
			HotkeysBackwardRemoveButton.Size = new Size(60, 37);
			HotkeysBackwardRemoveButton.TabIndex = 24;
			HotkeysBackwardRemoveButton.Text = "Del";
			HotkeysBackwardRemoveButton.Click += HotkeysBackwardRemoveButton_Click;
			// 
			// HotkeysClientsList
			// 
			HotkeysClientsList.Location = new Point(177, 82);
			HotkeysClientsList.Name = "HotkeysClientsList";
			HotkeysClientsList.Size = new Size(273, 200);
			HotkeysClientsList.TabIndex = 25;
			// 
			// HotkeysClientUpButton
			// 
			HotkeysClientUpButton.Location = new Point(451, 82);
			HotkeysClientUpButton.Name = "HotkeysClientUpButton";
			HotkeysClientUpButton.Size = new Size(66, 33);
			HotkeysClientUpButton.TabIndex = 26;
			HotkeysClientUpButton.Text = "Up";
			HotkeysClientUpButton.Click += HotkeysClientUpButton_Click;
			// 
			// HotkeysClientDownButton
			// 
			HotkeysClientDownButton.Location = new Point(451, 132);
			HotkeysClientDownButton.Name = "HotkeysClientDownButton";
			HotkeysClientDownButton.Size = new Size(66, 33);
			HotkeysClientDownButton.TabIndex = 27;
			HotkeysClientDownButton.Text = "Down";
			HotkeysClientDownButton.Click += HotkeysClientDownButton_Click;
			// 
			// HotkeyReadmeTextBox
			// 
			HotkeyReadmeTextBox.Location = new Point(159, 317);
			HotkeyReadmeTextBox.Multiline = true;
			HotkeyReadmeTextBox.Name = "HotkeyReadmeTextBox";
			HotkeyReadmeTextBox.ReadOnly = true;
			HotkeyReadmeTextBox.Size = new Size(368, 114);
			HotkeyReadmeTextBox.TabIndex = 28;
			// 
			// HotkeySaveButton
			// 
			HotkeySaveButton.Location = new Point(36, 397);
			HotkeySaveButton.Name = "HotkeySaveButton";
			HotkeySaveButton.Size = new Size(119, 37);
			HotkeySaveButton.TabIndex = 29;
			HotkeySaveButton.Text = "Save";
			HotkeySaveButton.Click += HotkeySaveButton_Click;
			// 
			// HotkeyCaptureTextBox
			// 
			HotkeyCaptureTextBox.Location = new Point(276, 12);
			HotkeyCaptureTextBox.Name = "HotkeyCaptureTextBox";
			HotkeyCaptureTextBox.Size = new Size(228, 31);
			HotkeyCaptureTextBox.TabIndex = 14;
			HotkeyCaptureTextBox.Enter += HotkeyCaptureTextBox_Enter;
			HotkeyCaptureTextBox.KeyDown += HotkeyCaptureTextBox_KeyDown;
			HotkeyCaptureTextBox.Leave += HotkeyCaptureTextBox_Leave;
			HotkeyCaptureTextBox.MouseDown += HotkeyCaptureTextBox_MouseDown;
			// 
			// HotkeyCaptureButton
			// 
			HotkeyCaptureButton.Location = new Point(159, 12);
			HotkeyCaptureButton.Margin = new Padding(0);
			HotkeyCaptureButton.Name = "HotkeyCaptureButton";
			HotkeyCaptureButton.Size = new Size(111, 37);
			HotkeyCaptureButton.TabIndex = 15;
			HotkeyCaptureButton.Text = "Capture";
			HotkeyCaptureButton.Click += HotkeyCaptureButton_Click;
			// 
			// ClientsTabPage
			// 
			ClientsTabPage.BackColor = SystemColors.Control;
			ClientsTabPage.Controls.Add(ClientsPanel);
			ClientsTabPage.Location = new Point(124, 4);
			ClientsTabPage.Margin = new Padding(6, 7, 6, 7);
			ClientsTabPage.Name = "ClientsTabPage";
			ClientsTabPage.Size = new Size(576, 472);
			ClientsTabPage.TabIndex = 4;
			ClientsTabPage.Text = "Active Clients";
			// 
			// ClientsPanel
			// 
			ClientsPanel.BorderStyle = BorderStyle.FixedSingle;
			ClientsPanel.Controls.Add(ThumbnailsList);
			ClientsPanel.Controls.Add(ThumbnailsListLabel);
			ClientsPanel.Dock = DockStyle.Fill;
			ClientsPanel.Location = new Point(0, 0);
			ClientsPanel.Margin = new Padding(6, 7, 6, 7);
			ClientsPanel.Name = "ClientsPanel";
			ClientsPanel.Size = new Size(576, 472);
			ClientsPanel.TabIndex = 32;
			// 
			// ThumbnailsList
			// 
			ThumbnailsList.BackColor = SystemColors.Window;
			ThumbnailsList.BorderStyle = BorderStyle.FixedSingle;
			ThumbnailsList.CheckOnClick = true;
			ThumbnailsList.Dock = DockStyle.Bottom;
			ThumbnailsList.FormattingEnabled = true;
			ThumbnailsList.IntegralHeight = false;
			ThumbnailsList.Location = new Point(0, 126);
			ThumbnailsList.Margin = new Padding(6, 7, 6, 7);
			ThumbnailsList.Name = "ThumbnailsList";
			ThumbnailsList.Size = new Size(574, 344);
			ThumbnailsList.TabIndex = 34;
			ThumbnailsList.ItemCheck += ThumbnailsList_ItemCheck_Handler;
			// 
			// ThumbnailsListLabel
			// 
			ThumbnailsListLabel.AutoSize = true;
			ThumbnailsListLabel.Location = new Point(13, 17);
			ThumbnailsListLabel.Margin = new Padding(6, 0, 6, 0);
			ThumbnailsListLabel.Name = "ThumbnailsListLabel";
			ThumbnailsListLabel.Size = new Size(268, 25);
			ThumbnailsListLabel.TabIndex = 33;
			ThumbnailsListLabel.Text = "Thumbnails (check to force hide)";
			// 
			// LanguageTabPage
			// 
			LanguageTabPage.Controls.Add(LanguageLabel);
			LanguageTabPage.Controls.Add(LanguageCombo);
			LanguageTabPage.Location = new Point(124, 4);
			LanguageTabPage.Margin = new Padding(4, 5, 4, 5);
			LanguageTabPage.Name = "LanguageTabPage";
			LanguageTabPage.Padding = new Padding(4, 5, 4, 5);
			LanguageTabPage.Size = new Size(576, 472);
			LanguageTabPage.TabIndex = 6;
			LanguageTabPage.Text = "Language";
			LanguageTabPage.UseVisualStyleBackColor = true;
			LanguageTabPage.Click += LanguageTabPage_Click;
			// 
			// LanguageLabel
			// 
			LanguageLabel.AutoSize = true;
			LanguageLabel.Location = new Point(24, 40);
			LanguageLabel.Margin = new Padding(6, 0, 6, 0);
			LanguageLabel.Name = "LanguageLabel";
			LanguageLabel.Size = new Size(89, 25);
			LanguageLabel.TabIndex = 2;
			LanguageLabel.Text = "Language";
			// 
			// LanguageCombo
			// 
			LanguageCombo.DropDownStyle = ComboBoxStyle.DropDownList;
			LanguageCombo.FormattingEnabled = true;
			LanguageCombo.Location = new Point(201, 35);
			LanguageCombo.Margin = new Padding(6, 7, 6, 7);
			LanguageCombo.Name = "LanguageCombo";
			LanguageCombo.Size = new Size(251, 33);
			LanguageCombo.TabIndex = 1;
			LanguageCombo.SelectedIndexChanged += LanguageCombo_SelectedIndexChanged;
			// 
			// AboutTabPage
			// 
			AboutTabPage.BackColor = SystemColors.Control;
			AboutTabPage.Controls.Add(AboutPanel);
			AboutTabPage.Location = new Point(124, 4);
			AboutTabPage.Margin = new Padding(6, 7, 6, 7);
			AboutTabPage.Name = "AboutTabPage";
			AboutTabPage.Size = new Size(576, 472);
			AboutTabPage.TabIndex = 5;
			AboutTabPage.Text = "About";
			// 
			// AboutPanel
			// 
			AboutPanel.BackColor = Color.Transparent;
			AboutPanel.BorderStyle = BorderStyle.FixedSingle;
			AboutPanel.Controls.Add(CreditMaintLabel);
			AboutPanel.Controls.Add(DocumentationLinkLabel);
			AboutPanel.Controls.Add(DescriptionLabel);
			AboutPanel.Controls.Add(VersionLabel);
			AboutPanel.Controls.Add(NameLabel);
			AboutPanel.Controls.Add(DocumentationLink);
			AboutPanel.Dock = DockStyle.Fill;
			AboutPanel.Location = new Point(0, 0);
			AboutPanel.Margin = new Padding(6, 7, 6, 7);
			AboutPanel.Name = "AboutPanel";
			AboutPanel.Size = new Size(576, 472);
			AboutPanel.TabIndex = 2;
			// 
			// CreditMaintLabel
			// 
			CreditMaintLabel.AutoSize = true;
			CreditMaintLabel.Location = new Point(0, 275);
			CreditMaintLabel.Margin = new Padding(6, 0, 6, 0);
			CreditMaintLabel.Name = "CreditMaintLabel";
			CreditMaintLabel.Padding = new Padding(13, 7, 13, 7);
			CreditMaintLabel.Size = new Size(435, 39);
			CreditMaintLabel.TabIndex = 7;
			CreditMaintLabel.Text = "Credit to previous maintainer: Phrynohyas Tig-Rah";
			// 
			// DocumentationLinkLabel
			// 
			DocumentationLinkLabel.AutoSize = true;
			DocumentationLinkLabel.Location = new Point(0, 313);
			DocumentationLinkLabel.Margin = new Padding(6, 0, 6, 0);
			DocumentationLinkLabel.Name = "DocumentationLinkLabel";
			DocumentationLinkLabel.Padding = new Padding(13, 7, 13, 7);
			DocumentationLinkLabel.Size = new Size(389, 39);
			DocumentationLinkLabel.TabIndex = 6;
			DocumentationLinkLabel.Text = "For more information visit the forum thread:";
			// 
			// DescriptionLabel
			// 
			DescriptionLabel.BackColor = Color.Transparent;
			DescriptionLabel.Location = new Point(0, 57);
			DescriptionLabel.Margin = new Padding(6, 0, 6, 0);
			DescriptionLabel.Name = "DescriptionLabel";
			DescriptionLabel.Padding = new Padding(13, 7, 13, 7);
			DescriptionLabel.Size = new Size(434, 278);
			DescriptionLabel.TabIndex = 5;
			DescriptionLabel.Text = resources.GetString("DescriptionLabel.Text");
			// 
			// VersionLabel
			// 
			VersionLabel.AutoSize = true;
			VersionLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
			VersionLabel.Location = new Point(221, 17);
			VersionLabel.Margin = new Padding(6, 0, 6, 0);
			VersionLabel.Name = "VersionLabel";
			VersionLabel.Size = new Size(69, 29);
			VersionLabel.TabIndex = 4;
			VersionLabel.Text = "1.0.0";
			// 
			// NameLabel
			// 
			NameLabel.AutoSize = true;
			NameLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
			NameLabel.Location = new Point(7, 17);
			NameLabel.Margin = new Padding(6, 0, 6, 0);
			NameLabel.Name = "NameLabel";
			NameLabel.Size = new Size(195, 29);
			NameLabel.TabIndex = 3;
			NameLabel.Text = "EVE-O-Preview";
			// 
			// DocumentationLink
			// 
			DocumentationLink.Location = new Point(0, 340);
			DocumentationLink.Margin = new Padding(50, 7, 6, 7);
			DocumentationLink.Name = "DocumentationLink";
			DocumentationLink.Padding = new Padding(13, 7, 13, 7);
			DocumentationLink.Size = new Size(437, 63);
			DocumentationLink.TabIndex = 2;
			DocumentationLink.TabStop = true;
			DocumentationLink.Text = "to be set from prresenter to be set from prresenter to be set from prresenter to be set from prresenter";
			DocumentationLink.LinkClicked += DocumentationLinkClicked_Handler;
			// 
			// NotifyIcon
			// 
			NotifyIcon.ContextMenuStrip = TrayMenu;
			NotifyIcon.Icon = (Icon)resources.GetObject("NotifyIcon.Icon");
			NotifyIcon.Text = "EVE-O-Preview";
			NotifyIcon.Visible = true;
			NotifyIcon.MouseDoubleClick += RestoreMainForm_Handler;
			// 
			// TrayMenu
			// 
			TrayMenu.ImageScalingSize = new Size(24, 24);
			TrayMenu.Items.AddRange(new ToolStripItem[] { TitleMenuItem, MenuConfigurationFile, SeparatorMenuItem, RestoreWindowMenuItem, SeparatorMenuItem2, ExitMenuItem });
			TrayMenu.Name = "contextMenuStrip1";
			TrayMenu.Size = new Size(225, 144);
			// 
			// MenuConfigurationFile
			// 
			MenuConfigurationFile.DropDownItems.AddRange(new ToolStripItem[] { defaultoolStripMenuItem });
			MenuConfigurationFile.Name = "MenuConfigurationFile";
			MenuConfigurationFile.Size = new Size(224, 32);
			MenuConfigurationFile.Text = "Configuration File";
			MenuConfigurationFile.DropDownItemClicked += MenuConfigurationFile_DropDownItemClicked;
			// 
			// SeparatorMenuItem2
			// 
			SeparatorMenuItem2.Name = "SeparatorMenuItem2";
			SeparatorMenuItem2.Size = new Size(221, 6);
			// 
			// defaultoolStripMenuItem
			// 
			defaultoolStripMenuItem.Name = "defaultoolStripMenuItem";
			defaultoolStripMenuItem.Size = new Size(270, 34);
			defaultoolStripMenuItem.Text = "*DEFAULT*";
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(10F, 25F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.Control;
			ClientSize = new Size(704, 480);
			Controls.Add(ContentTabControl);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Icon = (Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(0);
			MaximizeBox = false;
			Name = "MainForm";
			Text = "EVE-O-Preview";
			TopMost = true;
			FormClosing += MainFormClosing_Handler;
			Load += MainFormResize_Handler;
			Resize += MainFormResize_Handler;
			ContentTabControl.ResumeLayout(false);
			GeneralTabPage.ResumeLayout(false);
			GeneralSettingsPanel.ResumeLayout(false);
			GeneralSettingsPanel.PerformLayout();
			ThumbnailTabPage.ResumeLayout(false);
			ThumbnailSettingsPanel.ResumeLayout(false);
			ThumbnailSettingsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)ThumbnailSnapToGridSizeYNumericEdit).EndInit();
			((System.ComponentModel.ISupportInitialize)ThumbnailSnapToGridSizeXNumericEdit).EndInit();
			((System.ComponentModel.ISupportInitialize)ThumbnailsWidthNumericEdit).EndInit();
			((System.ComponentModel.ISupportInitialize)ThumbnailsHeightNumericEdit).EndInit();
			((System.ComponentModel.ISupportInitialize)ThumbnailOpacityTrackBar).EndInit();
			ZoomTabPage.ResumeLayout(false);
			ZoomSettingsPanel.ResumeLayout(false);
			ZoomSettingsPanel.PerformLayout();
			ZoomAnchorPanel.ResumeLayout(false);
			ZoomAnchorPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)ThumbnailZoomFactorNumericEdit).EndInit();
			OverlayTabPage.ResumeLayout(false);
			OverlaySettingsPanel.ResumeLayout(false);
			OverlaySettingsPanel.PerformLayout();
			panel2.ResumeLayout(false);
			panel2.PerformLayout();
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			CycleGroupTabPage.ResumeLayout(false);
			CycleGroupTabPage.PerformLayout();
			ClientsTabPage.ResumeLayout(false);
			ClientsPanel.ResumeLayout(false);
			ClientsPanel.PerformLayout();
			LanguageTabPage.ResumeLayout(false);
			LanguageTabPage.PerformLayout();
			AboutTabPage.ResumeLayout(false);
			AboutPanel.ResumeLayout(false);
			AboutPanel.PerformLayout();
			TrayMenu.ResumeLayout(false);
			ResumeLayout(false);

		}

		#endregion
		private NotifyIcon NotifyIcon;
		private ContextMenuStrip TrayMenu;
		private TabPage ZoomTabPage;
		private CheckBox EnableClientLayoutTrackingCheckBox;
		private CheckBox HideActiveClientThumbnailCheckBox;
		private CheckBox ShowThumbnailsAlwaysOnTopCheckBox;
		private CheckBox HideThumbnailsOnLostFocusCheckBox;
		private CheckBox EnablePerClientThumbnailsLayoutsCheckBox;
		private CheckBox MinimizeToTrayCheckBox;
		private CheckBox QuickSwitchEnabledCheckBox;
		private NumericUpDown ThumbnailsWidthNumericEdit;
		private NumericUpDown ThumbnailsHeightNumericEdit;
		private Button ThumbnailPresetTinyButton;
		private Button ThumbnailPresetCompactButton;
		private Button ThumbnailPresetStandardButton;
		private TrackBar ThumbnailOpacityTrackBar;
		private Panel ZoomAnchorPanel;
		private RadioButton ZoomAanchorNWRadioButton;
		private RadioButton ZoomAanchorNRadioButton;
		private RadioButton ZoomAanchorNERadioButton;
		private RadioButton ZoomAanchorWRadioButton;
		private RadioButton ZoomAanchorSERadioButton;
		private RadioButton ZoomAanchorCRadioButton;
		private RadioButton ZoomAanchorSRadioButton;
		private RadioButton ZoomAanchorERadioButton;
		private RadioButton ZoomAanchorSWRadioButton;
		private CheckBox EnableThumbnailZoomCheckBox;
		private NumericUpDown ThumbnailZoomFactorNumericEdit;
		private Label HighlightColorLabel;
		private Panel ActiveClientHighlightColorButton;
		private CheckBox EnableActiveClientHighlightCheckBox;
		private CheckBox ShowThumbnailOverlaysCheckBox;
		private CheckBox ShowThumbnailFramesCheckBox;
		private CheckedListBox ThumbnailsList;
		private LinkLabel DocumentationLink;
		private Label VersionLabel;
		private CheckBox MinimizeInactiveClientsCheckBox;
        private CheckBox LockThumbnailLocationCheckbox;
        private NumericUpDown ThumbnailSnapToGridSizeYNumericEdit;
        private Label SnapYLabel;
        private NumericUpDown ThumbnailSnapToGridSizeXNumericEdit;
        private Label SnapXLabel;
        private CheckBox ThumbnailSnapToGridCheckBox;
        private Label OverlayPositionLabel;
        private Label OverlayLabelColourLabel;
        private Panel OverlayLabelColorButton;
        private Panel panel1;
        private RadioButton OverlayLabelNWRadioButton;
        private RadioButton OverlayLabelNRadioButton;
        private RadioButton OverlayLabelNERadioButton;
        private RadioButton OverlayLabelWRadioButton;
        private RadioButton OverlayLabelSERadioButton;
        private RadioButton OverlayLabelCRadioButton;
        private RadioButton OverlayLabelSRadioButton;
        private RadioButton OverlayLabelERadioButton;
        private RadioButton OverlayLabelSWRadioButton;
		private ComboBox AnimationStyleCombo;
		private Button btnLabelFont;
		private Label LabelOverlayLabelFont;
		private CheckBox PreventPreviewsCheckBox;
		private Label DoNotDisplayPreviewColour;
		private Panel PreventPreviewColorButton;
		private Label CycleGroupIndicatorPositionLabel;
		private Panel panel2;
		private RadioButton CycleGroupIndicatorNWRadioButton;
		private RadioButton CycleGroupIndicatorNRadioButton;
		private RadioButton CycleGroupIndicatorNERadioButton;
		private RadioButton CycleGroupIndicatorWRadioButton;
		private RadioButton CycleGroupIndicatorSERadioButton;
		private RadioButton CycleGroupIndicatorCRadioButton;
		private RadioButton CycleGroupIndicatorSRadioButton;
		private RadioButton CycleGroupIndicatorERadioButton;
		private RadioButton CycleGroupIndicatorSWRadioButton;
		private ComboBox CaptionOnClientsStyleCombo;
		private TabPage LanguageTabPage;
		private Label LanguageLabel;
		private ComboBox LanguageCombo;
		private TabPage CycleGroupTabPage;
		private TextBox HotkeyCaptureTextBox;
		private Button HotkeyCaptureButton;
		private ComboBox CycleGroupSelectorComboBox;
		private Label HotkeysForwardLabel;
		private ListBox HotkeysForwardListBox;
		private Button HotkeysForwardAddButton;
		private Button HotkeysForwardRemoveButton;
		private Label HotkeysBackwardLabel;
		private ListBox HotkeysBackwardListBox;
		private Button HotkeysBackwardAddButton;
		private Button HotkeysBackwardRemoveButton;
		private CheckedListBox HotkeysClientsList;
		private Button HotkeysClientUpButton;
		private Button HotkeysClientDownButton;
		private TextBox HotkeyReadmeTextBox;
		private Button HotkeySaveButton;
		private ToolStripMenuItem MenuConfigurationFile;
		private ToolStripSeparator SeparatorMenuItem2;
		private ToolStripMenuItem defaultoolStripMenuItem;
	}
}
