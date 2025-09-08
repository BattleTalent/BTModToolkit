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


#if UNITY_EDITOR
        [EasyButtons.Button]
        public void MakeItSoundHeavier()
        {
            vol += 0.2f;
            volRandomRange += 0.1f;
            pitchMax -= 0.3f;
            pitchMin -= 0.3f;
            if (pitchMax < 0)
            {
                pitchMax = pitchMin + 0.2f;
            }
            if (pitchMin < 0)
            {
                pitchMin = pitchMax - 0.2f;
            }
        }
#endif
    }
}
