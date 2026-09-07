using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using EveOPreview.Configuration;
using EveOPreview.Configuration.Implementation;
using EveOPreview.Mediator.Messages;
using EveOPreview.View;
using MediatR;

namespace EveOPreview.Presenters
{
	public class MainFormPresenter : Presenter<IMainFormView>, IMainFormPresenter
	{
		#region Private constants
		private const string FORUM_URL = @"https://forums.eveonline.com/t/eve-o-preview-v8-0-2-0";
		#endregion

		#region Private fields
		private readonly IMediator _mediator;
		private readonly IThumbnailConfiguration _configuration;
		private readonly IConfigurationStorage _configurationStorage;
		private readonly IDictionary<string, IThumbnailDescription> _descriptionsCache;
		private bool _suppressSizeNotifications;

		private bool _exitApplication;
		private bool _isLoadingUi;
		private int _currentGroup = 1;
		#endregion

		public MainFormPresenter(IApplicationController controller, IMainFormView view, IMediator mediator, IThumbnailConfiguration configuration, IConfigurationStorage configurationStorage)
			: base(controller, view)
		{
			this._mediator = mediator;
			this._configuration = configuration;
			this._configurationStorage = configurationStorage;

			this._descriptionsCache = new Dictionary<string, IThumbnailDescription>();

			this._suppressSizeNotifications = false;
			this._exitApplication = false;

			this.View.FormActivated = this.Activate;
			this.View.FormMinimized = this.Minimize;
			this.View.FormCloseRequested = this.Close;
			this.View.ApplicationSettingsChanged = this.SaveApplicationSettings;
			this.View.ThumbnailsSizeChanged = this.UpdateThumbnailsSize;
			this.View.ThumbnailStateChanged = this.UpdateThumbnailState;
			this.View.DocumentationLinkActivated = this.OpenDocumentationLink;
			this.View.ApplicationExitRequested = this.ExitApplication;
			this.View.LoadNewSettings = this.LoadNewSettings;
			this.View.SaveSettings = this.SaveSettings;
			this.View.SelectedClientChanged = this.LoadSelectedClientSize;
			this.View.PerClientSizeApplyRequested = this.ApplyPerClientSize;
			this.View.PerClientSizeResetRequested = this.ResetPerClientSize;
			this.View.ResetAllPerClientSizesRequested = this.ResetAllPerClientSizes;
			this.View.SaveProfileRequested = this.SaveProfile;
			this.View.IconName = this._configuration.IconName;
		}

		private void Activate()
		{
			this._suppressSizeNotifications = true;
			this.LoadApplicationSettings();
			this.View.SetDocumentationUrl(MainFormPresenter.FORUM_URL);
			this.View.SetVersionInfo(this.GetApplicationVersion());
			if (this._configuration.MinimizeToTray)
			{
				this.View.Minimize();
			}

			this._mediator.Send(new StartService());
			this._suppressSizeNotifications = false;
		}

		private void Minimize()
		{
			if (!this._configuration.MinimizeToTray)
			{
				return;
			}

			this.View.Hide();
		}

		private void Close(ViewCloseRequest request)
		{
			if (this._exitApplication || !this.View.MinimizeToTray)
			{
				this._mediator.Send(new StopService()).Wait();

				this._configurationStorage.Save();
				request.Allow = true;
				return;
			}

			request.Allow = false;
			this.View.Minimize();
		}

		private async void UpdateThumbnailsSize()
		{
			if (!this._suppressSizeNotifications)
			{
				await this.SaveApplicationSettingsAsync();
				await this._mediator.Publish(new ThumbnailConfiguredSizeUpdated());
			}
		}

