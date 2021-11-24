using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{
    [System.Serializable]
    public class HandFinger
    {
        public Transform[] fingerNodes;
        [SerializeField]
        Vector3[] fingerOpenPos;
        [SerializeField]
        Quaternion[] fingerOpenRot;
        [SerializeField]
        Vector3[] fingerClosePos;
        [SerializeField]
        Quaternion[] fingerCloseRot;

        [System.NonSerialized]
        public float latestWeight;

        public void SetPose(float weight)
        {
            latestWeight = weight;
            for (int i = 0; i < fingerNodes.Length; ++i)
            {
                fingerNodes[i].localPosition = Vector3.Lerp(fingerOpenPos[i], fingerClosePos[i], weight);
                fingerNodes[i].localRotation = Quaternion.Slerp(fingerOpenRot[i], fingerCloseRot[i], weight);
            }
        }
    }

    public class HandPoseControl : MonoBehaviour
    {
        public const int ThumbFinger = 0;
        public const int IndexFinger = 1;
        public const int MiddleFinger = 2;
        public const int RingFinger = 3;
        public const int PinkieFinger = 4;
    
        public HandFinger[] fingers;
    
        public void SetHandPose(float weight)
        {
            for (int i = 0; i < fingers.Length; ++i)
            {
                fingers[i].SetPose(weight);
            }
        }

        public void SetHandPose(List<float> weightList)
        {
            for (int i = 0; i < fingers.Length; ++i)
            {
                fingers[i].SetPose(weightList[i]);
            }
        }
    }

}