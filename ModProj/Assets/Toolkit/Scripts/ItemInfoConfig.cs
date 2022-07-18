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
        [Tooltip("item id, should be the same as your icon and prefab, usually is [prefix][prefab name]")]
        public string addStoreItemName;

        public string dependItemName;

        [Tooltip("item name")]
        public string name;

        [Tooltip("item description")]
        public string desc;
    }


    [CreateAssetMenu(fileName = "ItemInfoConfig", menuName = "ItemInfoConfig")]
    [System.Serializable]
    public class ItemInfoConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("if true, will load this mod on demand. otherwise, will load this mod when the game started. if this is an item without config file and you add it to the store via script, then we'll treat it as loading on demand as well.")]
        public bool loadOnSpawn = true;

        [SerializeField]
        public StoreItemInfo[] storeItemInfo;

        [SerializeField]
        public HitInfo[] hitInfo;
    }
}