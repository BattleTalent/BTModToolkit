using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrossLink
{
    public class SpawnPointMgr : MonoBehaviour
    {
        static public SpawnPointMgr Instance;
        public Transform[] pointList;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < pointList.Length; ++i)
            {
                Gizmos.DrawSphere(pointList[i].position, 0.1f);
                Gizmos.DrawLine(pointList[i].position, pointList[i].position + pointList[i].forward);
            }
        }
#endif
    }
}
