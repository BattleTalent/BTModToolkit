using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{
    public class AttachObj : MonoBehaviour
    {
        public InteractBase interact;
        public Rigidbody selfRB;
        [Tooltip("disable self, if depend attach is not being grabbed. usually using with two handed weapon")]
        public AttachObj dependAttachObj;
        public string handPose = "HoldPose";

        public void Reset()
        {
            if (interact == null && transform.parent != null)
            {
                interact = transform.root.gameObject.GetComponent<InteractBase>();
                if (selfRB == null && interact != null)
                {
                    selfRB = interact.rb;
                }
            }
        }
    }

}