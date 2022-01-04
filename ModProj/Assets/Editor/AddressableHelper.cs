using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;
using System.Text;
using CrossLink;
using System;

public class AddressableHelper : MonoBehaviour
{
    static Dictionary<string, string> pathToName = new Dictionary<string, string> { 
        { "Weapon", "Weapon/" },
        { "Script", "LuaScript/" },
        { "ICon", "ICon/" },
        { "Effect", "Effect/" },
        { "FlyObj", "FlyObj/" },
        { "Audio", "Audio/Sound/" },
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
            if (Directory.Exists(weaponPath)){
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
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guids[i], group, readOnly: false, postEvent: false);
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string name = path.Replace(item.Key + "/", "");
                int idx = name.LastIndexOf(".");
                name = name.Remove(idx, name.Length - idx);
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
                    name = name.Remove(idx, name.Length - idx);
                    entry.address = item.Value + AddressableConfig.GetConfig().GetPrefix() + name;
                }
            }
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

    public static void RefreshAssetPrefix(string oldPrefix, string curPrefix)
    {
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}","GameObject"));

        for(int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

            GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            foreach (var fo in root.GetComponentsInChildren<GazeObj>())
            {
                fo.showInfo = fo.showInfo.Replace(oldPrefix, curPrefix);
                fo.showName = fo.showName.Replace(oldPrefix, curPrefix);
            }

            foreach (var fo in root.GetComponentsInChildren<FlyObjectX>())
            {
                LuaScript ls = fo.script;
                string name = ls.GetLuaScript();
                ls.SetLuaScript(name.Replace(oldPrefix, curPrefix));
                var stringList = ls.GetStringList();
                foreach(var str in stringList)
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
            }

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

    private static void SoundEffectReplacePrefix(SoundEffectInfo info, string old, string replace)
    {
        info.templateName = info.templateName.Replace(old, replace);
        for (int i = 0; i< info.soundNames.Length; i++)
        {
            info.soundNames[i] = info.soundNames[i].Replace(old, replace);
        }
    }

    public static void RefreshScriptPrefix(string oldPrefix, string curPrefix)
    {
        string curPath = Directory.GetCurrentDirectory();
        string buildPath = Path.Combine(curPath, "Assets/build");
        if (Directory.Exists(buildPath))
        {
            DirectoryInfo direction = new DirectoryInfo(buildPath);
            FileInfo[] files = direction.GetFiles("*.txt", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                ReplaceValue(files[i].FullName, oldPrefix, curPrefix);
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
}