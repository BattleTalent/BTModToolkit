using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{
    public enum HandSide
    {
        Right = 0,
        Left = 1,
        Both = 2,
        None = 3,
    }
    public class AttachObj : MonoBehaviour
    {
        public InteractBase interact;
        public Rigidbody selfRB;
        [Tooltip("disable self, if depend attach is not being grabbed. usually using with two handed weapon")]
        public AttachObj dependAttachObj;
        public string handPose = "HoldPose";
        [Tooltip("Auto update finger position to collider surface.")]
        public bool autoUpdateHandPose = false;
        public HandSide handSide = HandSide.Both;
        public bool allowTrigger = true;

        public bool isClimbAttach = false;
        public void Reset()
        {
            if (interact == null && transform.parent != null)
            {
                interact = transform.root.gameObject.GetComponent<InteractBase>();
                if (selfRB == null && interact != null)
                {
                    selfRB = interact.GetComponent<Rigidbody>();
                }
            }
        }
    }

}