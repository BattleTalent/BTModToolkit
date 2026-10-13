using System;
using Pathfinding;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrossLink.EditorTools
{
    public sealed class PathFinderPreviewSession : IDisposable
    {
        public const float WalkableHeight = 2;
        public const float CharacterRadius = 0.5f;
        public const bool UseTiles = true;
        public const int TileSize = 128;
        public const float MaxEdgeLength = 20;
        public const float ContourMaxError = 2;

        private static PathFinderPreviewSession owner;
        private GameObject host;
        private AstarPath astar;
        private BoxCollider range;
        private PathFinderPreviewParameters scannedParameters;
        private Matrix4x4 scannedTransform;
        private Vector3 scannedCenter;
        private Vector3 scannedSize;
        private PathFinderPreviewRenderer renderer;
        private bool subscribed;

        public bool IsScanning { get; private set; }
        public bool HasPreview => host != null && range != null && renderer != null;
        public int NodeCount { get; private set; }
        public float ScanSeconds { get; private set; }
        public int DrawCount => renderer != null ? renderer.DrawCount : 0;
        public string RenderError => renderer != null ? renderer.LastError : null;
        public RecastGraph Graph { get; private set; }

        public bool Visible
        {
            get { return renderer != null && renderer.Visible; }
            set
            {
                if (renderer != null)
                    renderer.Visible = value;
                SceneView.RepaintAll();
            }
        }

        public static string Validate(BoxCollider candidate, PathFinderPreviewParameters parameters)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return "Preview is only available outside Play Mode.";
            if (!PathFinderPreviewIntegration.PluginsReady)
                return "Wait for the editor-only A* dependencies to finish importing.";
            if (candidate == null)
                return "Select a scene BoxCollider as the scan range.";
            if (EditorUtility.IsPersistent(candidate) || !candidate.gameObject.scene.IsValid() ||
                !candidate.gameObject.scene.isLoaded)
                return "The range must be a BoxCollider in a loaded scene, not a prefab asset.";
            if (PrefabStageUtility.GetCurrentPrefabStage() != null ||
                EditorSceneManager.IsPreviewScene(candidate.gameObject.scene))
                return "Prefab Stage and preview scenes are not supported. Open a regular scene.";
            if (parameters == null)
                return "Preview parameters are missing.";
            string parameterError = parameters.ValidationError();
            if (parameterError != null)
                return parameterError;
            if (LayerMask.NameToLayer("CharacterObstacle") < 0)
                return "The CharacterObstacle layer is missing.";

            Vector3 center, size, rotation;
            CalculateBounds(candidate, out center, out size, out rotation);
            if (!IsFinite(center) || !IsFinite(size) || !IsFinite(rotation) ||
                size.x <= 0 || size.y <= 0 || size.z <= 0 ||
                candidate.size.x <= 0 || candidate.size.y <= 0 || candidate.size.z <= 0)
                return "The range must have finite coordinates and a positive size on every axis.";
            return null;
        }

        public static void CalculateBounds(BoxCollider candidate, out Vector3 center, out Vector3 size,
            out Vector3 rotation)
        {
            Transform transform = candidate.transform;
            Vector3 scale = transform.lossyScale;
            center = transform.TransformPoint(candidate.center);
            size = new Vector3(Mathf.Abs(candidate.size.x * scale.x),
                Mathf.Abs(candidate.size.y * scale.y), Mathf.Abs(candidate.size.z * scale.z));
            rotation = transform.rotation.eulerAngles;
        }

        private static bool IsFinite(Vector3 value)
        {
            return PathFinderPreviewParameters.IsFinite(value.x) &&
                PathFinderPreviewParameters.IsFinite(value.y) &&
                PathFinderPreviewParameters.IsFinite(value.z);
        }

        public bool IsStale(BoxCollider candidate, PathFinderPreviewParameters parameters)
        {
            return HasPreview && (candidate != range || !scannedParameters.Matches(parameters) ||
                range.transform.localToWorldMatrix != scannedTransform ||
                range.center != scannedCenter || range.size != scannedSize);
        }

        public void Generate(BoxCollider candidate, PathFinderPreviewParameters parameters)
        {
            Clear();
            if (owner != null && owner != this)
                owner.Clear();

            string validation = Validate(candidate, parameters);
            if (validation != null)
                throw new InvalidOperationException(validation);
            foreach (AstarPath existing in Resources.FindObjectsOfTypeAll<AstarPath>())
            {
                if (!EditorUtility.IsPersistent(existing) && existing.gameObject.scene.IsValid())
                    throw new InvalidOperationException("Another AstarPath instance exists. Remove or close it before previewing.");
            }
            if (AstarPath.active != null)
                throw new InvalidOperationException("AstarPath.active is owned by another instance.");

            IsScanning = true;
            owner = this;
            try
            {
                range = candidate;
                host = new GameObject("PathFinder Preview (temporary)");
                host.hideFlags = HideFlags.HideAndDontSave;
                SceneManager.MoveGameObjectToScene(host, candidate.gameObject.scene);
                astar = host.AddComponent<AstarPath>();
                if (astar == null)
                    throw new InvalidOperationException("Unity could not create the temporary AstarPath.");
                astar.hideFlags = HideFlags.HideAndDontSave;
                astar.scanOnStartup = false;
                astar.showNavGraphs = false;
                astar.debugMode = GraphDebugMode.SolidColor;
                astar.ConfigureReferencesInternal();
                astar.data.graphs = new NavGraph[0];
                astar.data.FindGraphTypes();
                Graph = astar.data.AddGraph(typeof(RecastGraph)) as RecastGraph;
                if (Graph == null)
                    throw new InvalidOperationException("RecastGraph is not available in the imported A* package.");
                ConfigureGraph(Graph, candidate, parameters);
                Physics.SyncTransforms();

                float lastProgress = float.NegativeInfinity;
                foreach (Pathfinding.Progress progress in astar.ScanAsync(Graph))
                {
                    if (Time.realtimeSinceStartup - lastProgress > 0.2f)
                    {
                        EditorUtility.DisplayProgressBar("PathFinder Preview", progress.description, progress.progress);
                        lastProgress = Time.realtimeSinceStartup;
                    }
                }

                NodeCount = Graph.CountNodes();
                ScanSeconds = astar.lastScanTime;
                scannedParameters = parameters.Copy();
                scannedTransform = range.transform.localToWorldMatrix;
                scannedCenter = range.center;
                scannedSize = range.size;
                renderer = new PathFinderPreviewRenderer(astar, Graph);
                Subscribe();
                SceneView.RepaintAll();
            }
            catch
            {
                Clear();
                throw;
            }
            finally
            {
                IsScanning = false;
                EditorUtility.ClearProgressBar();
            }
        }

        private static void ConfigureGraph(RecastGraph graph, BoxCollider candidate,
            PathFinderPreviewParameters parameters)
        {
            graph.cellSize = parameters.cellSize;
            graph.useTiles = UseTiles;
            graph.editorTileSize = TileSize;
            graph.minRegionSize = parameters.minRegionSize;
            graph.walkableHeight = WalkableHeight;
            graph.walkableClimb = parameters.walkableClimb;
            graph.characterRadius = CharacterRadius;
            graph.maxSlope = parameters.maxSlope;
            graph.maxEdgeLength = MaxEdgeLength;
            graph.contourMaxError = ContourMaxError;
            graph.rasterizeTerrain = parameters.rasterizeTerrain;
            graph.rasterizeMeshes = parameters.rasterizeMeshes;
            graph.rasterizeColliders = parameters.rasterizeColliders;
            Vector3 center, size, rotation;
            CalculateBounds(candidate, out center, out size, out rotation);
            graph.forcedBoundsCenter = center;
            graph.forcedBoundsSize = size;
            graph.rotation = rotation;
            graph.mask = 1 << LayerMask.NameToLayer("CharacterObstacle");
            graph.showMeshSurface = true;
            graph.showMeshOutline = true;
            graph.showNodeConnections = false;
        }

        private void Subscribe()
        {
            if (subscribed)
                return;
            subscribed = true;
            EditorApplication.update += CheckLifetime;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Clear;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
        }

        private void CheckLifetime()
        {
            if (host == null || range == null || AstarPath.active != astar)
                Clear();
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                Clear();
        }

        private void OnSceneClosing(Scene scene, bool removingScene) { Clear(); }
        private void OnSceneOpened(Scene scene, OpenSceneMode mode) { Clear(); }
        private void OnActiveSceneChanged(Scene previous, Scene next) { Clear(); }

        public void Clear()
        {
            if (subscribed)
            {
                subscribed = false;
                EditorApplication.update -= CheckLifetime;
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                AssemblyReloadEvents.beforeAssemblyReload -= Clear;
                EditorSceneManager.sceneClosing -= OnSceneClosing;
                EditorSceneManager.sceneOpened -= OnSceneOpened;
                EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
            }

            try
            {
                try
                {
                    if (renderer != null)
                        renderer.Dispose();
                }
                finally
                {
                    if (astar != null && astar.data != null)
                    {
                        // A* 4.2 destroys nodes through the static active owner. Preserve any replacement.
                        AstarPath previous = AstarPath.active;
                        try
                        {
                            AstarPath.active = astar;
                            astar.data.OnDestroy();
                        }
                        finally
                        {
                            AstarPath.active = previous == astar ? null : previous;
                        }
                    }
                }
            }
            finally
            {
                if (host != null)
                    UnityEngine.Object.DestroyImmediate(host);
                host = null;
                astar = null;
                range = null;
                Graph = null;
                renderer = null;
                scannedParameters = null;
                NodeCount = 0;
                ScanSeconds = 0;
                if (owner == this)
                    owner = null;
                SceneView.RepaintAll();
            }
        }

        public void Dispose() { Clear(); }
    }
}
