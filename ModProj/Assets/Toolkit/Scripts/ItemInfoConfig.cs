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

        [Tooltip("item type, Weapon,Music or other")]
        public ItemInfoConfig.ItemType itemType;
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

    [System.Serializable]
    public class SkinInfo
    {
        [Tooltip("skin name")]
        public string skinName;

        [Tooltip("the name displayed on game")]
        public string name;

        [Tooltip("skin discription")]
        public string desc;

        [Tooltip("name of the root node of all meshes in the prefab")]
        public string meshRoot;
    }

    [System.Serializable]
    public class RoleModInfo
    {
        [Tooltip("prefab name")]
        public string roleName;

        [Tooltip("the name displayed on game")]
        public string name;

        [Tooltip("role discription")]
        public string desc;

        [Tooltip("replace NPC.")]
        public string[] replaceRole;

        [Tooltip("character's weapon")]
        public string weapon;

        [Tooltip("character's attribute, read only the first data of this array, Use default data when array length is 0.")]
        public RoleAttr[] attr;

        public SoundEffectInfo boneBreakSound;
        public SoundEffectInfo hurtSound;
        public SoundEffectInfo deathSound;
        public SoundEffectInfo groundSound;
        public SoundEffectInfo warningSound;
        public SoundEffectInfo attackSound;
        public SoundEffectInfo tauntSound;
    }

    [System.Serializable]
    public class RoleAttr
    {
        public float hpMax;

        public float mpMax;

        public float atkMlp;

        public float massMlp;
    }

    [CreateAssetMenu(fileName = "ItemInfoConfig", menuName = "ItemInfoConfig")]
    [System.Serializable]
    public class ItemInfoConfig : ScriptableObject
    {
        [SerializeField]
        public enum ItemType
        {
            Weapon,
            Music,
            Other,
        }

        [SerializeField]
        [Tooltip("if true, load this mod when it is spawn, otherwise, load this mod when it is install.")]
        public bool loadOnSpawn = true;

        [SerializeField]
        public StoreItemInfo[] storeItemInfo;

        [SerializeField]
        public SceneModInfo[] sceneModInfo;

        [SerializeField]
        public SkinInfo[] skinInfo;

        [SerializeField]
        public RoleModInfo[] roleModInfo;

        [SerializeField]
        public HitInfo[] hitInfo;
    }
}