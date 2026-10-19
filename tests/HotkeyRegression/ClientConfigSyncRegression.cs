using EveOPreview.Services;
using EveOPreview.View;
using EveOPreview.Configuration.Implementation;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

internal static partial class Program
{
    private static void ClientConfigSyncTests()
    {
        Run("character sync strict local paths", ClientSyncLocalPathTests);
        Run("character sync backups and exact bytes", ClientSyncCopyTests);
        Run("character sync failure isolation", ClientSyncFailureTests);
        Run("character sync reparse rejection", ClientSyncReparseTests);
        Run("character sync cancellation and limits", ClientSyncCancellationAndSizeTests);
        // The parent harness runs on STA and may already own a WinForms context.
        // Run pure fake-HTTP tests away from that context rather than blocking its continuations.
        Run("character sync server-specific HTTP", () => Task.Run(ClientSyncHttpTests).GetAwaiter().GetResult());
        Run("character sync malformed responses", () => Task.Run(ClientSyncHttpFailureTests).GetAwaiter().GetResult());
        Run("character sync titles and no implicit network", ClientSyncPresentationTests);
    }

    private static string ClientSyncCase(string name)
    {
        string directory = Path.Combine(scratch, "client-sync-" + name + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string ClientSyncFile(string directory, long id) => Path.Combine(directory, "core_char_" + id.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".dat");

    private static void ClientSyncWrite(string directory, long id, string text) => File.WriteAllText(ClientSyncFile(directory, id), text, new UTF8Encoding(false));

    private static void ClientSyncExpectError<TError>(Action action, string name) where TError : Exception
    {
        try { action(); Check(false, name + " (no error)"); }
        catch (Exception error) { Check(Root(error) is TError, name + " (" + Root(error).GetType().Name + ")"); }
    }

    private static void ClientSyncLocalPathTests()
    {
        var service = new ClientConfigSyncService();
        string directory = ClientSyncCase("names");
        foreach (string name in new[]
        {
            "core_char_1.dat", "core_char_2.dat", "core_char_9223372036854775807.dat",
            "core_char_0.dat", "core_char_-1.dat", "core_char_01.dat", "core_char_9223372036854775808.dat",
            "core_char_3.dat.bak", "CORE_CHAR_4.dat", "core_user_7.dat", "other.dat"
        }) File.WriteAllText(Path.Combine(directory, name), "fixture");
        Directory.CreateDirectory(Path.Combine(directory, "core_char_6.dat"));
        Check(service.GetLocalCharacterIds(directory).SequenceEqual(new[] { 1L, 2L, long.MaxValue }), "sync scans only exact canonical positive-long character filenames");
        ClientSyncExpectError<ArgumentException>(() => service.GetLocalCharacterIds("relative-settings"), "sync rejects relative directory");
        ClientSyncExpectError<ArgumentException>(() => service.GetLocalCharacterIds(Path.Combine(directory, "..", Path.GetFileName(directory))), "sync rejects traversal segments before canonicalization");
        ClientSyncExpectError<ArgumentException>(() => service.GetLocalCharacterIds(@"\\localhost\not-a-real-share\settings"), "sync rejects UNC without contacting a network share");
        ClientSyncExpectError<IOException>(() => service.GetLocalCharacterIds(Path.Combine(directory, "missing")), "sync reports missing selected directory");
        ClientSyncExpectError<ArgumentException>(() => service.Sync(directory, 1, new[] { 1L }, CancellationToken.None), "sync rejects source as target");
        ClientSyncExpectError<ArgumentException>(() => service.Sync(directory, 1, new[] { 2L, -1L }, CancellationToken.None), "sync validates every target before any write");
        Check(!Directory.Exists(Path.Combine(directory, "GloryNavy-character-backups")), "sync validation errors create no backup directory or target mutations");
        Check(File.ReadAllText(Path.Combine(directory, "core_user_7.dat")) == "fixture", "sync never modifies an account settings fixture");
    }

    private static void ClientSyncCopyTests()
    {
        var service = new ClientConfigSyncService();
        string directory = ClientSyncCase("copy");
        ClientSyncWrite(directory, 101, "source\0\r\n荣耀海军");
        ClientSyncWrite(directory, 202, "first old target");
        ClientSyncWrite(directory, 303, "second old target");
        byte[] source = File.ReadAllBytes(ClientSyncFile(directory, 101));
        ClientConfigSyncResult result = service.Sync(directory, 101, new[] { 202L, 303L, 202L }, CancellationToken.None);
        Check(result.SuccessCount == 2 && result.FailureCount == 0 && !result.Cancelled, "sync deduplicates targets and reports two successful atomic copies");
        Check(File.ReadAllBytes(ClientSyncFile(directory, 202)).SequenceEqual(source) && File.ReadAllBytes(ClientSyncFile(directory, 303)).SequenceEqual(source), "sync preserves exact source bytes including Unicode and NUL");
        Check(File.ReadAllBytes(ClientSyncFile(directory, 101)).SequenceEqual(source), "sync does not alter source bytes");
        Check(!string.IsNullOrWhiteSpace(result.BackupDirectory) && Directory.Exists(result.BackupDirectory)
            && result.BackupDirectory.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), "sync reports a real per-run backup directory inside the selected directory");
        Check(result.Results.All(item => item.Success && !string.IsNullOrEmpty(item.BackupPath) && File.Exists(item.BackupPath)), "sync reports a completed backup for every successful target");
        Check(File.ReadAllText(result.Results.Single(item => item.TargetId == 202).BackupPath) == "first old target"
            && File.ReadAllText(result.Results.Single(item => item.TargetId == 303).BackupPath) == "second old target", "sync backup bytes retain both original target contents");
        Check(!Directory.EnumerateFiles(directory, ".glory-navy-sync-*").Any(), "sync removes all staged files after success");
        ClientConfigSyncResult second = service.Sync(directory, 101, new[] { 202L }, CancellationToken.None);
        Check(second.SuccessCount == 1 && second.BackupDirectory != result.BackupDirectory
            && File.ReadAllText(result.Results.Single(item => item.TargetId == 202).BackupPath) == "first old target", "later sync runs never overwrite an earlier backup");
    }

    private static void ClientSyncFailureTests()
    {
        var service = new ClientConfigSyncService();
        string directory = ClientSyncCase("backup-failure");
        ClientSyncWrite(directory, 101, "source");
        ClientSyncWrite(directory, 202, "unchanged");
        File.WriteAllText(Path.Combine(directory, "GloryNavy-character-backups"), "backup root is deliberately not a directory");
        ClientConfigSyncResult blocked = service.Sync(directory, 101, new[] { 202L }, CancellationToken.None);
        Check(blocked.SuccessCount == 0 && blocked.FailureCount == 1 && blocked.Errors.Count == 1, "backup directory failure is explicitly reported");
        Check(File.ReadAllText(ClientSyncFile(directory, 202)) == "unchanged" && blocked.Results[0].BackupPath == null, "backup failure never overwrites the target");

        string locked = ClientSyncCase("locked");
        ClientSyncWrite(locked, 101, "source");
        ClientSyncWrite(locked, 202, "locked old target");
        using (var held = new FileStream(ClientSyncFile(locked, 202), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            ClientConfigSyncResult result = service.Sync(locked, 101, new[] { 202L }, CancellationToken.None);
            Check(result.SuccessCount == 0 && result.FailureCount == 1 && result.Results[0].BackupPath == null, "an exclusively locked target cannot be backed up or counted successful");
        }
        Check(File.ReadAllText(ClientSyncFile(locked, 202)) == "locked old target", "exclusive target lock leaves original bytes unchanged");

        string replacement = ClientSyncCase("replace-lock");
        ClientSyncWrite(replacement, 101, "source");
        ClientSyncWrite(replacement, 202, "replace blocked original");
        using (var held = new FileStream(ClientSyncFile(replacement, 202), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            ClientConfigSyncResult result = service.Sync(replacement, 101, new[] { 202L }, CancellationToken.None);
            Check(result.SuccessCount == 0 && result.FailureCount == 1 && File.Exists(result.Results[0].BackupPath), "failed atomic replacement still reports the completed recovery backup");
        }
        Check(File.ReadAllText(ClientSyncFile(replacement, 202)) == "replace blocked original"
            && !Directory.EnumerateFiles(replacement, ".glory-navy-sync-*").Any(), "replace failure leaves original bytes and cleans the staged file");

        string partial = ClientSyncCase("partial");
        ClientSyncWrite(partial, 101, "source");
        ClientSyncWrite(partial, 303, "valid old target");
        ClientConfigSyncResult mixed = service.Sync(partial, 101, new[] { 202L, 303L }, CancellationToken.None);
        Check(mixed.FailureCount == 1 && mixed.SuccessCount == 1 && mixed.Results[0].TargetId == 202 && !mixed.Results[0].Success
            && mixed.Results[1].TargetId == 303 && mixed.Results[1].Success, "missing target is reported individually without blocking a separate valid target");
        Check(!File.Exists(ClientSyncFile(partial, 202)), "sync never creates a missing character settings target");
        ClientSyncExpectError<IOException>(() => service.Sync(partial, 999, new[] { 303L }, CancellationToken.None), "missing source is an explicit preflight error");
    }

    private static bool ClientSyncCreateDirectoryJunction(string link, string destination)
    {
        var start = new ProcessStartInfo("cmd.exe") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (string argument in new[] { "/d", "/c", "mklink", "/J", link, destination }) start.ArgumentList.Add(argument);
        using Process child = Process.Start(start)!;
        string output = child.StandardOutput.ReadToEnd() + child.StandardError.ReadToEnd();
        child.WaitForExit();
        if (child.ExitCode != 0) Console.WriteLine("NOT RUN directory junction fixture could not be created: " + output);
        return child.ExitCode == 0 && (File.GetAttributes(link) & FileAttributes.ReparsePoint) != 0;
    }

    private static void ClientSyncReparseTests()
    {
        var service = new ClientConfigSyncService();
        string outer = ClientSyncCase("reparse");
        string destination = Path.Combine(outer, "real");
        Directory.CreateDirectory(destination);
        ClientSyncWrite(destination, 101, "source");
        ClientSyncWrite(destination, 202, "target untouched");
        string alias = Path.Combine(outer, "junction");
        if (!ClientSyncCreateDirectoryJunction(alias, destination)) return;
        ClientSyncExpectError<IOException>(() => service.GetLocalCharacterIds(alias), "local scan refuses a selected directory junction");
        string subdirectory = Path.Combine(destination, "settings_Default");
        Directory.CreateDirectory(subdirectory);
        ClientSyncExpectError<IOException>(() => service.GetLocalCharacterIds(Path.Combine(alias, "settings_Default")), "local scan refuses a junction in any ancestor");
        ClientSyncExpectError<IOException>(() => service.Sync(alias, 101, new[] { 202L }, CancellationToken.None), "sync rejects a directory junction before reading or writing character contents");

        string outside = Path.Combine(outer, "outside-backup");
        Directory.CreateDirectory(outside);
        string backupAlias = Path.Combine(destination, "GloryNavy-character-backups");
        Check(ClientSyncCreateDirectoryJunction(backupAlias, outside), "create isolated backup-directory junction fixture");
        ClientConfigSyncResult blocked = service.Sync(destination, 101, new[] { 202L }, CancellationToken.None);
        Check(blocked.FailureCount == 1 && blocked.SuccessCount == 0 && !Directory.EnumerateFileSystemEntries(outside).Any(), "sync rejects a backup junction without writing outside the selected directory");
        Check(File.ReadAllText(ClientSyncFile(destination, 202)) == "target untouched", "backup junction failure preserves target bytes");
    }

    private static void ClientSyncCancellationAndSizeTests()
    {
        var service = new ClientConfigSyncService();
        string directory = ClientSyncCase("cancelled");
        ClientSyncWrite(directory, 101, "source");
        ClientSyncWrite(directory, 202, "target");
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        ClientConfigSyncResult result = service.Sync(directory, 101, new[] { 202L }, cancelled.Token);
        Check(result.Cancelled && result.Results.Count == 0 && result.SuccessCount == 0 && result.FailureCount == 0, "already-cancelled sync does not start any target");
        Check(File.ReadAllText(ClientSyncFile(directory, 202)) == "target" && Directory.EnumerateDirectories(directory).Count() == 0, "already-cancelled sync makes no backup or target writes");

        string largeSource = ClientSyncCase("large-source");
        using (var file = new FileStream(ClientSyncFile(largeSource, 101), FileMode.CreateNew, FileAccess.Write)) file.SetLength(64L * 1024 * 1024 + 1);
        ClientSyncWrite(largeSource, 202, "target");
        ClientSyncExpectError<IOException>(() => service.Sync(largeSource, 101, new[] { 202L }, CancellationToken.None), "sync rejects a source larger than 64 MiB before allocating or copying it");
        Check(File.ReadAllText(ClientSyncFile(largeSource, 202)) == "target" && Directory.EnumerateDirectories(largeSource).Count() == 0, "oversized source leaves target and backup state unchanged");

        string largeTarget = ClientSyncCase("large-target");
        ClientSyncWrite(largeTarget, 101, "source");
        using (var file = new FileStream(ClientSyncFile(largeTarget, 202), FileMode.CreateNew, FileAccess.Write)) file.SetLength(64L * 1024 * 1024 + 1);
        ClientConfigSyncResult tooLarge = service.Sync(largeTarget, 101, new[] { 202L }, CancellationToken.None);
        Check(tooLarge.FailureCount == 1 && tooLarge.SuccessCount == 0 && new FileInfo(ClientSyncFile(largeTarget, 202)).Length == 64L * 1024 * 1024 + 1, "oversized target is reported and never replaced");
    }

    private sealed class ClientSyncHttpHandler : HttpMessageHandler
    {
        internal readonly List<(Uri Uri, string Body)> Requests = new();
        internal Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Reply = (request, token) => Task.FromResult(ClientSyncJson("[]"));
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add((request.RequestUri!, request.Content == null ? "" : await request.Content.ReadAsStringAsync(cancellationToken)));
            HttpResponseMessage response = await Reply(request, cancellationToken);
            response.RequestMessage ??= request;
            return response;
        }
    }

    private static HttpResponseMessage ClientSyncJson(string json, HttpStatusCode status = HttpStatusCode.OK) => new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static async Task ClientSyncHttpTests()
    {
        foreach (EveServer server in new[] { EveServer.Serenity, EveServer.Tranquility })
        {
            using var handler = new ClientSyncHttpHandler();
            handler.Reply = (request, token) => Task.FromResult(ClientSyncJson(request.RequestUri!.AbsolutePath.Contains("/names/")
                ? """[{"id":101,"name":"Fixture Alpha","category":"character"},{"id":202,"name":"Fixture Beta","category":"character"}]"""
                : """{"characters":[{"id":101,"name":"Fixture Alpha"}],"corporations":[{"id":999,"name":"Fixture Alpha"}]}"""));
            using var http = new HttpClient(handler);
            var service = new ClientConfigSyncService(http);
            Check(handler.Requests.Count == 0, "sync service construction performs no network requests (" + server + ")");
            IDictionary<long, string> names = await service.ResolveNamesAsync(new[] { 101L, 202L, 101L }, server, CancellationToken.None);
            long? id = await service.ResolveIdAsync("Fixture Alpha", server, CancellationToken.None);
            string host = server == EveServer.Serenity ? "ali-esi.evepc.163.com" : "esi.evetech.net";
            string datasource = server == EveServer.Serenity ? "serenity" : "tranquility";
            Check(names.Count == 2 && names[101] == "Fixture Alpha" && id == 101, "explicit fake lookup returns only exact character identities (" + server + ")");
            Check(handler.Requests.Count == 2 && handler.Requests.All(request => request.Uri.Scheme == "https" && request.Uri.Host == host && request.Uri.Query.Contains("datasource=" + datasource)), "lookup uses only explicitly chosen server with no cross-server requests (" + server + ")");
            Check(JArray.Parse(handler.Requests[0].Body).Select(value => (long)value).SequenceEqual(new[] { 101L, 202L })
                && JArray.Parse(handler.Requests[1].Body).Single().Value<string>() == "Fixture Alpha", "lookup sends only deduplicated IDs or exact fixture name, never configuration bytes (" + server + ")");
        }

        using var batchingHandler = new ClientSyncHttpHandler();
        batchingHandler.Reply = async (request, token) =>
        {
            JArray ids = JArray.Parse(await request.Content!.ReadAsStringAsync(token));
            return ClientSyncJson(new JArray(ids.Select(id => new JObject { ["id"] = (long)id, ["name"] = "Fixture " + id, ["category"] = "character" })).ToString());
        };
        using var batchingHttp = new HttpClient(batchingHandler);
        var batching = new ClientConfigSyncService(batchingHttp);
        IDictionary<long, string> batchNames = await batching.ResolveNamesAsync(Enumerable.Range(1, 1001).Select(id => (long)id), EveServer.Serenity, CancellationToken.None);
        Check(batchNames.Count == 1001 && batchingHandler.Requests.Count == 2
            && batchingHandler.Requests.Select(request => JArray.Parse(request.Body).Count).SequenceEqual(new[] { 1000, 1 }), "name lookup observes official 1000-ID request maximum");
    }

    private static async Task ClientSyncHttpFailureTests()
    {
        foreach ((string Description, string Json) sample in new[]
        {
            ("missing one ID", "[{\"id\":101,\"name\":\"Fixture Alpha\",\"category\":\"character\"}]"),
            ("corporation is not a character", "[{\"id\":101,\"name\":\"Fixture Corp\",\"category\":\"corporation\"},{\"id\":202,\"name\":\"Fixture Beta\",\"category\":\"character\"}]"),
            ("unrequested ID cannot substitute", "[{\"id\":999,\"name\":\"Wrong\",\"category\":\"character\"},{\"id\":202,\"name\":\"Fixture Beta\",\"category\":\"character\"}]"),
            ("not JSON", "not a JSON response")
        })
        {
            using var handler = new ClientSyncHttpHandler { Reply = (request, token) => Task.FromResult(ClientSyncJson(sample.Json)) };
            using var http = new HttpClient(handler);
            var service = new ClientConfigSyncService(http);
            bool failed = false;
            try { await service.ResolveNamesAsync(new[] { 101L, 202L }, EveServer.Serenity, CancellationToken.None); }
            catch (Exception) { failed = true; }
            Check(failed && handler.Requests.Count == 1, "lookup explicitly fails without fallback: " + sample.Description);
        }

        foreach (string json in new[] { "{}", "{\"characters\":null}", "{\"characters\":{}}", "not-json" })
        {
            using var handler = new ClientSyncHttpHandler { Reply = (request, token) => Task.FromResult(ClientSyncJson(json)) };
            using var http = new HttpClient(handler);
            var service = new ClientConfigSyncService(http);
            bool failed = false;
            try { await service.ResolveIdAsync("Fixture Alpha", EveServer.Tranquility, CancellationToken.None); }
            catch (Exception) { failed = true; }
            Check(failed && handler.Requests.Count == 1, "ID lookup explicitly rejects missing/malformed character list: " + json);
        }

        foreach (string json in new[]
        {
            "{\"characters\":[]}",
            "{\"characters\":[{\"id\":202,\"name\":\"Fixture Alphabet\"}]}",
            "{\"characters\":[{\"id\":101,\"name\":\"Fixture Alpha\"},{\"id\":202,\"name\":\"Fixture Alpha\"}]}"
        })
        {
            using var handler = new ClientSyncHttpHandler { Reply = (request, token) => Task.FromResult(ClientSyncJson(json)) };
            using var http = new HttpClient(handler);
            var service = new ClientConfigSyncService(http);
            Check(await service.ResolveIdAsync("Fixture Alpha", EveServer.Serenity, CancellationToken.None) == null && handler.Requests.Count == 1,
                "unknown, non-exact, or ambiguous character name is never guessed: " + json);
        }

        foreach (HttpStatusCode status in new[] { HttpStatusCode.NotFound, HttpStatusCode.TooManyRequests, HttpStatusCode.InternalServerError, HttpStatusCode.Redirect })
        {
            using var handler = new ClientSyncHttpHandler { Reply = (request, token) => Task.FromResult(ClientSyncJson("{}", status)) };
            using var http = new HttpClient(handler);
            var service = new ClientConfigSyncService(http);
            bool failed = false;
            try { await service.ResolveNamesAsync(new[] { 101L }, EveServer.Tranquility, CancellationToken.None); }
            catch (HttpRequestException) { failed = true; }
            bool idFailed = false;
            try { await service.ResolveIdAsync("Fixture Alpha", EveServer.Tranquility, CancellationToken.None); }
            catch (HttpRequestException) { idFailed = true; }
            Check(failed && idFailed && handler.Requests.Count == 2 && handler.Requests.All(request => request.Uri.Host == "esi.evetech.net"), "HTTP " + (int)status + " fails both lookup directions without silent retries or cross-server fallback");
        }

        using (var handler = new ClientSyncHttpHandler
        {
            Reply = (request, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]"), RequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://not-the-selected-host.invalid/")
            })
        })
        using (var http = new HttpClient(handler))
        {
            var service = new ClientConfigSyncService(http);
            bool failed = false;
            try { await service.ResolveNamesAsync(new[] { 101L }, EveServer.Serenity, CancellationToken.None); }
            catch (HttpRequestException) { failed = true; }
            Check(failed, "name lookup rejects a response redirected to another host");
        }

