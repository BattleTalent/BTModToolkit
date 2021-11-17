using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    [CreateAssetMenu(fileName = "HandPosePreset", menuName = "HandPosePreset")]
    public class HandPosePreset : ScriptableObject
    {
        public List<float> fingerWeight;
    }

}
