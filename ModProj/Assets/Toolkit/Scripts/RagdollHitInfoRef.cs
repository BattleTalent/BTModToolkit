using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using EasyButtons;
#endif

namespace CrossLink
{
    public class RagdollHitInfoRef : MonoBehaviour
    {
        public RagdollHitInfoObj[] refs;

        private void Reset()
        {
            refs = GetComponentsInChildren<RagdollHitInfoObj>();
        }

#if UNITY_EDITOR
        [Button]
        void RefreshRefs()
        {
            Reset();
            EditorUtility.SetDirty(gameObject);
        }
#endif
    }

}