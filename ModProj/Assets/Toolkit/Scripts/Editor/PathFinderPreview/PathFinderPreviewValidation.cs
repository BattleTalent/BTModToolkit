using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Pathfinding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrossLink.EditorTools
{
    [InitializeOnLoad]
    public static class PathFinderPreviewValidation
    {
        private const string ReportPath = "Library/PathFinderPreviewValidation.txt";
        private const string DemoSceneKey = "CrossLink.PathFinderPreview.DemoScene";
        private const string PreviousSceneKey = "CrossLink.PathFinderPreview.PreviousScene";
        private const string ReloadCheckKey = "CrossLink.PathFinderPreview.ReloadCheck";
        private const string ReloadErrorsKey = "CrossLink.PathFinderPreview.ReloadErrors";
        private static Scene demoScene;
        private static Scene previousDemoScene;
        private static BoxCollider demoRange;
        private static SceneView demoView;
        private static Vector3 previousPivot;
        private static Quaternion previousRotation;
        private static float previousSize;
        private static bool previousOrthographic;

        static PathFinderPreviewValidation()
        {
            if (SessionState.GetBool(ReloadCheckKey, false))
                Application.logMessageReceived += TrackReloadErrors;
            EditorApplication.delayCall += RecoverDemoAfterReload;
        }

        private static void RecoverDemoAfterReload()
        {
            int sceneHandle = SessionState.GetInt(DemoSceneKey, 0);
            if (sceneHandle == 0)
                return;
            demoScene = FindLoadedScene(sceneHandle);
            previousDemoScene = FindLoadedScene(SessionState.GetInt(PreviousSceneKey, 0));
            Scene recoveringScene = demoScene;
            CloseDemo();
            if (SessionState.GetBool(ReloadCheckKey, false))
            {
                Application.logMessageReceived -= TrackReloadErrors;
                SessionState.EraseBool(ReloadCheckKey);
                int errors = SessionState.GetInt(ReloadErrorsKey, 0);
                bool passed = (!recoveringScene.IsValid() || !recoveringScene.isLoaded) &&
                    AstarPath.active == null && errors == 0;
                File.WriteAllText("Library/PathFinderPreviewReloadCheck.txt",
                    (passed ? "PASS" : "FAIL") + ": domain reload cleanup; errors=" + errors +
                    "; active Astar=" + (AstarPath.active != null));
            }
        }

        private static void TrackReloadErrors(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                SessionState.SetInt(ReloadErrorsKey, SessionState.GetInt(ReloadErrorsKey, 0) + 1);
        }

        public static void ValidateDomainReload()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            ShowDemo();
            SessionState.SetBool(ReloadCheckKey, true);
            SessionState.SetInt(ReloadErrorsKey, 0);
            Application.logMessageReceived -= TrackReloadErrors;
            Application.logMessageReceived += TrackReloadErrors;
            EditorApplication.delayCall += EditorUtility.RequestScriptReload;
        }

        private static Scene FindLoadedScene(int handle)
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene.handle == handle)
                    return scene;
            }
            return default(Scene);
        }

        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || !PathFinderPreviewIntegration.PluginsReady)
            {
                Debug.LogWarning("Wait for editor import to finish before validation.");
                return;
            }
            if (UnityEngine.Object.FindObjectOfType<AstarPath>() != null || AstarPath.active != null)
            {
                Debug.LogWarning("Clear the current preview before running validation.");
                return;
            }

            StringBuilder report = new StringBuilder();
            Scene previous = SceneManager.GetActiveScene();
            Scene testScene = default(Scene);
            TerrainData terrainData = null;
            Mesh rampMesh = null;
            PathFinderPreviewSession session = new PathFinderPreviewSession();
            try
            {
                testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                BoxCollider range = CreateRange(new Vector3(10000, 1, 10000), new Vector3(14, 6, 14));
                GameObject floor = CreateBox("Floor", new Vector3(10000, -0.25f, 10000), new Vector3(10, 0.5f, 10));
                PathFinderPreviewParameters parameters = new PathFinderPreviewParameters();
                session.Generate(range, parameters);
                Require(session.NodeCount > 0, "Default floor scan");
                Require(AstarPath.active.data.graphTypes.Length == 1 &&
                    AstarPath.active.data.graphTypes[0] == typeof(RecastGraph), "Recast-only graph registry");
                Type[] graphTypes = typeof(AstarPath).Assembly.GetTypes()
                    .Where(type => !type.IsAbstract && typeof(NavGraph).IsAssignableFrom(type)).ToArray();
                Require(graphTypes.Length == 1 && graphTypes[0] == typeof(RecastGraph), "Only Recast graph remains compiled");
                string[] cuttingDependencies = typeof(AstarPath).Assembly.GetReferencedAssemblies()
                    .Where(reference => reference.Name.StartsWith("Pathfinding.", StringComparison.Ordinal))
                    .Select(reference => reference.Name).OrderBy(name => name).ToArray();
                Require(cuttingDependencies.SequenceEqual(new[] { "Pathfinding.ClipperLib", "Pathfinding.Poly2Tri" }),
                    "Only the two editor-only cutting DLL dependencies");
                bool unsupported = false;
                try { AstarPath.active.data.AddGraph(typeof(NavGraph)); }
                catch (NotSupportedException) { unsupported = true; }
                Require(unsupported, "Other graph types rejected");
                unsupported = false;
                try { AstarPath.StartPath(null); }
                catch (NotSupportedException) { unsupported = true; }
                Require(unsupported, "Runtime path requests rejected");
                report.AppendLine("PASS Recast-only registry, only editor cutting DLL references and unsupported runtime APIs.");
                PathFinderPreviewParameters fine = parameters.Copy();
                fine.cellSize = 0.125f;
                Require(session.IsStale(range, fine), "cellSize stale marker");
                session.Generate(range, fine);
                Require(session.Graph.cellSize == fine.cellSize && session.Graph.TileWorldSizeX == 16 &&
                    session.NodeCount > 0, "cellSize forwarded to sampling and tile dimensions");
                List<GraphNode> previousNodes = new List<GraphNode>();
                session.Graph.GetNodes((Action<GraphNode>)previousNodes.Add);
                session.Clear();
                Require(previousNodes.All(node => node.Destroyed) && AstarPath.active == null, "Node and singleton cleanup");
                report.AppendLine("PASS default scan, cellSize sampling/tile width, stale detection and explicit node cleanup.");

                parameters.rasterizeTerrain = false;
                parameters.rasterizeMeshes = false;
                session.Generate(range, parameters);
                Require(session.NodeCount > 0, "Collider-only scan");
                parameters.rasterizeMeshes = true;
                parameters.rasterizeColliders = false;
                session.Generate(range, parameters);
                Require(session.NodeCount > 0, "Mesh-only scan");
                parameters.rasterizeMeshes = false;
                session.Generate(range, parameters);
                Require(session.NodeCount == 0, "All collectors disabled");
                report.AppendLine("PASS mesh-only, collider-only and empty scans.");

                session.Clear();
                UnityEngine.Object.DestroyImmediate(floor);
                terrainData = new TerrainData { heightmapResolution = 33, size = new Vector3(10, 1, 10) };
                GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);
                terrain.layer = LayerMask.NameToLayer("CharacterObstacle");
                terrain.transform.position = new Vector3(9995, 0, 9995);
                parameters.rasterizeTerrain = true;
                session.Generate(range, parameters);
                Require(session.NodeCount > 0, "Terrain-only scan");
                parameters.rasterizeTerrain = false;
                session.Generate(range, parameters);
                Require(session.NodeCount == 0, "Terrain disabled");
                session.Clear();
                UnityEngine.Object.DestroyImmediate(terrain);
                report.AppendLine("PASS terrain collection toggle.");

                GameObject lower = CreateBox("Lower", new Vector3(9997, -0.25f, 10000), new Vector3(6, 0.5f, 10));
                GameObject upper = CreateBox("Upper", new Vector3(10003, 0.5f, 10000), new Vector3(6, 0.5f, 10));
                parameters.rasterizeColliders = true;
                parameters.walkableClimb = 0.5f;
                session.Generate(range, parameters);
                int lowAreas = CountAreas(session.Graph);
                parameters.walkableClimb = 1;
                Require(session.IsStale(range, parameters), "Parameter stale marker");
                session.Generate(range, parameters);
                int highAreas = CountAreas(session.Graph);
                Require(lowAreas > highAreas && highAreas > 0, "walkableClimb changes step connectivity");
                report.AppendLine("PASS walkableClimb connectivity: areas " + lowAreas + " -> " + highAreas + ".");
                session.Clear();
                UnityEngine.Object.DestroyImmediate(lower);
                UnityEngine.Object.DestroyImmediate(upper);

                GameObject smallPlatform = CreateBox("Small Platform", new Vector3(10000, -0.25f, 10000),
                    new Vector3(2, 0.5f, 2));
                parameters.minRegionSize = 2;
                session.Generate(range, parameters);
                Require(session.NodeCount == 0, "Small isolated region removed");
                parameters.minRegionSize = 0.1f;
                Require(session.IsStale(range, parameters), "minRegionSize stale marker");
                session.Generate(range, parameters);
                Require(session.Graph.minRegionSize == 0.1f && session.NodeCount > 0, "Small isolated region retained");
                session.Clear();
                UnityEngine.Object.DestroyImmediate(smallPlatform);
                parameters.minRegionSize = PathFinderPreviewParameters.DefaultMinRegionSize;
                report.AppendLine("PASS minRegionSize removal/retention of small isolated platforms.");

                rampMesh = new Mesh { name = "Temporary Preview Ramp" };
                rampMesh.vertices = new[] { new Vector3(-5, 0, -5), new Vector3(-5, 0, 5),
                    new Vector3(5, 2.5f, -5), new Vector3(5, 2.5f, 5) };
                rampMesh.triangles = new[] { 0, 1, 2, 2, 1, 3 };
                rampMesh.RecalculateNormals();
                rampMesh.RecalculateBounds();
                GameObject ramp = new GameObject("Ramp");
                ramp.layer = LayerMask.NameToLayer("CharacterObstacle");
                ramp.transform.position = new Vector3(10000, 0, 10000);
                ramp.AddComponent<MeshFilter>().sharedMesh = rampMesh;
                ramp.AddComponent<MeshRenderer>();
                parameters.rasterizeMeshes = true;
                parameters.rasterizeColliders = false;
                session.Generate(range, parameters);
                Require(session.NodeCount > 0, "Walkable ramp");
                parameters.maxSlope = 10;
                Require(session.IsStale(range, parameters), "maxSlope stale marker");
                session.Generate(range, parameters);
                Require(session.Graph.maxSlope == 10 && session.NodeCount == 0, "Ramp rejected by lower maxSlope");
                session.Clear();
                UnityEngine.Object.DestroyImmediate(ramp);
                parameters.maxSlope = PathFinderPreviewParameters.DefaultMaxSlope;
                report.AppendLine("PASS maxSlope acceptance/rejection of ramp geometry.");

                GameObject narrow = CreateBox("Narrow", new Vector3(10000, -0.25f, 10000), new Vector3(0.75f, 0.5f, 10));
                parameters.rasterizeColliders = true;
                parameters.rasterizeMeshes = false;
                session.Generate(range, parameters);
                Require(session.NodeCount == 0, "Narrow platform radius erosion");
                session.Clear();
                UnityEngine.Object.DestroyImmediate(narrow);
                floor = CreateBox("Wide", new Vector3(10000, -0.25f, 10000), new Vector3(10, 0.5f, 10));

                range.center = new Vector3(0.5f, 0, -0.5f);
                range.transform.rotation = Quaternion.Euler(0, 35, 0);
                range.transform.localScale = new Vector3(-1.2f, 1.5f, 0.8f);
                session.Generate(range, parameters);
                Vector3 center, size, rotation;
                PathFinderPreviewSession.CalculateBounds(range, out center, out size, out rotation);
                Require(session.Graph.forcedBoundsCenter == center && session.Graph.forcedBoundsSize == size &&
                    session.Graph.rotation == rotation && session.NodeCount > 0, "Transformed bounds");
                range.transform.position += Vector3.right;
                Require(session.IsStale(range, parameters), "Transform stale marker");
                session.Visible = false;
                Require(!session.Visible, "Hide preview");
                session.Visible = true;
                Require(session.Visible, "Show preview");
                session.Clear();
                report.AppendLine("PASS radius erosion, rotated/scaled bounds, stale marker and visibility.");

                parameters.walkableClimb = -1;
                Require(PathFinderPreviewSession.Validate(range, parameters) != null, "Negative climb validation");
                parameters.walkableClimb = float.NaN;
                Require(PathFinderPreviewSession.Validate(range, parameters) != null, "NaN validation");
                parameters.walkableClimb = float.PositiveInfinity;
                Require(PathFinderPreviewSession.Validate(range, parameters) != null, "Infinite climb validation");
                parameters.walkableClimb = 0.5f;
                PathFinderPreviewParameters invalid = parameters.Copy();
                invalid.cellSize = 0;
                Require(PathFinderPreviewSession.Validate(range, invalid) != null, "Zero cell size");
                invalid.cellSize = float.NaN;
                Require(PathFinderPreviewSession.Validate(range, invalid) != null, "NaN cell size");
                invalid = parameters.Copy();
                invalid.minRegionSize = -1;
                Require(PathFinderPreviewSession.Validate(range, invalid) != null, "Negative region area");
                invalid.minRegionSize = float.PositiveInfinity;
                Require(PathFinderPreviewSession.Validate(range, invalid) != null, "Infinite region area");
                invalid = parameters.Copy();
                invalid.maxSlope = -1;
                Require(PathFinderPreviewSession.Validate(range, invalid) != null, "Negative slope");
                invalid.maxSlope = 91;
                Require(PathFinderPreviewSession.Validate(range, invalid) != null, "Slope exceeds 90 degrees");
                invalid.maxSlope = float.NaN;
                Require(PathFinderPreviewSession.Validate(range, invalid) != null, "NaN slope");
                string key;
                Require(!PathFinderPreviewSettings.TryGetKey(range, out key), "Unsaved scene uses session-only settings");
                PathFinderPreviewParameters roundTrip = JsonUtility.FromJson<PathFinderPreviewParameters>(JsonUtility.ToJson(parameters));
                Require(parameters.Matches(roundTrip), "Parameter serialization");
                report.AppendLine("PASS invalid inputs, unsaved-scene identity and parameter serialization.");
                CheckSettingsPersistence();
                report.AppendLine("PASS per-range settings disk save/load and copy isolation.");
                CheckLegacyMigration();
                report.AppendLine("PASS legacy four-parameter settings migration without losing original values.");

                GameObject foreignHost = new GameObject("Temporary Foreign Astar");
                AstarPath foreign = foreignHost.AddComponent<AstarPath>();
                foreign.ConfigureReferencesInternal();
                foreign.showNavGraphs = false;
                try
                {
                    bool refused = false;
                    try { session.Generate(range, parameters); }
                    catch (InvalidOperationException) { refused = true; }
                    Require(refused && AstarPath.active == foreign, "Foreign singleton conflict");
                }
                finally
                {
                    if (AstarPath.active == foreign)
                        AstarPath.active = null;
                    UnityEngine.Object.DestroyImmediate(foreignHost);
                }
                report.AppendLine("PASS foreign Astar refusal without overwriting its singleton.");

                for (int repeat = 0; repeat < 3; repeat++)
                {
                    session.Generate(range, parameters);
                    previousNodes.Clear();
                    session.Graph.GetNodes((Action<GraphNode>)previousNodes.Add);
                    session.Clear();
                    Require(previousNodes.All(node => node.Destroyed), "Repeated node cleanup");
                }
                session.Generate(range, parameters);
                SceneManager.SetActiveScene(previous);
                Require(!session.HasPreview && AstarPath.active == null, "Scene switching cleanup");
                report.AppendLine("PASS repeated scan/clear and active-scene switching cleanup.");

                foreach (string path in Directory.GetFiles(PathFinderPreviewIntegration.PackageRoot, "*.cs", SearchOption.AllDirectories))
                {
                    if (path.Replace('\\', '/').Contains("/Editor/"))
                        continue;
                    string source = File.ReadAllText(path);
                    int guard = source.IndexOf("#if UNITY_EDITOR", StringComparison.Ordinal);
                    Require(guard >= 0 && guard < 256 && source.TrimEnd().EndsWith("#endif"),
                        "Player source guard: " + path);
                }
                foreach (PluginImporter importer in PluginImporter.GetAllImporters())
                {
                    if (!importer.assetPath.StartsWith(PathFinderPreviewIntegration.PackageRoot + "/Plugins/"))
                        continue;
                    Require(!importer.GetCompatibleWithAnyPlatform() && importer.GetCompatibleWithEditor(),
                        "Editor-only plugin: " + importer.assetPath);
                    Require(!importer.GetCompatibleWithPlatform(BuildTarget.Android) &&
                        !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows) &&
                        !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64),
                        "Cutting DLL excluded from Android and Windows players: " + importer.assetPath);
                }
                Require(!AssetDatabase.IsValidFolder(PathFinderPreviewIntegration.PackageRoot + "/Resources"), "No player Resources folder");
                report.AppendLine("PASS vendor player guards, DLL isolation and Resources isolation.");
                report.AppendLine("RESULT: PASS (visual comparison, persisted saved-scene IDs and headset runtime are separate checks).");
                Debug.Log("PathFinder Preview integration checks passed. See " + ReportPath);
            }
            catch (Exception exception)
            {
                report.AppendLine("RESULT: FAIL\n" + exception);
                Debug.LogException(exception);
            }
            finally
            {
                session.Dispose();
                if (testScene.IsValid() && testScene.isLoaded)
                    EditorSceneManager.CloseScene(testScene, true);
                if (previous.IsValid() && previous.isLoaded)
                    SceneManager.SetActiveScene(previous);
                if (terrainData != null)
                    UnityEngine.Object.DestroyImmediate(terrainData);
                if (rampMesh != null)
                    UnityEngine.Object.DestroyImmediate(rampMesh);
                File.WriteAllText(ReportPath, report.ToString());
            }
        }

        private static int CountAreas(RecastGraph graph)
        {
            HashSet<uint> areas = new HashSet<uint>();
            graph.GetNodes(node => { if (node.Walkable) areas.Add(node.Area); });
            return areas.Count;
        }

        private static void CheckSettingsPersistence()
        {
            PathFinderPreviewSettings settings = PathFinderPreviewSettings.instance;
            string original = EditorJsonUtility.ToJson(settings);
            try
            {
                PathFinderPreviewParameters first = new PathFinderPreviewParameters
                    { walkableClimb = 1.25f, rasterizeMeshes = false, minRegionSize = 0.5f, cellSize = 0.125f, maxSlope = 40 };
                PathFinderPreviewParameters second = new PathFinderPreviewParameters
                    { walkableClimb = 0.75f, rasterizeTerrain = false, minRegionSize = 3, cellSize = 0.5f, maxSlope = 20 };
                settings.Store("PathFinderPreviewValidation_A", first);
                settings.Store("PathFinderPreviewValidation_B", second);
                UnityEngine.Object.DestroyImmediate(settings);
                settings = PathFinderPreviewSettings.instance;
                Require(settings.Load("PathFinderPreviewValidation_A").Matches(first), "Settings disk reload A");
                Require(settings.Load("PathFinderPreviewValidation_B").Matches(second), "Settings disk reload B");
                PathFinderPreviewParameters copy = settings.Load("PathFinderPreviewValidation_A");
                copy.walkableClimb = 99;
                Require(settings.Load("PathFinderPreviewValidation_A").Matches(first), "Settings copy isolation");
            }
            finally
            {
                settings = PathFinderPreviewSettings.instance;
                EditorJsonUtility.FromJsonOverwrite(original, settings);
                // Save is protected; the test restores the exact user's settings after its temporary entries.
                typeof(ScriptableSingleton<PathFinderPreviewSettings>).GetMethod("Save",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(settings, new object[] { true });
            }
        }

        private static void CheckLegacyMigration()
        {
            PathFinderPreviewSettings settings = PathFinderPreviewSettings.instance;
            string original = EditorJsonUtility.ToJson(settings);
            try
            {
                SerializedObject legacy = new SerializedObject(settings);
                legacy.FindProperty("settingsVersion").intValue = 0;
                SerializedProperty entries = legacy.FindProperty("entries");
                entries.arraySize = 1;
                SerializedProperty entry = entries.GetArrayElementAtIndex(0);
                entry.FindPropertyRelative("globalId").stringValue = "PathFinderPreviewValidation_Legacy";
                SerializedProperty values = entry.FindPropertyRelative("parameters");
                values.FindPropertyRelative("walkableClimb").floatValue = 1.75f;
                values.FindPropertyRelative("rasterizeTerrain").boolValue = false;
                values.FindPropertyRelative("rasterizeMeshes").boolValue = false;
                values.FindPropertyRelative("rasterizeColliders").boolValue = true;
                values.FindPropertyRelative("minRegionSize").floatValue = 0;
                values.FindPropertyRelative("cellSize").floatValue = 0;
                values.FindPropertyRelative("maxSlope").floatValue = 0;
                legacy.ApplyModifiedPropertiesWithoutUndo();
                PathFinderPreviewParameters migrated = settings.Load("PathFinderPreviewValidation_Legacy");
                Require(migrated.walkableClimb == 1.75f && !migrated.rasterizeTerrain &&
                    !migrated.rasterizeMeshes && migrated.rasterizeColliders, "Legacy values preserved");
                Require(migrated.minRegionSize == 2 && migrated.cellSize == 0.25f && migrated.maxSlope == 30,
                    "Legacy additions initialized with ProjSAO defaults");
                migrated.cellSize = 0.125f;
                settings.Store("PathFinderPreviewValidation_Legacy", migrated);
                Require(settings.Load("PathFinderPreviewValidation_Legacy").cellSize == 0.125f,
                    "Migration does not reset subsequent custom values");
            }
            finally
            {
                EditorJsonUtility.FromJsonOverwrite(original, settings);
                typeof(ScriptableSingleton<PathFinderPreviewSettings>).GetMethod("Save",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(settings, new object[] { true });
            }
        }

        private static void Require(bool condition, string description)
        {
            if (!condition)
                throw new InvalidOperationException("Validation failed: " + description);
        }

        private static BoxCollider CreateRange(Vector3 position, Vector3 size)
        {
            GameObject range = new GameObject("PathFinder Preview Test Range");
            range.transform.position = position;
            BoxCollider collider = range.AddComponent<BoxCollider>();
            collider.size = size;
            collider.isTrigger = true;
            return collider;
        }

        private static GameObject CreateBox(string name, Vector3 position, Vector3 scale)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.layer = LayerMask.NameToLayer("CharacterObstacle");
            box.transform.position = position;
            box.transform.localScale = scale;
            return box;
        }

        public static void ShowDemo()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            CloseDemo();
            previousDemoScene = SceneManager.GetActiveScene();
            demoScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            CreateBox("Demo Floor", new Vector3(10000, -0.25f, 10000), new Vector3(12, 0.5f, 12));
            CreateBox("Demo Step", new Vector3(10003, 0.35f, 10000), new Vector3(4, 0.7f, 4));
            NavmeshCutSetup demoCut = new GameObject("Demo Rectangle Navmesh Cut").AddComponent<NavmeshCutSetup>();
            demoCut.transform.position = new Vector3(9997, 0, 10000);
            demoCut.rectangleSize = new Vector2(2, 3);
            demoCut.height = 2;
            demoRange = CreateRange(new Vector3(10000, 1, 10000), new Vector3(14, 6, 14));
            PathFinderPreviewWindow.Open();
            PathFinderPreviewWindow window = EditorWindow.GetWindow<PathFinderPreviewWindow>();
            window.SetRange(demoRange);
            window.Generate();
            Selection.activeGameObject = demoRange.gameObject;
            SceneView sceneView = SceneView.lastActiveSceneView ?? EditorWindow.GetWindow<SceneView>();
            demoView = sceneView;
            previousPivot = sceneView.pivot;
            previousRotation = sceneView.rotation;
            previousSize = sceneView.size;
            previousOrthographic = sceneView.orthographic;
            sceneView.LookAt(new Vector3(10000, 0, 10000), Quaternion.Euler(45, -35, 0), 18);
            sceneView.Show();
            sceneView.Repaint();
            SessionState.SetInt(DemoSceneKey, demoScene.handle);
            SessionState.SetInt(PreviousSceneKey, previousDemoScene.handle);
            EditorApplication.playModeStateChanged += CloseDemoForPlayMode;
        }

        private static void CloseDemoForPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                CloseDemo();
        }

        public static void CloseDemo()
        {
            EditorApplication.playModeStateChanged -= CloseDemoForPlayMode;
            SessionState.EraseInt(DemoSceneKey);
            SessionState.EraseInt(PreviousSceneKey);
            if (demoScene.IsValid())
            {
                foreach (PathFinderPreviewWindow window in Resources.FindObjectsOfTypeAll<PathFinderPreviewWindow>())
                    window.ClearRangeInScene(demoScene);
            }
            if (demoScene.IsValid() && Selection.objects.Any(IsDemoObject))
            {
                Selection.objects = Selection.objects.Where(selected => !IsDemoObject(selected)).ToArray();
                ActiveEditorTracker.sharedTracker.ForceRebuild();
            }
            if (demoScene.IsValid() && demoScene.isLoaded)
                EditorSceneManager.CloseScene(demoScene, true);
            if (previousDemoScene.IsValid() && previousDemoScene.isLoaded)
                SceneManager.SetActiveScene(previousDemoScene);
            if (demoView != null)
                demoView.LookAt(previousPivot, previousRotation, previousSize, previousOrthographic);
            demoScene = default(Scene);
            previousDemoScene = default(Scene);
            demoRange = null;
            demoView = null;
        }

        private static bool IsDemoObject(UnityEngine.Object selected)
        {
            GameObject gameObject = selected as GameObject;
            Component component = selected as Component;
            if (component != null)
                gameObject = component.gameObject;
            return gameObject != null && gameObject.scene == demoScene;
        }
    }
}
