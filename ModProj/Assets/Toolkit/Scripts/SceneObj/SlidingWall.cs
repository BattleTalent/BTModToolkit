//#define WALL_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{
    public class SlidingWall : SceneObj
    {

        public MultiTriggerHub hub;
        public Transform slideDirIndicator;

        public float width = 5;
        public float centralHeigh = 1;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var heightSphere = transform.position;
            heightSphere.y += centralHeigh;
            Gizmos.DrawSphere(heightSphere, 0.05f);
        }
#endif
    }
}