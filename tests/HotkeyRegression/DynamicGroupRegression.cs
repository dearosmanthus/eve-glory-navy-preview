using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using EveOPreview.Configuration;
using EveOPreview.View;
using Newtonsoft.Json;

internal static partial class Program
{
    private static void DynamicGroupTests()
    {
        Run("canonical groups and legacy aliases", DynamicGroupCanonicalTests);
        Run("dynamic groups persistence and legacy migration", DynamicGroupPersistenceTests);
        Run("complete dynamic group snapshot", DynamicGroupSnapshotTests);
        Run("real Windows dynamic group transactions", DynamicGroupTransactionTests);
    }

    private static IThumbnailConfiguration ReadDynamicConfiguration(string json)
    {
        return (IThumbnailConfiguration)JsonConvert.DeserializeObject(json,
            T("EveOPreview.Configuration.Implementation.ThumbnailConfiguration"))!;
    }

    private static void DynamicGroupCanonicalTests()
    {
        const string canonical = """[{"Id":"canonical-one","Name":"主力组","ForwardHotkeys":["Control+Shift+F17"],"BackwardHotkeys":["Control+Shift+F18"],"ClientsOrder":{"EVE - Main":2,"EVE - Scout":1}}]""";
        const string legacy = """
            "CycleGroup1ForwardHotkeys":["Alt+F22"],"CycleGroup1BackwardHotkeys":["Alt+F23"],"CycleGroup1ClientsOrder":{"EVE - Wrong":99}
            """;
        foreach (bool canonicalFirst in new[] { true, false })
        {
            string json = canonicalFirst
                ? "{\"CycleGroups\":" + canonical + "," + legacy + "}"
                : "{" + legacy + ",\"CycleGroups\":" + canonical + "}";
            IThumbnailConfiguration config = ReadDynamicConfiguration(json);
            string where = canonicalFirst ? "before" : "after";
            Check(config.CycleGroups.Count == 1 && config.CycleGroups[0].Id == "canonical-one",
                "canonical collection wins when located " + where + " legacy JSON fields");
            Check(config.CycleGroup1ForwardHotkeys.SequenceEqual(new[] { "Control+Shift+F17" })
                && config.CycleGroup1BackwardHotkeys.SequenceEqual(new[] { "Control+Shift+F18" }),
                "legacy hotkey fields cannot overwrite canonical keys (" + where + ")");
            Check(config.CycleGroup1ClientsOrder.OrderBy(item => item.Value).Select(item => item.Key)
                    .SequenceEqual(new[] { "EVE - Scout", "EVE - Main" }),
                "legacy order cannot overwrite canonical membership (" + where + ")");

            // Default Json.NET settings and the application's Replace mode must both work.
            var populated = (IThumbnailConfiguration)Config();
            JsonConvert.PopulateObject(json, populated,
                new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
            Check(populated.CycleGroups.Count == 1 && populated.CycleGroups[0].Id == "canonical-one"
                    && populated.CycleGroup1ForwardHotkeys.SequenceEqual(config.CycleGroup1ForwardHotkeys),
                "PopulateObject canonical priority is independent of property order (" + where + ")");

            int initialCount = config.CycleGroups.Count;
            _ = config.CycleGroup5ForwardHotkeys;
            _ = config.CycleGroup4BackwardHotkeys;
            _ = config.CycleGroup3ClientsOrder;
            string saved = JsonConvert.SerializeObject(config);
            Check(config.CycleGroups.Count == initialCount && ReadDynamicConfiguration(saved).CycleGroups.Count == initialCount,
                "reading/serializing absent legacy aliases does not create phantom groups (" + where + ")");

            config.CycleGroup1ForwardHotkeys = new List<string> { "Shift+F19" };
            config.CycleGroup1ClientsOrder = new Dictionary<string, int> { ["EVE - Replacement"] = 1 };
            Check(config.CycleGroups[0].ForwardHotkeys.SequenceEqual(new[] { "Shift+F19" })
                    && config.CycleGroups[0].ClientsOrder.ContainsKey("EVE - Replacement"),
                "legacy setters still update the canonical first group (" + where + ")");
            config.CycleGroups[0].BackwardHotkeys.Add("Shift+F20");
            Check(config.CycleGroup1BackwardHotkeys.Contains("Shift+F20"),
                "legacy getters are live aliases for an existing canonical group (" + where + ")");
            config.CycleGroup5BackwardHotkeys = new List<string> { "Shift+F21" };
            Check(config.CycleGroups.Count == 5 && config.CycleGroups[4].BackwardHotkeys.Single() == "Shift+F21",
                "explicit legacy setter can extend collection for old callers (" + where + ")");
        }

        IThumbnailConfiguration malformed = ReadDynamicConfiguration("""{"CycleGroups":[{"Id":"same","Name":""},{"Id":"same","ForwardHotkeys":null,"BackwardHotkeys":null,"ClientsOrder":null},null],"CycleGroupAutoAddExclusions":["EVE - A","EVE - A",""]}""");
        Check(malformed.CycleGroups.All(group => !string.IsNullOrWhiteSpace(group.Id) && !string.IsNullOrWhiteSpace(group.Name))
                && malformed.CycleGroups.Select(group => group.Id).Distinct().Count() == 3,
            "normalization gives missing/duplicate group identities unique stable IDs");
        Check(malformed.CycleGroups.All(group => group.ForwardHotkeys != null && group.BackwardHotkeys != null && group.ClientsOrder != null)
                && malformed.CycleGroupAutoAddExclusions.SequenceEqual(new[] { "EVE - A" }),
            "normalization repairs null group collections and duplicate exclusions");
        IThumbnailConfiguration empty = ReadDynamicConfiguration("""{"CycleGroups":[],"CycleGroup1ForwardHotkeys":["Shift+F24"]}""");
        Check(empty.CycleGroups.Count == 1 && empty.CycleGroups[0].ForwardHotkeys.Count == 0,
            "explicit empty canonical collection does not resurrect stale legacy groups");
    }

    private static void DynamicGroupPersistenceTests()
    {
        var config = (IThumbnailConfiguration)Config();
        config.CycleGroups = Enumerable.Range(1, 8).Select(number => new CycleGroupDefinition
        {
            Id = "persist-" + number,
            Name = "编队 " + number,
            ForwardHotkeys = new List<string> { "Control+Shift+F" + (number + 13) },
            BackwardHotkeys = new List<string>(),
            ClientsOrder = new Dictionary<string, int> { ["EVE - Offline " + number] = 1, ["EVE - Shared"] = 2 }
        }).ToList();
        config.AutoAddUngroupedClients = true;
        config.CycleGroupAutoAddExclusions = new List<string> { "EVE - Removed" };
        config.ThumbnailSize = new Size(151, 83);
        config.PerClientThumbnailSize["EVE - Sized"] = new Size(123, 71);
        config.SetThumbnailLocation("EVE - Positioned", string.Empty, new Point(37, 61));
        config.ClientHotkey["EVE - Direct"] = "Control+Shift+F23";
        string path = Path.Combine(scratch, "dynamic-eight-groups.json");
        object appConfig = New("EveOPreview.Configuration.Implementation.AppConfig");
        Set(appConfig, "ConfigFileName", path);
        object storage = New("EveOPreview.Configuration.Implementation.ConfigurationStorage", appConfig, config);
        Call(storage, "Save");

        var reloaded = (IThumbnailConfiguration)Config();
        object storage2 = New("EveOPreview.Configuration.Implementation.ConfigurationStorage", appConfig, reloaded);
        Call(storage2, "Load");
        Check(reloaded.CycleGroups.Count == 8 && reloaded.CycleGroups.Select(group => group.Id).SequenceEqual(config.CycleGroups.Select(group => group.Id)),
            "saving/reloading retains groups beyond the old five-group limit and their IDs");
        Check(reloaded.CycleGroups[7].Name == "编队 8"
                && reloaded.CycleGroups[7].ForwardHotkeys.Single() == "Control+Shift+F21"
                && reloaded.CycleGroups[7].ClientsOrder.OrderBy(item => item.Value).Select(item => item.Key)
                    .SequenceEqual(new[] { "EVE - Offline 8", "EVE - Shared" }),
            "eighth group's name, keys, offline members, and exact order survive persistence");
        Check(reloaded.AutoAddUngroupedClients && reloaded.CycleGroupAutoAddExclusions.SequenceEqual(new[] { "EVE - Removed" }),
            "automatic collection option and explicit removals survive persistence");
        Check(reloaded.ThumbnailSize == new Size(151, 83)
                && reloaded.PerClientThumbnailSize["EVE - Sized"] == new Size(123, 71)
                && reloaded.GetThumbnailLocation("EVE - Positioned", string.Empty, Point.Empty) == new Point(37, 61)
                && reloaded.ClientHotkey["EVE - Direct"] == "Control+Shift+F23",
            "dynamic group persistence preserves existing sizes, layout, and per-role shortcuts");
        Check(reloaded.GetAllKnownClients().Contains("EVE - Offline 8")
                && reloaded.GetAllKnownClients().Contains("EVE - Positioned")
                && reloaded.GetAllKnownClients().Count(title => title == "EVE - Shared") == 1,
            "known client list includes offline group members and layouts without duplicates");

        // Reuse the same live object to ensure old profiles do not inherit dynamic extras.
        const string oldProfile = """{"CycleGroup1ForwardHotkeys":["Control+Alt+F17"],"CycleGroup1BackwardHotkeys":["Control+Alt+F18"],"CycleGroup1ClientsOrder":{"EVE - Scout":1,"EVE - Main":2},"CycleGroup2ForwardHotkeys":["Shift+F20"],"ThumbnailSize":"173, 97","ClientHotkey":{"EVE - Existing":"Shift+F24"},"FlatLayout":{"EVE - Existing":"23, 47"}}""";
        File.WriteAllText(path, oldProfile);
        Call(storage2, "Load");
        Check(reloaded.CycleGroups.Count == 5
                && reloaded.CycleGroups[0].ForwardHotkeys.SequenceEqual(new[] { "Control+Alt+F17" })
                && reloaded.CycleGroups[1].ForwardHotkeys.SequenceEqual(new[] { "Shift+F20" }),
            "old profile migrates its original five groups rather than retaining prior dynamic extras");
        Check(reloaded.CycleGroups[0].ClientsOrder.OrderBy(item => item.Value).Select(item => item.Key)
                    .SequenceEqual(new[] { "EVE - Scout", "EVE - Main" })
                && reloaded.ThumbnailSize == new Size(173, 97)
                && reloaded.ClientHotkey["EVE - Existing"] == "Shift+F24"
                && reloaded.GetThumbnailLocation("EVE - Existing", string.Empty, Point.Empty) == new Point(23, 47),
            "legacy migration preserves old role order and unrelated layout/shortcut settings");
        Check(!reloaded.AutoAddUngroupedClients && reloaded.CycleGroupAutoAddExclusions.Count == 0,
            "loading old profile defaults automatic collection off and clears prior exclusions");
        string[] migratedIds = reloaded.CycleGroups.Select(group => group.Id).ToArray();
        Call(storage2, "Save");
        Call(storage2, "Load");
        Check(reloaded.CycleGroups.Select(group => group.Id).SequenceEqual(migratedIds),
            "legacy migration assigns identities once and keeps them across subsequent loads");
    }

    private static void DynamicGroupSnapshotTests()
    {
        var config = (IThumbnailConfiguration)MinimalConfig(Keys.None, Keys.None);
        config.CycleGroups = new List<CycleGroupDefinition>
        {
            new() { Id = "snapshot-one", Name = "后勤", ForwardHotkeys = new() { "Shift+F17", "Shift+F18" },
                BackwardHotkeys = new() { "Shift+F19" }, ClientsOrder = new() { ["EVE - B"] = 1, ["EVE - A"] = 2 } },
            new() { Id = "snapshot-two", Name = "侦查", ClientsOrder = new() { ["EVE - C"] = 1 } }
        };
        config.AutoAddUngroupedClients = true;
        config.CycleGroupAutoAddExclusions = new() { "EVE - Excluded" };
        Type snapshotType = T("EveOPreview.UI.Hotkeys.HotkeySettingsSnapshot");
        object snapshot = snapshotType.GetMethod("Capture", BindingFlags.Static | BindingFlags.Public)!.Invoke(null, new object[] { config })!;
        config.CycleGroups[0].Id = "mutated-id";
        config.CycleGroups[0].Name = "变更";
        config.CycleGroups[0].ForwardHotkeys.Clear();
        config.CycleGroups[0].BackwardHotkeys.Add("Shift+F20");
        config.CycleGroups[0].ClientsOrder.Clear();
        config.CycleGroups.RemoveAt(1);
        config.CycleGroups.Add(new CycleGroupDefinition { Id = "new-group", Name = "新增" });
        config.AutoAddUngroupedClients = false;
        config.CycleGroupAutoAddExclusions.Add("EVE - New exclusion");
        config.ThumbnailSize = new Size(211, 113);
        config.SetThumbnailLocation("EVE - Unrelated", string.Empty, new Point(51, 77));
        Call(snapshot, "Restore", config);
        Check(config.CycleGroups.Count == 2 && config.CycleGroups.Select(group => group.Id).SequenceEqual(new[] { "snapshot-one", "snapshot-two" })
                && config.CycleGroups[0].Name == "后勤",
            "snapshot restores group identities, names, additions, removals, and group ordering");
        Check(config.CycleGroups[0].ForwardHotkeys.SequenceEqual(new[] { "Shift+F17", "Shift+F18" })
                && config.CycleGroups[0].BackwardHotkeys.SequenceEqual(new[] { "Shift+F19" })
                && config.CycleGroups[0].ClientsOrder.OrderBy(item => item.Value).Select(item => item.Key)
                    .SequenceEqual(new[] { "EVE - B", "EVE - A" }),
            "snapshot deeply restores each group's key lists and member order");
        Check(config.AutoAddUngroupedClients && config.CycleGroupAutoAddExclusions.SequenceEqual(new[] { "EVE - Excluded" }),
            "snapshot restores automatic collection and exclusion list");
        Check(config.ThumbnailSize == new Size(211, 113)
                && config.GetThumbnailLocation("EVE - Unrelated", string.Empty, Point.Empty) == new Point(51, 77),
            "group rollback does not roll back unrelated sizes or preview positions");
        config.CycleGroups[0].ForwardHotkeys.Add("Shift+F23");
        config.CycleGroupAutoAddExclusions.Clear();
        Call(snapshot, "Restore", config);
        Check(config.CycleGroups[0].ForwardHotkeys.SequenceEqual(new[] { "Shift+F17", "Shift+F18" })
                && config.CycleGroupAutoAddExclusions.SequenceEqual(new[] { "EVE - Excluded" }),
            "restored collections do not share mutable references with their saved snapshot");
    }

    private static void DynamicGroupTransactionTests()
    {
        var available = new List<Keys>();
        foreach (Keys code in new[] { Keys.F17, Keys.F18, Keys.F19, Keys.F20, Keys.F21, Keys.F22, Keys.F23, Keys.F24 })
        {
            Keys candidate = Keys.Control | Keys.Alt | Keys.Shift | code;
            if (CanOwn(candidate)) available.Add(candidate);
        }
        if (available.Count < 2) throw new Exception("Dynamic-group fixture needs two unoccupied Ctrl+Alt+Shift+F17..F24 shortcuts.");
        Keys activeKey = available[0], busyKey = available[1];
        string activeGesture = new KeysConverter().ConvertToInvariantString(activeKey)!;
        string busyGesture = new KeysConverter().ConvertToInvariantString(busyKey)!;
        var config = (IThumbnailConfiguration)MinimalConfig(Keys.None, Keys.None);
        config.CycleGroups = Enumerable.Range(1, 7).Select(number => new CycleGroupDefinition { Id = "native-" + number, Name = "Native " + number }).ToList();
        config.CycleGroups[6].Name = "第七组";
        config.CycleGroups[6].ForwardHotkeys.Add(activeGesture);
        config.CycleGroups[6].ClientsOrder = new() { ["EVE - B"] = 1, ["EVE - A"] = 2 };
        config.AutoAddUngroupedClients = true;
        config.CycleGroupAutoAddExclusions = new() { "EVE - Removed" };
        var windows = new FakeWindowManager();
        object manager = New("EveOPreview.Services.ThumbnailManager", null, config, null, windows, null);
        try
        {
            Check(!CanOwn(activeKey), "seventh group's shortcut is actually registered in Windows");
            var views = (Dictionary<IntPtr, IThumbnailView>)manager.GetType().GetField("_thumbnailViews", Any)!.GetValue(manager)!;
            IThumbnailView a = MakeView(801, "EVE - A"), b = MakeView(802, "EVE - B");
            views.Add(a.Id, a); views.Add(b.Id, b);
            ExecuteBinding(manager, activeKey);
            Check(windows.Activated.Last() == b.Id, "seventh group routes to its configured first fake client");
            ExecuteBinding(manager, activeKey);
            Check(windows.Activated.Last() == a.Id, "seventh group cycles using its persisted member order");
            using (var foreignOwner = new NativeOwner(busyKey))
            {
                Check(foreignOwner.Registered, "independent thread creates genuine dynamic-group Windows conflict");
                config.CycleGroups[6].Id = "changed-id";
                config.CycleGroups[6].Name = "冲突新名称";
                config.CycleGroups[6].ForwardHotkeys[0] = busyGesture;
                config.CycleGroups[6].ClientsOrder = new() { ["EVE - A"] = 1 };
                config.CycleGroups.RemoveAt(0);
                config.AutoAddUngroupedClients = false;
                config.CycleGroupAutoAddExclusions.Add("EVE - Newly removed");
                Check(!Apply(manager, out string error) && error.Contains("冲突新名称"),
                    "Windows conflict beyond fifth group reports the requested group name");
                Check(config.CycleGroups.Count == 7 && config.CycleGroups[0].Id == "native-1"
                        && config.CycleGroups[6].Id == "native-7" && config.CycleGroups[6].Name == "第七组"
                        && config.CycleGroups[6].ForwardHotkeys.SequenceEqual(new[] { activeGesture })
                        && config.CycleGroups[6].ClientsOrder.Count == 2,
                    "real OS conflict rolls back complete dynamic collection, group identity, name, keys, and members");
                Check(config.AutoAddUngroupedClients && config.CycleGroupAutoAddExclusions.SequenceEqual(new[] { "EVE - Removed" })
                        && !CanOwn(activeKey),
                    "OS conflict restores automatic collection policy and previous Windows binding");
            }
            config.CycleGroups[1].BackwardHotkeys.Add(activeGesture);
            Check(!Apply(manager, out string duplicateError) && duplicateError.Contains("第七组")
                    && config.CycleGroups[1].BackwardHotkeys.Count == 0 && config.CycleGroups.Count == 7,
                "duplicate between early and seventh groups rejects the full edit and restores all groups");
            Check(!CanOwn(activeKey), "internal duplicate rollback retains original dynamic-group registration");
        }
        finally { Call(manager, "SetHotkeyCaptureActive", true); Call(manager, "Stop"); }
        Check(CanOwn(activeKey) && CanOwn(busyKey), "dynamic-group fixture releases its own native registrations");
    }
}
