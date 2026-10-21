using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathfinding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Path = System.IO.Path;

namespace CrossLink.EditorTools
{
    public static class PathFinderPreviewCutValidation
    {
        private const string ReportPath = "Library/PathFinderPreviewCutValidation.txt";
        private const string RequestName = "PathFinderPreviewCuts.request";

        [InitializeOnLoadMethod]
        private static void ScheduleRequestedCheck()
        {
            if (!AssetDatabase.IsAssetImportWorkerProcess() &&
                File.Exists(Path.Combine(Path.GetDirectoryName(Application.dataPath), RequestName)))
                EditorApplication.delayCall += RunRequestedCheck;
        }

        private static void RunRequestedCheck()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode || !PathFinderPreviewIntegration.PluginsReady)
            {
                EditorApplication.delayCall += RunRequestedCheck;
                return;
            }
            string path = Path.Combine(Path.GetDirectoryName(Application.dataPath), RequestName);
            if (!File.Exists(path))
                return;
            string mode = File.ReadAllText(path).Trim();
            File.Delete(path);
            if (mode == "all")
            {
                PathFinderPreviewIntegration.RunSmokeCheck();
                PathFinderPreviewValidation.Run();
            }
            Run();
            if (mode == "all")
                EditorApplication.delayCall += PathFinderPreviewValidation.ValidateDomainReload;
        }

        [MenuItem("Tools/Battle Talent/Validate PathFinder Preview Cuts %#F6")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || !PathFinderPreviewIntegration.PluginsReady)
            {
                Debug.LogWarning("Wait for editor import to finish before validating preview cuts.");
                return;
            }
            if (AstarPath.active != null)
            {
                Debug.LogWarning("Clear the current PathFinder preview before validating cuts.");
                return;
            }

            StringBuilder report = new StringBuilder();
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = default(Scene);
            PathFinderPreviewSession session = new PathFinderPreviewSession();
            Mesh customMesh = null;
            int nativeCutsBefore = Resources.FindObjectsOfTypeAll<NavmeshCut>().Length;
            int registeredBefore = NavmeshClipper.allEnabled.Count;
            try
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                Vector3 origin = new Vector3(20000, 0, 20000);
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.layer = LayerMask.NameToLayer("CharacterObstacle");
                floor.transform.position = origin - Vector3.up * 0.25f;
                floor.transform.localScale = new Vector3(12, 0.5f, 12);
                BoxCollider range = new GameObject("Cut Test Range").AddComponent<BoxCollider>();
                range.isTrigger = true;
                range.transform.position = origin + Vector3.up;
                range.size = new Vector3(16, 6, 16);
                PathFinderPreviewParameters parameters = new PathFinderPreviewParameters();
                session.Generate(range, parameters);
                Require(Covers(session.Graph, origin), "Uncut floor covers center");
                float originalArea = Area(session.Graph);
                int originalNodes = session.NodeCount;

                NavmeshCutSetup setup = new GameObject("Rectangle Cut").AddComponent<NavmeshCutSetup>();
                setup.transform.position = origin;
                setup.rectangleSize = new Vector2(2, 3);
                setup.height = 2;
                Require(session.IsStale(range, parameters), "Adding a cut marks the preview stale");
                session.Generate(range, parameters);
                Require(session.CutCount >= 1 && !Covers(session.Graph, origin) &&
                    Covers(session.Graph, origin + Vector3.right * 3), "Rectangle removes only the interior");
                Require(Mathf.Abs(Area(session.Graph) - (originalArea - 6)) < 0.1f, "Rectangle removes six square meters");
                report.AppendLine("PASS rectangle cuts actual graph triangles, leaves the surrounding floor, and detects added cuts.");
                NavmeshCutSetup overlapping = new GameObject("Overlapping Cut").AddComponent<NavmeshCutSetup>();
                overlapping.transform.position = origin + Vector3.right;
                overlapping.rectangleSize = new Vector2(2, 3);
                overlapping.height = 2;
                session.Generate(range, parameters);
                Require(Mathf.Abs(Area(session.Graph) - (originalArea - 9)) < 0.1f, "Overlapping cuts remove their union");
                Object.DestroyImmediate(overlapping.gameObject);
                Require(session.IsStale(range, parameters), "Deleting a cut invalidates preview");
                setup.rectangleSize = new Vector2(1, 20);
                session.Generate(range, parameters);
                HashSet<uint> areas = new HashSet<uint>();
                session.Graph.GetNodes(node => { if (node.Walkable) areas.Add(node.Area); });
                Require(areas.Count >= 2 && Covers(session.Graph, origin - Vector3.right * 3) &&
                    Covers(session.Graph, origin + Vector3.right * 3), "A spanning cut separates connected navigation areas");
                setup.rectangleSize = new Vector2(2, 3);
                session.Generate(range, parameters);
                report.AppendLine("PASS overlapping cut union, deletion detection and connectivity separation.");

                foreach (int mask in new[] { 0, 2, 1, -1 })
                {
                    setup.graphMask = mask;
                    Require(session.IsStale(range, parameters), "Mask changes invalidate preview");
                    session.Generate(range, parameters);
                    Require(Covers(session.Graph, origin) == ((mask & 1) == 0), "Graph mask " + mask);
                }
                setup.enabled = false;
                Require(session.IsStale(range, parameters), "Disabled component invalidates preview");
                session.Generate(range, parameters);
                Require(Covers(session.Graph, origin), "Disabled component is ignored");
                setup.enabled = true;
                setup.gameObject.SetActive(false);
                session.Generate(range, parameters);
                Require(Covers(session.Graph, origin), "Inactive object is ignored");
                setup.gameObject.SetActive(true);
                Require(session.IsStale(range, parameters), "Reactivation invalidates preview");
                session.Generate(range, parameters);
                Require(!Covers(session.Graph, origin), "Reactivated cut is included");
                report.AppendLine("PASS enabled/active filtering, graph 0 masks, and mask/activation stale detection.");

                setup.center = Vector3.up * 8;
                session.Generate(range, parameters);
                Require(Covers(session.Graph, origin), "Height filter leaves distant floors untouched");
                setup.center = Vector3.right * 2;
                session.Generate(range, parameters);
                Require(Covers(session.Graph, origin) && !Covers(session.Graph, origin + Vector3.right * 2),
                    "Center offset moves the hole");
                setup.center = Vector3.zero;
                setup.isDual = true;
                session.Generate(range, parameters);
                Require(Covers(session.Graph, origin) && Mathf.Abs(Area(session.Graph) - originalArea) < 0.1f &&
                    session.NodeCount > originalNodes, "Dual splits triangles without removing floor area");
                setup.isDual = false;
                setup.cutsAddedGeom = false;
                session.Generate(range, parameters);
                Require(!Covers(session.Graph, origin), "Cuts Added Geom does not disable normal floor cuts");
                report.AppendLine("PASS center and height, true dual splitting, and cutsAddedGeom semantics.");

                setup.type = NavmeshCutSetup.MeshType.Circle;
                setup.circleRadius = 1.5f;
                setup.circleResolution = 6;
                session.Generate(range, parameters);
                float hexagonArea = 0.5f * 6 * 1.5f * 1.5f * Mathf.Sin(2 * Mathf.PI / 6);
                Require(!Covers(session.Graph, origin) &&
                    Covers(session.Graph, origin + new Vector3(1.4f, 0, 1.4f)) &&
                    Mathf.Abs(Area(session.Graph) - (originalArea - hexagonArea)) < 0.1f,
                    "Circle uses its actual polygon, not a rectangle approximation");
                setup.circleResolution = 24;
                session.Generate(range, parameters);
                Require(Area(session.Graph) < originalArea - hexagonArea, "Circle resolution changes the removed area");
                report.AppendLine("PASS circle shape and resolution-dependent polygon cutting.");

                setup.type = NavmeshCutSetup.MeshType.Rectangle;
                setup.rectangleSize = new Vector2(1, 2);
                setup.center = new Vector3(0.5f, 0, 0);
                setup.useRotationAndScale = true;
                GameObject parent = new GameObject("Nonuniformly Scaled Cut Parent");
                parent.transform.position = origin;
                parent.transform.rotation = Quaternion.Euler(0, 25, 0);
                parent.transform.localScale = new Vector3(2, 1, 1);
                setup.transform.SetParent(parent.transform, false);
                setup.transform.localPosition = Vector3.right;
                setup.transform.localRotation = Quaternion.Euler(0, 35, 0);
                setup.transform.localScale = new Vector3(-1, 3, 1.5f);
                session.Generate(range, parameters);
                Require(!Covers(session.Graph, setup.transform.TransformPoint(setup.center)) &&
                    Covers(session.Graph, setup.transform.TransformPoint(setup.center + Vector3.right)),
                    "Mirrored hierarchy preserves rotation, negative scale and shear");
                setup.updateDistance = 100;
                setup.transform.localPosition += Vector3.forward;
                Require(session.IsStale(range, parameters), "Transform edits invalidate preview independent of runtime thresholds");
                session.Generate(range, parameters);
                Require(!Covers(session.Graph, setup.transform.TransformPoint(setup.center)), "Regeneration uses new transforms");
                setup.transform.SetParent(null);
                setup.transform.SetPositionAndRotation(origin + Vector3.up * 3, Quaternion.identity);
                setup.transform.localScale = new Vector3(1, 10, 1);
                setup.center = Vector3.zero;
                setup.height = 1;
                session.Generate(range, parameters);
                Require(Covers(session.Graph, origin), "Cut height does not scale with Transform");
                setup.transform.position = origin;
                setup.transform.localScale = Vector3.one;
                setup.height = 2;
                range.transform.rotation = Quaternion.Euler(0, 37, 0);
                session.Generate(range, parameters);
                Require(!Covers(session.Graph, origin), "Cutting in rotated graph space");
                range.transform.rotation = Quaternion.identity;
                report.AppendLine("PASS full transform hierarchy, negative scale/shear, unscaled height and rotated graph space.");

                customMesh = new Mesh { name = "Concave L Cut" };
                customMesh.vertices = new[]
                {
                    new Vector3(-2, 0, -2), new Vector3(0, 0, -2), new Vector3(2, 0, -2),
                    new Vector3(-2, 0, 0), Vector3.zero, new Vector3(2, 0, 0),
                    new Vector3(-2, 0, 2), new Vector3(0, 0, 2)
                };
                customMesh.triangles = new[] { 0, 3, 1, 1, 3, 4, 1, 4, 2, 2, 4, 5, 3, 6, 4, 4, 6, 7 };
                setup.type = NavmeshCutSetup.MeshType.CustomMesh;
                setup.mesh = customMesh;
                session.Generate(range, parameters);
                Require(!Covers(session.Graph, origin + new Vector3(-1, 0, 1)) &&
                    !Covers(session.Graph, origin + new Vector3(1, 0, -1)) &&
                    Covers(session.Graph, origin + new Vector3(1, 0, 1)), "Concave custom contour preserves the notch");
                setup.meshScale = 0.5f;
                Require(session.IsStale(range, parameters), "Custom scale changes invalidate preview");
                session.Generate(range, parameters);
                Require(Mathf.Abs(Area(session.Graph) - (originalArea - 3)) < 0.1f, "Custom mesh scale affects area");
                report.AppendLine("PASS concave custom mesh cutting and meshScale.");

                customMesh.vertices = new[]
                {
                    new Vector3(-20, 0, -1), new Vector3(-20, 0, 1), new Vector3(-18, 0, 1), new Vector3(-18, 0, -1),
                    new Vector3(18, 0, -1), new Vector3(18, 0, 1), new Vector3(20, 0, 1), new Vector3(20, 0, -1)
                };
                customMesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7 };
                EditorUtility.SetDirty(customMesh);
                Require(session.IsStale(range, parameters), "In-place custom mesh edits invalidate preview");
                setup.meshScale = 1;
                floor.transform.localScale = new Vector3(70, 0.5f, 12);
                range.size = new Vector3(80, 6, 16);
                session.Generate(range, parameters);
                Require(session.Graph.tileXCount > 1 &&
                    !Covers(session.Graph, origin + Vector3.left * 19) &&
                    !Covers(session.Graph, origin + Vector3.right * 19) &&
                    Covers(session.Graph, origin), "Disconnected custom contours cut every touched tile");
                report.AppendLine("PASS dirty custom mesh stale detection and disconnected contours spanning multiple tiles.");

                List<GraphNode> nodes = new List<GraphNode>();
                session.Graph.GetNodes((Action<GraphNode>)nodes.Add);
                session.Clear();
                Require(nodes.All(node => node.Destroyed) && AstarPath.active == null &&
                    Resources.FindObjectsOfTypeAll<NavmeshCut>().Length == nativeCutsBefore &&
                    NavmeshClipper.allEnabled.Count == registeredBefore, "Explicit clear destroys all nodes and proxies without registry leaks");
                setup.mesh = null;
                bool failed = false;
                try { session.Generate(range, parameters); }
                catch (InvalidOperationException) { failed = true; }
                Require(failed && !session.HasPreview && AstarPath.active == null, "Invalid mesh fails with complete preview cleanup");
                setup.mesh = customMesh;
                for (int i = 0; i < 3; i++)
                {
                    session.Generate(range, parameters);
                    session.Clear();
                    Require(Resources.FindObjectsOfTypeAll<NavmeshCut>().Length == nativeCutsBefore &&
                        NavmeshClipper.allEnabled.Count == registeredBefore, "Repeated scan/clear has no proxy leaks");
                }
                report.AppendLine("PASS invalid-mesh failure cleanup, repeated regeneration, node destruction and native clipper registry isolation.");
                session.Generate(range, parameters);
                SceneManager.SetActiveScene(previous);
                Require(!session.HasPreview && AstarPath.active == null &&
                    Resources.FindObjectsOfTypeAll<NavmeshCut>().Length == nativeCutsBefore &&
                    NavmeshClipper.allEnabled.Count == registeredBefore, "Scene switch clears native cut proxies");
                report.AppendLine("PASS scene-switch cleanup with active cuts.");
                report.AppendLine("RESULT: PASS");
                Debug.Log("PathFinder Preview cut checks passed. See " + ReportPath);
            }
            catch (Exception exception)
            {
                report.AppendLine("RESULT: FAIL\n" + exception);
                Debug.LogException(exception);
            }
            finally
            {
                session.Dispose();
                if (scene.IsValid() && scene.isLoaded)
                    EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded)
                    SceneManager.SetActiveScene(previous);
                if (customMesh != null)
                    Object.DestroyImmediate(customMesh);
                File.WriteAllText(ReportPath, report.ToString());
            }
        }

        private static bool Covers(RecastGraph graph, Vector3 point)
        {
            bool found = false;
            graph.GetNodes(node =>
            {
                TriangleMeshNode triangle = node as TriangleMeshNode;
                if (triangle != null && triangle.ContainsPoint(point))
                    found = true;
            });
            return found;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException("PathFinder Preview cuts: " + message);
        }

        private static float Area(RecastGraph graph)
        {
            float area = 0;
            graph.GetNodes(node =>
            {
                TriangleMeshNode triangle = node as TriangleMeshNode;
                if (triangle == null)
                    return;
                Vector3 a = (Vector3)triangle.GetVertex(0);
                Vector3 b = (Vector3)triangle.GetVertex(1);
                Vector3 c = (Vector3)triangle.GetVertex(2);
                area += Vector3.Cross(b - a, c - a).magnitude * 0.5f;
            });
            return area;
        }
    }
}
