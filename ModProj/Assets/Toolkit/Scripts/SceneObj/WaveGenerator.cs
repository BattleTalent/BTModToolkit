using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace CrossLink
{
    [System.Serializable]
    public class PipelineDefine
    {
        public WaveGroup wave;
        public int roleNum;
        public int waitNumToStart;
        public float intervalTime = 1;
    }

    [System.Serializable]
    public class MetaDefine
    {
        public int[] spawnNum = new int[] { 1, 2, 3, 4 };
    }

    [CreateAssetMenu(fileName = "WaveGenerator", menuName = "WaveGenerator")]
    public class WaveGenerator:ScriptableObject
    {
        public Object statue;
        public MetaDefine meta;
        public string[] pool;
        public PipelineDefine[] waveData;
    }
}