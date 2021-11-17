using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class Tagger : MonoBehaviour
    {
        public enum Tag
        {
            // the stability, we're better to specific the enum value
            None,
            Env = 100,
            EnvGround = 101,

            SceneObject = 210,

            FlyObject = 200,

            InteractRole = 220,

            InteractObj = 230,
            InteractWeapon = 240,

            PlayerRole = 300,
            PlayerHand = 301,
        }

        public Tag tt;
    }
    public class FlyObject : PhysicsUnit
    {
        [Header("Fly Logic")]
        //public Rigidbody rb;
        public Collider col;
        [Tooltip("the initial fly speed, it could be overrided by shooter")]
        public float force = 10;
        [Tooltip("it'll disappear when fly time reach, unit is second")]
        public float maxFlyTime = 8f;
        [Tooltip("detemine this flyobj will pointing toward the fly track or not")]
        public bool forwardAlwayFollowVelocity = false;
        [Tooltip("tread this fly obj as arrow, so it will be count as range damage")]
        public bool dealDmgAsArrow = false;
        [Tooltip("create another flyobj when impact")]
        public string flyObjTobeCreatedOnImpact;
        [Tooltip("max collision count, then it will disappear")]
        public int collisionCount = 1;
        [Tooltip("max fly collision count, then it will stop the fly state and stop control the forward")]
        public int collisionFlyCount = 1;
        [Tooltip("fly at spawn")]
        public bool flyAtSpawn = true;
        
        [Header("delay")]
        public float delayToAcc = -1;
        public SoundEffectInfo delaySound;
        protected int delayActId = -1;

        [Header("Effect")]
        public string shootEffect;
        public SoundEffectInfo shootSound;
        public string impactEffect;// = "BulletImpact";
        public string impactSceneDecal;// = "BulletImpactDecal";        
        public SoundEffectInfo impactSound;
        public string tailEffect;

        public TrailRenderer trail;
        public ParticleSystem[] resetParticles;
        public bool playImpactOnTimeout = false;

        public Tagger.Tag[] ignoreDamageList;
        protected void Reset()
        {
            col = GetComponentInChildren<Collider>();
            trail = GetComponent<TrailRenderer>();
        }

    }

}
