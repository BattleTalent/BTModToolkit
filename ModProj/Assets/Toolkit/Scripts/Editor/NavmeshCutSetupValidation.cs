using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrossLink
{
    public static class NavmeshCutSetupValidation
    {
        [MenuItem("Tools/Battle Talent/Validate Navmesh Cut Setup %#F8")]
        public static void Run()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Wait for script import to finish and leave Play Mode before validation.");
                return;
            }

            GameObject first = null;
            GameObject second = null;
            Editor inspector = null;
            Mesh mesh = null;
            string report;
            try
            {
                first = new GameObject("Navmesh Cut Setup Validation")
                    { hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave };
                second = new GameObject("Navmesh Cut Setup Copy")
                    { hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave };
                NavmeshCutSetup setup = first.AddComponent<NavmeshCutSetup>();
                NavmeshCutSetup copy = second.AddComponent<NavmeshCutSetup>();
                CheckDefaults(setup);
                CheckProperties(setup);

                inspector = Editor.CreateEditor(setup);
                Require(inspector is NavmeshCutSetupEditor, "Custom inspector discovery");
                Require(setup.GetType().Assembly == typeof(SandBoxControlSetup).Assembly,
                    "Setup belongs to the toolkit runtime assembly");

                mesh = new Mesh { name = "Navmesh Cut Flat Test Mesh", hideFlags = HideFlags.HideAndDontSave };
                mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.forward, Vector3.right + Vector3.forward };
                mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
                // Editor JSON cannot round-trip references to a transient DontSave mesh.
                Mesh referenceMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
                Require(referenceMesh != null, "Built-in persistent reference mesh");
                setup.mesh = referenceMesh;
                setup.rectangleSize = new Vector2(2.3f, 2.2f);
                setup.circleRadius = 2;
                setup.circleResolution = 8;
                setup.height = 2.08f;
                setup.meshScale = -2;
                setup.center = new Vector3(-0.05f, 1.01f, 0.23f);
                setup.updateDistance = 0.2f;
                setup.updateRotationDistance = 15;
                setup.isDual = true;
                setup.cutsAddedGeom = false;
                setup.useRotationAndScale = true;
                setup.graphMask = unchecked((int)0x80000001);
                foreach (NavmeshCutSetup.MeshType type in Enum.GetValues(typeof(NavmeshCutSetup.MeshType)))
                {
                    setup.type = type;
                    string json = EditorJsonUtility.ToJson(setup);
                    EditorJsonUtility.FromJsonOverwrite(json, copy);
                    Require(EditorJsonUtility.ToJson(copy) == json, "Serialized round trip for " + type);
                    Require(copy.graphMask == unchecked((int)0x80000001),
                        "Highest graph bit survives serialization: " + copy.graphMask);
                    Require(copy.mesh == referenceMesh, "Persistent mesh reference survives serialization");
                }
                CheckMultiEdit(setup, copy);
                setup.mesh = mesh;
                CheckGeometry(setup);

                report = "PASS: ProjSAO defaults; all 14 serialized fields; custom inspector discovery; " +
                    "three shape round trips; mesh references; 32-bit graph mask; mixed multi-object editing; " +
                    "Undo; offset/rotation/scale projection; custom mesh boundary extraction. " +
                    "No scenes or prefabs were saved. Game-side cutting was not tested.";
                Debug.Log("[NavmeshCutSetupValidation] " + report);
            }
            catch (Exception exception)
            {
                report = "FAIL: " + exception;
                Debug.LogException(exception);
            }
            finally
            {
                if (inspector != null)
                    Object.DestroyImmediate(inspector);
                if (first != null)
                {
                    Undo.ClearUndo(first.GetComponent<NavmeshCutSetup>());
                    Object.DestroyImmediate(first);
                }
                if (second != null)
                {
                    Undo.ClearUndo(second.GetComponent<NavmeshCutSetup>());
                    Object.DestroyImmediate(second);
                }
                if (mesh != null)
                    Object.DestroyImmediate(mesh);
            }

            string logRoot = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Logs");
            Directory.CreateDirectory(logRoot);
            File.WriteAllText(Path.Combine(logRoot, "NavmeshCutSetupValidation.txt"), report);
        }

        private static void CheckDefaults(NavmeshCutSetup setup)
        {
            Require((int)NavmeshCutSetup.MeshType.Rectangle == 0 &&
                (int)NavmeshCutSetup.MeshType.Circle == 1 &&
                (int)NavmeshCutSetup.MeshType.CustomMesh == 2, "Original shape enum values");
            Require(setup.type == NavmeshCutSetup.MeshType.Rectangle && setup.mesh == null &&
                setup.rectangleSize == Vector2.one && setup.circleRadius == 1 &&
                setup.circleResolution == 6 && setup.height == 1 && setup.meshScale == 1 &&
                setup.center == Vector3.zero && setup.updateDistance == 0.4f &&
                !setup.isDual && setup.cutsAddedGeom && setup.updateRotationDistance == 10 &&
                !setup.useRotationAndScale && setup.graphMask == -1, "Original ProjSAO defaults");
        }

        private static void CheckProperties(NavmeshCutSetup setup)
        {
            string[] names =
            {
                "type", "mesh", "rectangleSize", "circleRadius", "circleResolution", "height", "meshScale",
                "center", "updateDistance", "isDual", "cutsAddedGeom", "updateRotationDistance",
                "useRotationAndScale", "graphMask"
            };
            SerializedObject serialized = new SerializedObject(setup);
            foreach (string name in names)
                Require(serialized.FindProperty(name) != null, "Serialized field " + name);
            Require(serialized.FindProperty("graphMask").propertyType == SerializedPropertyType.Integer,
                "Graph mask is a dependency-free integer");
        }

        private static void CheckMultiEdit(NavmeshCutSetup first, NavmeshCutSetup second)
        {
            first.type = NavmeshCutSetup.MeshType.Rectangle;
            second.type = NavmeshCutSetup.MeshType.Circle;
            first.useRotationAndScale = false;
            second.useRotationAndScale = true;
            SerializedObject multi = new SerializedObject(new Object[] { first, second });
            multi.Update();
            Require(multi.FindProperty("type").hasMultipleDifferentValues &&
                multi.FindProperty("useRotationAndScale").hasMultipleDifferentValues, "Mixed shape and rotation values");
            Undo.IncrementCurrentGroup();
            multi.FindProperty("graphMask").intValue = -1;
            Require(multi.ApplyModifiedProperties(), "Multi-object edit applies a change");
            Require(first.graphMask == -1 && second.graphMask == -1,
                "Multi-object graph mask edit: " + first.graphMask + ", " + second.graphMask);
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            Require(first.graphMask == unchecked((int)0x80000001) &&
                second.graphMask == unchecked((int)0x80000001), "Undo restores both graph masks");
        }

        private static void CheckGeometry(NavmeshCutSetup setup)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            MethodInfo point = typeof(NavmeshCutSetupGizmos).GetMethod("OutlinePoint", flags);
            MethodInfo countEdge = typeof(NavmeshCutSetupGizmos).GetMethod("CountEdge", flags);
            Require(point != null && countEdge != null, "Gizmo geometry helpers");
            setup.transform.SetPositionAndRotation(new Vector3(4, 5, 6), Quaternion.Euler(20, 90, 15));
            setup.transform.localScale = new Vector3(-2, 3, 4);
            Vector3 local = new Vector3(1, 0, 2);
            setup.useRotationAndScale = false;
            Vector3 actual = (Vector3)point.Invoke(null, new object[] { setup, local });
            Require(Vector3.Distance(actual, setup.transform.position + setup.center + local) < 0.0001f,
                "Position-only outline ignores rotation and scale");
            setup.useRotationAndScale = true;
            actual = (Vector3)point.Invoke(null, new object[] { setup, local });
            Require(Vector3.Distance(actual, setup.transform.TransformPoint(setup.center + local)) < 0.0001f,
                "Rotated and negatively scaled outline");

            Dictionary<ulong, int> edges = new Dictionary<ulong, int>();
            int[] triangles = setup.mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
                for (int j = 0; j < 3; j++)
                    countEdge.Invoke(null, new object[] { edges, triangles[i + j], triangles[i + (j + 1) % 3] });
            int boundaries = 0;
            foreach (int count in edges.Values)
                if (count == 1)
                    boundaries++;
            Require(boundaries == 4 && edges.Count == 5, "Custom mesh hides shared triangle edges");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException("Navmesh Cut Setup validation: " + message);
        }
    }
}