		private void LoadApplicationSettings()
		{

			this._configurationStorage.Load();
			this._isLoadingUi = true;

			if (!string.IsNullOrEmpty(this._configuration.Language) && this._configuration.Language != "en-US")
			{
				LocalizationExtensions.SetLanguage(this._configuration.Language);
			}
			this.View.InitializeLanguageControls();

			this.View.Language = this._configuration.Language;	

			this.View.MinimizeToTray = this._configuration.MinimizeToTray;

			this.View.ThumbnailOpacity = this._configuration.ThumbnailOpacity;

			this.View.EnableClientLayoutTracking = this._configuration.EnableClientLayoutTracking;
			this.View.HideActiveClientThumbnail = this._configuration.HideActiveClientThumbnail;
			this.View.MinimizeInactiveClients = this._configuration.MinimizeInactiveClients;
			this.View.CaptionOnClientsStyle = ViewCaptionBarStyleConverter.Convert(this._configuration.CaptionOnClientsStyle);
			this.View.WindowsAnimationStyle = ViewAnimationStyleConverter.Convert(this._configuration.WindowsAnimationStyle);
			this.View.ShowThumbnailsAlwaysOnTop = this._configuration.ShowThumbnailsAlwaysOnTop;
			this.View.PreventPreviews = this._configuration.PreventPreviews;
			this.View.HideThumbnailsOnLostFocus = this._configuration.HideThumbnailsOnLostFocus;
			this.View.EnablePerClientThumbnailLayouts = this._configuration.EnablePerClientThumbnailLayouts;
			this.View.QuickSwitchEnabled = this._configuration.QuickSwitchEnabled;
			this.View.QuickHideEnabled = this._configuration.QuickHideEnabled;
			this.View.KeepMinimizedClientsLive = this._configuration.KeepMinimizedClientsLive;

			this.View.SetThumbnailSizeLimitations(this._configuration.ThumbnailMinimumSize, this._configuration.ThumbnailMaximumSize);
			this.View.ThumbnailSize = this._configuration.ThumbnailSize;

			this.View.EnableThumbnailZoom = this._configuration.ThumbnailZoomEnabled;
			this.View.ThumbnailZoomFactor = this._configuration.ThumbnailZoomFactor;
			this.View.ThumbnailZoomAnchor = ViewZoomAnchorConverter.Convert(this._configuration.ThumbnailZoomAnchor);
			this.View.OverlayLabelAnchor = ViewZoomAnchorConverter.Convert(this._configuration.OverlayLabelAnchor);
			this.View.CycleGroupIndicatorAnchor = ViewZoomAnchorConverter.Convert(this._configuration.CycleGroupIndicatorAnchor);

			this.View.ShowThumbnailOverlays = this._configuration.ShowThumbnailOverlays;
			this.View.ShowThumbnailFrames = this._configuration.ShowThumbnailFrames;
			this.View.LockThumbnailLocation = this._configuration.LockThumbnailLocation;
			this.View.ThumbnailSnapToGrid = this._configuration.ThumbnailSnapToGrid;
			this.View.ThumbnailSnapToGridSizeX = this._configuration.ThumbnailSnapToGridSizeX;
			this.View.ThumbnailSnapToGridSizeY = this._configuration.ThumbnailSnapToGridSizeY;
			this.View.EnableActiveClientHighlight = this._configuration.EnableActiveClientHighlight;
			this.View.ActiveClientHighlightColor = this._configuration.ActiveClientHighlightColor;
			this.View.PreventPreviewColor = this._configuration.PreventPreviewColor;

			this.View.OverlayLabelColor = this._configuration.OverlayLabelColor;
			this.View.OverlayLabelFont = this._configuration.OverlayLabelFont;

			this.View.IconName = this._configuration.IconName;

			// Hotkeys tab: populate clients and default group
			var configuredClients = this._configuration.GetAllKnownClients();
			this.View.SetAvailableClients(configuredClients);
			this._currentGroup = 1;
			this.View.SelectedCycleGroup = this._currentGroup;
			this.LoadGroupToView(this._currentGroup);

			// Wire group changed handler
			this.View.SelectedCycleGroupChanged = this.OnSelectedCycleGroupChanged;
			this.View.EndUpdateUI();
			this._isLoadingUi = false;
		}

		private void OnSelectedCycleGroupChanged()
		{
			if (this._isLoadingUi) return;
			// Save current group's UI to config, then load new group's config into UI
			this.SaveGroupFromView(this._currentGroup);
			this._currentGroup = this.View.SelectedCycleGroup;
			this.View.BeginUpdateUI();
			this.LoadGroupToView(this._currentGroup);
			this.View.EndUpdateUI();
			this._configurationStorage.Save();
		}

