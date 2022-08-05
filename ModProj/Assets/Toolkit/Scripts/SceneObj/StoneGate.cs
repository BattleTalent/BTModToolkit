using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{

    public class StoneGate : SceneObj
    {
        public float doorOpenZ = -1.5f;
        public float doorCloseZ = -0.7f;

        public EventToMoveObj[] moveEvents;
        public Material[] mats;
        public Renderer[] rnds;
        public Collider blocker;

        public void OpenDoor()
        {

        }

        public void CloseDoor()
        {
        }
    }
}