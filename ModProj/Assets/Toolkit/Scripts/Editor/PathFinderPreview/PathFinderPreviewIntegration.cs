using System;
using System.IO;
using Pathfinding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrossLink.EditorTools
{
    [InitializeOnLoad]
    public static class PathFinderPreviewIntegration
    {
        public const string PackageRoot = "Assets/ThirdParty/AstarPathfindingProject";
        private const string ReportPath = "Library/PathFinderPreviewSmokeCheck.txt";

        static PathFinderPreviewIntegration()
        {
            EditorApplication.delayCall += ConfigurePlugins;
        }

        public static bool PluginsReady { get; private set; }

        private static void ConfigurePlugins()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += ConfigurePlugins;
                return;
            }

            // The Recast-only subset has no external DLL or runtime Resources dependencies.
            PluginsReady = true;
        }

        [MenuItem("Tools/Battle Talent/Validate PathFinder Preview")]
        public static void RunSmokeCheck()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || !PluginsReady)
            {
                Debug.LogWarning("PathFinder Preview: wait for editor import to finish before validation.");
                return;
            }

            Scene previousScene = SceneManager.GetActiveScene();
            Scene testScene = default(Scene);
            AstarPath astar = null;
            try
            {
                if (AstarPath.active != null || UnityEngine.Object.FindObjectOfType<AstarPath>() != null)
                    throw new InvalidOperationException("An AstarPath instance already exists.");

                testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                int layer = LayerMask.NameToLayer("CharacterObstacle");
                if (layer < 0)
                    throw new InvalidOperationException("CharacterObstacle layer is missing.");

                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = "PathFinder Preview Smoke Floor";
                floor.layer = layer;
                floor.transform.position = new Vector3(10000, -0.25f, 10000);
                floor.transform.localScale = new Vector3(10, 0.5f, 10);

                GameObject host = new GameObject("PathFinder Preview Smoke Astar");
                host.hideFlags = HideFlags.HideAndDontSave;
                astar = host.AddComponent<AstarPath>();
                astar.showNavGraphs = false;
                astar.scanOnStartup = false;
                astar.ConfigureReferencesInternal();
                astar.data.graphs = new NavGraph[0];
                astar.data.FindGraphTypes();
                RecastGraph graph = astar.data.AddGraph(typeof(RecastGraph)) as RecastGraph;
                graph.cellSize = 0.25f;
                graph.useTiles = true;
                graph.editorTileSize = 128;
                graph.minRegionSize = 2;
                graph.walkableHeight = 2;
                graph.walkableClimb = 0.5f;
                graph.characterRadius = 0.5f;
                graph.maxSlope = 30;
                graph.maxEdgeLength = 20;
                graph.contourMaxError = 2;
                graph.rasterizeTerrain = false;
                graph.rasterizeMeshes = true;
                graph.rasterizeColliders = true;
                graph.forcedBoundsCenter = new Vector3(10000, 1, 10000);
                graph.forcedBoundsSize = new Vector3(12, 4, 12);
                graph.mask = 1 << layer;
                Physics.SyncTransforms();
                foreach (Pathfinding.Progress progress in astar.ScanAsync())
                    EditorUtility.DisplayProgressBar("PathFinder Preview", progress.description, progress.progress);

                int count = graph.CountNodes();
                if (count == 0)
                    throw new InvalidOperationException("The flat floor scan produced no nodes.");

                string[] materials = { "Navmesh", "NavmeshOutline" };
                foreach (string name in materials)
                {
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(
                        PackageRoot + "/Editor/EditorAssets/Materials/" + name + ".mat");
                    if (material == null || material.shader == null || !material.shader.isSupported ||
                        ShaderUtil.ShaderHasError(material.shader))
                        throw new InvalidOperationException("Invalid preview material: " + name);
                }

                File.WriteAllText(ReportPath, "PASS: A* " + AstarPath.Version + "; floor nodes=" + count +
                    "; original preview shaders supported; managed Recast-only subset configured.");
                Debug.Log("PathFinder Preview smoke check passed. Nodes: " + count);
            }
            catch (Exception exception)
            {
                File.WriteAllText(ReportPath, "FAIL: " + exception);
                Debug.LogException(exception);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (astar != null)
                {
                    astar.data.OnDestroy();
                    if (AstarPath.active == astar)
                        AstarPath.active = null;
                    UnityEngine.Object.DestroyImmediate(astar.gameObject);
                }
                if (testScene.IsValid())
                    EditorSceneManager.CloseScene(testScene, true);
                if (previousScene.IsValid() && previousScene.isLoaded)
                    SceneManager.SetActiveScene(previousScene);
            }
        }
    }
}
