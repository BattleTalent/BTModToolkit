using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{

    public class BoneDrawer : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            DebugDraw.DrawBone(transform, true);
        }
#endif
    }

}
