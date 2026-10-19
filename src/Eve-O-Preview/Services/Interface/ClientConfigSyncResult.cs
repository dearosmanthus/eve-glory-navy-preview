using System.Collections.Generic;
using System.Linq;

namespace EveOPreview.Services
{
	public enum EveServer
	{
		Serenity,
		Tranquility
	}

	public sealed class ClientConfigSyncTargetResult
	{
		public long TargetId { get; internal set; }
		public bool Success { get; internal set; }
		public string BackupPath { get; internal set; }
		public string Error { get; internal set; }
	}

	public sealed class ClientConfigSyncResult
	{
		internal readonly List<ClientConfigSyncTargetResult> Items = new List<ClientConfigSyncTargetResult>();
		public IReadOnlyList<ClientConfigSyncTargetResult> Results => Items.AsReadOnly();
		public int SuccessCount => Items.Count(item => item.Success);
		public int FailureCount => Items.Count(item => !item.Success);
		public bool Cancelled { get; internal set; }
		public string BackupDirectory { get; internal set; }
		public IReadOnlyList<string> Errors => Items.Where(item => !item.Success).Select(item => item.TargetId + ": " + item.Error).ToList().AsReadOnly();
	}
}
