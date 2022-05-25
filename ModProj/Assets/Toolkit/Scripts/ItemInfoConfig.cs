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
        [Tooltip("Add store's prefab name")]
        public string addStoreItemName;

        public string dependItemName;

        [Tooltip("the name displayed by store")]
        public string name;

        [Tooltip("item description")]
        public string desc;
    }


    [CreateAssetMenu(fileName = "ItemInfoConfig", menuName = "ItemInfoConfig")]
    [System.Serializable]
    public class ItemInfoConfig : ScriptableObject
    {
        [SerializeField]
        public StoreItemInfo[] storeItemInfo;

        [SerializeField]
        public HitInfo[] hitInfo;
    }
}