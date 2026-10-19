using System;
using System.Collections.Generic;

namespace EveOPreview.Configuration
{
	/// <summary>A named cycle group. Id remains unchanged when the group is renamed or reordered.</summary>
	public sealed class CycleGroupDefinition
	{
		public string Id { get; set; } = Guid.NewGuid().ToString("N");
		public string Name { get; set; } = string.Empty;
		public List<string> ForwardHotkeys { get; set; } = new List<string>();
		public List<string> BackwardHotkeys { get; set; } = new List<string>();
		public Dictionary<string, int> ClientsOrder { get; set; } = new Dictionary<string, int>();

		public CycleGroupDefinition DeepClone()
		{
			return new CycleGroupDefinition
			{
				Id = this.Id,
				Name = this.Name,
				ForwardHotkeys = this.ForwardHotkeys == null ? new List<string>() : new List<string>(this.ForwardHotkeys),
				BackwardHotkeys = this.BackwardHotkeys == null ? new List<string>() : new List<string>(this.BackwardHotkeys),
				ClientsOrder = this.ClientsOrder == null ? new Dictionary<string, int>() : new Dictionary<string, int>(this.ClientsOrder)
			};
		}
	}
}