		private void LoadGroupToView(int group)
		{
			// Set forward/backward hotkeys CSV
			var fwd = GetForwardHotkeys(group) ?? new List<string>();
			var bwd = GetBackwardHotkeys(group) ?? new List<string>();
			this.View.CycleGroupForwardHotkeysText = string.Join(",", fwd);
			this.View.CycleGroupBackwardHotkeysText = string.Join(",", bwd);
			// Set clients order
			var orderDict = GetClientsOrder(group) ?? new Dictionary<string, int>();
			var ordered = orderDict.OrderBy(kv => kv.Value).Select(kv => kv.Key).ToList();
			this.View.SetSelectedClientsForCurrentGroup(ordered);
		}

		private void SaveGroupFromView(int group)
		{
			// Save forward/backward hotkeys
			var fwdCsv = this.View.CycleGroupForwardHotkeysText ?? string.Empty;
			var bwdCsv = this.View.CycleGroupBackwardHotkeysText ?? string.Empty;
			SetForwardHotkeys(group, ParseCsv(fwdCsv));
			SetBackwardHotkeys(group, ParseCsv(bwdCsv));
			// Save clients order (only selected/checked ones, in current order)
			var selected = this.View.GetSelectedClientsForCurrentGroup() ?? new List<string>();
			var dict = new Dictionary<string, int>();
			for (int i = 0; i < selected.Count; i++)
			{
				dict[selected[i]] = i + 1;
			}
			SetClientsOrder(group, dict);
		}

		private static List<string> ParseCsv(string csv)
		{
			var list = new List<string>();
			if (!string.IsNullOrWhiteSpace(csv))
			{
				foreach (var part in csv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					var s = part.Trim();
					if (!string.IsNullOrEmpty(s)) list.Add(s);
				}
			}
			return list;
		}

		private List<string> GetForwardHotkeys(int g) => g switch
		{
			1 => this._configuration.CycleGroup1ForwardHotkeys,
			2 => this._configuration.CycleGroup2ForwardHotkeys,
			3 => this._configuration.CycleGroup3ForwardHotkeys,
			4 => this._configuration.CycleGroup4ForwardHotkeys,
			5 => this._configuration.CycleGroup5ForwardHotkeys,
			_ => this._configuration.CycleGroup1ForwardHotkeys,
		};
		private List<string> GetBackwardHotkeys(int g) => g switch
		{
			1 => this._configuration.CycleGroup1BackwardHotkeys,
			2 => this._configuration.CycleGroup2BackwardHotkeys,
			3 => this._configuration.CycleGroup3BackwardHotkeys,
			4 => this._configuration.CycleGroup4BackwardHotkeys,
			5 => this._configuration.CycleGroup5BackwardHotkeys,
			_ => this._configuration.CycleGroup1BackwardHotkeys,
		};
		private Dictionary<string, int> GetClientsOrder(int g) => g switch
		{
			1 => this._configuration.CycleGroup1ClientsOrder,
			2 => this._configuration.CycleGroup2ClientsOrder,
			3 => this._configuration.CycleGroup3ClientsOrder,
			4 => this._configuration.CycleGroup4ClientsOrder,
			5 => this._configuration.CycleGroup5ClientsOrder,
			_ => this._configuration.CycleGroup1ClientsOrder,
		};
		private void SetForwardHotkeys(int g, List<string> v)
		{
			switch (g)
			{
				case 1: this._configuration.CycleGroup1ForwardHotkeys = v; break;
				case 2: this._configuration.CycleGroup2ForwardHotkeys = v; break;
				case 3: this._configuration.CycleGroup3ForwardHotkeys = v; break;
				case 4: this._configuration.CycleGroup4ForwardHotkeys = v; break;
				case 5: this._configuration.CycleGroup5ForwardHotkeys = v; break;
			}
		}
		private void SetBackwardHotkeys(int g, List<string> v)
		{
			switch (g)
			{
				case 1: this._configuration.CycleGroup1BackwardHotkeys = v; break;
				case 2: this._configuration.CycleGroup2BackwardHotkeys = v; break;
				case 3: this._configuration.CycleGroup3BackwardHotkeys = v; break;
				case 4: this._configuration.CycleGroup4BackwardHotkeys = v; break;
				case 5: this._configuration.CycleGroup5BackwardHotkeys = v; break;
			}
		}
		private void SetClientsOrder(int g, Dictionary<string, int> v)
		{
			switch (g)
			{
				case 1: this._configuration.CycleGroup1ClientsOrder = v; break;
				case 2: this._configuration.CycleGroup2ClientsOrder = v; break;
				case 3: this._configuration.CycleGroup3ClientsOrder = v; break;
				case 4: this._configuration.CycleGroup4ClientsOrder = v; break;
				case 5: this._configuration.CycleGroup5ClientsOrder = v; break;
			}
		}

