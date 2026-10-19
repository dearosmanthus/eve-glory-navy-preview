using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EveOPreview.Services
{
	public interface IClientConfigSyncService
	{
		IReadOnlyList<string> FindSettingsDirectories();
		IReadOnlyList<long> GetLocalCharacterIds(string directory);
		Task<IDictionary<long, string>> ResolveNamesAsync(IEnumerable<long> ids, EveServer server, CancellationToken cancellationToken);
		Task<long?> ResolveIdAsync(string name, EveServer server, CancellationToken cancellationToken);
		ClientConfigSyncResult Sync(string directory, long sourceId, IEnumerable<long> targetIds, CancellationToken cancellationToken);
	}
}
