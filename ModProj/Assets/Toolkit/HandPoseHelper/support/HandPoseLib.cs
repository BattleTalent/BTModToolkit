using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink {
    [CreateAssetMenu(fileName = "HandPoseLib", menuName = "HandPoseLib")]
    public class HandPoseLib : ScriptableObject
    {
        [System.Serializable]
        public class HandPoseDefine
        {
            public string poseName;
            public Object[] handPoses;
        }

        public HandPoseDefine[] poseDefines;
    }
}
