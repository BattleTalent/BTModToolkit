using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
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

        public MerchandiseItemInfo[] dungeonInfos;

        public bool supportedMultiplayer;
    }


    [System.Serializable]
    public class MerchandiseItemInfo
    {
        [Tooltip("the dungeon level where item sold")]
        public string level;

        [Tooltip("weight of on item sold")]
        public int weight;

        [Tooltip("price of item sold")]
        public int price;

        [Tooltip("enhance level")]
        public int enhanceLevel = 0;

        //[Tooltip("buffs")]
        //public string[] buffs;
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

        public bool supportedMultiplayer;

        [Tooltip("multiplayer config")]
        public Network.NetworkMapConfigData mutiplayerConfig;
    }

    [System.Serializable]
    public class HandPoseModifier
    {
        [Tooltip("HandPoseName")]
        public string name;

        [Tooltip("Adjusting the position of the avatar left hand pose.")]
        public Vector3 leftHandPosition;

        [Tooltip("Adjusting the rotation of the avatar left hand pose.")]
        public Quaternion leftHandRotation;

        [Tooltip("Adjusting the position of the avatar right hand pose.")]
        public Vector3 rightHandPosition;

        [Tooltip("Adjusting the rotation of the avatar right hand pose.")]
        public Quaternion rightHandRotation;

        [Tooltip("Degree of finger flexion. The fingers are, in order, thumb, index, middle, ring and pinky(pinkie).")]
        public float[] fingerWeight;

#if UNITY_EDITOR

        static public void SetDefaultPoseValue(HandPoseModifier handPose)
        {
            HandPoseLib lib = UnityEditor.AssetDatabase.LoadAssetAtPath<HandPoseLib>("Assets/Toolkit/HandPoseHelper/HandPoseLib.asset");

            if (lib == null)
                return;

            HandPoseSetup hps;
            GameObject preset;
            for (int i = 0; i < lib.poseDefines.Length; i++)
            {
                if (handPose.name.Equals(lib.poseDefines[i].poseName))
                {
                    for (int j = 0; j < lib.poseDefines[i].handPoses.Length; j++)
                    {
                        preset = lib.poseDefines[i].handPoses[j] as GameObject;
                        if (preset)
                        {
                            hps = preset.GetComponentInChildren<HandPoseSetup>();
                            if (hps)
                            {
                                if (j == RagdollBoneInfo.RIGHT_HAND)
                                {
                                    handPose.rightHandPosition = hps.transform.localPosition;
                                    handPose.rightHandRotation = hps.transform.localRotation;
                                }
                                else
                                {
                                    handPose.leftHandPosition = hps.transform.localPosition;
                                    handPose.leftHandRotation = hps.transform.localRotation;
                                }
                                handPose.fingerWeight = hps.preset.fingerWeight.ToArray();
                            }
                        }
                    }
                }
            }
        }
#endif
    }

    //static public void SetDefaultPoseValue(HandPoseModifier handPose)
    //{
    //    string name = string.Empty;
    //    switch (handPose.name)
    //    {
    //        case "HoldPose":
    //            handPose.leftHandPosition = Vector3.zero;
    //            handPose.leftHandRotation = Quaternion.Euler(0, 90, 90);
    //            handPose.rightHandPosition = new Vector3(0, 0, -0.0603f);
    //            handPose.rightHandRotation = Quaternion.Euler(0, -90, -90);
    //            handPose.fingerWeight = new float[5] { 0.5f, 0.7f, 0.65f, 0.65f, 0.6f };
    //            break;
    //        case "GunPose":
    //            handPose.leftHandPosition = new Vector3(0.0025f, 0.0117f, -0.002f);
    //            handPose.leftHandRotation = Quaternion.Euler(-257.273f, -253.278f, 33.84999f);
    //            handPose.rightHandPosition = new Vector3(-0.0079f, 0.0119f, 0.0076f);
    //            handPose.rightHandRotation = Quaternion.Euler(-109.337f, -71.92599f, -7.527985f);
    //            handPose.fingerWeight = new float[5] { 0, 0, 0.7f, 0.7f, 0.7f };
    //            break;
    //        case "GunPose2":
    //            handPose.leftHandPosition = new Vector3(-0.013f, -0.0082f, -0.0153f);
    //            handPose.leftHandRotation = Quaternion.Euler(64.861f, 74.691f, -5.991f);
    //            handPose.rightHandPosition = new Vector3(-0.0078f, 0.0155f, -0.0169f);
    //            handPose.rightHandRotation = Quaternion.Euler(-106.35f, -72.60699f, -1.055969f);
    //            handPose.fingerWeight = new float[5] { 0, 0.15f, 0.7f, 0.7f, 0.7f };
    //            break;
    //        case "GrabPose":
    //            handPose.leftHandPosition = Vector3.zero;
    //            handPose.leftHandRotation = Quaternion.Euler(-90f, 0, 90f);
    //            handPose.rightHandPosition = Vector3.zero;
    //            handPose.rightHandRotation = Quaternion.Euler(-90f, 0, -90f);
    //            handPose.fingerWeight = new float[5] { 0, 0, 0, 0, 0 };
    //            break;
    //        case "DefaultPose":
    //            handPose.leftHandPosition = Vector3.zero;
    //            handPose.leftHandRotation = Quaternion.Euler(-90f, 0, 90f);
    //            handPose.rightHandPosition = Vector3.zero;
    //            handPose.rightHandRotation = Quaternion.Euler(-90f, 0, -90f);
    //            handPose.fingerWeight = new float[5] { 0, 0, 0, 0, 0 };
    //            break;
    //        case "GlovePose":
    //            handPose.leftHandPosition = new Vector3(-0.0232f, -0.0179f, -0.0047f);
    //            handPose.leftHandRotation = Quaternion.Euler(58.557f, 80.137f, 340.902f);
    //            handPose.rightHandPosition = new Vector3(0.0034f, 0.0017f, -0.0032f);
    //            handPose.rightHandRotation = Quaternion.Euler(-67.786f, 90.00001f, -180f);
    //            handPose.fingerWeight = new float[5] { 0.85f, 0.83f, 0.87f, 0.87f, 0.87f };
    //            break;
    //        case "GuitarPose":
    //            handPose.leftHandPosition = new Vector3(0.0394f, -0.0037f, -0.0504f);
    //            handPose.leftHandRotation = Quaternion.Euler(80.827f, 13.572f, 81.94801f);
    //            handPose.rightHandPosition = new Vector3(-0.0113f, 0.0032f, -0.0093f);
    //            handPose.rightHandRotation = Quaternion.Euler(-249.778f, -98.87201f, 178.619f);
    //            handPose.fingerWeight = new float[5] { 0.85f, 0.83f, 0.87f, 0.87f, 0.87f };
    //            break;
    //        default:
    //            Debug.Log("Your handPose :" + handPose.name + " is not the default value, only the default handPose can be set.");
    //            break;
    //    }
    //}
    [System.Serializable]
    public class AvatarInfo
    {
        [Tooltip("prefab name")]
        public string avatarName;

        [Tooltip("the name displayed on game")]
        public string name;

        [Tooltip("skin discription")]
        public string desc;

        [Tooltip("Adjust the data of the HandPose exclusive to this model, the default handPose are HoldPose, GunPose, GunPose2, GrabPose, DefaultPose, GlovePose.")]
        public HandPoseModifier[] handposes;
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

        [Tooltip("default replace NPC.")]
        public string defaultReplacement;

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

        //BodyFragment[] bodyFragments;
    }

    [System.Serializable]
    public class RoleAttr
    {
        public float hpMax;

        public float mpMax;

        public float atkMlp;

        public float massMlp;
    }

    [System.Serializable]
    public class HandPose
    {
        public string name;

        public string leftHandPreset;

        public string rightHandPreset;
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
        public PropInfo[] propInfo;

        [SerializeField]
        public AvatarInfo[] avatarInfo;

        [SerializeField]
        public SkinInfo[] skinInfo;

        [SerializeField]
        public RoleModInfo[] roleModInfo;

        [SerializeField]
        public HandPose[] handPoseInfo;

        [SerializeField]
        public HitInfo[] hitInfo;

        [Tooltip("FlyObject need to be registered to be used in multiplayer mode. Please make sure that the registered prefab supports multiplayer mode!")]
        public string[] networkPrefabRegister;



#if UNITY_EDITOR

        [EasyButtons.Button]
        public void AutoRegisterNetworkPrefab()
        {
            Debug.Log("Please make sure that the registered prefab supports multiplayer mode!");

            var assetPath = AssetDatabase.GetAssetPath(this);
            if (!assetPath.Contains("Assets/Build"))
            {
                Debug.LogError($"This ItemInfoConfig is not in Assets/Build");
                return;
            }
            if (!assetPath.Contains("Config/"))
            {
                Debug.LogError($"This ItemInfoConfig is not in the Config folder.");
                return;
            }

            List<string> list = new List<string>();

            string modPath = assetPath.Substring(0, assetPath.LastIndexOf("/", assetPath.LastIndexOf("/") - 1));

            //flyObject
            string foPath = System.IO.Path.Combine(modPath, "FlyObj");
            var foGuids = AssetDatabase.FindAssets("t:prefab", new string[] { foPath });
            foreach (var guid in foGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                path = path.Replace(".prefab", string.Empty);
                var idx = path.LastIndexOf("/");
                string name = path.Substring(idx + 1, path.Length - idx - 1);
                list.Add(AddressableConfig.GetConfig().GetPrefix() + name);
            }

            ////SceneObject
            //string sceneObjPath = System.IO.Path.Combine(modPath, "SceneObj");
            //var soGuids = AssetDatabase.FindAssets("t:prefab", new string[] { sceneObjPath });
            //foreach (var guid in soGuids)
            //{
            //    var path = AssetDatabase.GUIDToAssetPath(guid);
            //    path = path.Replace(".prefab", string.Empty);
            //    var idx = path.LastIndexOf("/");
            //    string name = path.Substring(idx + 1, path.Length - idx - 1);
            //    list.Add(AddressableConfig.GetConfig().GetPrefix() + name);
            //}

            networkPrefabRegister = list.ToArray();
        }


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

            //SceneObj
            if (propInfo != null)
            {
                foreach (var item in propInfo)
                {
                    if (string.IsNullOrEmpty(item.name))
                        continue;


                    if (!item.name.Contains(prefix))
                    {
                        item.name = prefix + item.name;
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

            //Avatar
            if (avatarInfo != null)
            {
                foreach (var item in avatarInfo)
                {
                    if (string.IsNullOrEmpty(item.avatarName))
                        continue;

                    if (!item.avatarName.Contains(prefix))
                    {
                        item.avatarName = prefix + item.avatarName;
                    }
                }
            }

            //HandPose
            if (handPoseInfo != null)
            {
                foreach (var item in handPoseInfo)
                {
                    if (!string.IsNullOrEmpty(item.name))
                    {
                        if (!item.name.Contains(prefix))
                        {
                            item.name = prefix + item.name;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.leftHandPreset))
                    {
                        if (!item.leftHandPreset.Contains(prefix))
                        {
                            item.leftHandPreset = prefix + item.leftHandPreset;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.rightHandPreset))
                    {
                        if (!item.rightHandPreset.Contains(prefix))
                        {
                            item.rightHandPreset = prefix + item.rightHandPreset;
                        }
                    }
                }
            }
        }

        [EasyButtons.Button]
        public void SetDefaultPoseValueToAvatar(string avatarName, string pose)
        {
            if (avatarInfo == null)
                return;

            foreach (var item in avatarInfo)
            {
                if (item.avatarName == avatarName)
                {
                    foreach (var info in item.handposes)
                    {
                        if (info.name == pose)
                        {
                            HandPoseModifier.SetDefaultPoseValue(info);
                            break;
                        }
                    }
                    break;
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }

        [EasyButtons.Button]
        public void Check()
        {
            bool isPass = true;
            string prefix = AddressableConfig.GetConfig().GetPrefix();

            //Weapon
            if (storeItemInfo != null)
            {
                foreach (var item in storeItemInfo) {
                    if (!item.addStoreItemName.Contains(prefix))
                    {
                        Debug.LogError("The prefix of addStoreItemName:" + item.addStoreItemName + " is wrong or missing, please fill in " +
                            "the same prefix as in AddressableConfig.");
                        isPass = false;
                    }

                    if (string.IsNullOrEmpty(item.name))
                    {
                        Debug.LogError("Please fill in the name of the StoreItemInfo.");
                        isPass = false;
                    }
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
                        isPass = false;
                    }

                    if (string.IsNullOrEmpty(item.name))
                    {
                        Debug.LogError("Please fill in the name of the SceneModInfo.");
                        isPass = false;
                    }
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
                        isPass = false;
                    }

                    if (string.IsNullOrEmpty(item.name))
                    {
                        Debug.LogError("Please fill in the name of the SkinInfo.");
                        isPass = false;
                    }
                    if (string.IsNullOrEmpty(item.meshRoot))
                    {
                        Debug.LogError("Please fill in the MeshRoot of the SkinInfo.");
                        isPass = false;
                    }

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
                        isPass = false;
                    }

                    if (string.IsNullOrEmpty(item.name))
                    {
                        Debug.LogError("Please fill in the name of the RoleModInfo.");
                        isPass = false;
                    }
                }
            }

            //handpose
            if (handPoseInfo != null)
            {
                foreach (var item in handPoseInfo)
                {
                    if (!item.name.Contains(prefix))
                    {
                        Debug.LogError("The Prefix of name:" + item.name + " is wrong or missing, please fill in " +
                            "the same prefix as in AddressableConfig.");
                        isPass = false;
                    }

                    if (string.IsNullOrEmpty(item.name))
                    {
                        Debug.LogError("Please fill in the name of the HandPoseInfo.");
                        isPass = false;
                    }

                    if (!item.leftHandPreset.Contains(prefix))
                    {
                        Debug.LogError("The Prefix of LeftHandPreset:" + item.leftHandPreset + " is wrong or missing, please fill in " +
                            "the same prefix as in AddressableConfig.");
                        isPass = false;
                    }

                    if (string.IsNullOrEmpty(item.leftHandPreset))
                    {
                        Debug.LogError("Please fill in the name of the HandPoseInfo.");
                        isPass = false;
                    }

                    if (!item.rightHandPreset.Contains(prefix))
                    {
                        Debug.LogError("The Prefix of RightHandPreset:" + item.rightHandPreset + " is wrong or missing, please fill in " +
                            "the same prefix as in AddressableConfig.");
                        isPass = false;
                    }

                    if (string.IsNullOrEmpty(item.rightHandPreset))
                    {
                        Debug.LogError("Please fill in the name of the HandPoseInfo.");
                        isPass = false;
                    }
                }
            }

            if (isPass)
            {
                Debug.Log("Pass");
            }
        }
#endif
    }
}