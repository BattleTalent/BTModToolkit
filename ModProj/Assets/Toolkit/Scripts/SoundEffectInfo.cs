using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{

    [System.Serializable]
    public class SoundEffectInfo
    {
        public string templateName;
        public string[] soundNames;
        public float vol = 0.5f;
        public float volRandomRange = 0;
        public float pitchMax = -1;
        public float pitchMin = 0.8f;
    }

}