        using (var handler = new ClientSyncHttpHandler { Reply = async (request, token) => { await Task.Delay(Timeout.Infinite, token); return ClientSyncJson("[]"); } })
        using (var http = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(80) })
        {
            var service = new ClientConfigSyncService(http);
            bool failed = false;
            try { await service.ResolveNamesAsync(new[] { 101L }, EveServer.Serenity, CancellationToken.None); }
            catch (OperationCanceledException) { failed = true; }
            Check(failed && handler.Requests.Count == 1, "network timeout is an explicit failure without a fallback request");
        }

        using (var handler = new ClientSyncHttpHandler { Reply = async (request, token) => { await Task.Delay(Timeout.Infinite, token); return ClientSyncJson("[]"); } })
        using (var http = new HttpClient(handler))
        using (var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(80)))
        {
            var service = new ClientConfigSyncService(http);
            bool failed = false;
            try { await service.ResolveIdAsync("Fixture Alpha", EveServer.Tranquility, cancellation.Token); }
            catch (OperationCanceledException) { failed = true; }
            Check(failed && handler.Requests.Count == 1, "user-cancelled name resolution propagates cancellation without guessing or retrying");
        }

        using (var handler = new ClientSyncHttpHandler())
        using (var http = new HttpClient(handler))
        using (var cancellation = new CancellationTokenSource())
        {
            cancellation.Cancel();
            var service = new ClientConfigSyncService(http);
            bool cancelled = false;
            try { await service.ResolveNamesAsync(new[] { 101L }, EveServer.Serenity, cancellation.Token); }
            catch (OperationCanceledException) { cancelled = true; }
            Check(cancelled && handler.Requests.Count == 0, "already-cancelled name lookup never enters the HTTP handler");
        }
    }

    private static void ClientSyncPresentationTests()
    {
        Check(ClientTitleFormatter.GetCharacterName("EVE - Fixture Alpha") == "Fixture Alpha"
            && ClientTitleFormatter.GetCharacterName("星战前夜：晨曦 [Serenity] - 测试角色") == "测试角色"
            && ClientTitleFormatter.GetCharacterName("星战前夜:晨曦 [Serenity] - 测试角色") == "测试角色", "character title formatter recognizes international and both Serenity colon forms");
        Check(ClientTitleFormatter.GetCharacterName("EVE") == "" && ClientTitleFormatter.GetCharacterName(null!) == "", "login/empty window titles do not become guessed character names");
        using var handler = new ClientSyncHttpHandler();
        using var http = new HttpClient(handler);
        var service = new ClientConfigSyncService(http);
        using var dialog = new ClientConfigSyncDialog(service, "EVE - Fixture Alpha", new List<string> { "EVE - Fixture Beta" }, new List<string> { "EVE - Fixture Alpha", "EVE - Fixture Beta" }, new List<string> { "EVE - Fixture Alpha" });
        Check(handler.Requests.Count == 0, "constructing the settings-copy dialog performs no implicit online lookup");
        var list = (System.Windows.Forms.CheckedListBox)dialog.Controls.Find("ConfigSyncTargetList", true).Single();
        var source = (System.Windows.Forms.ComboBox)dialog.Controls.Find("ConfigSyncSourceCombo", true).Single();
        Check(source.SelectedIndex == -1 && list.CheckedItems.Count == 0, "copy dialog does not guess a source ID or preselect destructive targets");
        SetMember(dialog, "_localIds", new[] { 101L, 202L, 303L });
        Call(dialog, "RefreshCharacters", new object?[] { null });
        Check(source.Items.Count == 3 && source.SelectedIndex == -1 && list.Items.Count == 3, "unidentified local IDs remain visible and require manual source selection");
        source.SelectedIndex = 0;
        var all = (System.Windows.Forms.Button)dialog.Controls.Find("ConfigSyncSelectAllButton", true).Single();
        var group = (System.Windows.Forms.Button)dialog.Controls.Find("ConfigSyncSelectGroupButton", true).Single();
        Call(all, "OnClick", EventArgs.Empty);
        Check(list.Items.Count == 2 && list.CheckedItems.Count == 2, "manual local-ID selection excludes the selected source from targets");
        Call(group, "OnClick", EventArgs.Empty);
        Check(list.CheckedItems.Count == 0, "current-group selection cannot guess membership for unidentified local IDs");
        var online = (System.Windows.Forms.CheckBox)dialog.Controls.Find("ConfigSyncOnlineOnlyCheckBox", true).Single();
        online.Checked = true;
        Check(list.Items.Count == 0 && handler.Requests.Count == 0, "online filter never guesses unknown ID identity and does not start a network lookup");
    }

    // Call only from the parent's watchdog-isolated UI child. These are real
    // modal windows, but every service boundary below is a pure in-memory fake.
    private static void ClientConfigSyncUiTests()
    {
        foreach (string language in new[] { "zh-CN", "en-US" })
        {
            LocalizationExtensions.SetLanguage(language);
            using var owner = new Form
            {
                Text = "Isolated character sync owner", TopMost = true, ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-10000, -10000), Size = new Size(500, 400)
            };
            owner.Show();
            try
            {
                ClientSyncDialogLoadAndSelectionTest(owner, language);
                if (language == "zh-CN")
                {
                    ClientSyncDialogLookupCancellationTest(owner);
                    ClientSyncDialogLookupTimeoutTest(owner);
                    ClientSyncDialogCopyCloseCancellationTest(owner);
                    ClientSyncDialogDiscoveryCloseTest(owner);
                }
            }
            finally { owner.Close(); }
        }
    }

    private sealed class ClientSyncUiService : IClientConfigSyncService, IDisposable
    {
        internal readonly string First = Path.Combine(scratch, "virtual-sync-profile-A");
        internal readonly string Second = Path.Combine(scratch, "virtual-sync-profile-B");
        internal readonly ManualResetEventSlim DiscoveryEntered = new(false);
        internal readonly ManualResetEventSlim DiscoveryRelease = new(true);
        internal readonly ManualResetEventSlim LookupEntered = new(false);
        internal readonly ManualResetEventSlim SyncEntered = new(false);
        internal readonly ManualResetEventSlim SyncRelease = new(false);
        internal readonly List<string> ReadDirectories = new();
        internal int NameCalls, IdCalls, SyncCalls, NameCancellationObserved;
        internal bool BlockNames;
		internal bool TimeoutNames;
        internal bool BlockSync;
        internal string? CopiedDirectory;
        internal long CopiedSource;
        internal long[] CopiedTargets = Array.Empty<long>();

        public IReadOnlyList<string> FindSettingsDirectories()
        {
            DiscoveryEntered.Set();
            if (!DiscoveryRelease.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Isolated fake discovery was not released.");
            return new[] { First, Second };
        }
        public IReadOnlyList<long> GetLocalCharacterIds(string directory)
        {
            // No Directory/File methods and no game-data fallback are allowed in
            // this fake. A typo must fail instead of escaping the fixture.
            if (directory != First && directory != Second) throw new InvalidOperationException("UI tried to read a non-fixture directory.");
            lock (ReadDirectories) ReadDirectories.Add(directory);
            return directory == First ? new[] { 101L, 202L, 303L } : new[] { 404L, 505L };
        }
        public async Task<IDictionary<long, string>> ResolveNamesAsync(IEnumerable<long> ids, EveServer server, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref NameCalls);
            LookupEntered.Set();
			if (TimeoutNames)
			{
				await Task.Delay(50).ConfigureAwait(false);
				throw new TaskCanceledException("Fixture request timed out without user cancellation.");
			}
            if (BlockNames)
            {
                using var registration = cancellationToken.Register(() => Interlocked.Increment(ref NameCancellationObserved));
                await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();
            var all = new Dictionary<long, string>
            {
                [101] = "Fixture Alpha", [202] = "Fixture Beta", [303] = "Fixture Gamma", [404] = "Fixture Delta", [505] = "Fixture Echo"
            };
            return ids.ToDictionary(id => id, id => all[id]);
        }
        public Task<long?> ResolveIdAsync(string name, EveServer server, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref IdCalls);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<long?>(name == "Fixture Alpha" ? 101L : null);
        }
        public ClientConfigSyncResult Sync(string directory, long sourceId, IEnumerable<long> targetIds, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref SyncCalls);
            CopiedDirectory = directory;
            CopiedSource = sourceId;
            CopiedTargets = targetIds.ToArray();
            SyncEntered.Set();
            if (BlockSync)
            {
                int signalled = WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle, SyncRelease.WaitHandle }, TimeSpan.FromSeconds(5));
                if (signalled == WaitHandle.WaitTimeout) throw new TimeoutException("Isolated fake copy was not cancelled.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new ClientConfigSyncResult();
        }
        internal void ReleaseWorkers() { DiscoveryRelease.Set(); SyncRelease.Set(); }
        public void Dispose()
        {
            DiscoveryEntered.Dispose(); DiscoveryRelease.Dispose(); LookupEntered.Dispose(); SyncEntered.Dispose(); SyncRelease.Dispose();
        }
    }

    private static bool ClientSyncDialogBusy(Form dialog) => (bool)dialog.GetType().GetField("_busy", Any)!.GetValue(dialog)!;
    private static TControl ClientSyncControl<TControl>(Control parent, string name) where TControl : Control => (TControl)parent.Controls.Find(name, true).Single();
    private static void ClientSyncActualClick(Control parent, string name) => ClientSyncControl<Button>(parent, name).PerformClick();

    private static void ClientSyncRunModal(Form owner, ClientSyncUiService service, string name, Func<ClientConfigSyncDialog, bool> step)
    {
        using var dialog = new ClientConfigSyncDialog(service, "EVE - Fixture Alpha",
            new[] { "EVE - Fixture Beta" }, new[] { "EVE - Fixture Alpha", "EVE - Fixture Beta", "EVE - Fixture Gamma" },
            new[] { "EVE - Fixture Alpha", "EVE - Fixture Beta" });
        Exception? failure = null;
        bool completed = false;
        var watch = Stopwatch.StartNew();
        using var timer = new System.Windows.Forms.Timer { Interval = 30 };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            try
            {
                if (failure != null)
                {
                    service.ReleaseWorkers();
                    (dialog.GetType().GetField("_operation", Any)!.GetValue(dialog) as CancellationTokenSource)?.Cancel();
                    if (!ClientSyncDialogBusy(dialog)) dialog.Close();
                    else timer.Start();
                    return;
                }
                if (watch.Elapsed > TimeSpan.FromSeconds(6)) throw new TimeoutException(name + " timed out; parent watchdog remains authoritative.");
                if (step(dialog)) { completed = true; dialog.Close(); }
                else timer.Start();
            }
            catch (Exception error)
            {
                failure = Root(error);
                service.ReleaseWorkers();
                (dialog.GetType().GetField("_operation", Any)!.GetValue(dialog) as CancellationTokenSource)?.Cancel();
                if (!ClientSyncDialogBusy(dialog)) dialog.Close();
                else timer.Start();
            }
        };
        timer.Start();
        dialog.ShowDialog(owner);
        timer.Stop();
        if (failure != null) throw failure;
        Check(completed && owner.Enabled && IsWindowEnabled(owner.Handle), name + " closes and returns native input to its owner");
    }

    private static void ClientSyncDialogLoadAndSelectionTest(Form owner, string language)
    {
        using var fake = new ClientSyncUiService();
        int stage = 0;
        ClientSyncRunModal(owner, fake, "sync show/load/selection " + language, dialog =>
        {
            if (ClientSyncDialogBusy(dialog)) return false;
            var source = ClientSyncControl<ComboBox>(dialog, "ConfigSyncSourceCombo");
            var targets = ClientSyncControl<CheckedListBox>(dialog, "ConfigSyncTargetList");
            var directory = ClientSyncControl<ComboBox>(dialog, "ConfigSyncDirectoryCombo");
            if (stage == 0)
            {
                if (source.Items.Count != 3) return false;
                Check(dialog.Visible && dialog.Modal && dialog.Owner == owner && GetWindow(dialog.Handle, 4) == owner.Handle
                    && dialog.TopMost && (GetWindowLong(dialog.Handle, -20) & 8) != 0 && IsAboveOwner(dialog.Handle, owner.Handle), "actual copy dialog is owned by and above a topmost main window (" + language + ")");
                Check(IsWindowEnabled(dialog.Handle) && !IsWindowEnabled(owner.Handle), "actual copy dialog blocks only its owner (" + language + ")");
                Check(source.SelectedIndex == -1 && fake.NameCalls == 0 && fake.IdCalls == 0 && fake.SyncCalls == 0,
                    "Show asynchronously loads only fake local IDs, without network or a guessed source (" + language + ")");
                Check(ClientSyncControl<Button>(dialog, "ConfigSyncCancelButton").DialogResult == DialogResult.None,
                    "busy/close button has no implicit modal DialogResult (" + language + ")");
                SaveLifecycleScreenshot(dialog, "client-sync-local-" + language);
                Size previous = dialog.Size;
                dialog.Size = dialog.MinimumSize;
                dialog.PerformLayout();
                Application.DoEvents();
                Rectangle viewport = dialog.RectangleToScreen(dialog.ClientRectangle);
                var close = ClientSyncControl<Button>(dialog, "ConfigSyncCancelButton");
                var copy = ClientSyncControl<Button>(dialog, "ConfigSyncStartButton");
                Check(viewport.Contains(close.RectangleToScreen(close.ClientRectangle))
                    && viewport.Contains(copy.RectangleToScreen(copy.ClientRectangle)) && targets.Height >= targets.MinimumSize.Height,
                    "minimum-size dialog keeps copy/cancel controls and target list inside its viewport (" + language + ")");
                SaveLifecycleScreenshot(dialog, "client-sync-minimum-" + language);
                dialog.Size = previous;
                dialog.PerformLayout();
                source.SelectedIndex = 0;
                ClientSyncActualClick(dialog, "ConfigSyncSelectAllButton");
                Check(targets.CheckedItems.Count == 2, "actual sync dialog selects both explicit local targets (" + language + ")");
                ClientSyncConfirmOwned(dialog, () => ClientSyncActualClick(dialog, "ConfigSyncStartButton"), 7, language == "zh-CN" ? "游戏客户端" : "game clients", "sync-copy-decline-" + language);
                Check(fake.SyncCalls == 0 && targets.CheckedItems.Count == 2, "declining overwrite confirmation does not reach the copy service (" + language + ")");
                ClientSyncConfirmOwned(dialog, () => ClientSyncActualClick(dialog, "ConfigSyncResolveButton"), 6, "ali-esi.evepc.163.com", "sync-network-consent-" + language);
                stage = 1;
                return false;
            }
            if (stage == 1)
            {
                if (fake.IdCalls != 1) return false;
                Check(fake.NameCalls == 1 && source.SelectedIndex == 0 && source.Text.Contains("Fixture Alpha"), "explicit lookup identifies source through the fake service (" + language + ")");
                ClientSyncActualClick(dialog, "ConfigSyncSelectGroupButton");
                Check(targets.CheckedItems.Count == 1 && targets.CheckedItems[0]!.ToString()!.Contains("Fixture Beta"), "identified current-group selection checks only its member (" + language + ")");
                SaveLifecycleScreenshot(dialog, "client-sync-identified-" + language);
                directory.Text = fake.Second;
                Check(source.Items.Count == 0 && source.SelectedIndex == -1 && targets.Items.Count == 0
                    && !ClientSyncControl<Button>(dialog, "ConfigSyncStartButton").Enabled
                    && !ClientSyncControl<Button>(dialog, "ConfigSyncResolveButton").Enabled,
                    "editing a directory immediately invalidates source IDs, names, targets and destructive actions (" + language + ")");
                ClientSyncActualClick(dialog, "ConfigSyncStartButton");
                Check(fake.SyncCalls == 0, "an edited but unread directory cannot reach copying (" + language + ")");
                ClientSyncActualClick(dialog, "ConfigSyncReloadButton");
                stage = 2;
                return false;
            }
			if (stage == 2)
			{
				if (source.Items.Count != 2) return false;
				Check(source.SelectedIndex == -1 && targets.Items.Count == 2 && targets.CheckedItems.Count == 0
					&& source.Items.Cast<object>().All(item => !item.ToString()!.Contains("Fixture Alpha") && !item.ToString()!.Contains("Fixture Beta")),
					"a different loaded directory has only its own unidentified IDs with no stale source or checks (" + language + ")");
				Check(fake.ReadDirectories.SequenceEqual(new[] { fake.First, fake.Second }) && fake.NameCalls == 1 && fake.IdCalls == 1,
					"directory switching neither reads a real CCP path nor performs implicit name lookup (" + language + ")");
				SaveLifecycleScreenshot(dialog, "client-sync-directory-changed-" + language);
				directory.Text = Path.Combine(scratch, "non-fixture-sync-directory");
				ClientSyncActualClick(dialog, "ConfigSyncReloadButton");
				stage = 3;
				return false;
			}
			Check(source.Items.Count == 0 && targets.Items.Count == 0 && !ClientSyncControl<Button>(dialog, "ConfigSyncStartButton").Enabled
				&& !ClientSyncControl<Button>(dialog, "ConfigSyncResolveButton").Enabled
				&& ClientSyncControl<TextBox>(dialog, "ConfigSyncReportBox").Text.Contains("non-fixture directory"),
				"failed directory load retains no stale identities, target choices or enabled destructive action (" + language + ")");
			SaveLifecycleScreenshot(dialog, "client-sync-directory-error-" + language);
            return true;
        });
    }

    private static void ClientSyncDialogLookupCancellationTest(Form owner)
    {
        using var fake = new ClientSyncUiService { BlockNames = true };
        int stage = 0;
        ClientSyncRunModal(owner, fake, "sync lookup cancellation", dialog =>
        {
            var source = ClientSyncControl<ComboBox>(dialog, "ConfigSyncSourceCombo");
            if (stage == 0)
            {
                if (ClientSyncDialogBusy(dialog) || source.Items.Count != 3) return false;
                ClientSyncConfirmOwned(dialog, () => ClientSyncActualClick(dialog, "ConfigSyncResolveButton"), 6, "ali-esi.evepc.163.com", "sync-network-before-cancel");
                stage = 1;
                return false;
            }
            if (stage == 1)
            {
                if (!fake.LookupEntered.IsSet) return false;
                Check(ClientSyncDialogBusy(dialog) && !source.Enabled && ClientSyncControl<Button>(dialog, "ConfigSyncCancelButton").Enabled,
                    "pending online lookup keeps the dialog responsive while editing is disabled");
                ClientSyncActualClick(dialog, "ConfigSyncCancelButton");
                Check(dialog.Visible, "cancelling an operation does not implicitly close its modal dialog");
                stage = 2;
                return false;
            }
            if (ClientSyncDialogBusy(dialog)) return false;
            Check(dialog.Visible && dialog.DialogResult == DialogResult.None && fake.NameCancellationObserved == 1
                && fake.IdCalls == 0 && fake.SyncCalls == 0, "lookup cancellation unwinds asynchronously without a deadlock, follow-up request, or file copy");
            SaveLifecycleScreenshot(dialog, "client-sync-network-cancelled");
            return true;
        });
    }

    private static void ClientSyncDialogCopyCloseCancellationTest(Form owner)
    {
        using var fake = new ClientSyncUiService { BlockSync = true };
        int stage = 0;
        ClientSyncRunModal(owner, fake, "sync copy close/cancellation", dialog =>
        {
            var source = ClientSyncControl<ComboBox>(dialog, "ConfigSyncSourceCombo");
            if (stage == 0)
            {
                if (ClientSyncDialogBusy(dialog) || source.Items.Count != 3) return false;
                source.SelectedIndex = 0;
                ClientSyncActualClick(dialog, "ConfigSyncSelectAllButton");
                ClientSyncConfirmOwned(dialog, () => ClientSyncActualClick(dialog, "ConfigSyncStartButton"), 6, "游戏客户端", "sync-copy-confirmed-before-cancel");
                stage = 1;
                return false;
            }
            if (stage == 1)
            {
                if (!fake.SyncEntered.IsSet) return false;
                Check(ClientSyncDialogBusy(dialog) && fake.CopiedDirectory == fake.First && fake.CopiedSource == 101
                    && fake.CopiedTargets.SequenceEqual(new[] { 202L, 303L }), "confirmed copy reaches the fake service with only the explicitly selected directory/source/targets");
                dialog.Close();
                Check(dialog.Visible, "Close during a pending target requests cancellation instead of disposing active controls");
                stage = 2;
                return false;
            }
            if (ClientSyncDialogBusy(dialog)) return false;
            Check(dialog.Visible && dialog.DialogResult == DialogResult.None && fake.SyncCalls == 1
                && ClientSyncControl<Label>(dialog, "ConfigSyncStatusLabel").Text.Contains("取消"),
                "pending copy cancellation returns to an open usable dialog and does not report an invented success");
            SaveLifecycleScreenshot(dialog, "client-sync-copy-cancelled");
            return true;
        });
    }

	private static void ClientSyncDialogLookupTimeoutTest(Form owner)
	{
		using var fake = new ClientSyncUiService { TimeoutNames = true };
		int stage = 0;
		ClientSyncRunModal(owner, fake, "sync lookup independent timeout", dialog =>
		{
			if (stage == 0)
			{
				if (ClientSyncDialogBusy(dialog) || ClientSyncControl<ComboBox>(dialog, "ConfigSyncSourceCombo").Items.Count != 3) return false;
				ClientSyncConfirmOwned(dialog, () => ClientSyncActualClick(dialog, "ConfigSyncResolveButton"), 6, "ali-esi.evepc.163.com", "sync-network-before-timeout");
				stage = 1;
				return false;
			}
			if (ClientSyncDialogBusy(dialog)) return false;
			Check(dialog.Visible && fake.NameCalls == 1 && fake.IdCalls == 0 && fake.NameCancellationObserved == 0
				&& ClientSyncControl<Label>(dialog, "ConfigSyncStatusLabel").Text.Contains("操作未完成")
				&& ClientSyncControl<TextBox>(dialog, "ConfigSyncReportBox").Text.Contains("timed out"),
				"HTTP-like timeout is an explicit operation failure, not misreported as user cancellation");
			SaveLifecycleScreenshot(dialog, "client-sync-network-timeout");
			return true;
		});
	}

    private static void ClientSyncDialogDiscoveryCloseTest(Form owner)
    {
        using var fake = new ClientSyncUiService();
        fake.DiscoveryRelease.Reset();
        int stage = 0;
        ClientSyncRunModal(owner, fake, "sync discovery close/cancellation", dialog =>
        {
            if (stage == 0)
            {
                if (!fake.DiscoveryEntered.IsSet) return false;
                Check(ClientSyncDialogBusy(dialog), "default-directory discovery runs off the UI thread");
                dialog.Close();
                Check(dialog.Visible, "Close during directory discovery cancels the operation without a synchronous UI wait");
                fake.DiscoveryRelease.Set();
                stage = 1;
                return false;
            }
            if (ClientSyncDialogBusy(dialog)) return false;
            Check(dialog.Visible && fake.ReadDirectories.Count == 0 && fake.NameCalls == 0 && fake.SyncCalls == 0,
                "cancelled discovery stops before reading any profile, networking, or copying");
            return true;
        });
    }

    private static void ClientSyncConfirmOwned(Form dialog, Action trigger, int choice, string expectedText, string screenshotName)
    {
        bool seen = false;
        Exception? failure = null;
        using var timer = new System.Windows.Forms.Timer { Interval = 30 };
        timer.Tick += (_, _) => EnumThreadWindows(GetCurrentThreadId(), (window, _) =>
        {
            var className = new StringBuilder(80);
            var title = new StringBuilder(300);
            GetClassName(window, className, className.Capacity);
            GetWindowText(window, title, title.Capacity);
            if (className.ToString() != "#32770" || title.ToString() != dialog.Text || !IsWindowVisible(window)) return true;
            timer.Stop();
            seen = true;
            try
            {
                Check(GetWindow(window, 4) == dialog.Handle && IsWindowEnabled(window) && !IsWindowEnabled(dialog.Handle),
                    screenshotName + " uses a native confirmation explicitly owned by the copy dialog");
                Check(!dialog.TopMost || ((GetWindowLong(window, -20) & 8) != 0 && IsAboveOwner(window, dialog.Handle)),
                    screenshotName + " confirmation remains above the topmost owner chain");
                var text = new StringBuilder();
                ClientSyncEnumChildWindows(window, (child, _) =>
                {
                    var value = new StringBuilder(4096);
                    GetWindowText(child, value, value.Capacity);
                    text.AppendLine(value.ToString());
                    return true;
                }, IntPtr.Zero);
                Check(text.ToString().Contains(expectedText, StringComparison.OrdinalIgnoreCase), screenshotName + " states its server/privacy or game-overwrite risk");
                Check(ClientSyncGetDlgCtrlID(ClientSyncGetFocus()) == 7, screenshotName + " defaults to No rather than consent");
                ClientSyncSaveNativeScreenshot(window, screenshotName);
            }
            catch (Exception error) { failure = Root(error); }
            finally { EndDialog(window, (IntPtr)(failure == null ? choice : 7)); }
            return false;
        }, IntPtr.Zero);
        timer.Start();
        trigger();
        timer.Stop();
        if (failure != null) throw failure;
        Check(seen && dialog.Enabled && IsWindowEnabled(dialog.Handle), screenshotName + " returns input after an explicit answer");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ClientSyncWindowRect { internal int Left, Top, Right, Bottom; }
    [DllImport("user32.dll", EntryPoint = "EnumChildWindows")] private static extern bool ClientSyncEnumChildWindows(IntPtr parent, EnumThreadWindowProc callback, IntPtr parameter);
    [DllImport("user32.dll", EntryPoint = "GetFocus")] private static extern IntPtr ClientSyncGetFocus();
    [DllImport("user32.dll", EntryPoint = "GetDlgCtrlID")] private static extern int ClientSyncGetDlgCtrlID(IntPtr window);
    [DllImport("user32.dll", EntryPoint = "GetWindowRect")] private static extern bool ClientSyncGetWindowRect(IntPtr window, out ClientSyncWindowRect rectangle);
    [DllImport("user32.dll", EntryPoint = "PrintWindow")] private static extern bool ClientSyncPrintWindow(IntPtr window, IntPtr device, uint flags);

    private static void ClientSyncSaveNativeScreenshot(IntPtr ownMessageBox, string name)
    {
        // Caller has already restricted this HWND to a visible, same-thread
        // message box bearing our dialog's title. No unrelated desktop is read.
        if (!ClientSyncGetWindowRect(ownMessageBox, out ClientSyncWindowRect rectangle)) throw new InvalidOperationException("Cannot measure the owned confirmation.");
        using var bitmap = new Bitmap(rectangle.Right - rectangle.Left, rectangle.Bottom - rectangle.Top);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            IntPtr device = graphics.GetHdc();
            try { Check(ClientSyncPrintWindow(ownMessageBox, device, 0), name + " renders its own native confirmation screenshot"); }
            finally { graphics.ReleaseHdc(device); }
        }
        string path = Path.Combine(screenshots, name + ".png");
        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        Console.WriteLine("SCREENSHOT " + path);
    }
}
