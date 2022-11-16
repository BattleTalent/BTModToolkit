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

        [Tooltip("character's weapon, fill in the addStoreItemName in ItemInfoConfig when using a mod weapon.")]
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

        [EasyButtons.Button]
        public void AutoAddPrefix()
        {
            //Weapon
            string prefix = AddressableConfig.GetConfig().GetPrefix();
            if (storeItemInfo != null)
            {
                foreach (var item in storeItemInfo)
                {
                    if (string.IsNullOrEmpty(item.addStoreItemName))
                        continue;

                    if (!item.addStoreItemName.Contains(prefix))
                    {
                        item.addStoreItemName = prefix + item.addStoreItemName;
                    }
                }
            }

            //Scene
            if (sceneModInfo != null)
            {
                foreach (var item in sceneModInfo)
                {
                    if (string.IsNullOrEmpty(item.sceneName))
                        continue;


                    if (!item.sceneName.Contains(prefix))
                    {
                        item.sceneName = prefix + item.sceneName;
                    }
                }
            }

            //Skin
            if (skinInfo != null)
            {
                foreach (var item in skinInfo)
                {
                    if (string.IsNullOrEmpty(item.skinName))
                        continue;

                    if (!item.skinName.Contains(prefix))
                    {
                        item.skinName = prefix + item.skinName;
                    }
                }
            }

            //Role
            if (roleModInfo != null)
            {
                foreach (var item in roleModInfo)
                {
                    if (string.IsNullOrEmpty(item.roleName))
                        continue;

                    if (!item.roleName.Contains(prefix))
                    {
                        item.roleName = prefix + item.roleName;
                    }
                }
            }
        }

        [EasyButtons.Button]
        public void Check()
        {
            string prefix = AddressableConfig.GetConfig().GetPrefix();

            //Weapon
            if (storeItemInfo != null)
            {
                foreach(var item in storeItemInfo){
                    if (!item.addStoreItemName.Contains(prefix))
                    {
                        Debug.LogError("The prefix of addStoreItemName:" + item.addStoreItemName + " is wrong or missing, please fill in " +
                            "the same prefix as in AddressableConfig.");
                    }

                    if (string.IsNullOrEmpty(item.name))
                        Debug.LogError("Please fill in the name of the StoreItemInfo.");
                }
            }

            //Scene
            if (sceneModInfo != null)
            {
                foreach (var item in sceneModInfo)
                {
                    if (!item.sceneName.Contains(prefix))
                    {
                        Debug.LogError("The Prefix of sceneName:" + item.sceneName + " is wrong or missing, please fill in " +
                            "the same prefix as in AddressableConfig.");
                    }

                    if (string.IsNullOrEmpty(item.name))
                        Debug.LogError("Please fill in the name of the SceneModInfo.");
                }
            }

            //Skin
            if (skinInfo != null)
            {
                foreach (var item in skinInfo)
                {
                    if (!item.skinName.Contains(prefix))
                    {
                        Debug.LogError("The Prefix of skinName:" + item.skinName + " is wrong or missing, please fill in " +
                            "the same prefix as in AddressableConfig.");
                    }

                    if (string.IsNullOrEmpty(item.name))
                        Debug.LogError("Please fill in the name of the SkinInfo.");
                    if (string.IsNullOrEmpty(item.meshRoot))
                        Debug.LogError("Please fill in the MeshRoot of the SkinInfo.");

                }
            }

            //Role
            if (roleModInfo != null)
            {
                foreach (var item in roleModInfo)
                {
                    if (!item.roleName.Contains(prefix))
                    {
                        Debug.LogError("The Prefix of roleName:" + item.roleName + " is wrong or missing, please fill in " +
                            "the same prefix as in AddressableConfig.");
                    }

                    if (string.IsNullOrEmpty(item.name))
                        Debug.LogError("Please fill in the name of the RoleModInfo.");
                }
            }
        }
    }
}