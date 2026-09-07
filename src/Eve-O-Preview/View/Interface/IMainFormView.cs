using EveOPreview.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace EveOPreview.View
{
	/// <summary>
	/// Main view interface
	/// Presenter uses it to access GUI properties
	/// </summary>
	public interface IMainFormView : IView
	{
		string Language { get; set; }
		bool MinimizeToTray { get; set; }

		double ThumbnailOpacity { get; set; }

		bool EnableClientLayoutTracking { get; set; }
		bool HideActiveClientThumbnail { get; set; }
		bool MinimizeInactiveClients { get; set; }
		ViewCaptionBarStyle CaptionOnClientsStyle { get; set; }
		ViewAnimationStyle WindowsAnimationStyle { get; set; }
        bool ShowThumbnailsAlwaysOnTop { get; set; }
		bool PreventPreviews { get; set; }
		bool HideThumbnailsOnLostFocus { get; set; }
		bool EnablePerClientThumbnailLayouts { get; set; }
		bool QuickSwitchEnabled { get; set; }
		bool QuickHideEnabled { get; set; }
		bool KeepMinimizedClientsLive { get; set; }

		Size ThumbnailSize { get; set; }

		bool EnableThumbnailZoom { get; set; }
		int ThumbnailZoomFactor { get; set; }
		ViewZoomAnchor ThumbnailZoomAnchor { get; set; }
		ViewZoomAnchor OverlayLabelAnchor { get; set; }
		ViewZoomAnchor CycleGroupIndicatorAnchor { get; set; }

		bool ShowThumbnailOverlays { get; set; }
		bool ShowThumbnailFrames { get; set; }

		bool LockThumbnailLocation { get; set; }
		bool ThumbnailSnapToGrid { get; set; }
		int ThumbnailSnapToGridSizeX { get; set; }
		int ThumbnailSnapToGridSizeY { get; set; }

		bool EnableActiveClientHighlight { get; set; }
		Color ActiveClientHighlightColor { get; set; }
		Color PreventPreviewColor { get; set; }
		Color OverlayLabelColor { get; set; }
		Font OverlayLabelFont { get; set; }

		string IconName { get; set; }

		int SelectedCycleGroup { get; set; } // 1..5
		string CycleGroupForwardHotkeysText { get; set; }
		string CycleGroupBackwardHotkeysText { get; set; }

		void SetAvailableClients(IList<string> clients);
		IList<string> GetSelectedClientsForCurrentGroup();
		void SetSelectedClientsForCurrentGroup(IList<string> orderedClients);

		void SetDocumentationUrl(string url);
		void SetVersionInfo(string version);
		void SetThumbnailSizeLimitations(Size minimumSize, Size maximumSize);
		void SetSelectedClientThumbnailSize(Size size, bool hasCustomSize);
		void SetupConfigList();
		void SetProfileStatus(string status);
		void SelectProfileFilename(string filename);
		bool ConfirmProfileOverwrite(string name);
		bool ConfirmResetAllPerClientSizes();

		void Minimize();

		void AddThumbnails(IList<IThumbnailDescription> thumbnails);
		void RemoveThumbnails(IList<IThumbnailDescription> thumbnails);
		void RefreshZoomSettings();

		Action ApplicationExitRequested { get; set; }
		Action<string> LoadNewSettings { get; set; }
		Action SaveSettings { get; set; }

		Action FormActivated { get; set; }
		Action FormMinimized { get; set; }
		Action<ViewCloseRequest> FormCloseRequested { get; set; }
		Action ApplicationSettingsChanged { get; set; }
		Action ThumbnailsSizeChanged { get; set; }
		Action<string> ThumbnailStateChanged { get; set; }
		Action<string> SelectedClientChanged { get; set; }
		Action<string, Size> PerClientSizeApplyRequested { get; set; }
		Action<string> PerClientSizeResetRequested { get; set; }
		Action ResetAllPerClientSizesRequested { get; set; }
		Action<string> SaveProfileRequested { get; set; }
		Action DocumentationLinkActivated { get; set; }
		void InitializeLanguageControls();

		Action SelectedCycleGroupChanged { get; set; }
		void BeginUpdateUI();
		void EndUpdateUI();

	}
}
