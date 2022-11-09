using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{

    public class EventToRotateObj : EventToBase
    {
        public Vector3 targetLocalRot;
        public float moveSec = 0.5f;
        public AudioSource moveSound;
        public Transform target;
    }

}