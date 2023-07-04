using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CrossLink
{
    //[RequireComponent(typeof(TransformTracker))]
    public class HandPoseSetup : MonoBehaviour
    {
        public TransformTracker tracker;
        public HandPosePreset preset;
        public float followSpeed = -1;
    }
}