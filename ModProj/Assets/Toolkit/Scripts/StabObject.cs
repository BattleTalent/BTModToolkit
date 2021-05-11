using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    public class StabObject : MonoBehaviour
    {
        [System.Serializable]
        public class StabGeometry
        {
            public enum FreeAxis
            {
                None,
                X,
                Y,
                Z,
            }

            [Tooltip("stab area forward")]
            public float geoFwdDis = 0.1f;
            [Tooltip("stab area thick")]
            public float thickness = 0.08f;
            [Tooltip("stab area width")]
            public float geoWidth = 0.5f;
            [Tooltip("stab area original point based on sub obj or this obj")]
            public Vector3 geoPoint;
            [Tooltip("stab area forward dir based on sub obj or this obj")]
            public Vector3 geoForward = Vector3.forward;
            [Tooltip("stab area thick dir based on sub obj or this obj")]
            public Vector3 geoThick = Vector3.up;
            [Tooltip("stabbed slide distance, negative number, the smaller the deeper")]
            public float stabMin = -0.08f;
            [Tooltip("stabbed slide distance, negative number, the smaller the deeper")]
            public float stabMax = -0.2f;
            [Tooltip("stabbed slide distance, should be between min and max, the smaller the deeper")]
            public float stabTie = -0.12f;
            [Tooltip("detemine how precise the direction you stab in, 0 means super easy, 1 means super hard")]
            public float stabDot = 0.5f;
            [Tooltip("detemine how fast the weapon should have, it's a sqrt number, if you want it to be 4, you should fill in 4x4=16")]
            public float stabHandVelSqrtRequire = 3.5f * 3.5f;
            [Tooltip("detemine how slick it's. the higher the slicker")]
            public float stabSlideFactor = 1000;
            //public float spring = 10000;
            //public float damper = 1000;
            [Tooltip("control which collider could stab, you could use this as fliter")]
            public Collider[] cols;
            [Tooltip("detemine how slick it's. the higher the slicker")]
            public Rigidbody subObj;
            [Tooltip("-1 means pointtip, >=0 means sidecut, only sidecut can cut object off")]
            public int sideGeo = -1;
            [Tooltip("once true, then this object can not be stabout by itself, but enemy's action can still remove it")]
            public bool noStabOut = false;
            public bool IsSideGeo() { return sideGeo >= 0; }
            public bool CanNotBuildStab() { return stabMax >= 0; }
            [Tooltip("sharp or blunt, true means sharp")]
            public bool isBlade = true;
            //[Tooltip("which axis should be")]
            //public FreeAxis freeAxis = FreeAxis.None;

            public Vector3 GetGeoPoint(Transform obj)
            {
                if (subObj != null)
                {
                    obj = subObj.transform;
                }
                return obj.TransformPoint(geoPoint);
            }

            public Vector3 GetDecalUp(Transform obj, Quaternion rot)
            {
                if (subObj != null)
                {
                    obj = subObj.transform;
                }
                return rot * obj.TransformDirection(geoThick);
            }

            public Vector3 GetThickWorld(Transform obj)
            {
                if (subObj != null)
                {
                    obj = subObj.transform;
                }
                return obj.TransformDirection(geoThick);
            }

            public Vector3 GetForwardWorld(Transform obj)
            {
                if (subObj != null)
                {
                    obj = subObj.transform;
                }
                return obj.TransformDirection(geoForward);
            }

            public Vector3 GetSideWorld(Transform obj)
            {
                if (subObj != null)
                {
                    obj = subObj.transform;
                }
                return obj.TransformDirection(Vector3.Cross(geoThick, geoForward));
            }

            public bool IsDirRight(Transform obj, Vector3 dir)
            {
                if (subObj != null)
                {
                    obj = subObj.transform;
                }
                var dot = Vector3.Dot(dir, GetForwardWorld(obj));
                return dot > stabDot;
            }
        }




        public StabGeometry[] geos;
        public InteractBase ib;
        public bool trackback = false;


        private void Reset()
        {
            ib = GetComponent<InteractBase>();
            geos = new StabGeometry[] {
                new StabGeometry()
            };
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
#if true
            for (int i = 0; i < geos.Length; ++i)
            {
                var geo = geos[i];
                var geoPos = geo.GetGeoPoint(transform);
                var widthDir = geo.GetSideWorld(transform);
                var thickDir = geo.GetThickWorld(transform);
                var forwardDir = geo.GetForwardWorld(transform);

                DebugDraw.DrawBox(geoPos + forwardDir * geo.geoFwdDis / 2f,
                    Quaternion.LookRotation(forwardDir, thickDir),
                    new Vector3(geo.geoWidth, geo.thickness, geo.geoFwdDis), Color.yellow);

                var stabPos = geoPos + forwardDir * geo.geoFwdDis;
                Debug.DrawLine(stabPos, stabPos + forwardDir * geo.stabMin, Color.black);
                Debug.DrawLine(stabPos + forwardDir * geo.stabMin, stabPos + forwardDir * geo.stabMax, Color.red);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(stabPos + forwardDir * geo.stabTie, 0.02f);
                //Gizmos.DrawLine(geoPos + widthDir)

            }
#endif

            //DebugDraw.DrawBox(transform.position, transform.rotation, Vector3.one, c);
        }
#endif
    }

}
