using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{
    public class RelativeHandPose : MonoBehaviour
    {
        public Vector3 index;
        public Vector3 thumb;
        public Vector3 palm;

        public Transform realHandPose;
        public Vector3 rotateSpace;
        public Vector3 grabRotateSpace;
    }

}