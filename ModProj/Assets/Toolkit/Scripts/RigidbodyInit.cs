using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{
    public class RigidbodyInit : MonoBehaviour
    {
        public Rigidbody rb;

        public Vector3 inertiaTensor;
        public Vector3 centerMass;

        private void Reset()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow - DebugDraw.alpha05;
            //Gizmos.DrawSphere(transform.position + transform.forward * designCallingPos, 0.05f);

            if (Vector3.zero != centerMass)
            {
                Gizmos.DrawSphere(transform.TransformPoint(centerMass), 0.025f);
            }

            if (Vector3.zero != inertiaTensor)
            {
                //Gizmos.DrawSphere(transform.TransformPoint(inertiaTensor), 0.025f);
                Gizmos.color = Color.red - DebugDraw.alpha05;
                Gizmos.DrawLine(transform.position,
                    transform.TransformPoint(new Vector3(inertiaTensor.x, 0, 0)));

                Gizmos.color = Color.green - DebugDraw.alpha05;
                Gizmos.DrawLine(transform.position,
                    transform.TransformPoint(new Vector3(0, inertiaTensor.y, 0)));

                Gizmos.color = Color.blue - DebugDraw.alpha05;
                Gizmos.DrawLine(transform.position,
                    transform.TransformPoint(new Vector3(0, 0, inertiaTensor.z)));
            }
        }
    }

}
