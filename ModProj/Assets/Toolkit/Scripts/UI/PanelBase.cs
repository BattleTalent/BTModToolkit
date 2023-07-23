using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CrossLink
{

    public class PanelBase : MonoBehaviour {
        [HideInInspector]
        public string panelName;

#if UNITY_EDITOR
        [EasyButtons.Button]
        void ReplaceMat()
        {            
            var mat = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Toolkit/UITexture/HUDMat.mat", typeof(Material)) as Material;
            var graphics = GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; ++i)
            {
                graphics[i].material = mat;
                UnityEditor.EditorUtility.SetDirty(graphics[i]);
            }
        }

        [EasyButtons.Button]
        void ReplaceMatDefault()
        {
            var mat = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Toolkit/UITexture/HUDMatDefault.mat", typeof(Material)) as Material;
            var graphics = GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; ++i)
            {
                graphics[i].material = mat;
                UnityEditor.EditorUtility.SetDirty(graphics[i]);
            }
        }
#endif
    }

}