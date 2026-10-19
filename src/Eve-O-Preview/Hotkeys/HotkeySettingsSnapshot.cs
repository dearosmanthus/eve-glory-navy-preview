using EveOPreview.Configuration;
using System.Collections.Generic;

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
		public List<string>[] ForwardHotkeys { get; private set; }
		public List<string>[] BackwardHotkeys { get; private set; }
		public Dictionary<string, int>[] ClientOrders { get; private set; }
		public List<string> MinimizeAllClientsHotkeys { get; private set; }
		public List<string> RefreshMinimizedClientsHotkeys { get; private set; }
		public Dictionary<string, string> ClientHotkeys { get; private set; }

		public static HotkeySettingsSnapshot Capture(IThumbnailConfiguration configuration)
		{
			return new HotkeySettingsSnapshot
			{
				QuickSwitchEnabled = configuration.QuickSwitchEnabled,
				QuickSwitchForwardHotkey = configuration.QuickSwitchForwardHotkey,
				QuickSwitchBackwardHotkey = configuration.QuickSwitchBackwardHotkey,
				QuickHideEnabled = configuration.QuickHideEnabled,
				QuickHideHotkey = configuration.QuickHideHotkey,
				ForwardHotkeys = new[] { Copy(configuration.CycleGroup1ForwardHotkeys), Copy(configuration.CycleGroup2ForwardHotkeys), Copy(configuration.CycleGroup3ForwardHotkeys), Copy(configuration.CycleGroup4ForwardHotkeys), Copy(configuration.CycleGroup5ForwardHotkeys) },
				BackwardHotkeys = new[] { Copy(configuration.CycleGroup1BackwardHotkeys), Copy(configuration.CycleGroup2BackwardHotkeys), Copy(configuration.CycleGroup3BackwardHotkeys), Copy(configuration.CycleGroup4BackwardHotkeys), Copy(configuration.CycleGroup5BackwardHotkeys) },
				ClientOrders = new[] { Copy(configuration.CycleGroup1ClientsOrder), Copy(configuration.CycleGroup2ClientsOrder), Copy(configuration.CycleGroup3ClientsOrder), Copy(configuration.CycleGroup4ClientsOrder), Copy(configuration.CycleGroup5ClientsOrder) },
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
			configuration.CycleGroup1ForwardHotkeys = Copy(this.ForwardHotkeys[0]);
			configuration.CycleGroup2ForwardHotkeys = Copy(this.ForwardHotkeys[1]);
			configuration.CycleGroup3ForwardHotkeys = Copy(this.ForwardHotkeys[2]);
			configuration.CycleGroup4ForwardHotkeys = Copy(this.ForwardHotkeys[3]);
			configuration.CycleGroup5ForwardHotkeys = Copy(this.ForwardHotkeys[4]);
			configuration.CycleGroup1BackwardHotkeys = Copy(this.BackwardHotkeys[0]);
			configuration.CycleGroup2BackwardHotkeys = Copy(this.BackwardHotkeys[1]);
			configuration.CycleGroup3BackwardHotkeys = Copy(this.BackwardHotkeys[2]);
			configuration.CycleGroup4BackwardHotkeys = Copy(this.BackwardHotkeys[3]);
			configuration.CycleGroup5BackwardHotkeys = Copy(this.BackwardHotkeys[4]);
			configuration.CycleGroup1ClientsOrder = Copy(this.ClientOrders[0]);
			configuration.CycleGroup2ClientsOrder = Copy(this.ClientOrders[1]);
			configuration.CycleGroup3ClientsOrder = Copy(this.ClientOrders[2]);
			configuration.CycleGroup4ClientsOrder = Copy(this.ClientOrders[3]);
			configuration.CycleGroup5ClientsOrder = Copy(this.ClientOrders[4]);
			configuration.MinimizeAllClientsHotkeys = Copy(this.MinimizeAllClientsHotkeys);
			configuration.RefreshMinimizedClientsHotkeys = Copy(this.RefreshMinimizedClientsHotkeys);
			configuration.ClientHotkey = Copy(this.ClientHotkeys);
		}

		private static List<string> Copy(List<string> source) => source == null ? new List<string>() : new List<string>(source);
		private static Dictionary<string, TValue> Copy<TValue>(Dictionary<string, TValue> source) => source == null ? new Dictionary<string, TValue>() : new Dictionary<string, TValue>(source);
	}
}
