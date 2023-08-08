using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class ClothBone : MonoBehaviour
    {
        public Transform root = null;
        public float updateRate = 60.0f;
        [Range(0, 1)]
        public float damping = 0.1f;
        public AnimationCurve dampingDistrib = null;
        [Range(0, 1)]
        public float elasticity = 0.1f;
        public AnimationCurve elasticityDistrib = null;
        [Range(0, 1)]
        public float stiffness = 0.1f;
        public AnimationCurve stiffnessDistrib = null;
        [Range(0, 1)]
        public float inert = 0;
        public AnimationCurve inertDistrib = null;
        public float radius = 0;
        public AnimationCurve radiusDistrib = null;

        public float endLength = 0;
        public Vector3 endOffset = Vector3.zero;
        public Vector3 gravity = Vector3.zero;
        public Vector3 force = Vector3.zero;
        public List<ClothBoneCollider> colliders = null;
        public List<Transform> exclusions = null;
        public enum FreezeAxis
        {
            None, X, Y, Z
        }
        public FreezeAxis freezeAxis = FreezeAxis.None;
        public bool distantDisable = false;
        public Transform referenceObject = null;
        public float distanceToObject = 20;

        public Transform outputTrans;
        
        [EasyButtons.Button]
        public void ApplyToAvatar()
        {

        }
    }
}
