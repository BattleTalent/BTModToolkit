using UnityEditor;
using UnityEngine;

namespace CrossLink
{
    [CustomEditor(typeof(SandBoxControlSetup))]
    [CanEditMultipleObjects]
    public class SandBoxControlSetupEditor : Editor
    {
        private static readonly string[] SceneBlockLabels = { "des", "dun", "tomb" };
        private static readonly string[] SceneBlockNames = { "Sandbox_Des", "Sandbox_Dun", "Sandbox_Tomb" };

        private SerializedProperty sceneBlockDataNameProperty;

        private void OnEnable()
        {
            sceneBlockDataNameProperty = serializedObject.FindProperty(nameof(SandBoxControlSetup.sceneBlockDataName));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, nameof(SandBoxControlSetup.sceneBlockDataName));

            var rect = EditorGUILayout.GetControlRect();
            var label = new GUIContent(sceneBlockDataNameProperty.displayName);
            EditorGUI.BeginProperty(rect, label, sceneBlockDataNameProperty);
            EditorGUI.showMixedValue = sceneBlockDataNameProperty.hasMultipleDifferentValues;
            int selectedIndex = System.Array.IndexOf(SceneBlockNames, sceneBlockDataNameProperty.stringValue);
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(rect, label.text, selectedIndex, SceneBlockLabels);
            if (EditorGUI.EndChangeCheck() && newIndex >= 0)
            {
                sceneBlockDataNameProperty.stringValue = SceneBlockNames[newIndex];
            }
            EditorGUI.showMixedValue = false;
            EditorGUI.EndProperty();

            if (!sceneBlockDataNameProperty.hasMultipleDifferentValues &&
                System.Array.IndexOf(SceneBlockNames, sceneBlockDataNameProperty.stringValue) < 0)
            {
                EditorGUILayout.HelpBox("Select a scene block type: des, dun or tomb.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
