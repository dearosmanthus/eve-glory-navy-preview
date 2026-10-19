using EveOPreview.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace EveOPreview.UI.Hotkeys
{
	/// <summary>Only hotkey settings are rolled back; unrelated layout changes remain untouched.</summary>
	sealed class HotkeySettingsSnapshot
	{
		public bool QuickSwitchEnabled { get; private set; }
		public string QuickSwitchForwardHotkey { get; private set; }
		public string QuickSwitchBackwardHotkey { get; private set; }
		public bool QuickHideEnabled { get; private set; }
		public string QuickHideHotkey { get; private set; }
		public List<CycleGroupDefinition> CycleGroups { get; private set; }
		public bool AutoAddUngroupedClients { get; private set; }
		public List<string> CycleGroupAutoAddExclusions { get; private set; }
		public List<string> MinimizeAllClientsHotkeys { get; private set; }
		public List<string> RefreshMinimizedClientsHotkeys { get; private set; }
		public Dictionary<string, string> ClientHotkeys { get; private set; }

		public static HotkeySettingsSnapshot Capture(IThumbnailConfiguration configuration)
		{
			configuration.EnsureCycleGroups();
			return new HotkeySettingsSnapshot
			{
				QuickSwitchEnabled = configuration.QuickSwitchEnabled,
				QuickSwitchForwardHotkey = configuration.QuickSwitchForwardHotkey,
				QuickSwitchBackwardHotkey = configuration.QuickSwitchBackwardHotkey,
				QuickHideEnabled = configuration.QuickHideEnabled,
				QuickHideHotkey = configuration.QuickHideHotkey,
				CycleGroups = CopyGroups(configuration.CycleGroups),
				AutoAddUngroupedClients = configuration.AutoAddUngroupedClients,
				CycleGroupAutoAddExclusions = Copy(configuration.CycleGroupAutoAddExclusions),
				MinimizeAllClientsHotkeys = Copy(configuration.MinimizeAllClientsHotkeys),
				RefreshMinimizedClientsHotkeys = Copy(configuration.RefreshMinimizedClientsHotkeys),
				ClientHotkeys = Copy(configuration.ClientHotkey)
			};
		}

		public void Restore(IThumbnailConfiguration configuration)
		{
			configuration.QuickSwitchEnabled = this.QuickSwitchEnabled;
			configuration.QuickSwitchForwardHotkey = this.QuickSwitchForwardHotkey;
			configuration.QuickSwitchBackwardHotkey = this.QuickSwitchBackwardHotkey;
			configuration.QuickHideEnabled = this.QuickHideEnabled;
			configuration.QuickHideHotkey = this.QuickHideHotkey;
			configuration.CycleGroups = CopyGroups(this.CycleGroups);
			configuration.AutoAddUngroupedClients = this.AutoAddUngroupedClients;
			configuration.CycleGroupAutoAddExclusions = Copy(this.CycleGroupAutoAddExclusions);
			configuration.MinimizeAllClientsHotkeys = Copy(this.MinimizeAllClientsHotkeys);
			configuration.RefreshMinimizedClientsHotkeys = Copy(this.RefreshMinimizedClientsHotkeys);
			configuration.ClientHotkey = Copy(this.ClientHotkeys);
			configuration.EnsureCycleGroups();
		}

		private static List<CycleGroupDefinition> CopyGroups(List<CycleGroupDefinition> source) => source == null ? new List<CycleGroupDefinition>() : source.Select(group => group?.DeepClone() ?? new CycleGroupDefinition()).ToList();
		private static List<string> Copy(List<string> source) => source == null ? new List<string>() : new List<string>(source);
		private static Dictionary<string, TValue> Copy<TValue>(Dictionary<string, TValue> source) => source == null ? new Dictionary<string, TValue>() : new Dictionary<string, TValue>(source);
	}
}
