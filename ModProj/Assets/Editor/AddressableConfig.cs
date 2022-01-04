using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "AddressableConfig", menuName = "AddressableConfig")]
public class AddressableConfig : ScriptableObject
{
    static AddressableConfig config;

    [TextArea(10, 100)]
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

    [Tooltip("Modify prefix in addressables")]
    public string prefix;

    [Tooltip("need addressable's weapons path")]
    public List<string> addressablePaths;


    [MenuItem("Tools/Select Addressables Config %#e")]
    static void SelectAddressablesConfig()
    {
        UnityEditor.Selection.activeObject = GetConfig();
    }

    public string GetPrefix()
    {
        return prefix;
    }    

    public List<string> GetAddressablePaths()
    {
        return addressablePaths;
    }

    static public AddressableConfig GetConfig()
    {
        config = Resources.Load("AddressableConfig") as AddressableConfig;
        return config;
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
    }

    [EasyButtons.Button]
    void RefreshAddressableNameOnly()
    {
        AddressableHelper.RefreshAddressables();
    }


    [EasyButtons.Button]
    void ModifyPrefixInPathsPrefabsAndScripts(string oldPrefix, string newPrefix)
    {
        //RefreshPath
        AddressableHelper.RefreshAddressables();
        //RefreshPrefab
        AddressableHelper.RefreshAssetPrefix(oldPrefix, newPrefix);
        //RefreshScript
        AddressableHelper.RefreshScriptPrefix(oldPrefix, newPrefix);
        prefix = newPrefix;
    }
}