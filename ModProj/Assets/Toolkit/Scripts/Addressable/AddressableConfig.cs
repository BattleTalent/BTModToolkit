using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace CrossLink
{
    [CreateAssetMenu(fileName = "AddressableConfig", menuName = "AddressableConfig")]
    public class AddressableConfig : ScriptableObject
    {
        static AddressableConfig config;
#if UNITY_EDITOR
        [TextArea(10, 100)]
#endif
        public string tips = @"
    steps to make a mod:
    1. make the prefab
    2. make sure your prefab are reference by the addressables (Window/AssetManagement/Addressables/Groups)
    3. make sure your Entry script will load your prefab at runtime
    4. edit your own product name and prefix
    5. select build from BuildTools/

    shortcut:
    ctrl-shift-e: select this config file
    ctrl-shift-t: open mod and log files on PC

    how to use this tool:
    1. put your resources in the corresponding path: http://jjyy.guru/BTModToolkit/#autotoc_md39
    2. fill your path in the weapon paths
    3. make your own prefix
    4. click CreateAndRefreshAddressableName

    how to debug:
    1. install mod and run simulator
    2. check out log error in Player.log (ctrl-shift-t)
    3. fix it then do it again

    for more information, please check out document and youtube tutorial
    ";

#if UNITY_EDITOR 
        [Tooltip("Modify prefix in addressables")]
#endif
        public string prefix;
#if UNITY_EDITOR 
        [Tooltip("need addressable's weapons path")]
#endif
        public List<string> addressablePaths;


        static public AddressableConfig GetConfig()
        {
            config = Resources.Load("AddressableConfig") as AddressableConfig;
            return config;
        }

        public string GetPrefix()
        {
            return prefix;
        }

#if UNITY_EDITOR 
        [MenuItem("Tools/Select Addressables Config %#e")]
        static void SelectAddressablesConfig()
        {
            UnityEditor.Selection.activeObject = GetConfig();
        }

        public List<string> GetAddressablePaths()
        {
            return addressablePaths;
        }

        [EasyButtons.Button]
        void ClearAddressables()
        {
            AddressableHelper.Clear();
        }

        [EasyButtons.Button]
        void CreateAndRefreshAddressableName()
        {
            AddressableHelper.CreateAndRefreshAddressables();


            AddressableHelper.AutoCompleteGazeObj();


            //AddressableHelper.ModelSetUp();
        }

        [EasyButtons.Button]
        void RefreshAddressableNameOnly()
        {
            AddressableHelper.RefreshAddressables();


            AddressableHelper.AutoCompleteGazeObj();


            //AddressableHelper.ModelSetUp();
        }

        void AutoCompleteGazeObj()
        {
            AddressableHelper.AutoCompleteGazeObj();
        }

        [EasyButtons.Button]
        void ModifyPrefixInPathsPrefabsAndScripts(string oldPrefix, string newPrefix)
        {
            if (oldPrefix == null || newPrefix == null)
            {
                Debug.LogError("Please set the old prefix and the new prefix.");
                return;
            }
            //RefreshPath
            AddressableHelper.RefreshAddressables();
            //RefreshPrefab
            AddressableHelper.RefreshPrefabPrefix(oldPrefix, newPrefix);
            //RefreshAsset
            AddressableHelper.RefreshAssetPrefix(oldPrefix, newPrefix);
            //RefreshScript
            AddressableHelper.RefreshScriptPrefix(oldPrefix, newPrefix);
            prefix = newPrefix;
        }

        [EasyButtons.Button]
        void RemoveEntry()
        {
            AddressableHelper.RemoveEntry();
        }

        [EasyButtons.Button]
        void Check()
        {
            AddressableHelper.CheckItemInfoConfig();
        }
#endif
    }
}
