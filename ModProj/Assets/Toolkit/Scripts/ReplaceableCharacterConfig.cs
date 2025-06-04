using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CrossLink
{
    [CreateAssetMenu(fileName = "ReplaceableCharacterConfig", menuName = "ReplaceableCharacterConfig")]
    [System.Serializable]
    public class ReplaceableCharacterConfig : ScriptableObject
    {
        public string[] characters;

        static ReplaceableCharacterConfig config;

        static public ReplaceableCharacterConfig GetConfig()
        {
            config = Resources.Load("ReplaceableCharacterConfig") as ReplaceableCharacterConfig;
            return config;
        }
    }
}