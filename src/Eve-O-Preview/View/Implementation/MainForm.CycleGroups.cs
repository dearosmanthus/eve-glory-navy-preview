using EveOPreview.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EveOPreview.View
{
	public partial class MainForm
	{
		private DynamicCycleGroupsPanel _dynamicCycleGroupsPanel;

		public Action CycleGroupsChanged { get; set; }
		public Action<string> ClientPreviewToggleRequested { get; set; }
		public Action<string> ClientConfigSyncRequested { get; set; }
		public string SelectedDynamicCycleGroupId => this._dynamicCycleGroupsPanel?.SelectedGroupId ?? string.Empty;
		public bool AutoAddUngroupedClients
		{
			get => this._dynamicCycleGroupsPanel?.AutoAddUngroupedClients ?? false;
			set { if (this._dynamicCycleGroupsPanel != null) this._dynamicCycleGroupsPanel.AutoAddUngroupedClients = value; }
		}

		public void SetCycleGroups(IList<CycleGroupDefinition> groups)
		{
			this._dynamicCycleGroupsPanel?.SetGroups(groups);
		}

		public List<CycleGroupDefinition> GetCycleGroups()
		{
			return this._dynamicCycleGroupsPanel?.GetGroups() ?? new List<CycleGroupDefinition>();
		}

		public List<string> ConsumeRemovedCycleGroupClients()
		{
			return this._dynamicCycleGroupsPanel?.ConsumeRemovedClients() ?? new List<string>();
		}

		public void RefreshCycleGroupClients(IList<string> onlineTitles, IList<string> knownTitles, IList<string> hiddenTitles)
		{
			this._dynamicCycleGroupsPanel?.RefreshClients(onlineTitles, knownTitles, hiddenTitles);
		}

		private void InitializeDynamicCycleGroupsControls()
		{
			foreach (Control legacy in this.CycleGroupTabPage.Controls.Cast<Control>().ToArray()) legacy.Visible = false;
			this._dynamicCycleGroupsPanel = new DynamicCycleGroupsPanel(this.CaptureHotkey, this.ShowOwnedSettingsDialog)
			{
				Name = "DynamicCycleGroupsPanel",
				Dock = DockStyle.Fill
			};
			this._dynamicCycleGroupsPanel.GroupsChanged += () => { if (!this._suppressEvents) this.CycleGroupsChanged?.Invoke(); };
			this._dynamicCycleGroupsPanel.PreviewToggleRequested += title => this.ClientPreviewToggleRequested?.Invoke(title);
			this._dynamicCycleGroupsPanel.ConfigSyncRequested += title => this.ClientConfigSyncRequested?.Invoke(title);
			this.CycleGroupTabPage.Controls.Add(this._dynamicCycleGroupsPanel);
			this._dynamicCycleGroupsPanel.BringToFront();
			this.RefreshDynamicCycleGroupLanguage();
		}

		private void RefreshDynamicCycleGroupLanguage()
		{
			this._dynamicCycleGroupsPanel?.ApplyLanguage(this.Language);
		}
	}
}
