using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    public class LightPassEffect : MonoBehaviour
    {
        public Light li;
        public AnimationCurve lightCurve;
        public float lightTime = 3f;
        public float initIntensity = 7;
    }
}