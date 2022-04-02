
//#define MOD_ASSET_LOAD_LOG
//#define LOAD_ASSET_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Threading.Tasks;

namespace CrossLink
{

    public class ModInfo
    {
        public string path;
        public IResourceLocator resLocator;
        public List<AsyncOperationHandle<Object>> handles = new List<AsyncOperationHandle<Object>>();
        public Dictionary<string, Object> res = new Dictionary<string, Object>();

        public List<string> gos = new List<string>();
        public List<string> scripts = new List<string>();
        public bool LoadingCompeleted() { return res.Count == handles.Count; }
    }


    public class ModManager
    {
        public static ModManager Instance = new ModManager();

        static string ModsPath = "Mods/";

        public const string LuaKey = "LuaScript";
        public const string WeaponPath = "Weapon/";


#if UNITY_ANDROID
        const string BuildTarget = "Android";
#else
        const string BuildTarget = "StandaloneWindows";
#endif

        public event System.Action UpdateWeaponModEvent;

        public delegate void loadCompleted();

        public List<ModInfo> mods = new List<ModInfo>();
        public List<ModInfo> GetModInfos() { return mods; }

        System.IO.FileSystemWatcher watcher;
        public void Init()
        {
            SetProjectInfo("CrossLink", "BattleTalent");
            
            DiscoverMods();
            LoadMods();
        }

        public bool IsReadyToLoadAgain()
        {
            for (int i = 0; i < mods.Count; ++i)
            {
                if (mods[i].LoadingCompeleted() == false)
                    return false;
            }
            return true;
        }

        public void ClearMods()
        {
            if (mods.Count == 0)
                return;



            for (int i = 0; i < mods.Count; ++i)
            {



                for (int j = 0; j < mods[i].handles.Count; ++j)
                {
                    try
                    {
                        if (mods[i].handles[j].IsValid() == false)
                            continue;
                        if (mods[i].handles[j].Result == null)
                            continue;
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log(e);
                        continue;
                    }
#if LOAD_ASSET_LOG
                    Debug.Log("ReleaseInstanceHanlde:" + mods[i].handles[j].Result.name);
#endif
                    Addressables.ReleaseInstance(mods[i].handles[j]);
                }


                var resIte = mods[i].res.GetEnumerator();
                while (resIte.MoveNext())
                {
                    if (resIte.Current.Value == null)
                        continue;

#if LOAD_ASSET_LOG
                    Debug.Log("ReleaseRes:" + resIte.Current.Key);
#endif
                    Addressables.Release(resIte.Current.Value);
                }



                try
                {
                    Addressables.RemoveResourceLocator(mods[i].resLocator);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                }
            }
            mods.Clear();
            Addressables.ClearResourceLocators();
            Caching.ClearCache();
            Resources.UnloadUnusedAssets();
            Debug.Log("Mod Cleared");
        }



#if false
        // you should check is ready first
        public void ReloadMods()
        {
            ClearMods();
            DiscoverMods();
            LoadMods();
        }
#endif

        public void SetLoadModPath(string path)
        {
            ModsPath = path;
            ModsPath = ModsPath.Replace("\\", "/");
        }

        // find all folders that contain mod files
        public void DiscoverMods()
        {

            ModsPath = ModsPath == "" ? Application.persistentDataPath + "/Mods/" : ModsPath + "/";
            //ModsPath = "Assets/Resources/Mods/";
            //ModsPath = Application.dataPath + "/Resources/Mods/";


            Debug.Log("Discovering Mods:" + ModsPath);


            DirectoryInfo modDirectory = new DirectoryInfo(ModsPath);
            var dirs = modDirectory.GetDirectories();
            //var files = modDirectory.GetFiles("*", SearchOption.TopDirectoryOnly);


            //Get all files in directory
            foreach (var dir in dirs)
            {
                //if (file.Attributes == FileAttributes.Directory)
                {
                    var mod = new ModInfo();
                    //mod.path = dir.FullName.Replace("\\", "/") + "/" + BuildTarget + "/";
                    //Path.Combine
                    //mod.path = dir.FullName + "\\" + BuildTarget;
                    mod.path = Path.Combine(dir.FullName, BuildTarget);//.Replace("\\", "/");
                    Debug.Log("Mod Found:" + mod.path);
                    mods.Add(mod);
                }
            }
        }

        // load all mods from discoverd

        int loadCount = 0;
        public void LoadMods()
        {
            Addressables.InternalIdTransformFunc = InternalIdTransformFunc;

            for (int i = 0; i < mods.Count; ++i)
            {
                LoadAndInitMod(mods[i]);
            }
        }