		private async void SaveApplicationSettings()
		{
			await this.SaveApplicationSettingsAsync();
		}

		private async Task SaveApplicationSettingsAsync()
		{
			if (this._isLoadingUi) return;
			this._configuration.MinimizeToTray = this.View.MinimizeToTray;

			this._configuration.ThumbnailOpacity = (float)this.View.ThumbnailOpacity;

			if (this._configuration.Language != this.View.Language) {
				this._configuration.Language = this.View.Language;
			}

			this._configuration.EnableClientLayoutTracking = this.View.EnableClientLayoutTracking;
			this._configuration.HideActiveClientThumbnail = this.View.HideActiveClientThumbnail;
			this._configuration.MinimizeInactiveClients = this.View.MinimizeInactiveClients;

			this._configuration.WindowsAnimationStyle = ViewAnimationStyleConverter.Convert(this.View.WindowsAnimationStyle);

			this._configuration.CaptionOnClientsStyle= ViewCaptionBarStyleConverter.Convert(this.View.CaptionOnClientsStyle);
			await this._mediator.Publish(new ThumbnailFrameSettingsUpdated());

			this._configuration.ShowThumbnailsAlwaysOnTop = this.View.ShowThumbnailsAlwaysOnTop;

			if (this._configuration.PreventPreviews != this.View.PreventPreviews)
			{
				this._configuration.PreventPreviews = this.View.PreventPreviews;
				await this._mediator.Publish(new ThumbnailFrameSettingsUpdated());
			}

			this._configuration.HideThumbnailsOnLostFocus = this.View.HideThumbnailsOnLostFocus;
			this._configuration.EnablePerClientThumbnailLayouts = this.View.EnablePerClientThumbnailLayouts;
			this._configuration.QuickSwitchEnabled = this.View.QuickSwitchEnabled;
			this._configuration.QuickHideEnabled = this.View.QuickHideEnabled;
			this._configuration.KeepMinimizedClientsLive = this.View.KeepMinimizedClientsLive;
			if (this._configuration.KeepMinimizedClientsLive)
			{
				this._configuration.MinimizeInactiveClients = false;
			}

			this._configuration.ThumbnailSize = this.View.ThumbnailSize;

			this._configuration.ThumbnailZoomEnabled = this.View.EnableThumbnailZoom;
			this._configuration.ThumbnailZoomFactor = this.View.ThumbnailZoomFactor;
			this._configuration.ThumbnailZoomAnchor = ViewZoomAnchorConverter.Convert(this.View.ThumbnailZoomAnchor);
			this._configuration.OverlayLabelAnchor = ViewZoomAnchorConverter.Convert(this.View.OverlayLabelAnchor);

			if (this._configuration.CycleGroupIndicatorAnchor != ViewZoomAnchorConverter.Convert(this.View.CycleGroupIndicatorAnchor))
			{
				this._configuration.CycleGroupIndicatorAnchor = ViewZoomAnchorConverter.Convert(this.View.CycleGroupIndicatorAnchor);
				await this._mediator.Publish(new ThumbnailCycleGroupIndicatorUpdated());
			}

			this._configuration.ShowThumbnailOverlays = this.View.ShowThumbnailOverlays;
			if (this._configuration.ShowThumbnailFrames != this.View.ShowThumbnailFrames)
			{
				this._configuration.ShowThumbnailFrames = this.View.ShowThumbnailFrames;
				await this._mediator.Publish(new ThumbnailFrameSettingsUpdated());
			}

            this._configuration.LockThumbnailLocation = this.View.LockThumbnailLocation;
			this._configuration.ThumbnailSnapToGrid = this.View.ThumbnailSnapToGrid;
			this._configuration.ThumbnailSnapToGridSizeX = this.View.ThumbnailSnapToGridSizeX;
            this._configuration.ThumbnailSnapToGridSizeY = this.View.ThumbnailSnapToGridSizeY;

            this._configuration.EnableActiveClientHighlight = this.View.EnableActiveClientHighlight;
			this._configuration.ActiveClientHighlightColor = this.View.ActiveClientHighlightColor;

			if (this._configuration.PreventPreviewColor != this.View.PreventPreviewColor)
			{
				this._configuration.PreventPreviewColor = this.View.PreventPreviewColor;
				await this._mediator.Publish(new ThumbnailFrameSettingsUpdated());
			}

			this._configuration.OverlayLabelColor = this.View.OverlayLabelColor;
			this._configuration.OverlayLabelFont = this.View.OverlayLabelFont;

			this._configuration.IconName = this.View.IconName;

			this.SaveGroupFromView(this._currentGroup);
			this._configurationStorage.Save();
			this.View.RefreshZoomSettings();
			await this._mediator.Publish(new ThumbnailUpdateClientsLayouts());
			await this._mediator.Send(new SaveConfiguration());
			await this._mediator.Publish(new HotkeysConfigurationUpdated());
		}


