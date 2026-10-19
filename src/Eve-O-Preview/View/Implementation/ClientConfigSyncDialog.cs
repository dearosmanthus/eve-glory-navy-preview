using EveOPreview.Configuration.Implementation;
using EveOPreview.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EveOPreview.View
{
	/// <summary>Explicit, local-first character-settings copy workflow.</summary>
	public sealed class ClientConfigSyncDialog : Form
	{
		private sealed class CharacterItem
		{
			internal long Id;
			internal string Display;
			public override string ToString() => Display;
		}

		private readonly IClientConfigSyncService _service;
		private readonly string _sourceName;
		private readonly HashSet<string> _groupNames;
		private readonly HashSet<string> _knownNames;
		private readonly HashSet<string> _onlineNames;
		private readonly bool _chinese;
		private readonly ComboBox _directories;
		private readonly ComboBox _sources;
		private readonly ComboBox _servers;
		private readonly CheckedListBox _targets;
		private readonly CheckBox _onlineOnly;
		private readonly Label _status;
		private readonly Label _counts;
		private readonly TextBox _report;
		private readonly Button _browse;
		private readonly Button _reload;
		private readonly Button _resolve;
		private readonly Button _all;
		private readonly Button _group;
		private readonly Button _none;
		private readonly Button _sync;
		private readonly Button _cancel;
		private readonly ProgressBar _progress;
		private long[] _localIds = Array.Empty<long>();
		private Dictionary<long, string> _names = new Dictionary<long, string>();
		private CancellationTokenSource _operation;
		private bool _busy;
		private bool _refreshing;
		private bool _initialized;
		private string _loadedDirectory;

		public ClientConfigSyncDialog(IClientConfigSyncService service, string sourceTitle, IList<string> group, IList<string> known, IList<string> online)
		{
			_service = service ?? throw new ArgumentNullException(nameof(service));
			_sourceName = ClientTitleFormatter.GetCharacterName(sourceTitle);
			_groupNames = ToNames(group);
			_knownNames = ToNames(known);
			_onlineNames = ToNames(online);
			_chinese = !string.Equals(LocalizationExtensions.GetString("Messages.Cancel", "取消"), "Cancel", StringComparison.Ordinal);
			Name = "ClientConfigSyncDialog";
			Text = T("复制角色游戏配置", "Copy character game settings");
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimizeBox = false;
			AutoScaleMode = AutoScaleMode.Dpi;
			MinimumSize = new Size(700, 600);
			ClientSize = new Size(850, 680);
			Padding = new Padding(14);

			var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 9 };
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			for (int row = 0; row < 5; row++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			Controls.Add(layout);
			layout.Controls.Add(WrappingLabel(T("只复制所选目录的角色设置文件，不读取账号配置或登录凭据。正常退出相关游戏后操作，避免游戏退出时写回旧设置。", "Copies only character settings in the selected directory, not account settings or login credentials. Exit affected game clients normally first to prevent old settings being written back.")), 0, 0);

			var directoryRow = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 4, Margin = new Padding(0, 10, 0, 4) };
			directoryRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			directoryRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			directoryRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			directoryRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			directoryRow.Controls.Add(new Label { AutoSize = true, Anchor = AnchorStyles.Left, Text = T("配置目录", "Directory") }, 0, 0);
			_directories = new ComboBox { Name = "ConfigSyncDirectoryCombo", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown };
			_browse = MakeButton("ConfigSyncBrowseButton", T("浏览…", "Browse…"));
			_reload = MakeButton("ConfigSyncReloadButton", T("读取目录", "Read directory"));
			directoryRow.Controls.Add(_directories, 1, 0);
			directoryRow.Controls.Add(_browse, 2, 0);
			directoryRow.Controls.Add(_reload, 3, 0);
			layout.Controls.Add(directoryRow, 0, 1);

			var sourceRow = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2, Margin = new Padding(0, 4, 0, 4) };
			sourceRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			sourceRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			sourceRow.Controls.Add(new Label { AutoSize = true, Anchor = AnchorStyles.Left, Text = T("源角色 ID", "Source ID") }, 0, 0);
			_sources = new ComboBox { Name = "ConfigSyncSourceCombo", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
			sourceRow.Controls.Add(_sources, 1, 0);
			layout.Controls.Add(sourceRow, 0, 2);

			var lookupRow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Margin = new Padding(0, 4, 0, 4) };
			lookupRow.Controls.Add(new Label { AutoSize = true, Padding = new Padding(0, 6, 0, 0), Text = T("角色名称服务器", "Name lookup server") });
			_servers = new ComboBox { Name = "ConfigSyncServerCombo", DropDownStyle = ComboBoxStyle.DropDownList, Width = 190 };
			_servers.Items.Add(T("国服 · Serenity", "Serenity · China"));
			_servers.Items.Add(T("国际服 · Tranquility", "Tranquility · Global"));
			_servers.SelectedIndex = 0;
			_resolve = MakeButton("ConfigSyncResolveButton", T("联网识别名称", "Look up names online"));
			lookupRow.Controls.Add(_servers);
			lookupRow.Controls.Add(_resolve);
			lookupRow.Controls.Add(new Label { AutoSize = true, Padding = new Padding(0, 6, 0, 0), Text = T("不联网也可手动选择本地 ID。", "Local IDs work without a network connection.") });
			layout.Controls.Add(lookupRow, 0, 3);

			var selectionRow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Margin = new Padding(0, 4, 0, 4) };
			_all = MakeButton("ConfigSyncSelectAllButton", T("全选", "All"));
			_group = MakeButton("ConfigSyncSelectGroupButton", T("选择本组", "Current group"));
			_none = MakeButton("ConfigSyncSelectNoneButton", T("清空选择", "None"));
			_onlineOnly = new CheckBox { Name = "ConfigSyncOnlineOnlyCheckBox", AutoSize = true, Text = T("只显示已识别的在线角色", "Only identified online characters"), Margin = new Padding(12, 6, 0, 0) };
			selectionRow.Controls.AddRange(new Control[] { _all, _group, _none, _onlineOnly });
			layout.Controls.Add(selectionRow, 0, 4);
			_targets = new CheckedListBox { Name = "ConfigSyncTargetList", Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false, HorizontalScrollbar = true, MinimumSize = new Size(100, 80) };
			layout.Controls.Add(_targets, 0, 5);
			var statusPanel = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1 };
			statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			_counts = WrappingLabel(string.Empty);
			_status = WrappingLabel(T("本地 ID 未识别前，不推测其角色名、所属组或在线状态。", "Until IDs are identified, names, group membership, and online state are unknown."));
			_status.Name = "ConfigSyncStatusLabel";
			_status.ForeColor = SystemColors.GrayText;
			statusPanel.Controls.Add(_counts, 0, 0);
			statusPanel.Controls.Add(_status, 0, 1);
			layout.Controls.Add(statusPanel, 0, 6);
			_report = new TextBox { Name = "ConfigSyncReportBox", Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Text = T("来源窗口：", "Source window: ") + (string.IsNullOrEmpty(_sourceName) ? T("未登录角色", "No character identified") : _sourceName) + Environment.NewLine + T("请选择本地源 ID 和目标；点击开始复制后仍需确认。", "Choose a local source ID and targets. A confirmation is required before copying.") };
			layout.Controls.Add(_report, 0, 7);
			var footer = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 3 };
			footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			_progress = new ProgressBar { Dock = DockStyle.Fill, Style = ProgressBarStyle.Marquee, Visible = false, Margin = new Padding(0, 9, 12, 9) };
			_sync = MakeButton("ConfigSyncStartButton", T("开始复制…", "Copy settings…"));
			_cancel = MakeButton("ConfigSyncCancelButton", T("关闭", "Close"));
			footer.Controls.Add(_progress, 0, 0);
			footer.Controls.Add(_sync, 1, 0);
			footer.Controls.Add(_cancel, 2, 0);
			layout.Controls.Add(footer, 0, 8);
			CancelButton = _cancel;
			// Form.CancelButton assigns DialogResult.Cancel automatically. Cancellation
			// must not implicitly close a modal while a worker is finishing a target.
			_cancel.DialogResult = DialogResult.None;

			_browse.Click += async (sender, args) => await BrowseAsync();
			_reload.Click += async (sender, args) => await LoadDirectoryAsync();
			_directories.SelectionChangeCommitted += async (sender, args) => await LoadDirectoryAsync();
			_directories.TextChanged += (sender, args) =>
			{
				_loadedDirectory = null;
				_localIds = Array.Empty<long>();
				_names.Clear();
				_onlineOnly.Checked = false;
				RefreshCharacters(null);
				SetStatus(T("目录已改变，请先点击“读取目录”。旧角色识别和选择已清除。", "Directory changed. Read it before selecting roles; previous identities and selections were cleared."));
			};
			_sources.SelectedIndexChanged += (sender, args) => { if (!_refreshing) RefreshTargets(); };
			_servers.SelectedIndexChanged += (sender, args) =>
			{
				_names.Clear();
				RefreshCharacters(null);
				SetStatus(T("服务器已切换，旧的识别结果已清除。需要时请重新点击联网识别。", "Server changed; previous lookup results were cleared. Click online lookup again if needed."));
			};
			_onlineOnly.CheckedChanged += (sender, args) =>
			{
				RefreshTargets();
				if (_onlineOnly.Checked) SetStatus(T("仅列出已通过当前服务器识别，且名称与在线窗口对应的角色。未知 ID 不算离线，也不会被猜测。", "Only IDs identified on this server and matched to online window names are shown. Unknown IDs are not guessed."));
			};
			_all.Click += (sender, args) => SelectTargets(item => true);
			_none.Click += (sender, args) => SelectTargets(item => false);
			_group.Click += (sender, args) =>
			{
				SelectTargets(item => _names.TryGetValue(item.Id, out string name) && _groupNames.Contains(name));
				SetStatus(T("已按识别后的角色名称选择本组成员；未识别 ID 不会被猜测加入。", "Selected identified members of the current group; unknown IDs were not guessed."));
			};
			_targets.ItemCheck += (sender, args) => { if (!_refreshing && IsHandleCreated) BeginInvoke(new Action(UpdateCounts)); };
			_resolve.Click += async (sender, args) => await ResolveAsync();
			_sync.Click += async (sender, args) => await SyncAsync();
			_cancel.Click += (sender, args) => { if (_busy) _operation?.Cancel(); else Close(); };
			Shown += async (sender, args) => { if (!_initialized) { _initialized = true; await DiscoverDirectoriesAsync(); } };
			SetBusy(false);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			TopMost = Owner?.TopMost ?? false;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (_busy)
			{
				_operation?.Cancel();
				e.Cancel = true;
				SetStatus(T("正在停止；当前目标完成后即可关闭。不会中断一次文件替换。", "Stopping; close after the current target finishes. An individual file replacement will not be interrupted."));
			}
			base.OnFormClosing(e);
		}

		private async Task DiscoverDirectoriesAsync()
		{
			await RunOperationAsync(async token =>
			{
				IReadOnlyList<string> found = await Task.Run(() => _service.FindSettingsDirectories());
				token.ThrowIfCancellationRequested();
				foreach (string directory in found) _directories.Items.Add(directory);
				if (found.Count > 0)
				{
					_directories.SelectedIndex = 0;
					await ReadDirectoryAsync(_directories.Text, token);
				}
				else SetStatus(T("未找到含角色配置的默认目录，可点浏览选择 settings_* 文件夹。", "No default character settings were found. Browse to a settings_* directory."));
			});
		}

		private async Task BrowseAsync()
		{
			if (_busy) return;
			using var browse = new FolderBrowserDialog { Description = T("选择含 core_char_数字.dat 的游戏配置目录", "Choose the directory containing core_char_<ID>.dat files"), UseDescriptionForTitle = true, ShowNewFolderButton = false };
			if (Directory.Exists(_directories.Text)) browse.SelectedPath = _directories.Text;
			if (browse.ShowDialog(this) != DialogResult.OK) return;
			_directories.Text = browse.SelectedPath;
			await LoadDirectoryAsync();
		}

		private async Task LoadDirectoryAsync()
		{
			string directory = _directories.Text;
			await RunOperationAsync(token => ReadDirectoryAsync(directory, token));
		}

		private async Task ReadDirectoryAsync(string directory, CancellationToken token)
		{
			_loadedDirectory = null;
			_localIds = Array.Empty<long>();
			_names.Clear();
			_onlineOnly.Checked = false;
			RefreshCharacters(null);
			IReadOnlyList<long> ids = await Task.Run(() => _service.GetLocalCharacterIds(directory));
			token.ThrowIfCancellationRequested();
			if (!string.Equals(_directories.Text, directory, StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException(T("读取期间目录已变化，请重新读取。", "Directory changed while loading; read it again."));
			_localIds = ids.ToArray();
			_loadedDirectory = directory;
			RefreshCharacters(null);
			SetStatus(string.Format(CultureInfo.CurrentCulture, T("读取到 {0} 个本地角色 ID。未联网识别，可按 ID 选择。", "Found {0} local character IDs. Select by ID or request online names."), ids.Count));
		}

		private async Task ResolveAsync()
		{
			if (_busy || !IsCurrentDirectoryLoaded || _localIds.Length == 0) return;
			EveServer server = SelectedServer;
			string destination = server == EveServer.Serenity ? "ali-esi.evepc.163.com (Serenity)" : "esi.evetech.net (Tranquility)";
			if (MessageBox.Show(this, T("仅本次向所选服务器发送本地角色 ID 和来源角色名以识别名称，不发送配置内容或凭据。\n\n服务器：", "This lookup sends local character IDs and the source character name to the selected server, not settings contents or credentials.\n\nServer: ") + destination + T("\n\n是否继续？", "\n\nContinue?"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
			await RunOperationAsync(async token =>
			{
				IDictionary<long, string> names = await _service.ResolveNamesAsync(_localIds, server, token);
				long? source = null;
				if (!string.IsNullOrWhiteSpace(_sourceName))
				{
					source = await _service.ResolveIdAsync(_sourceName, server, token);
					if (source.HasValue && _localIds.Contains(source.Value)) names[source.Value] = _sourceName;
				}
				_names = new Dictionary<long, string>(names.Where(pair => _localIds.Contains(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value)).ToDictionary(pair => pair.Key, pair => pair.Value));
				RefreshCharacters(source.HasValue && _localIds.Contains(source.Value) ? source : SelectedSourceId);
				string sourceStatus = string.IsNullOrEmpty(_sourceName) || source.HasValue ? string.Empty : T(" 来源角色未得到精确 ID，请手动选择本地源 ID。", " The source name did not resolve exactly; choose its local source ID manually.");
				SetStatus(string.Format(CultureInfo.CurrentCulture, T("已识别 {0}/{1} 个角色。不会改用另一服务器。", "Identified {0}/{1} characters. No other server is queried."), _names.Count, _localIds.Length) + sourceStatus);
			});
		}

		private async Task SyncAsync()
		{
			if (_busy || !IsCurrentDirectoryLoaded || !SelectedSourceId.HasValue) return;
			long source = SelectedSourceId.Value;
			long[] targets = _targets.CheckedItems.Cast<CharacterItem>().Select(item => item.Id).ToArray();
			if (targets.Length == 0) return;
			string directory = _directories.Text;
			string targetText = string.Join(", ", targets.Take(25)) + (targets.Length > 25 ? " …" : string.Empty);
			string warning = string.Format(CultureInfo.CurrentCulture, T("将源 ID {0} 的角色配置覆盖到 {1} 个目标。\n目标 ID：{2}\n目录：{3}\n\n每个目标先备份，备份失败不会覆盖。只处理 core_char 文件，不处理账号配置。\n\n请先正常退出这些游戏客户端，否则客户端可能在退出时回写旧设置。本软件不会关闭游戏。\n\n确认开始复制？", "Copy source ID {0} over {1} target character settings?\nTarget IDs: {2}\nDirectory: {3}\n\nEach target is backed up first; backup failure prevents its overwrite. Only core_char files are handled, not account settings.\n\nExit these game clients normally first, or they may write old settings back on exit. This app will not close the game.\n\nStart copying?"), source, targets.Length, targetText, directory);
			if (MessageBox.Show(this, warning, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
			await RunOperationAsync(async token =>
			{
				ClientConfigSyncResult result = await Task.Run(() => _service.Sync(directory, source, targets, token));
				var report = new StringBuilder();
				report.AppendLine(string.Format(CultureInfo.CurrentCulture, T("成功 {0}，失败 {1}。{2}", "Succeeded: {0}; failed: {1}. {2}"), result.SuccessCount, result.FailureCount, result.Cancelled ? T("已取消未处理的目标。", "Remaining targets cancelled.") : string.Empty));
				if (!string.IsNullOrWhiteSpace(result.BackupDirectory)) report.AppendLine(T("备份目录：", "Backup directory: ") + result.BackupDirectory);
				foreach (ClientConfigSyncTargetResult item in result.Results)
				{
					report.AppendLine(item.TargetId + (item.Success ? T("：已复制", ": copied") : T("：失败，", ": failed, ") + item.Error));
					if (!string.IsNullOrWhiteSpace(item.BackupPath)) report.AppendLine("  " + T("备份：", "Backup: ") + item.BackupPath);
				}
				_report.Text = report.ToString();
				SetStatus(result.Cancelled ? T("已安全停止，未开始的目标保持原样。", "Stopped safely; unprocessed targets were left unchanged.") : T("操作完成。结果和备份路径见下方，可选中文本复制。", "Finished. Results and selectable backup paths are shown below."));
			});
		}

		private async Task RunOperationAsync(Func<CancellationToken, Task> action)
		{
			if (_busy) return;
			using var operation = new CancellationTokenSource();
			_operation = operation;
			SetBusy(true);
			try { await action(operation.Token); }
			catch (OperationCanceledException) when (operation.IsCancellationRequested) { SetStatus(T("操作已取消。未进行跨服回退。", "Operation cancelled. No cross-server fallback was attempted.")); }
			catch (Exception error)
			{
				SetStatus(T("操作未完成：", "Operation did not complete: ") + error.Message);
				_report.Text = T("未完成的操作不会被计为成功。请核对目录、所选服务器或文件占用情况后重试。\r\n", "An incomplete operation is not counted as success. Check the directory, selected server, or file locks and retry.\r\n") + error.Message;
			}
			finally { _operation = null; SetBusy(false); }
		}

		private void RefreshCharacters(long? preferredSource)
		{
			long? selection = preferredSource ?? SelectedSourceId;
			_refreshing = true;
			try
			{
				_sources.Items.Clear();
				foreach (long id in _localIds) _sources.Items.Add(CreateItem(id));
				_sources.SelectedIndex = -1;
				if (selection.HasValue)
					for (int i = 0; i < _sources.Items.Count; i++)
						if (((CharacterItem)_sources.Items[i]).Id == selection.Value) { _sources.SelectedIndex = i; break; }
			}
			finally { _refreshing = false; }
			RefreshTargets();
		}

		private void RefreshTargets()
		{
			var selected = new HashSet<long>(_targets.CheckedItems.Cast<CharacterItem>().Select(item => item.Id));
			_refreshing = true;
			_targets.BeginUpdate();
			try
			{
				_targets.Items.Clear();
				foreach (long id in _localIds)
				{
					if (id == SelectedSourceId) continue;
					bool identifiedOnline = _names.TryGetValue(id, out string name) && _onlineNames.Contains(name);
					if (_onlineOnly.Checked && !identifiedOnline) continue;
					_targets.Items.Add(CreateItem(id), selected.Contains(id));
				}
			}
			finally { _targets.EndUpdate(); _refreshing = false; }
			UpdateCounts();
		}

		private CharacterItem CreateItem(long id)
		{
			string display = "ID " + id.ToString(CultureInfo.InvariantCulture);
			if (_names.TryGetValue(id, out string name))
			{
				string state = _onlineNames.Contains(name) ? T("在线", "online") : _knownNames.Contains(name) ? T("离线", "offline") : T("本地配置", "local settings");
				display = name + " — " + display + " [" + state + "]";
			}
			else display += T(" [未识别]", " [unidentified]");
			return new CharacterItem { Id = id, Display = display };
		}

		private void SelectTargets(Func<CharacterItem, bool> predicate)
		{
			if (_busy) return;
			_refreshing = true;
			try { for (int i = 0; i < _targets.Items.Count; i++) _targets.SetItemChecked(i, predicate((CharacterItem)_targets.Items[i])); }
			finally { _refreshing = false; }
			UpdateCounts();
		}

		private void SetBusy(bool busy)
		{
			_busy = busy;
			foreach (Control control in new Control[] { _directories, _sources, _servers, _targets, _onlineOnly, _browse, _reload, _resolve, _all, _group, _none }) control.Enabled = !busy;
			_progress.Visible = busy;
			_cancel.Text = busy ? T("取消当前操作", "Cancel operation") : T("关闭", "Close");
			UpdateCounts();
		}

		private void UpdateCounts()
		{
			_counts.Text = string.Format(CultureInfo.CurrentCulture, T("本地 {0} 个 ID；当前显示 {1} 个目标，已选 {2} 个。", "{0} local IDs; {1} visible targets; {2} selected."), _localIds.Length, _targets.Items.Count, _targets.CheckedItems.Count);
			_sync.Enabled = !_busy && IsCurrentDirectoryLoaded && SelectedSourceId.HasValue && _targets.CheckedItems.Count > 0;
			_resolve.Enabled = !_busy && IsCurrentDirectoryLoaded && _localIds.Length > 0;
		}

		private long? SelectedSourceId => (_sources.SelectedItem as CharacterItem)?.Id;
		private bool IsCurrentDirectoryLoaded => _loadedDirectory != null && string.Equals(_loadedDirectory, _directories.Text, StringComparison.OrdinalIgnoreCase);
		private EveServer SelectedServer => _servers.SelectedIndex == 1 ? EveServer.Tranquility : EveServer.Serenity;
		private void SetStatus(string value) => _status.Text = value;
		private string T(string chinese, string english) => _chinese ? chinese : english;
		private static HashSet<string> ToNames(IEnumerable<string> titles) => new HashSet<string>((titles ?? Array.Empty<string>()).Select(ClientTitleFormatter.GetCharacterName).Where(name => !string.IsNullOrWhiteSpace(name)), StringComparer.OrdinalIgnoreCase);
		private static Button MakeButton(string name, string text) => new Button { Name = name, Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(64, 28), Margin = new Padding(3) };
		private static Label WrappingLabel(string text) => new Label { Text = text, AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 3, 0, 3) };
	}
}