        // load specific mod, locate all res, and load them all into memory at the beginning
        // then wait until loading finished, we start to init
        async void LoadAndInitMod(ModInfo mod, loadCompleted cp = null)
        {
            // locate to the correct platform
            //var modpath = mod.path + "/" + BuildTarget + "/"
            string[] fileEntries;
            try
            {
                fileEntries = Directory.GetFiles(mod.path, "*.json", SearchOption.TopDirectoryOnly);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
                return;
            }
            //string[] fileEntries = Directory.GetFiles("C: \\Users\\hank\\AppData\\LocalLow\\CrossLink\\BattleTalent\\Mods\\ModProj\\StandaloneWindows", "(*.json)", SearchOption.TopDirectoryOnly);

            if (fileEntries.Length == 0)
                return;

            Debug.Log("Mod Loading:" + fileEntries[0]);

            // load catalog as res locator
            try
            {
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
            IResourceLocator modLocator = await LoadCatalogAsync(fileEntries[0]);
            mod.resLocator = modLocator;


            var keyIte = modLocator.Keys.GetEnumerator();
            while (keyIte.MoveNext())
            {
                var resKey = keyIte.Current.ToString();
                var locate = FindAssetInMod<Object>(resKey, modLocator);
                if (locate == null)
                    continue;

                try
                {
                    var handle = Addressables.LoadAssetAsync<Object>(locate);

                    //Debug.Log("Asset Loading:" + resKey);
                    mod.handles.Add(handle);
                    loadCount = loadCount + 1;
                    handle.Completed += (aso) =>
                    {
                        if (aso.Result.GetType() == typeof(UnityEngine.GameObject))
                        {
                            mod.gos.Add(resKey);
                        }else if(resKey.Contains(LuaKey))
                        {
                            mod.scripts.Add(resKey);
                        }

                        mod.res[resKey] = aso.Result;
                        if (mod.LoadingCompeleted())
                        {
                            InitMod(mod);
                            if (cp != null)
                                cp();
                        }

                        loadCount = loadCount - 1;
                        if (loadCount <= 0)
                        {
                            AllLoadCompeleted();
                        }
                    };
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                }
            }
        }

        void AllLoadCompeleted()
        {
            ModImporter.Instance.RefleshView();
        }

        
        public static ModInfo LoadingMod = null;
        void InitMod(ModInfo mod)
        {
            LoadingMod = mod;
            ResourceMgr.InjectRes(mod.res);

            
            LoadingMod = null;
        }

        public void GetLuaScript(Object obj)
        {

        }


        private string InternalIdTransformFunc(UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation location)
        {
            if (location.Data is UnityEngine.ResourceManagement.ResourceProviders.AssetBundleRequestOptions)
            {
                string path = location.InternalId;
                string projModPath = Application.persistentDataPath + "/Mods/";
                path = path.Replace("\\", "/");
                if (path.Contains(projModPath))
                {
                    path = path.Replace(projModPath, ModsPath);
                    return path;
                }
            }
            return location.InternalId;
        }

        private async Task<IResourceLocator> LoadCatalogAsync(string path)
        {
            AsyncOperationHandle<IResourceLocator> operation = Addressables.LoadContentCatalogAsync(path);
            IResourceLocator modLocator = await operation.Task; //Wait until the catalog file is loaded then retrieve the IResourceLocator for this mod
            return modLocator;
        }

        private IResourceLocator LoadCatalog(string path)
        {
            AsyncOperationHandle<IResourceLocator> operation = Addressables.LoadContentCatalog(path);
            IResourceLocator modLocator = operation.Result; //Wait until the catalog file is loaded then retrieve the IResourceLocator for this mod
            return modLocator;
        }

        public IResourceLocation FindAssetInMod<T>(string key, IResourceLocator modLocator)
        {
#if false//LOAD_ASSET_LOG
            Debug.Log("Asset Finding:" + key);
#endif

            if (key.Length == 32 && key.Contains("/") == false && key.Contains(".") == false)
                return null;

            //An IResourceLocation "contains enough information to load an asset (what/where/how/dependencies)" (Unity Docs)
            IList<IResourceLocation> locs;

            //Use the IResourceLocator.Locate function to find IResourceLocation
            if (modLocator.Locate(key, typeof(T), out locs))
            {
                // only cache those res in the list
                if (locs[0].HasDependencies == false)
                    return null;

#if LOAD_ASSET_LOG
                Debug.Log("Asset Loading:" + key);
#endif

                //Return the first location for singular asset
                return locs[0];
            }
#if false//UNITY_EDITOR
            Debug.Log("This asset could not be found, ensure you are using the right key, or that the key exists in this mod" + key);
#endif
            return null;
        }


        public void SetProjectInfo(string companyName, string productName)
        {
#if UNITY_EDITOR
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = productName;
#endif
        }
    }

}

