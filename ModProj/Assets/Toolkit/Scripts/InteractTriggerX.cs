using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using EasyButtons;
#endif


namespace CrossLink
{

    public class InteractTriggerX : InteractTrigger
    {
        public LuaScript script = new LuaScript();

#if UNITY_EDITOR
        [Button]
        void PullSkillChangeColors()
        {
            skillChangeColors = GetComponentsInChildren<Renderer>();
            //enabled = false;
            EditorUtility.SetDirty(gameObject);
        }
#endif
    }

}
