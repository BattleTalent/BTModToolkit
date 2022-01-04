#if false
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), editorForChildClasses: true)]
public class MonoBehaviourBaseEditor : Editor
{
    private string tooltipText;

    private void OnEnable()
    {
        var attributes = target.GetType().GetCustomAttributes(inherit: false);
        foreach (var attr in attributes)
        {
            if (attr is ClassTooltip tooltip)
            {
                this.tooltipText = tooltip.description;
                break;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField(this.tooltipText);
        base.OnInspectorGUI();
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ClassTooltip : PropertyAttribute
{
    public readonly string description;

    public ClassTooltip(string description)
    {
        this.description = description;
    }
}
#endif