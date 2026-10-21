using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Newtonsoft.Json;
using EveOPreview.UI.Hotkeys;

namespace EveOPreview.Configuration.Implementation
{
	sealed class ThumbnailConfiguration : IThumbnailConfiguration
	{
		private static readonly Size CustomMinimumThumbnailSize = new Size(64, 36);

		#region Private fields
		private bool _enablePerClientThumbnailLayouts;
		private bool _enableClientLayoutTracking;
		private List<CycleGroupDefinition> _cycleGroups = new List<CycleGroupDefinition>();
		private bool _isDeserializingGroups;
		private bool _hasDeserializedCycleGroups;
		private List<CycleGroupDefinition> _deserializedCycleGroups;
		private Dictionary<int, List<string>> _deserializedForwardHotkeys;
		private Dictionary<int, List<string>> _deserializedBackwardHotkeys;
		private Dictionary<int, Dictionary<string, int>> _deserializedClientOrders;
		#endregion

		public ThumbnailConfiguration()
		{
			this.ConfigVersion = 1;

			this.Language = "zh-CN";

			this.CycleGroup1ForwardHotkeys = new List<string> { "F14", "Control+F14" };
			this.CycleGroup1BackwardHotkeys = new List<string> { "F13", "Control+F13" };
			this.CycleGroup1ClientsOrder = new Dictionary<string, int>
			{
				{ "EVE - Example DPS Toon 1", 1 },
				{ "EVE - Example DPS Toon 2", 2 },
				{ "EVE - Example DPS Toon 3", 3 }
			};

			this.CycleGroup2ForwardHotkeys = new List<string> { "F16", "Control+F16" };
			this.CycleGroup2BackwardHotkeys = new List<string> { "F15", "Control+F15" };
			this.CycleGroup2ClientsOrder = new Dictionary<string, int>
			{
				{ "EVE - Example Logi Toon 1", 1 },
				{ "EVE - Example Scout Toon 2", 2 },
				{ "EVE - Example Tackle Toon 3", 3 }
			};

			this.CycleGroup3ForwardHotkeys = new List<string> { "" };
			this.CycleGroup3BackwardHotkeys = new List<string> { "" };
			this.CycleGroup3ClientsOrder = new Dictionary<string, int>
			{
				{ "EVE - cycle group 3", 1 },
			};
			this.CycleGroup4ForwardHotkeys = new List<string> { "" };
			this.CycleGroup4BackwardHotkeys = new List<string> { "" };
			this.CycleGroup4ClientsOrder = new Dictionary<string, int>
			{
				{ "EVE - cycle group 4", 1 },
			};
			this.CycleGroup5ForwardHotkeys = new List<string> { "" };
			this.CycleGroup5BackwardHotkeys = new List<string> { "" };
			this.CycleGroup5ClientsOrder = new Dictionary<string, int>
			{
				{ "EVE - cycle group 5", 1 },
			};
			this.AutoAddUngroupedClients = false;
			this.CycleGroupAutoAddExclusions = new List<string>();

			// Simple global shortcuts for users who do not need advanced cycle groups.
			// These only change the foreground EVE client; they do not broadcast input.
			this.QuickSwitchEnabled = true;
			this.QuickSwitchForwardHotkey = "Control+Alt+Right";
			this.QuickSwitchBackwardHotkey = "Control+Alt+Left";
			this.QuickHideEnabled = true;
			this.QuickHideHotkey = "Control+Alt+H";
			this.KeepMinimizedClientsLive = false;

			this.PerClientActiveClientHighlightColor = new Dictionary<string, Color>
			{
				{"EVE - Example Toon 1", Color.Red},
				{"EVE - Example Toon 2", Color.Green}
			};
			this.PerClientPreventPreviewColor = new Dictionary<string, Color>
			{
				{"EVE - Example Toon 1", Color.Red},
				{"EVE - Example Toon 2", Color.Green}
			};
			this.PerClientPreventPreviews = new Dictionary<string, bool>
			{
				{"EVE - Example Toon 1", false},
				{"EVE - Example Toon 2", true}
			};

			this.PerClientAliases = new Dictionary<string, string>
			{
				{"EVE - Example Toon 1", "Main"},
				{"EVE - Example Toon 2", "Alt"}
			};

			this.PerClientThumbnailSize = new Dictionary<string, Size>
			{
				{"EVE - Example Toon 1", new Size(200, 200)},
				{"EVE - Example Toon 2", new Size(200, 200)}
			};

			this.PerClientZoomAnchor = new Dictionary<string, ZoomAnchor>
			{
				{"EVE - Example Toon 1", ZoomAnchor.N },
				{"EVE - Example Toon 2", ZoomAnchor.S}
			};

			this.PerClientLayout = new Dictionary<string, Dictionary<string, Point>>();
			this.FlatLayout = new Dictionary<string, Point>();
			this.ClientLayout = new Dictionary<string, ClientLayout>();
			this.ClientHotkey = new Dictionary<string, string>();
			this.MinimizeAllClientsHotkeys = new List<string> { "Control+F22" };
			this.RefreshMinimizedClientsHotkeys = new List<string> { "Control+F21" };
			this.DisableThumbnail = new Dictionary<string, bool>();
			this.PriorityClients = new List<string>();

			this.ExecutablesToPreview = new List<string> { "exefile" };

			this.MinimizeToTray = false;
			this.ThumbnailRefreshPeriod = 500;
			this.ThumbnailResizeTimeoutPeriod = 500;

#if LINUX
			this.EnableWineCompatibilityMode = true;
#else
			this.EnableWineCompatibilityMode = false;
#endif

			this.ThumbnailOpacity = 0.5;

			this.EnableClientLayoutTracking = false;
			this.HideActiveClientThumbnail = false;
			this.HideLoginClientThumbnail = false;
			this.MinimizeInactiveClients = false;
			this.CaptionOnClientsStyle = CaptionBarStyle.DoNothing;
			this.WindowsAnimationStyle = AnimationStyle.NoAnimation;
			this.ShowThumbnailsAlwaysOnTop = true;
			this.EnablePerClientThumbnailLayouts = false;

			this.HideThumbnailsOnLostFocus = false;
			this.PreventPreviews = false;
			this.HideThumbnailsDelay = 2; // 2 thumbnails refresh cycles (1.0 sec)

			this.ThumbnailSize = new Size(384, 216);
			this.ThumbnailMinimumSize = CustomMinimumThumbnailSize;
			this.ThumbnailMaximumSize = new Size(960, 540);

			this.EnableThumbnailSnap = true;

			this.ThumbnailZoomEnabled = false;
			this.ThumbnailZoomFactor = 2;
			this.ThumbnailZoomAnchor = ZoomAnchor.NW;
			this.OverlayLabelAnchor = ZoomAnchor.NW;
			this.CycleGroupIndicatorAnchor = ZoomAnchor.NW;

			this.ShowThumbnailOverlays = true;
			this.ShowThumbnailFrames = false;
			this.LockThumbnailLocation = false;

			this.ThumbnailSnapToGrid = true;
			this.ThumbnailSnapToGridSizeX = 100;
			this.ThumbnailSnapToGridSizeY = 50;

            this.EnableActiveClientHighlight = false;
			this.ActiveClientHighlightColor = Color.GreenYellow;
			this.PreventPreviewColor = Color.Purple;
			this.ActiveClientHighlightThickness = 3;

			this.OverlayLabelColor = Color.Orange;
			this.OverlayLabelFont = new Font(FontFamily.GenericSansSerif,10.0F, FontStyle.Bold);

			this.IconName = "GloryNavy";

			this.LoginThumbnailLocation = new Point(5, 5);
		}


