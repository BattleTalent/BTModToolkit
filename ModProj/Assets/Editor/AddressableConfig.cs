using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "AddressableConfig", menuName = "AddressableConfig")]
public class AddressableConfig : ScriptableObject
{
    static AddressableConfig config;

    public string prefix;

    [Tooltip("need addressable's weapons path")]
    public List<string> weaponPaths;


    public string GetPrefix()
    {
        return prefix;
    }    

    public List<string> GetWeaponPaths()
    {
        return weaponPaths;
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
    void ModifyPrefix(string oldPrefix, string newPrefix)
    {
        //RefreshPath
        AddressableHelper.RefreshAddressables();
        //RefreshPerfab
        AddressableHelper.RefreshAssetPrefix(oldPrefix, newPrefix);
        //RefreshScript
        AddressableHelper.RefreshScriptPrefix(oldPrefix, newPrefix);
        prefix = newPrefix;
    }
}