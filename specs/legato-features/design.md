# 实现约定

## 动态组

`Configuration.CycleGroupDefinition` 包含 `Id`、`Name`、`ForwardHotkeys`、`BackwardHotkeys`、`ClientsOrder`，集合均为可序列化 List／Dictionary。`IThumbnailConfiguration` 新增 `List<CycleGroupDefinition> CycleGroups`、`bool AutoAddUngroupedClients`、`List<string> CycleGroupAutoAddExclusions`、`void EnsureCycleGroups()`。

旧五组字段仍可读取和写出，作为前五组的兼容别名；动态模型存在时优先。旧配置没有动态字段时按原字段迁移。旧调用者直接设置五组字段时仍须生效。快照深拷贝动态组，失败回滚包含组名、ID、按键和顺序，不回滚无关布局。

设置页以独立控件管理组状态；所有输出都是深拷贝草稿，通过 Presenter 校验后才保存。组编辑会触发 `CycleGroupsChanged`；状态刷新不得清空未应用的快捷键输入。原始窗口标题作为键，缩短后的角色名只作展示。

`IMainFormView` 新增 `SetCycleGroups(IList<CycleGroupDefinition>)`、`List<CycleGroupDefinition> GetCycleGroups()`、`string SelectedDynamicCycleGroupId`、`bool AutoAddUngroupedClients`、`Action CycleGroupsChanged`、`Action<string> ClientPreviewToggleRequested`、`Action<string> ClientConfigSyncRequested`、`RefreshCycleGroupClients(IList<string> onlineTitles, IList<string> knownTitles, IList<string> hiddenTitles)`。旧五组视图 API 暂保留兼容测试／旧调用，生产页改用统一模型。

## 游戏角色配置同步

`Services.IClientConfigSyncService` 约定：

```csharp
IReadOnlyList<string> FindSettingsDirectories();
IReadOnlyList<long> GetLocalCharacterIds(string directory);
Task<IDictionary<long, string>> ResolveNamesAsync(IEnumerable<long> ids, EveServer server, CancellationToken cancellationToken);
Task<long?> ResolveIdAsync(string name, EveServer server, CancellationToken cancellationToken);
ClientConfigSyncResult Sync(string directory, long sourceId, IEnumerable<long> targetIds, CancellationToken cancellationToken);
```

`EveServer` 明确分为 `Serenity`、`Tranquility`。实现类提供公共无参构造；测试可注入 HttpClient 或假接口。结果提供成功／失败计数、错误列表与备份目录。路径验证拒绝重解析点与源目标相同，任何备份失败都不得覆盖对应目标。只在临时目录测试真实文件复制。

`View.ClientConfigSyncDialog` 构造接收服务、源窗口标题、当前组标题、全部已知标题、在线标题；显示时有明确主窗 Owner，并继承其置顶状态。用户可选择目录与源角色 ID，明确点击联网识别后才请求角色名；同步前确认覆盖及运行中客户端可能回写旧设置的风险。磁盘工作异步执行，取消在目标间检查，不留下半写文件。不会替用户停止 EVE。

新增 `View.ClientTitleFormatter.GetCharacterName(string)` 仅用于展示与显式联网查询，不迁移配置键。已存在功能不重做。

## 所有权

- 后端代理：配置模型、兼容迁移、热键快照与管理器动态组绑定。
- 同步代理：新增同步服务／模型／弹窗／标题格式化，不能写现有 Presenter 或 MainForm。
- UI 代理：新增动态组控件与 MainForm.CycleGroups 部分，不写现有 MainForm／Presenter／接口。
- 主代理：共享接口、MainForm 集成、Presenter、测试集成、说明和最终验收。构建串行，真实桌面操作只有主代理。
