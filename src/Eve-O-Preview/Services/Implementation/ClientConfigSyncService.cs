using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EveOPreview.Services
{
	/// <summary>Copies only explicitly selected local character settings, never account settings.</summary>
	public sealed class ClientConfigSyncService : IClientConfigSyncService
	{
		private static readonly Regex CharacterFileName = new Regex(@"\Acore_char_([1-9][0-9]*)\.dat\z", RegexOptions.CultureInvariant);
		private static readonly HttpClient SharedClient = CreateHttpClient();
		private readonly HttpClient _httpClient;
		private const int MaximumSettingsBytes = 64 * 1024 * 1024;
		private const string BackupFolderName = "GloryNavy-character-backups";

		public ClientConfigSyncService() : this(SharedClient) { }

		// Injection avoids network access in regression tests. The caller retains client ownership.
		public ClientConfigSyncService(HttpClient httpClient)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		public IReadOnlyList<string> FindSettingsDirectories()
		{
			var result = new List<string>();
			string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CCP", "EVE");
			if (!Directory.Exists(root)) return result.AsReadOnly();
			try
			{
				root = ValidateDirectory(root);
				// Known EVE layout: CCP/EVE/<installation cache>/settings_<profile>.
				// Never recursively traverse arbitrary directories or reparse points.
				foreach (string candidate in Directory.EnumerateDirectories(root).Take(500))
				{
					try
					{
						string installation = ValidateDirectory(candidate);
						if (Path.GetFileName(installation).StartsWith("settings_", StringComparison.OrdinalIgnoreCase))
							AddSettingsDirectory(result, installation);
						else
							foreach (string settings in Directory.EnumerateDirectories(installation, "settings_*", SearchOption.TopDirectoryOnly).Take(100))
								AddSettingsDirectory(result, settings);
					}
					catch (Exception error) when (IsFileError(error)) { /* One unavailable installation must not hide others. */ }
				}
			}
			catch (Exception error) when (IsFileError(error)) { }
			return result.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList().AsReadOnly();
		}

		public IReadOnlyList<long> GetLocalCharacterIds(string directory)
		{
			string root = ValidateDirectory(directory);
			var ids = new SortedSet<long>();
			foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly))
			{
				Match match = CharacterFileName.Match(Path.GetFileName(file));
				if (!match.Success || !long.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out long id) || id <= 0) continue;
				// Do not inspect even the contents of links or unsupported files.
				FileAttributes attributes = File.GetAttributes(file);
				if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Directory)) != 0) continue;
				ids.Add(id);
			}
			return ids.ToList().AsReadOnly();
		}

		public async Task<IDictionary<long, string>> ResolveNamesAsync(IEnumerable<long> ids, EveServer server, CancellationToken cancellationToken)
		{
			if (ids == null) throw new ArgumentNullException(nameof(ids));
			long[] requested = ids.Distinct().ToArray();
			if (requested.Any(id => id <= 0)) throw new ArgumentOutOfRangeException(nameof(ids));
			var names = new Dictionary<long, string>();
			for (int offset = 0; offset < requested.Length; offset += 1000)
			{
				cancellationToken.ThrowIfCancellationRequested();
				long[] batch = requested.Skip(offset).Take(1000).ToArray();
				string json = await PostAsync("universe/names/", batch, server, cancellationToken).ConfigureAwait(false);
				if (json == null) throw new InvalidDataException("The selected server could not identify the requested character IDs.");
				foreach (JToken item in JArray.Parse(json))
				{
					if (!string.Equals((string)item["category"], "character", StringComparison.Ordinal)) continue;
					long? id = (long?)item["id"];
					string name = (string)item["name"];
					if (id.HasValue && Array.IndexOf(batch, id.Value) >= 0 && !string.IsNullOrWhiteSpace(name)) names[id.Value] = name;
				}
			}
			long[] missing = requested.Where(id => !names.ContainsKey(id)).ToArray();
			if (missing.Length > 0) throw new InvalidDataException("The selected server did not identify every requested character ID. Unresolved IDs: " + string.Join(", ", missing.Take(25)));
			return names;
		}

		public async Task<long?> ResolveIdAsync(string name, EveServer server, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(name)) return null;
			name = name.Trim();
			if (name.Length > 128 || name.Any(char.IsControl)) throw new ArgumentException("Invalid character name.", nameof(name));
			string json = await PostAsync("universe/ids/", new[] { name }, server, cancellationToken).ConfigureAwait(false);
			if (json == null) return null;
			JArray characters = JObject.Parse(json)["characters"] as JArray;
			if (characters == null) throw new InvalidDataException("The selected server did not return a character match list for this name.");
			var matches = characters
				.Where(item => string.Equals((string)item["name"], name, StringComparison.OrdinalIgnoreCase))
				.Select(item => (long?)item["id"]).Where(id => id.HasValue && id.Value > 0).Distinct().ToArray();
			return matches.Length == 1 ? matches[0] : null;
		}

		public ClientConfigSyncResult Sync(string directory, long sourceId, IEnumerable<long> targetIds, CancellationToken cancellationToken)
		{
			if (targetIds == null) throw new ArgumentNullException(nameof(targetIds));
			if (sourceId <= 0) throw new ArgumentOutOfRangeException(nameof(sourceId));
			long[] targets = targetIds.Distinct().ToArray();
			if (targets.Any(id => id <= 0 || id == sourceId)) throw new ArgumentException("Select positive target IDs different from the source.", nameof(targetIds));
			var result = new ClientConfigSyncResult();
			if (cancellationToken.IsCancellationRequested) { result.Cancelled = true; return result; }
			string root = ValidateDirectory(directory);
			string source = GetCharacterPath(root, sourceId);
			ValidateRegularFile(source);
			byte[] contents;
			using (var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				if (input.Length > MaximumSettingsBytes) throw new IOException("The character settings file exceeds the 64 MiB safety limit.");
				contents = new byte[checked((int)input.Length)];
				input.ReadExactly(contents);
			}
			if (targets.Length == 0) return result;
			string backupRoot = Path.Combine(root, BackupFolderName);
			string runBackup = Path.Combine(backupRoot, DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N"));
			foreach (long targetId in targets)
			{
				// Cancellation is deliberately between targets, never during an atomic replacement.
				if (cancellationToken.IsCancellationRequested) { result.Cancelled = true; break; }
				var item = new ClientConfigSyncTargetResult { TargetId = targetId };
				string temporary = null;
				try
				{
					ValidateDirectory(root);
					ValidateRegularFile(source);
					string target = GetCharacterPath(root, targetId);
					ValidateRegularFile(target);
					EnsureSafeDirectory(backupRoot);
					EnsureSafeDirectory(runBackup);
					result.BackupDirectory = runBackup;
					string backup = Path.Combine(runBackup, Path.GetFileName(target));
					// CreateNew + flush makes an unsuccessful backup a hard stop for this target.
					using (var oldFile = new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.Read))
					using (var backupFile = new FileStream(backup, FileMode.CreateNew, FileAccess.Write, FileShare.None))
					{
						if (oldFile.Length > MaximumSettingsBytes) throw new IOException("The target settings file exceeds the 64 MiB safety limit.");
						oldFile.CopyTo(backupFile);
						backupFile.Flush(true);
					}
					item.BackupPath = backup;
					temporary = Path.Combine(root, ".glory-navy-sync-" + Guid.NewGuid().ToString("N") + ".tmp");
					using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
					{
						output.Write(contents, 0, contents.Length);
						output.Flush(true);
					}
					ValidateDirectory(root);
					ValidateRegularFile(target);
					ValidateRegularFile(temporary);
					// Same-directory File.Replace preserves the old target if the atomic replace fails.
					// There is intentionally no delete/copy fallback.
					File.Replace(temporary, target, null);
					temporary = null;
					item.Success = true;
				}
				catch (Exception error) when (IsFileError(error)) { item.Error = error.Message; }
				finally
				{
					if (temporary != null)
					{
						try { File.Delete(temporary); } catch (Exception error) when (IsFileError(error)) { }
					}
				}
				result.Items.Add(item);
			}
			return result;
		}

		private void AddSettingsDirectory(List<string> result, string directory)
		{
			try { if (GetLocalCharacterIds(directory).Count > 0) result.Add(ValidateDirectory(directory)); }
			catch (Exception error) when (IsFileError(error)) { }
		}

		private static string GetCharacterPath(string directory, long id) => Path.Combine(directory, "core_char_" + id.ToString(CultureInfo.InvariantCulture) + ".dat");

		private static string ValidateDirectory(string directory)
		{
			if (string.IsNullOrWhiteSpace(directory) || !Path.IsPathFullyQualified(directory) || directory.StartsWith(@"\\", StringComparison.Ordinal))
				throw new ArgumentException("Choose an absolute local settings directory.", nameof(directory));
			if (directory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part => part == "." || part == ".."))
				throw new ArgumentException("Relative path segments are not permitted.", nameof(directory));
			string full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
			if (new DriveInfo(Path.GetPathRoot(full)).DriveType == DriveType.Network)
				throw new IOException("Choose a local directory, not a mapped network drive.");
			for (DirectoryInfo current = new DirectoryInfo(full); current != null; current = current.Parent)
			{
				FileAttributes attributes = File.GetAttributes(current.FullName);
				if ((attributes & FileAttributes.ReparsePoint) != 0) throw new IOException("Symbolic links and junctions are not supported: " + current.FullName);
				if ((attributes & FileAttributes.Directory) == 0) throw new IOException("The selected path is not a directory.");
			}
			return full;
		}

		private static void ValidateRegularFile(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Directory)) != 0) throw new IOException("Only regular character settings files are supported: " + Path.GetFileName(path));
		}

		private static void EnsureSafeDirectory(string path)
		{
			ValidateDirectory(Path.GetDirectoryName(path));
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			ValidateDirectory(path);
		}

		private static bool IsFileError(Exception error) => error is IOException || error is UnauthorizedAccessException || error is ArgumentException || error is NotSupportedException || error is System.Security.SecurityException;

		private static HttpClient CreateHttpClient() => new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(20) };

		private async Task<string> PostAsync(string route, object payload, EveServer server, CancellationToken cancellationToken)
		{
			string host;
			string datasource;
			switch (server)
			{
				case EveServer.Serenity: host = "https://ali-esi.evepc.163.com/latest/"; datasource = "serenity"; break;
				case EveServer.Tranquility: host = "https://esi.evetech.net/latest/"; datasource = "tranquility"; break;
				default: throw new ArgumentOutOfRangeException(nameof(server));
			}
			var uri = new Uri(host + route + "?datasource=" + datasource);
			using var request = new HttpRequestMessage(HttpMethod.Post, uri);
			request.Headers.UserAgent.ParseAdd("EVE-O-Preview-Glory-Navy/8.0");
			request.Headers.Accept.ParseAdd("application/json");
			request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
			using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
			if (response.RequestMessage?.RequestUri != null && !string.Equals(response.RequestMessage.RequestUri.Host, uri.Host, StringComparison.OrdinalIgnoreCase))
				throw new HttpRequestException("The selected server redirected to a different host.");
			response.EnsureSuccessStatusCode();
			if (response.Content.Headers.ContentLength > 4 * 1024 * 1024) throw new HttpRequestException("Name lookup response is too large.");
			string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			if (json.Length > 4 * 1024 * 1024) throw new HttpRequestException("Name lookup response is too large.");
			return json;
		}
	}
}