		[JsonProperty("ConfigVersion")]
		public int ConfigVersion { get; set; }

		[JsonProperty("Language")]
		public string Language { get; set; }

		[JsonIgnore]
		public Dictionary<string, bool> CycleGroupExclusions { get; set; }

		[JsonProperty("CycleGroups", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public List<CycleGroupDefinition> CycleGroups
		{
			get => this._cycleGroups;
			set
			{
				if (this._isDeserializingGroups)
				{
					this._hasDeserializedCycleGroups = true;
					this._deserializedCycleGroups = value;
				}
				else this._cycleGroups = value;
			}
		}

		public bool AutoAddUngroupedClients { get; set; }
		public List<string> CycleGroupAutoAddExclusions { get; set; }

		[JsonProperty("CycleGroup1ForwardHotkeys", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public List<string> CycleGroup1ForwardHotkeys { get => this.GetLegacyForwardHotkeys(0); set => this.SetLegacyForwardHotkeys(0, value); }

		[JsonProperty("CycleGroup1BackwardHotkeys", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public List<string> CycleGroup1BackwardHotkeys { get => this.GetLegacyBackwardHotkeys(0); set => this.SetLegacyBackwardHotkeys(0, value); }

		[JsonProperty("CycleGroup1ClientsOrder", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public Dictionary<string, int> CycleGroup1ClientsOrder { get => this.GetLegacyClientsOrder(0); set => this.SetLegacyClientsOrder(0, value); }

		[JsonProperty("CycleGroup2ForwardHotkeys", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public List<string> CycleGroup2ForwardHotkeys { get => this.GetLegacyForwardHotkeys(1); set => this.SetLegacyForwardHotkeys(1, value); }

		[JsonProperty("CycleGroup2BackwardHotkeys", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public List<string> CycleGroup2BackwardHotkeys { get => this.GetLegacyBackwardHotkeys(1); set => this.SetLegacyBackwardHotkeys(1, value); }

		[JsonProperty("CycleGroup2ClientsOrder", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public Dictionary<string, int> CycleGroup2ClientsOrder { get => this.GetLegacyClientsOrder(1); set => this.SetLegacyClientsOrder(1, value); }

		[JsonProperty("CycleGroup3ForwardHotkeys", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public List<string> CycleGroup3ForwardHotkeys { get => this.GetLegacyForwardHotkeys(2); set => this.SetLegacyForwardHotkeys(2, value); }

		[JsonProperty("CycleGroup3BackwardHotkeys", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public List<string> CycleGroup3BackwardHotkeys { get => this.GetLegacyBackwardHotkeys(2); set => this.SetLegacyBackwardHotkeys(2, value); }

		[JsonProperty("CycleGroup3ClientsOrder", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public Dictionary<string, int> CycleGroup3ClientsOrder { get => this.GetLegacyClientsOrder(2); set => this.SetLegacyClientsOrder(2, value); }

		[JsonProperty("CycleGroup4ForwardHotkeys", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public List<string> CycleGroup4ForwardHotkeys { get => this.GetLegacyForwardHotkeys(3); set => this.SetLegacyForwardHotkeys(3, value); }

		[JsonProperty("CycleGroup4BackwardHotkeys", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public List<string> CycleGroup4BackwardHotkeys { get => this.GetLegacyBackwardHotkeys(3); set => this.SetLegacyBackwardHotkeys(3, value); }

		[JsonProperty("CycleGroup4ClientsOrder", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public Dictionary<string, int> CycleGroup4ClientsOrder { get => this.GetLegacyClientsOrder(3); set => this.SetLegacyClientsOrder(3, value); }

		[JsonProperty("CycleGroup5ForwardHotkeys", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public List<string> CycleGroup5ForwardHotkeys { get => this.GetLegacyForwardHotkeys(4); set => this.SetLegacyForwardHotkeys(4, value); }

		[JsonProperty("CycleGroup5BackwardHotkeys", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public List<string> CycleGroup5BackwardHotkeys { get => this.GetLegacyBackwardHotkeys(4); set => this.SetLegacyBackwardHotkeys(4, value); }

		[JsonProperty("CycleGroup5ClientsOrder", ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public Dictionary<string, int> CycleGroup5ClientsOrder { get => this.GetLegacyClientsOrder(4); set => this.SetLegacyClientsOrder(4, value); }

		[JsonProperty("QuickSwitchEnabled")]
		public bool QuickSwitchEnabled { get; set; }

		[JsonProperty("QuickSwitchForwardHotkey")]
		public string QuickSwitchForwardHotkey { get; set; }

		[JsonProperty("QuickSwitchBackwardHotkey")]
		public string QuickSwitchBackwardHotkey { get; set; }

		[JsonProperty("QuickHideEnabled")]
		public bool QuickHideEnabled { get; set; }

		[JsonProperty("QuickHideHotkey")]
		public string QuickHideHotkey { get; set; }

		[JsonProperty("KeepMinimizedClientsLive")]
		public bool KeepMinimizedClientsLive { get; set; }

		[JsonProperty("PerClientPreventPreviewColor")]
		public Dictionary<string, Color> PerClientPreventPreviewColor { get; set; }

		[JsonProperty("PerClientActiveClientHighlightColor")]
		public Dictionary<string, Color> PerClientActiveClientHighlightColor { get; set; }

		[JsonProperty("PerClientPreventPreviews")]
		public Dictionary<string, bool> PerClientPreventPreviews { get; set; }

		[JsonProperty("PerClientAliases")]
		public Dictionary<string, string> PerClientAliases { get; set; }

		[JsonProperty("PerClientThumbnailSize")]
		public Dictionary<string, Size> PerClientThumbnailSize { get; set; }

		[JsonProperty("PerClientZoomAnchor")]
		public Dictionary<string, ZoomAnchor> PerClientZoomAnchor{ get; set; }
		public bool MinimizeToTray { get; set; }
		public int ThumbnailRefreshPeriod { get; set; }
		public int ThumbnailResizeTimeoutPeriod { get; set; }

		[JsonProperty("WineCompatibilityMode")]
		public bool EnableWineCompatibilityMode { get; set; }

		[JsonProperty("ThumbnailsOpacity")]
		public double ThumbnailOpacity { get; set; }

		public bool EnableClientLayoutTracking
		{
			get => this._enableClientLayoutTracking;
			set
			{
				if (!value)
				{
					this.ClientLayout.Clear();
				}

				this._enableClientLayoutTracking = value;
			}
		}

		public bool HideActiveClientThumbnail { get; set; }
		public bool HideLoginClientThumbnail { get; set; }
		public bool MinimizeInactiveClients { get; set; }
		public CaptionBarStyle CaptionOnClientsStyle { get; set; }
		public AnimationStyle WindowsAnimationStyle { get; set; }
		public bool ShowThumbnailsAlwaysOnTop { get; set; }

		public bool EnablePerClientThumbnailLayouts
		{
			get => this._enablePerClientThumbnailLayouts;
			set
			{
				if (!value)
				{
					this.PerClientLayout.Clear();
				}

				this._enablePerClientThumbnailLayouts = value;
			}
		}

		public bool PreventPreviews { get; set; }
		public bool HideThumbnailsOnLostFocus { get; set; }
		public int HideThumbnailsDelay { get; set; }

		public Size ThumbnailSize { get; set; }
		public Size ThumbnailMaximumSize { get; set; }
		public Size ThumbnailMinimumSize { get; set; }

		public bool EnableThumbnailSnap { get; set; }

		[JsonProperty("EnableThumbnailZoom")]
		public bool ThumbnailZoomEnabled { get; set; }
		public int ThumbnailZoomFactor { get; set; }
		public ZoomAnchor ThumbnailZoomAnchor { get; set; }
		public ZoomAnchor OverlayLabelAnchor { get; set; }
		public ZoomAnchor CycleGroupIndicatorAnchor { get; set; }

		public bool ShowThumbnailOverlays { get; set; }
		public bool ShowThumbnailFrames { get; set; }
		public bool LockThumbnailLocation { get; set; }
		public bool ThumbnailSnapToGrid { get; set; }
		public int ThumbnailSnapToGridSizeX {  get; set; }
		public int ThumbnailSnapToGridSizeY { get; set; }

		public bool EnableActiveClientHighlight { get; set; }

		public Color ActiveClientHighlightColor { get; set; }
		public Color PreventPreviewColor { get; set; }
		public Color OverlayLabelColor { get; set; }

		[JsonProperty]
		public Font OverlayLabelFont { get; set; }
		public string IconName { get; set; }

		public int ActiveClientHighlightThickness { get; set; }

		[JsonProperty("LoginThumbnailLocation")]
		public Point LoginThumbnailLocation { get; set; }

		[JsonProperty]
		private Dictionary<string, Dictionary<string, Point>> PerClientLayout { get; set; }
		[JsonProperty]
		private Dictionary<string, Point> FlatLayout { get; set; }
		[JsonProperty]
		private Dictionary<string, ClientLayout> ClientLayout { get; set; }
		[JsonProperty]
		public Dictionary<string, string> ClientHotkey { get; set; }
		[JsonProperty]
		public List<string> MinimizeAllClientsHotkeys { get; set; }
		[JsonProperty]
		public List<string> RefreshMinimizedClientsHotkeys { get; set; }
		[JsonProperty]
		private Dictionary<string, bool> DisableThumbnail { get; set; }
		[JsonProperty]
		private List<string> PriorityClients { get; set; }
		[JsonProperty]
		private List<string> ExecutablesToPreview { get; set; }

		public Point GetThumbnailLocation(string currentClient, string activeClient, Point defaultLocation)
		{
			Point location;

			// What this code does:
			// If Per-Client layouts are enabled
			//    and client name is known
			//    and there is a separate thumbnails layout for this client
			//    and this layout contains an entry for the current client
			// then return that entry
			// otherwise try to get client layout from the flat all-clients layout
			// If there is no layout too then use the default one
			if (this.EnablePerClientThumbnailLayouts && !string.IsNullOrEmpty(activeClient))
			{
				Dictionary<string, Point> layoutSource;
				if (this.PerClientLayout.TryGetValue(activeClient, out layoutSource) && layoutSource.TryGetValue(currentClient, out location))
				{
					return location;
				}
			}

			return this.FlatLayout.TryGetValue(currentClient, out location) ? location : defaultLocation;
		}

		public Size GetThumbnailSize(string currentClient, string activeClient, Size defaultSize)
		{
			Size sizeOfThumbnail;
			return this.PerClientThumbnailSize.TryGetValue(currentClient, out sizeOfThumbnail) ? sizeOfThumbnail : defaultSize;
		}
		public ZoomAnchor GetZoomAnchor(string currentClient, ZoomAnchor defaultZoomAnchor)
		{
			ZoomAnchor zoomAnchor;
			return this.PerClientZoomAnchor.TryGetValue(currentClient, out zoomAnchor) ? zoomAnchor : defaultZoomAnchor;
		}

		public void SetThumbnailLocation(string currentClient, string activeClient, Point location)
		{
			Dictionary<string, Point> layoutSource;

			if (this.EnablePerClientThumbnailLayouts)
			{
				if (string.IsNullOrEmpty(activeClient))
				{
					return;
				}

				if (!this.PerClientLayout.TryGetValue(activeClient, out layoutSource))
				{
					layoutSource = new Dictionary<string, Point>();
					this.PerClientLayout[activeClient] = layoutSource;
				}
			}
			else
			{
				layoutSource = this.FlatLayout;
			}

			layoutSource[currentClient] = location;
		}

		public ClientLayout GetClientLayout(string currentClient)
		{
			ClientLayout layout;
			this.ClientLayout.TryGetValue(currentClient, out layout);

			return layout;
		}

		public void SetClientLayout(string currentClient, ClientLayout layout)
		{
			this.ClientLayout[currentClient] = layout;
		}

		public Keys GetClientHotkey(string currentClient)
		{
			return this.ClientHotkey != null && currentClient != null && this.ClientHotkey.TryGetValue(currentClient, out string hotkey)
				? this.StringToKey(hotkey) : Keys.None;
		}

		public void SetClientHotkey(string currentClient, Keys hotkey)
		{
			if (string.IsNullOrWhiteSpace(currentClient)) return;
			this.ClientHotkey ??= new Dictionary<string, string>();
			if (hotkey == Keys.None)
			{
				this.ClientHotkey.Remove(currentClient);
				return;
			}
			this.ClientHotkey[currentClient] = HotkeyGesture.Format(hotkey);
		}

		public Keys StringToKey(string hotkey)
		{
			return HotkeyGesture.TryParse(hotkey, out Keys key) ? key : Keys.None;
		}

		public bool IsPriorityClient(string currentClient)
		{
			return this.PriorityClients.Contains(currentClient);
		}
		public bool IsExecutableToPreview(string processName)
		{
			return this.ExecutablesToPreview.Any(s => s.Equals(processName, StringComparison.OrdinalIgnoreCase));
		}

		public bool IsThumbnailDisabled(string currentClient)
		{
			return this.DisableThumbnail.TryGetValue(currentClient, out bool isDisabled) && isDisabled;
		}

		public void ToggleThumbnail(string currentClient, bool isDisabled)
		{
			this.DisableThumbnail[currentClient] = isDisabled;
		}

		private CycleGroupDefinition GetLegacyGroup(int index, bool create)
		{
			if (create)
			{
				this._cycleGroups ??= new List<CycleGroupDefinition>();
				while (this._cycleGroups.Count <= index)
				{
					int number = this._cycleGroups.Count + 1;
					this._cycleGroups.Add(new CycleGroupDefinition { Id = $"legacy-{number}", Name = $"Group {number}" });
				}
				this._cycleGroups[index] ??= new CycleGroupDefinition { Name = $"Group {index + 1}" };
			}
			return this._cycleGroups != null && index < this._cycleGroups.Count ? this._cycleGroups[index] : null;
		}

		private List<string> GetLegacyForwardHotkeys(int index) => this.GetLegacyGroup(index, false)?.ForwardHotkeys ?? new List<string>();
		private List<string> GetLegacyBackwardHotkeys(int index) => this.GetLegacyGroup(index, false)?.BackwardHotkeys ?? new List<string>();
		private Dictionary<string, int> GetLegacyClientsOrder(int index) => this.GetLegacyGroup(index, false)?.ClientsOrder ?? new Dictionary<string, int>();

		private void SetLegacyForwardHotkeys(int index, List<string> value)
		{
			if (this._isDeserializingGroups) this._deserializedForwardHotkeys[index] = value;
			else this.GetLegacyGroup(index, true).ForwardHotkeys = value ?? new List<string>();
		}

		private void SetLegacyBackwardHotkeys(int index, List<string> value)
		{
			if (this._isDeserializingGroups) this._deserializedBackwardHotkeys[index] = value;
			else this.GetLegacyGroup(index, true).BackwardHotkeys = value ?? new List<string>();
		}

		private void SetLegacyClientsOrder(int index, Dictionary<string, int> value)
		{
			if (this._isDeserializingGroups) this._deserializedClientOrders[index] = value;
			else this.GetLegacyGroup(index, true).ClientsOrder = value ?? new Dictionary<string, int>();
		}

		[OnDeserializing]
		private void BeginDeserializeCycleGroups(StreamingContext context)
		{
			this._isDeserializingGroups = true;
			this._hasDeserializedCycleGroups = false;
			this._deserializedCycleGroups = null;
			this._deserializedForwardHotkeys = new Dictionary<int, List<string>>();
			this._deserializedBackwardHotkeys = new Dictionary<int, List<string>>();
			this._deserializedClientOrders = new Dictionary<int, Dictionary<string, int>>();
		}

		[OnDeserialized]
		private void FinishDeserializeCycleGroups(StreamingContext context)
		{
			// Stage legacy properties separately so JSON property order cannot overwrite the canonical collection.
			this._isDeserializingGroups = false;
			if (this._hasDeserializedCycleGroups) this._cycleGroups = this._deserializedCycleGroups;
			else
			{
				foreach (var value in this._deserializedForwardHotkeys) this.SetLegacyForwardHotkeys(value.Key, value.Value);
				foreach (var value in this._deserializedBackwardHotkeys) this.SetLegacyBackwardHotkeys(value.Key, value.Value);
				foreach (var value in this._deserializedClientOrders) this.SetLegacyClientsOrder(value.Key, value.Value);
			}
			this._deserializedCycleGroups = null;
			this._deserializedForwardHotkeys = null;
			this._deserializedBackwardHotkeys = null;
			this._deserializedClientOrders = null;
			this.EnsureCycleGroups();
		}

		public void EnsureCycleGroups()
		{
			this._cycleGroups ??= new List<CycleGroupDefinition>();
			if (this._cycleGroups.Count == 0) this._cycleGroups.Add(new CycleGroupDefinition { Name = "Group 1" });
			var ids = new HashSet<string>(StringComparer.Ordinal);
			for (int index = 0; index < this._cycleGroups.Count; index++)
			{
				CycleGroupDefinition group = this._cycleGroups[index] ??= new CycleGroupDefinition();
				if (string.IsNullOrWhiteSpace(group.Id) || !ids.Add(group.Id))
				{
					do { group.Id = Guid.NewGuid().ToString("N"); } while (!ids.Add(group.Id));
				}
				if (string.IsNullOrWhiteSpace(group.Name)) group.Name = $"Group {index + 1}";
				group.ForwardHotkeys ??= new List<string>();
				group.BackwardHotkeys ??= new List<string>();
				group.ClientsOrder ??= new Dictionary<string, int>();
			}
			this.CycleGroupAutoAddExclusions = (this.CycleGroupAutoAddExclusions ?? new List<string>())
				.Where(title => !string.IsNullOrWhiteSpace(title)).Distinct(StringComparer.Ordinal).ToList();
		}

		/// <summary>
		/// Applies restrictions to different parameters of the config
		/// </summary>
		public void ApplyRestrictions()
		{
			this.EnsureCycleGroups();
			this.ClientHotkey ??= new Dictionary<string, string>();
			this.MinimizeAllClientsHotkeys ??= new List<string>();
			this.RefreshMinimizedClientsHotkeys ??= new List<string>();
			// Migrate configurations created by the official build, which persisted
			// the old 192x108 lower bound in JSON.
			this.ThumbnailMinimumSize = CustomMinimumThumbnailSize;
			this.ThumbnailMaximumSize = new Size(
				Math.Max(this.ThumbnailMaximumSize.Width, this.ThumbnailMinimumSize.Width),
				Math.Max(this.ThumbnailMaximumSize.Height, this.ThumbnailMinimumSize.Height));

#if LINUX
			this.ThumbnailRefreshPeriod = ThumbnailConfiguration.ApplyRestrictions(this.ThumbnailRefreshPeriod, 10, 1000);
#else
			this.ThumbnailRefreshPeriod = ThumbnailConfiguration.ApplyRestrictions(this.ThumbnailRefreshPeriod, 100, 2000);
#endif
			this.ThumbnailResizeTimeoutPeriod = ThumbnailConfiguration.ApplyRestrictions(this.ThumbnailResizeTimeoutPeriod, 200, 5000);
			this.ThumbnailSize = new Size(ThumbnailConfiguration.ApplyRestrictions(this.ThumbnailSize.Width, this.ThumbnailMinimumSize.Width, this.ThumbnailMaximumSize.Width),
				ThumbnailConfiguration.ApplyRestrictions(this.ThumbnailSize.Height, this.ThumbnailMinimumSize.Height, this.ThumbnailMaximumSize.Height));
			this.PerClientThumbnailSize ??= new Dictionary<string, Size>();
			foreach (string client in this.PerClientThumbnailSize.Keys.ToList())
			{
				Size size = this.PerClientThumbnailSize[client];
				this.PerClientThumbnailSize[client] = new Size(
					ThumbnailConfiguration.ApplyRestrictions(size.Width, this.ThumbnailMinimumSize.Width, this.ThumbnailMaximumSize.Width),
					ThumbnailConfiguration.ApplyRestrictions(size.Height, this.ThumbnailMinimumSize.Height, this.ThumbnailMaximumSize.Height));
			}
			this.ThumbnailOpacity = ThumbnailConfiguration.ApplyRestrictions((int)(this.ThumbnailOpacity * 100.00), 20, 100) / 100.00;
			this.ThumbnailZoomFactor = ThumbnailConfiguration.ApplyRestrictions(this.ThumbnailZoomFactor, 2, 10);
			this.ActiveClientHighlightThickness = ThumbnailConfiguration.ApplyRestrictions(this.ActiveClientHighlightThickness, 1, 6);
		}
		public IList<string> GetAllKnownClients()
		{
			// Group members stay available even while offline or before a preview position is recorded.
			IEnumerable<string> positionedClients = this.FlatLayout == null ? Enumerable.Empty<string>() : this.FlatLayout.Keys;
			IEnumerable<string> groupedClients = (this._cycleGroups ?? new List<CycleGroupDefinition>())
				.Where(group => group?.ClientsOrder != null).SelectMany(group => group.ClientsOrder.Keys);
			return positionedClients.Concat(groupedClients).Where(title => !string.IsNullOrWhiteSpace(title))
				.Distinct(StringComparer.Ordinal).ToList();
		}

		private static int ApplyRestrictions(int value, int minimum, int maximum)
		{
			if (value <= minimum)
			{
				return minimum;
			}

			if (value >= maximum)
			{
				return maximum;
			}

			return value;
		}
	}
}
