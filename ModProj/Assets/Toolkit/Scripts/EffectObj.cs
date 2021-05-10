using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    public class EffectObj : MonoBehaviour
    {
        public float autoRemoveTime = 3f;
        public float startPosition = 0;
        public ParticleSystem[] particles;


        private void Reset()
        {
            particles = GetComponentsInChildren<ParticleSystem>();
        }

    }

}