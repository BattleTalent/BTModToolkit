using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    public class HUDAnchor : MonoBehaviour
    {


        public Vector3 localPos;
        public Vector3 localRot;
        public bool shouldLimitViewAngle = true;
        // this is the follower
        public Transform followMe;
        // it will keep following if false
        public bool stopFollowWhenStare = false;
        public bool keepAliveAfterLeave = false; // such as login panel will use it as true

    }

}