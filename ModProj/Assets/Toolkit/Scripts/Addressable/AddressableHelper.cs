using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEngine;
using System.IO;
using System.Text;
using CrossLink;
using System;

#if UNITY_EDITOR
namespace CrossLink
{
    public class AddressableHelper : MonoBehaviour
    {
        static Dictionary<string, string> pathToName = new Dictionary<string, string> {
        { "Weapon", "Weapon/" },
        { "Script", "LuaScript/" },
        { "ICon", "ICon/" },
        { "Effect", "Effect/" },
        { "FlyObj", "FlyObj/" },
        { "Audio", "Audio/Sound/" },
        { "Config", "Config/" },
        { "Scene", "Scene/" },
        { "Skin", "Skin/" },
        { "Role", "Role/" },
    };


        private static Dictionary<string, string> GetPaths()
        {
            string curPath = Directory.GetCurrentDirectory();
            var list = AddressableConfig.GetConfig().GetAddressablePaths();
            var ite = list.GetEnumerator();

            Dictionary<string, string> paths = new Dictionary<string, string>();
            while (ite.MoveNext())
            {
                string weaponPath = Path.Combine(curPath, ite.Current);
                if (Directory.Exists(weaponPath))
                {
                    foreach (var item in pathToName)
                    {
                        string resPath = ite.Current + "/" + item.Key;
                        if (Directory.Exists(curPath + "/" + resPath))
                        {
                            paths.Add(resPath, item.Value);
                        }
                    }
                }
                else
                {
                    Debug.Log("path does't exist:" + weaponPath);
                }
            }
            return paths;
        }

        [MenuItem("Tools/Create And Refresh Addressables")]
        public static void CreateAndRefreshAddressables()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.DefaultGroup;

            var paths = GetPaths();
            foreach (var item in paths)
            {
                string[] guids = AssetDatabase.FindAssets("t:Object", new[] { item.Key });
                List<AddressableAssetEntry> List = new List<AddressableAssetEntry>();

                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    string name = path.Replace(item.Key + "/", "");
                    int idx = name.LastIndexOf(".");
                    if (idx < 0)
                        continue;
                    if (name.Contains("OcclusionCullingData"))
                        continue;

                    AddressableAssetEntry entry = settings.CreateOrMoveEntry(guids[i], group, readOnly: false, postEvent: false);
                    name = name.Remove(idx, name.Length - idx);

                    if (item.Value == pathToName["Scene"])
                        entry.address = AddressableConfig.GetConfig().GetPrefix() + name;
                    else
                        entry.address = item.Value + AddressableConfig.GetConfig().GetPrefix() + name;
                }
            }

