using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class Cannon : SceneObj
    {
        public string shootBullet;
        public string shootEffect;

        public float shootForce = 0;

        public Transform shootPosition;
        public Transform shootTarget;
        public bool mountBulletOnCannon = false;

        public event System.Action<GameObject> SpawnEvent;

        public float shootInterval = -1;
        public bool autoRunAtStart = false;

    }
}