		public void AddThumbnails(IList<string> thumbnailTitles)
		{
			IList<IThumbnailDescription> descriptions = new List<IThumbnailDescription>(thumbnailTitles.Count);

			lock (this._descriptionsCache)
			{
				foreach (string title in thumbnailTitles)
				{
					IThumbnailDescription description = this.CreateThumbnailDescription(title);
					this._descriptionsCache[title] = description;

					descriptions.Add(description);
				}
			}

			this.View.AddThumbnails(descriptions);
		}

		public void RemoveThumbnails(IList<string> thumbnailTitles)
		{
			IList<IThumbnailDescription> descriptions = new List<IThumbnailDescription>(thumbnailTitles.Count);

			lock (this._descriptionsCache)
			{
				foreach (string title in thumbnailTitles)
				{
					if (!this._descriptionsCache.TryGetValue(title, out IThumbnailDescription description))
					{
						continue;
					}

					this._descriptionsCache.Remove(title);
					descriptions.Add(description);
				}
			}

			this.View.RemoveThumbnails(descriptions);
		}

		private IThumbnailDescription CreateThumbnailDescription(string title)
		{
			bool isDisabled = this._configuration.IsThumbnailDisabled(title);
			return new ThumbnailDescription(title, isDisabled);
		}

		private async void UpdateThumbnailState(String title)
		{
			if (this._descriptionsCache.TryGetValue(title, out IThumbnailDescription description))
			{
				this._configuration.ToggleThumbnail(title, description.IsDisabled);
			}

			await this._mediator.Send(new SaveConfiguration());
		}

		public void UpdateThumbnailSize(Size size)
		{
			this._suppressSizeNotifications = true;
			this.View.ThumbnailSize = size;
			this._suppressSizeNotifications = false;
		}

		private void LoadSelectedClientSize(string title)
		{
			if (string.IsNullOrEmpty(title))
			{
				return;
			}

			bool hasCustomSize = this._configuration.PerClientThumbnailSize.TryGetValue(title, out Size size);
			this.View.SetSelectedClientThumbnailSize(hasCustomSize ? size : this._configuration.ThumbnailSize, hasCustomSize);
		}

		private async void ApplyPerClientSize(string title, Size size)
		{
			if (string.IsNullOrEmpty(title))
			{
				return;
			}

			size = new Size(
				Math.Max(this._configuration.ThumbnailMinimumSize.Width, Math.Min(this._configuration.ThumbnailMaximumSize.Width, size.Width)),
				Math.Max(this._configuration.ThumbnailMinimumSize.Height, Math.Min(this._configuration.ThumbnailMaximumSize.Height, size.Height)));
			this._configuration.PerClientThumbnailSize[title] = size;
			this._configurationStorage.Save();
			this.View.SetSelectedClientThumbnailSize(size, true);
			await this._mediator.Publish(new ThumbnailConfiguredSizeUpdated());
		}

