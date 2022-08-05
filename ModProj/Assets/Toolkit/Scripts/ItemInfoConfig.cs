using UnityEngine;

namespace CrossLink
{
    [System.Serializable]
    public class HitInfo
    {
        [Tooltip("name of hitInfo")]
        public string Name;
        public float VelocityMlp;
        public float DamageMlp;
        public float DamageThrough;
        public float DamageCrit;
        public float StabMlp;
        public float HitMlp;
        public float HitRandom;
        public float StabDamage;
        public float BreakDefenceMlp;
        public float HitBackMlp;
        public float KnockoutFactor;
        public float DizzyFactor;
        public float StiffValue;
    }

    [System.Serializable]
    public class StoreItemInfo
    {
        [Tooltip("prefab name")]
        public string addStoreItemName;

        public string dependItemName;

        [Tooltip("dependencies of item, prompt when installing")]
        public string[] dependencies;

        [Tooltip("the name displayed by store")]
        public string name;

        [Tooltip("item description")]
        public string desc;
    }

    [System.Serializable]
    public class SceneModInfo
    {
        [Tooltip("scene name")]
        public string sceneName;

        [Tooltip("the name displayed on game")]
        public string name;

        [Tooltip("scene discription")]
        public string desc;
    }

    [CreateAssetMenu(fileName = "ItemInfoConfig", menuName = "ItemInfoConfig")]
    [System.Serializable]
    public class ItemInfoConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("if true, load this mod when it is spawn, otherwise, load this mod when it is install.")]
        public bool loadOnSpawn = true;

        [SerializeField]
        public StoreItemInfo[] storeItemInfo;

        [SerializeField]
        public SceneModInfo[] sceneModInfo;

        [SerializeField]
        public HitInfo[] hitInfo;
    }
}