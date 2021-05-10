using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    public enum MixerType
    {
        Sound = 0,
        Voice = 1,
        MiscSound = 2,
        Music = 100,
        Amb = 101,
    }

    public class MixerSetup : MonoBehaviour
    {


        public MixerType mixerType;
        public AudioSource source;

        private void Reset()
        {
            source = GetComponent<AudioSource>();
        }

    }

}