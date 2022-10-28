using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    public class EventToMoveObj : EventToBase
    {
        public Vector3 targetLocalPos;
        public Vector3 sourceLocalPos;
        public float moveSec = 0.5f;
        public AudioSource moveSound;
        public Transform target;
    }
}