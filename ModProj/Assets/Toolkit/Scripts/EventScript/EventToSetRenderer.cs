using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    public class EventToSetRenderer : EventToBase
    {
        public Renderer[] rnds;
        public string field;
        public Vector4 vecValue = new Vector4(0,0,0,0);
        public float lerpSec = 0.25f;
    }

}