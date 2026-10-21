using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrossLink.EditorTools
{
    public sealed class PathFinderPreviewWindow : EditorWindow
    {
        [SerializeField] private BoxCollider range;
        [SerializeField] private PathFinderPreviewParameters parameters = new PathFinderPreviewParameters();
        [SerializeField] private int parametersVersion;
        [SerializeField] private bool showAdvanced;
        [SerializeField] private bool showFixedSettings;
        private static readonly GUIContent RangeLabel = new GUIContent("Scan Range",
            "Scene BoxCollider defining the scan volume. Its center, size, world rotation and scale are used. Only geometry on the CharacterObstacle layer is collected.");
        private static readonly GUIContent ClimbLabel = new GUIContent("walkableClimb (m)",
            "Maximum height difference the graph allows between adjacent surfaces. Default: 0.5 m. This changes navigation connectivity, not the character's physical climbing ability. A* caps it at the fixed walkableHeight (2 m).");
        private static readonly GUIContent RegionLabel = new GUIContent("minRegionSize (m^2)",
            "Minimum navigation region area. Small isolated regions may be removed; regions touching tile borders can be kept. Lower it to retain small platforms. Default: 2 square meters.");
        private static readonly GUIContent TerrainLabel = new GUIContent("rasterizeTerrain",
            "Collect Unity Terrain geometry in the scan volume. Disable this for scenes without Terrain. Default: enabled.");
        private static readonly GUIContent MeshesLabel = new GUIContent("rasterizeMeshes",
            "Collect active scene meshes with an enabled Renderer. Meshes must have Read/Write enabled. Visual geometry may differ from collision geometry. Default: enabled.");
        private static readonly GUIContent CollidersLabel = new GUIContent("rasterizeColliders",
            "Collect supported, enabled, non-trigger colliders as scan geometry. MeshCollider meshes must be readable. This can be used without rasterizeMeshes. Default: enabled.");
        private static readonly GUIContent CellLabel = new GUIContent("cellSize (m)",
            "Horizontal voxel sample size. Smaller values capture finer edges and narrow passages but increase scan time and memory use. This also changes tile width and contour tolerance in world units. Default: 0.25 m.");
        private static readonly GUIContent SlopeLabel = new GUIContent("maxSlope (degrees)",
            "Maximum surface inclination marked walkable by the graph. Default: 30 degrees. Increasing it does not make the physical character capable of walking up steeper slopes.");
        private static readonly GUIContent HeightLabel = new GUIContent("walkableHeight (m)",
            "Required vertical clearance above a walkable surface. Fixed at 2 m to match the standard game configuration; do not lower it to force navigation beneath low ceilings.");
        private static readonly GUIContent RadiusLabel = new GUIContent("characterRadius (m)",
            "Clearance from walls and obstacle edges. The walkable surface is eroded by this radius. Fixed at 0.5 m; reducing it would make narrow routes appear usable without changing the character's body.");
        private static readonly GUIContent TilesLabel = new GUIContent("useTiles",
            "Split the graph into tiles instead of scanning one large tile. Fixed on to match ProjSAO.");
        private static readonly GUIContent TileSizeLabel = new GUIContent("editorTileSize (voxels)",
            "Width and depth of each tile in voxels. Fixed at 128. World width is editorTileSize multiplied by cellSize (32 m at the default cellSize). Affects scan performance and tile boundaries.");
        private static readonly GUIContent EdgeLabel = new GUIContent("maxEdgeLength (m)",
            "Long polygon edges are subdivided. Smaller values can improve triangle distribution but add nodes and pathfinding cost. Fixed at 20 m.");
        private static readonly GUIContent ContourLabel = new GUIContent("contourMaxError (voxels)",
            "Allowed contour simplification error, measured in voxels. Fixed at 2, approximately 0.5 m at the default cellSize. Changing cellSize also changes this world-space tolerance.");
        private static readonly GUIContent CenterLabel = new GUIContent("forcedBoundsCenter",
            "World-space scan center computed by transforming the BoxCollider's local center. Edit the range BoxCollider rather than this read-only value.");
        private static readonly GUIContent SizeLabel = new GUIContent("forcedBoundsSize",
            "Scan dimensions computed from the BoxCollider size and absolute world scale. Edit the range BoxCollider rather than this read-only value.");
        private static readonly GUIContent RotationLabel = new GUIContent("rotation (degrees)",
            "World rotation of the range BoxCollider, used to orient the Recast graph. Edit the range transform rather than this read-only value.");
        private readonly Dictionary<int, PathFinderPreviewParameters> sessionSettings =
            new Dictionary<int, PathFinderPreviewParameters>();
        private PathFinderPreviewSession session;
        private string message;
        private MessageType messageType;
        private string persistedKey;
        private Vector2 scroll;

        [MenuItem("Tools/Battle Talent/PathFinder Preview")]
        public static void Open()
        {
            PathFinderPreviewWindow window = GetWindow<PathFinderPreviewWindow>("PathFinder Preview");
            window.minSize = new Vector2(390, 390);
            window.Show();
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            session = new PathFinderPreviewSession();
            message = null;
            persistedKey = null;
            if (range == null)
                range = null;
            if (parameters == null)
                parameters = new PathFinderPreviewParameters();
            if (parametersVersion < 1)
            {
                parameters.ResetAdditionalDefaults();
                parametersVersion = 1;
            }
            string key;
            if (PathFinderPreviewSettings.TryGetKey(range, out key))
            {
                parameters = PathFinderPreviewSettings.instance.Load(key);
                persistedKey = key;
            }
            EditorApplication.update += CheckState;
        }

        private void OnDisable()
        {
            EditorApplication.update -= CheckState;
            StoreParameters();
            if (session != null)
                session.Dispose();
            session = null;
        }

        private void CheckState()
        {
            string key;
            if (range != null && PathFinderPreviewSettings.TryGetKey(range, out key) && key != persistedKey)
                StoreParameters();
            if (session != null && session.HasPreview)
                Repaint();
        }

        public void SetRange(BoxCollider candidate)
        {
            StoreParameters();
            if (session != null)
                session.Clear();
            range = candidate;
            persistedKey = null;
            message = null;
            string key;
            PathFinderPreviewParameters cached;
            if (PathFinderPreviewSettings.TryGetKey(range, out key))
            {
                parameters = PathFinderPreviewSettings.instance.Load(key);
                persistedKey = key;
            }
            else if (range != null && sessionSettings.TryGetValue(range.GetInstanceID(), out cached))
                parameters = cached.Copy();
            else
                parameters = new PathFinderPreviewParameters();
            Repaint();
        }

        public void ClearRangeInScene(UnityEngine.SceneManagement.Scene scene)
        {
            if (range != null && range.gameObject.scene == scene)
                SetRange(null);
        }

        private void StoreParameters()
        {
            if (range == null || parameters == null)
                return;
            sessionSettings[range.GetInstanceID()] = parameters.Copy();
            string key;
            if (PathFinderPreviewSettings.TryGetKey(range, out key))
            {
                PathFinderPreviewSettings.instance.Store(key, parameters);
                persistedKey = key;
            }
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("PathFinder Preview", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("These settings affect the preview only and are not automatically applied in game.", MessageType.Info);

            using (new EditorGUI.DisabledScope(session != null && session.IsScanning))
            {
                BoxCollider candidate = (BoxCollider)EditorGUILayout.ObjectField(
                    RangeLabel, range, typeof(BoxCollider), true);
                if (candidate != range)
                    SetRange(candidate);
                if (GUILayout.Button("Use Selected Object"))
                {
                    GameObject selected = Selection.activeGameObject;
                    SetRange(selected != null ? selected.GetComponent<BoxCollider>() : null);
                }

                string key;
                if (range != null && !PathFinderPreviewSettings.TryGetKey(range, out key))
                    EditorGUILayout.HelpBox("Settings are session-only until the scene and range object have been saved.", MessageType.Info);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Preview Parameters", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                parameters.walkableClimb = EditorGUILayout.FloatField(ClimbLabel, parameters.walkableClimb);
                parameters.minRegionSize = EditorGUILayout.FloatField(RegionLabel, parameters.minRegionSize);
                parameters.rasterizeTerrain = EditorGUILayout.Toggle(TerrainLabel, parameters.rasterizeTerrain);
                parameters.rasterizeMeshes = EditorGUILayout.Toggle(MeshesLabel, parameters.rasterizeMeshes);
                parameters.rasterizeColliders = EditorGUILayout.Toggle(CollidersLabel, parameters.rasterizeColliders);
                if (EditorGUI.EndChangeCheck())
                    StoreParameters();
                showAdvanced = EditorGUILayout.Foldout(showAdvanced,
                    new GUIContent("Advanced", "Generation precision and slope classification. Defaults match ProjSAO. These values only affect this editor preview."), true);
                if (showAdvanced)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    parameters.cellSize = EditorGUILayout.FloatField(CellLabel, parameters.cellSize);
                    parameters.maxSlope = EditorGUILayout.FloatField(SlopeLabel, parameters.maxSlope);
                    if (EditorGUI.EndChangeCheck())
                        StoreParameters();
                    EditorGUI.indentLevel--;
                }
                if (GUILayout.Button("Restore ProjSAO Defaults"))
                {
                    parameters = new PathFinderPreviewParameters();
                    StoreParameters();
                }
                DrawFixedSettings();

                if (!parameters.rasterizeTerrain && !parameters.rasterizeMeshes && !parameters.rasterizeColliders)
                    EditorGUILayout.HelpBox("All geometry collection is disabled. An empty result is expected.", MessageType.Warning);
                if (parameters.walkableClimb > PathFinderPreviewSession.WalkableHeight)
                    EditorGUILayout.HelpBox("A* will cap walkableClimb at the fixed walkableHeight of 2 m.", MessageType.Warning);
                if (parameters.cellSize > 0 && parameters.cellSize < 0.05f)
                    EditorGUILayout.HelpBox("A very small cellSize can make scans slow and memory-intensive. Start with 0.25 m and decrease only when needed.", MessageType.Warning);
                if (session != null && session.IsStale(range, parameters))
                    EditorGUILayout.HelpBox("Preview is out of date. Generate again to apply range, parameter or NavmeshCutSetup changes.", MessageType.Warning);

                string validation = PathFinderPreviewSession.Validate(range, parameters);
                if (validation != null)
                    EditorGUILayout.HelpBox(validation, MessageType.Warning);
                using (new EditorGUI.DisabledScope(validation != null || EditorApplication.isCompiling || EditorApplication.isUpdating))
                {
                    if (GUILayout.Button(session != null && session.HasPreview ? "Regenerate Preview" : "Generate Preview", GUILayout.Height(28)))
                        Generate();
                }
                using (new EditorGUI.DisabledScope(session == null || !session.HasPreview))
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(session != null && session.Visible ? "Hide Preview" : "Show Preview"))
                        session.Visible = !session.Visible;
                    if (GUILayout.Button("Clear Preview"))
                    {
                        session.Clear();
                        message = null;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (session != null && session.HasPreview)
                EditorGUILayout.LabelField("Triangles: " + session.NodeCount + "   Cuts: " + session.CutCount +
                    "   Scan: " + session.ScanSeconds.ToString("0.00") + " s");
            if (!string.IsNullOrEmpty(message) &&
                (messageType == MessageType.Error || (session != null && session.HasPreview)))
                EditorGUILayout.HelpBox(message, messageType);
            if (session != null && !string.IsNullOrEmpty(session.RenderError))
                EditorGUILayout.HelpBox("Preview rendering stopped: " + session.RenderError, MessageType.Error);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Enabled NavmeshCutSetup components in loaded scenes cut the actual preview mesh. " +
                "The preview uses graph 0 for Affected Graphs. Enable Scene view Gizmos to see the surface, outlines and bounds. " +
                "Geometry and cut edits require Regenerate Preview.", MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        private void DrawFixedSettings()
        {
            showFixedSettings = EditorGUILayout.Foldout(showFixedSettings,
                new GUIContent("Fixed Settings", "Read-only game defaults and the scan bounds derived from the BoxCollider. Hover each label for its meaning."), true);
            if (!showFixedSettings)
                return;
            EditorGUI.indentLevel++;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.FloatField(HeightLabel, PathFinderPreviewSession.WalkableHeight);
                EditorGUILayout.FloatField(RadiusLabel, PathFinderPreviewSession.CharacterRadius);
                EditorGUILayout.Toggle(TilesLabel, PathFinderPreviewSession.UseTiles);
                EditorGUILayout.IntField(TileSizeLabel, PathFinderPreviewSession.TileSize);
                EditorGUILayout.FloatField(EdgeLabel, PathFinderPreviewSession.MaxEdgeLength);
                EditorGUILayout.FloatField(ContourLabel, PathFinderPreviewSession.ContourMaxError);
                if (range != null && !EditorUtility.IsPersistent(range) && range.gameObject.scene.IsValid())
                {
                    Vector3 center, size, rotation;
                    PathFinderPreviewSession.CalculateBounds(range, out center, out size, out rotation);
                    EditorGUILayout.Vector3Field(CenterLabel, center);
                    EditorGUILayout.Vector3Field(SizeLabel, size);
                    EditorGUILayout.Vector3Field(RotationLabel, rotation);
                }
            }
            EditorGUI.indentLevel--;
        }

        public void Generate()
        {
            StoreParameters();
            try
            {
                session.Generate(range, parameters);
                message = session.NodeCount == 0
                    ? "No walkable triangles were generated. Check collection switches, CharacterObstacle layers, range, clearance and mesh Read/Write settings."
                    : "Preview generated with NavmeshCutSetup cuts. Geometry changes are not tracked; regenerate after editing. Cut changes mark the preview out of date.";
                messageType = session.NodeCount == 0 ? MessageType.Warning : MessageType.Info;
            }
            catch (Exception exception)
            {
                message = exception.Message;
                messageType = MessageType.Error;
                Debug.LogException(exception);
            }
        }
    }
}
