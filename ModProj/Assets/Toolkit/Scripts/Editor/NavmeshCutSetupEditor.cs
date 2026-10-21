using UnityEditor;
using UnityEngine;

namespace CrossLink
{
    [CustomEditor(typeof(NavmeshCutSetup))]
    [CanEditMultipleObjects]
    public class NavmeshCutSetupEditor : Editor
    {
        private static readonly string[] GraphLabels = CreateGraphLabels();
        private bool showAdvanced;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

            EditorGUILayout.HelpBox(
                "Authoring settings only. A game-side bridge must create and configure a NavmeshCut. " +
                "Gizmos show the cutting volume, not the resulting navigation mesh.",
                MessageType.Info);

            SerializedProperty type = Property(nameof(NavmeshCutSetup.type));
            EditorGUILayout.PropertyField(type, new GUIContent("Shape", type.tooltip));
            EditorGUI.indentLevel++;
            if (!type.hasMultipleDifferentValues)
            {
                switch ((NavmeshCutSetup.MeshType)type.intValue)
                {
                    case NavmeshCutSetup.MeshType.Rectangle:
                        Draw(nameof(NavmeshCutSetup.rectangleSize));
                        break;
                    case NavmeshCutSetup.MeshType.Circle:
                        Draw(nameof(NavmeshCutSetup.circleRadius));
                        Draw(nameof(NavmeshCutSetup.circleResolution));
                        SerializedProperty resolution = Property(nameof(NavmeshCutSetup.circleResolution));
                        if (!resolution.hasMultipleDifferentValues && resolution.intValue >= 20)
                            EditorGUILayout.HelpBox(
                                "Prefer a low circle resolution. High resolutions create more navigation triangles.",
                                MessageType.Warning);
                        break;
                    case NavmeshCutSetup.MeshType.CustomMesh:
                        Draw(nameof(NavmeshCutSetup.mesh));
                        Draw(nameof(NavmeshCutSetup.meshScale));
                        EditorGUILayout.HelpBox(
                            "Use a low-poly flat XZ surface, not a 3D volume. Vertices along shared edges " +
                            "must be shared (avoid split normals/UV seams). Enable Read/Write for game builds.",
                            MessageType.Info);
                        SerializedProperty mesh = Property(nameof(NavmeshCutSetup.mesh));
                        if (!mesh.hasMultipleDifferentValues)
                        {
                            Mesh value = mesh.objectReferenceValue as Mesh;
                            if (value == null)
                                EditorGUILayout.HelpBox("Assign a custom cutting mesh.", MessageType.Warning);
                            else if (!value.isReadable)
                                EditorGUILayout.HelpBox(
                                    "The custom mesh is not readable. Enable Read/Write in its import settings.",
                                    MessageType.Warning);
                        }
                        break;
                }
            }
            Draw(nameof(NavmeshCutSetup.height));
            Draw(nameof(NavmeshCutSetup.center));
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            Draw(nameof(NavmeshCutSetup.updateDistance));
            Draw(nameof(NavmeshCutSetup.useRotationAndScale));
            SerializedProperty rotation = Property(nameof(NavmeshCutSetup.useRotationAndScale));
            if (rotation.hasMultipleDifferentValues || rotation.boolValue)
            {
                EditorGUI.indentLevel++;
                Draw(nameof(NavmeshCutSetup.updateRotationDistance));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.HelpBox(
                "Height is not scaled by Transform. Runtime size/center/scale changes require " +
                "NavmeshCut.ForceUpdate(); automatic updates only detect movement and rotation.",
                MessageType.Info);

            showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced", true);
            if (showAdvanced)
            {
                EditorGUI.indentLevel++;
                Draw(nameof(NavmeshCutSetup.isDual));
                Draw(nameof(NavmeshCutSetup.cutsAddedGeom));
                DrawGraphMask();
                EditorGUI.indentLevel--;
            }
            SerializedProperty dual = Property(nameof(NavmeshCutSetup.isDual));
            if (!dual.hasMultipleDifferentValues && dual.boolValue)
                EditorGUILayout.HelpBox(
                    "Dual mode only splits triangles; it does not block navigation inside the shape.",
                    MessageType.Warning);

            if (serializedObject.ApplyModifiedProperties())
                SceneView.RepaintAll();
        }

        private SerializedProperty Property(string name)
        {
            return serializedObject.FindProperty(name);
        }

        private void Draw(string name)
        {
            EditorGUILayout.PropertyField(Property(name));
        }

        private void DrawGraphMask()
        {
            SerializedProperty mask = Property(nameof(NavmeshCutSetup.graphMask));
            GUIContent label = new GUIContent("Affected Graphs", mask.tooltip);
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(rect, label, mask);
            bool previousMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mask.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int value = EditorGUI.MaskField(rect, label, mask.intValue, GraphLabels);
            if (EditorGUI.EndChangeCheck())
                mask.intValue = value;
            EditorGUI.showMixedValue = previousMixed;
            EditorGUI.EndProperty();
        }

        private static string[] CreateGraphLabels()
        {
            string[] labels = new string[32];
            for (int i = 0; i < labels.Length; i++)
                labels[i] = "Graph " + i;
            return labels;
        }
    }
}