            {
                var guid = AssetDatabase.AssetPathToGUID("Assets/Build/Entry.txt");
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
                entry.address = "Entry";
            }

        }


        [MenuItem("Tools/Refresh Addressables Only")]
        public static void RefreshAddressables()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.DefaultGroup;

            var paths = GetPaths();
            foreach (var item in paths)
            {
                string[] guids = AssetDatabase.FindAssets("t:Object", new[] { item.Key });

                for (int i = 0; i < guids.Length; i++)
                {
                    AddressableAssetEntry entry = settings.FindAssetEntry(guids[i]);
                    if (entry != null)
                    {
                        settings.MoveEntry(entry, group, readOnly: false, postEvent: false);
                        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        string name = path.Replace(item.Key + "/", "");
                        int idx = name.LastIndexOf(".");
                        if (idx < 0)
                            continue;
                        if (name.Contains("OcclusionCullingData"))
                            continue;

                        name = name.Remove(idx, name.Length - idx);

                        if (item.Value == pathToName["Scene"])
                            entry.address = AddressableConfig.GetConfig().GetPrefix() + name;
                        else
                            entry.address = item.Value + AddressableConfig.GetConfig().GetPrefix() + name;
                    }
                }
            }

        }


        public static void AutoCompleteGazeObj()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.DefaultGroup;
            ItemInfoConfig info = null;

            List<StoreItemInfo> storeItems = new List<StoreItemInfo>();

            var eIte = group.entries.GetEnumerator();
            while (eIte.MoveNext())
            {
                if (eIte.Current.address.Contains(pathToName["Config"]))
                {
                    info = AssetDatabase.LoadAssetAtPath<ItemInfoConfig>(eIte.Current.AssetPath);

                    if (info != null)
                    {
                        for (int i = 0; i < info.storeItemInfo.Length; i++)
                        {
                            storeItems.Add(info.storeItemInfo[i]);
                        }
                    }
                }
            }

            var sIte = storeItems.GetEnumerator();
            string name = "";
            string str = "";
            string path = pathToName["Weapon"];
            GameObject go = null;
            while (sIte.MoveNext())
            {
                name = sIte.Current.addStoreItemName;
                eIte.Reset();
                while (eIte.MoveNext())
                {
                    str = eIte.Current.address.Replace(path, "");
                    if (name == str)
                    {
                        go = AssetDatabase.LoadAssetAtPath<GameObject>(eIte.Current.AssetPath);
                        if (go != null)
                        {
                            CompleteGazeObjByItemInfo(sIte.Current, go);
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CompleteGazeObjByItemInfo(StoreItemInfo info, GameObject go)
        {
            GazeObj[] gazes = null;
            gazes = go.GetComponentsInChildren<GazeObj>();
            if (gazes != null)
            {
                for (int i = 0; i < gazes.Length; i++)
                {
                    if (gazes[i].showName == "")
                    {
                        gazes[i].showName = info.addStoreItemName;
                    }
                    if (gazes[i].showInfo == "")
                    {
                        gazes[i].showInfo = info.addStoreItemName + "_Desc";
                    }
                }
            }
        }

        public static void ModelSetUp()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.DefaultGroup;
            var eIte = group.entries.GetEnumerator();

            string rolePath = pathToName["Role"];
            while (eIte.MoveNext())
            {
                if (eIte.Current.address.Contains(rolePath))
                {
                    string[] dependencies = AssetDatabase.GetDependencies(eIte.Current.AssetPath);
                    foreach (var path in dependencies)
                    {
                        var modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
                        if (modelImporter != null)
                        {
                            modelImporter.animationType = ModelImporterAnimationType.Human;
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RemoveEntry()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.DefaultGroup;

            var guid = AssetDatabase.AssetPathToGUID("Assets/Build/Entry.txt");
            AddressableAssetEntry entry = settings.FindAssetEntry(guid);
            if (entry != null)
            {
                group.RemoveAssetEntry(entry);
            }
        }

        public static void Clear()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.DefaultGroup;
            settings.RemoveGroup(group);
#if false
        var paths = GetPaths();
        foreach (var item in paths)
        {
            string[] guids = AssetDatabase.FindAssets("t:Object", new[] { item.Key });
            for (int i = 0; i < guids.Length; i++)
            {
                AddressableAssetEntry entry = settings.FindAssetEntry(guids[i]);
                if (entry != null)
                    group.RemoveAssetEntry(entry);
            }
        }
#endif
        }

        public static void UpdatePrefab(GameObject root, String oldPrefix, String curPrefix) {
            var hasChanged = false;
            foreach (var fo in root.GetComponentsInChildren<GazeObj>())
            {
                fo.showInfo = fo.showInfo.Replace(oldPrefix, curPrefix);
                fo.showName = fo.showName.Replace(oldPrefix, curPrefix);
                hasChanged = true;
            }

            foreach (var so in root.GetComponentsInChildren<SoundEffectPlayer>())
            {
                for (int j = 0; j < so.soundInfo.soundNames.Length; j++)
                {
                    so.soundInfo.soundNames[j] = so.soundInfo.soundNames[j].Replace(oldPrefix, curPrefix);
                    hasChanged = true;
                }
            }

            foreach (var rg in root.GetComponentsInChildren<RagdollHitInfoObj>())
            {
                rg.hitInfo.templateName = rg.hitInfo.templateName.Replace(oldPrefix, curPrefix);
                hasChanged = true;
            }

            foreach (var fo in root.GetComponentsInChildren<FlyObjectX>())
            {
                LuaScript ls = fo.script;
                string name = ls.GetLuaScript();
                ls.SetLuaScript(name.Replace(oldPrefix, curPrefix));
                var stringList = ls.GetStringList();
                foreach (var str in stringList)
                {
                    str.value = str.value.Replace(oldPrefix, curPrefix);
                }

                fo.flyObjTobeCreatedOnImpact = fo.flyObjTobeCreatedOnImpact.Replace(oldPrefix, curPrefix);
                SoundEffectReplacePrefix(fo.delaySound, oldPrefix, curPrefix);

                fo.shootEffect = fo.shootEffect.Replace(oldPrefix, curPrefix);
                SoundEffectReplacePrefix(fo.shootSound, oldPrefix, curPrefix);

                fo.impactEffect = fo.impactEffect.Replace(oldPrefix, curPrefix);
                fo.impactSceneDecal = fo.impactSceneDecal.Replace(oldPrefix, curPrefix);
                SoundEffectReplacePrefix(fo.impactSound, oldPrefix, curPrefix);

                fo.tailEffect = fo.tailEffect.Replace(oldPrefix, curPrefix);
                hasChanged = true;
            }
        
            foreach (var it in root.GetComponentsInChildren<InteractTriggerX>())
            {
                LuaScript ls = it.script;
                string name = ls.GetLuaScript();
                ls.SetLuaScript(name.Replace(oldPrefix, curPrefix));
                var stringList = ls.GetStringList();
                foreach (var str in stringList)
                {
                    str.value = str.value.Replace(oldPrefix, curPrefix);
                }

                it.chargeEffect = it.chargeEffect.Replace(oldPrefix, curPrefix);
                it.chargeEndEffect = it.chargeEndEffect.Replace(oldPrefix, curPrefix);

                SoundEffectReplacePrefix(it.chargeSound, oldPrefix, curPrefix);
                SoundEffectReplacePrefix(it.chargeEndSound, oldPrefix, curPrefix);

                it.activateEffect = it.activateEffect.Replace(oldPrefix, curPrefix);
                hasChanged = true;
            }

            foreach (var lb in root.GetComponentsInChildren<LuaBehaviour>())
            {
                LuaScript ls = lb.script;
                string name = ls.GetLuaScript();
                ls.SetLuaScript(name.Replace(oldPrefix, curPrefix));
                var stringList = ls.GetStringList();
                foreach (var str in stringList)
                {
                    str.value = str.value.Replace(oldPrefix, curPrefix);
                }
                hasChanged = true;
            }

            if (hasChanged) {
                try
                {
                    PrefabUtility.SavePrefabAsset(root);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
        }

        public static void RefreshPrefabPrefix(string oldPrefix, string curPrefix)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.DefaultGroup;

            var eIte = group.entries.GetEnumerator();
            while (eIte.MoveNext())
            {
                string assetPath = eIte.Current.AssetPath;

                GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                
                if (root == null)
                {
                    continue;
                }

                UpdatePrefab(root, oldPrefix, curPrefix);
            }
        }

        public static void RefreshAssetPrefix(string oldPrefix, string curPrefix)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.DefaultGroup;

            var eIte = group.entries.GetEnumerator();
            while (eIte.MoveNext())
            {
                string assetPath = eIte.Current.AssetPath;

                ItemInfoConfig info = AssetDatabase.LoadAssetAtPath<ItemInfoConfig>(assetPath);
                if (info == null)
                {
                    continue;
                }

                foreach (var item in info.storeItemInfo)
                {
                    item.addStoreItemName = item.addStoreItemName.Replace(oldPrefix, curPrefix);
                    item.dependItemName = item.dependItemName.Replace(oldPrefix, curPrefix);
                    item.dependItemName = item.dependItemName.Replace(oldPrefix, curPrefix);
                }

                foreach (var item in info.storeItemInfo)
                {
                    item.addStoreItemName = item.addStoreItemName.Replace(oldPrefix, curPrefix);
                    item.dependItemName = item.dependItemName.Replace(oldPrefix, curPrefix);
                    item.dependItemName = item.dependItemName.Replace(oldPrefix, curPrefix);
                }

                foreach (var item in info.hitInfo)
                {
                    item.Name = item.Name.Replace(oldPrefix, curPrefix);
                }

                //Scene
                if (info.sceneModInfo != null)
                {
                    foreach (var item in info.sceneModInfo)
                    {
                        item.sceneName = item.sceneName.Replace(oldPrefix, curPrefix);
                    }
                }

                //Skin
                if (info.skinInfo != null)
                {
                    foreach (var item in info.skinInfo)
                    {
                        item.skinName = item.skinName.Replace(oldPrefix, curPrefix);
                    }
                }

                //Role
                if (info.roleModInfo != null)
                {
                    foreach (var item in info.roleModInfo)
                    {
                        item.roleName = item.roleName.Replace(oldPrefix, curPrefix);
                        item.weapon = item.weapon.Replace(oldPrefix, curPrefix);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var toolkitPrefabsGuids = AssetDatabase.FindAssets("t:prefab", new string[] {"Assets/Toolkit/Prefabs"});
            foreach( var guid in toolkitPrefabsGuids )
            {
                var path = AssetDatabase.GUIDToAssetPath( guid );
                GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>( path );

                UpdatePrefab(root, oldPrefix, curPrefix);
            }
        }

        public static void CheckItemInfoConfig()
        {
            bool isPass = true;
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.DefaultGroup;

            var prefix = AddressableConfig.GetConfig().prefix;

            Dictionary<string, string> names = new Dictionary<string, string>();
            var eIte = group.entries.GetEnumerator();
            while (eIte.MoveNext())
            {
                string assetPath = eIte.Current.AssetPath;

                ItemInfoConfig info = AssetDatabase.LoadAssetAtPath<ItemInfoConfig>(assetPath);
                if (info == null)
                {
                    continue;
                }

                foreach (var item in info.storeItemInfo)
                {
                    if (!names.ContainsKey(item.addStoreItemName))
                    {
                        names.Add(item.addStoreItemName, assetPath);
                    }
                }
                if (info.sceneModInfo != null)
                {
                    foreach (var item in info.sceneModInfo)
                    {
                        if (!names.ContainsKey(item.sceneName))
                        {
                            names.Add(item.sceneName, assetPath);
                        }
                    }
                }

                if (info.skinInfo != null)
                {
                    foreach (var item in info.skinInfo)
                    {
                        if (!names.ContainsKey(item.skinName))
                        {
                            names.Add(item.skinName, assetPath);
                        }
                    }
                }
                if (info.roleModInfo != null)
                {
                    foreach (var item in info.roleModInfo)
                    {
                        if (!names.ContainsKey(item.roleName))
                        {
                            names.Add(item.roleName, assetPath);
                        }

                        if (!string.IsNullOrEmpty(item.weapon))
                        {
                            if (item.weapon.Contains(prefix))
                            {
                                if (!names.ContainsKey(item.weapon))
                                {
                                    names.Add(item.weapon, assetPath);
                                }
                            }
                        }
                    }
                }
            }

            foreach(var item in names)
            {
                if (CheckAddressableExistItem(item.Key) == false)
                {
                    Debug.LogError("The item:"+ item.Key + " in ItemInfoConfig:" + item.Value + " did not find a matching file in Addressables.");
                    isPass = false;
                }
            }

            if (isPass)
            {
                Debug.Log("Pass");
            }
        }

        private static bool CheckAddressableExistItem(string name)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.DefaultGroup;

            var eIte = group.entries.GetEnumerator();
            var strs = new string[2];
            while (eIte.MoveNext())
            {
                strs = eIte.Current.address.Split('/');
                if (strs == null)
                    continue;

                if (strs[strs.Length - 1] == name)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SoundEffectReplacePrefix(SoundEffectInfo info, string old, string replace)
        {
            info.templateName = info.templateName.Replace(old, replace);
            for (int i = 0; i < info.soundNames.Length; i++)
            {
                info.soundNames[i] = info.soundNames[i].Replace(old, replace);
            }
        }

        public static void RefreshScriptPrefix(string oldPrefix, string curPrefix)
        {
            string curPath = Directory.GetCurrentDirectory();
            var list = AddressableConfig.GetConfig().GetAddressablePaths();
            var ite = list.GetEnumerator();

            string path = "";
            while (ite.MoveNext())
            {
                path = Path.Combine(curPath, ite.Current);
                path = Path.Combine(path, "Script");

                if (Directory.Exists(path))
                {
                    DirectoryInfo direction = new DirectoryInfo(path);
                    FileInfo[] files = direction.GetFiles("*.txt", SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; i++)
                    {
                        ReplaceValue(files[i].FullName, oldPrefix, curPrefix);
                    }
                }
            }
        }
        static void ReplaceValue(string strFilePath, string strIndex, string newValue)
        {
            if (File.Exists(strFilePath))
            {
                string[] lines = File.ReadAllLines(strFilePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(strIndex))
                    {
                        lines[i] = lines[i].Replace(strIndex, newValue);
                    }
                }
                File.WriteAllLines(strFilePath, lines);
            }
        }



        public static bool IsSceneExist()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.DefaultGroup;

            var eIte = group.entries.GetEnumerator();
            while (eIte.MoveNext())
            {
                string assetPath = eIte.Current.AssetPath;

                SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
                if (scene != null)
                {
                    return true;
                }

                ItemInfoConfig info = AssetDatabase.LoadAssetAtPath<ItemInfoConfig>(assetPath);
                if (info != null)
                {
                    if (info.storeItemInfo.Length > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
#endif