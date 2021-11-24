using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CrossLink
{
    [System.Serializable]
    public class Pose
    {
        public string id;
        public HandPosePreset asset;
    }

    public class HandPoses : MonoBehaviour
    {
        [Header("Poses")]
        public HandPosePreset defaultPose;
        public Pose[] poses;

        [Header("Fit Offsets")]
        public GameObject fitOffset_l;
        public GameObject fitOffset_r;
    }
}