		private async void ResetPerClientSize(string title)
		{
			if (string.IsNullOrEmpty(title))
			{
				return;
			}

			this._configuration.PerClientThumbnailSize.Remove(title);
			this._configurationStorage.Save();
			this.View.SetSelectedClientThumbnailSize(this._configuration.ThumbnailSize, false);
			await this._mediator.Publish(new ThumbnailConfiguredSizeUpdated());
		}

		private async void ResetAllPerClientSizes()
		{
			if (!this.View.ConfirmResetAllPerClientSizes())
			{
				return;
			}

			this._configuration.PerClientThumbnailSize.Clear();
			this._configurationStorage.Save();
			await this._mediator.Publish(new ThumbnailConfiguredSizeUpdated());
			this.View.SetSelectedClientThumbnailSize(this._configuration.ThumbnailSize, false);
			this.View.SetProfileStatus(LocalizationExtensions.GetString("Messages.AllClientSizesReset", "All custom client sizes were reset."));
		}

		private async void SaveProfile(string requestedName)
		{
			string name = (requestedName ?? string.Empty).Trim();
			char[] invalidCharacters = Path.GetInvalidFileNameChars();
			name = new string(name.Where(character => !invalidCharacters.Contains(character)).ToArray()).Trim().Trim('.');
			if (string.IsNullOrWhiteSpace(name))
			{
				this.View.SetProfileStatus(LocalizationExtensions.GetString("Messages.ProfileNameRequired", "Enter a profile name."));
				return;
			}

			await this.SaveApplicationSettingsAsync();
			string filename = $"Eve-O-Preview-{name}.json";
			if (File.Exists(filename) && !this.View.ConfirmProfileOverwrite(name))
			{
				return;
			}
			this._configurationStorage.SetConfigurationFilename(filename);
			this._configurationStorage.Save();
			this.View.SetupConfigList();
			this.View.SelectProfileFilename(filename);
			this.View.SetProfileStatus(string.Format(
				LocalizationExtensions.GetString("Messages.ProfileSaved", "Saved profile: {0}"),
				name));
		}

		private void OpenDocumentationLink()
		{
			// funtimes
			// https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
			// https://github.com/dotnet/runtime/issues/17938

			// TODO Move out to a separate service / presenter / message handler
#if LINUX
			Process.Start("xdg-open", new Uri(MainFormPresenter.FORUM_URL).AbsoluteUri);
#else
			ProcessStartInfo processStartInfo = new ProcessStartInfo(new Uri(MainFormPresenter.FORUM_URL).AbsoluteUri);
			processStartInfo.UseShellExecute = true;
			Process.Start(processStartInfo);
#endif
		}

		private string GetApplicationVersion()
		{
			Version version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
			string target = "Windows";
#if LINUX
  target = "Linux";
#endif
			return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision} {target}";
		}

		private void ExitApplication()
		{
			this._exitApplication = true;
			this.View.Close();
		}

		public List<string> GetActiveClients()
		{
			return _descriptionsCache?.Select(x => x.Value.Title).ToList();
		}

		public async void LoadNewSettings(string filename)
		{
			if (filename != null && filename.Length > 0)
			{
				await this.SaveApplicationSettingsAsync();
				this._configurationStorage.SetConfigurationFilename(filename);
			}

			this.LoadApplicationSettings();
			if (!string.IsNullOrEmpty(filename))
			{
				this.View.SelectProfileFilename(filename);
			}

			await this._mediator.Publish(new ThumbnailFrameSettingsUpdated());
			await this._mediator.Publish(new ThumbnailApplyAllClientsLayouts());
			await this._mediator.Publish(new ThumbnailCycleGroupIndicatorUpdated());
			this.View.RefreshZoomSettings();
			await this._mediator.Publish(new HotkeysConfigurationUpdated());
		}

		public void SaveSettings()
		{
			this.SaveApplicationSettings();
		}
	}
}
