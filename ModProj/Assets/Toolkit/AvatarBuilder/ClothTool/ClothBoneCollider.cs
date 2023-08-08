using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class ClothBoneCollider : MonoBehaviour
    {
        public Vector3 center = Vector3.zero;
        public float radius = 0.5f;
        public float height = 0;

        public enum Direction
        {
            X, Y, Z
        }
        public Direction direction = Direction.X;

        public enum Bound
        {
            Outside,
            Inside
        }
        public Bound bound = Bound.Outside;

        public SphereCollider collider1;
        public SphereCollider collider2;

        private void Reset()
        {
            var other = transform.GetComponents<ClothBoneCollider>();
            if (other.Length > 1)
            {
                Debug.LogError("This node already exists ClothBoneCollider!");
                Object.DestroyImmediate(this);
            }
        }

#if false
        void OnDrawGizmosSelected()
        {
            if (!enabled)
                return;

            if (bound == Bound.Outside)
                Gizmos.color = Color.yellow;
            else
                Gizmos.color = Color.magenta;
            float r = radius * Mathf.Abs(transform.lossyScale.x);
            float h = height * 0.5f - radius;
            if (h <= 0)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(center), r);
            }
            else
            {
                Vector3 c0 = center;
                Vector3 c1 = center;

                switch (direction)
                {
                    case Direction.X:
                        c0.x -= h;
                        c1.x += h;
                        break;
                    case Direction.Y:
                        c0.y -= h;
                        c1.y += h;
                        break;
                    case Direction.Z:
                        c0.z -= h;
                        c1.z += h;
                        break;
                }
                Gizmos.DrawWireSphere(transform.TransformPoint(c0), r);
                Gizmos.DrawWireSphere(transform.TransformPoint(c1), r);
            }
        }

#endif

        private void OnValidate()
        {
            ReflashCollider();
        }

        [EasyButtons.Button]
        void ReflashCollider()
        {
            float r = radius * Mathf.Abs(transform.lossyScale.x);
            float h = height * 0.5f - radius;

            if (!collider1 || !collider2)
                return;

            collider1.center = Vector3.zero;
            collider2.center = Vector3.zero;

            collider1.radius = r;
            collider2.radius = r;

            if (h <= 0)
            {
                collider1.transform.position = transform.TransformPoint(center);
                collider2.transform.position = transform.TransformPoint(center);

            }
            else
            {
                Vector3 c0 = center;
                Vector3 c1 = center;

                switch (direction)
                {
                    case Direction.X:
                        c0.x -= h;
                        c1.x += h;
                        break;
                    case Direction.Y:
                        c0.y -= h;
                        c1.y += h;
                        break;
                    case Direction.Z:
                        c0.z -= h;
                        c1.z += h;
                        break;
                }
                collider1.transform.position = transform.TransformPoint(c0);
                collider2.transform.position = transform.TransformPoint(c1);
            }
        }

        [EasyButtons.Button]
        void ApplyToClothBoneTool(LuaBehaviour lua)
        {
            var objList = lua.script.GetObjList();
            if (objList == null)
            {
                objList = new Injection[2];

                return;
            }

            var idx = 0;
            var curNum = 0;
            if (objList.Length > 0)
            {
                for (int i = 0; i < objList.Length; i++)
                {
                    if (objList[i].name.Contains("colliderGroup"))
                    {
                        if (int.TryParse((objList[i].name.Replace("colliderGroup", "")), out curNum))
                        {
                            if (curNum > idx)
                                idx = curNum;
                        }
                        
                        if (objList[i].value == collider1.transform)
                        {
                            return;
                        }
                    }
                }
            }

            List<Injection> list = new List<Injection>(objList);
            
            var colGroup = new Injection();
            colGroup.name = "colliderGroup" + (idx + 1).ToString();
            colGroup.value = collider1.transform;
            list.Add(colGroup);

            lua.script.SetObjList(list.ToArray());
        }

    }
}