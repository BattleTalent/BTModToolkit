using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace XftWeapon
{
    public class XWeaponTrail : MonoBehaviour
    {

        #region public members
        public bool UseWith2D = false;
        static public bool UseWithSRP = true;
        public string SortingLayerName;
        public int SortingOrder;
        public Transform PointStart;
        public Transform PointEnd;

        const int MaxFrame = 4;
        const int Granularity = 6;
        const float Fps = 24f;
        const float UpdateInterval = 1f / Fps;
        public Color MyColor = Color.white;
        public Material MyMaterial;
        #endregion

        private void OnDrawGizmosSelected()
        {
            if (PointStart == null || PointEnd == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(PointEnd.position, 0.05f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(PointStart.position, 0.05f);
        }

    }

}


