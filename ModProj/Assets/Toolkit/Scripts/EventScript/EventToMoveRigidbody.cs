using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    public class EventToMoveRigidbody : EventToBase
    {
        public Transform followPos;
        public float moveSec = 0.5f;
        public Rigidbody target;
    }

}