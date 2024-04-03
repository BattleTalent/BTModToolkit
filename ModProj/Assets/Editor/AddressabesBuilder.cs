using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;

namespace CrossLink
{
    static public class AddressabesBuilder
    {

        [MenuItem("Tools/Open PC Mod Folder %#t")]
        static void OpenPCModFolder()
        {
            EditorUtility.RevealInFinder(GetPCModPath());
        }

        static string GetPCModPath()
        {
            var targetpath = Application.persistentDataPath;

            var splitedPath = targetpath.Split('/');
            int len = splitedPath.Length;
            targetpath = "";
            for (int i = 0; i < len - 2; ++i)
            {
                targetpath += splitedPath[i] + "/";
            }
            targetpath += "CyDream/BattleTalent/Mods";

            return targetpath;
        }


        [MenuItem("BuildTools/FastBuildAndInstallForWindows")]
        public static void FastBuildAndInstallForWindows()
        {
            ClearOldFiles();

            if (!AddressableHelper.ValidateAddressables()) {
                return;
            }

            BuildWithProfile(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);

            InstallModOnWindows();
        }

        [MenuItem("BuildTools/FastBuildAndInstallForAndroid")]
        public static void FastBuildAndInstallForAndroid()
        {
            ClearOldFiles();

            if (!AddressableHelper.ValidateAddressables()) {
                return;
            }

            if (!isBuiltAndroid)
            {
                SetPlayerSetting();
                isBuiltAndroid = true;
            }

            BuildWithProfile(BuildTargetGroup.Android, BuildTarget.Android);

            InstallModOnAndroid();
        }
#if false
        [MenuItem("BuildTools/BuildAndroid")]
        public static void BuildAndroid()
        {
            ClearOldFiles();

            if (!AddressableHelper.ValidateAddressables()) {
                return;
            }

            BuildWithProfile(BuildTargetGroup.Android, BuildTarget.Android);

            //InstallModOnAndroid();
            Debug.Log("Click the InstallModToQuest.bat command to install");
        }
#endif

        #region check if all shaders support singlepassinstanced
        [MenuItem("BuildTools/CheckShaders")]
        public static bool CheckAllShadersSupport()
        {
            // Get all the shader guids in the project
            string[] guids = AssetDatabase.FindAssets("t:Shader");
            List<Shader> shaders = new List<Shader>();

            float progress = 0f;
            int count = 0;

            foreach (string guid in guids)
            {
                count += 1;
                progress = count / guids.Length;
                // Get the asset path from the guid
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Load the shader from the path
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);

                // Check if the shader is not null and does not support singlepass instanced rendering
                if (shader != null && !IsSinglePassInstancedSupported(shader))
                {
                    // Add the shader and its path to the lists
                    shaders.Add(shader);
                }

                EditorUtility.DisplayProgressBar("processing", "checking shaders...", progress);
            }

            EditorUtility.ClearProgressBar();

            if (shaders.Count <= 0)
            {
                Debug.Log("[CheckShaders] All shaders support SinglePassInstanced!!!");
                return true;
            }

            // Create a new asset object and assign the shader list to it
            var asset = ScriptableObject.CreateInstance<ShaderListAsset>();
            asset.shaders = shaders;

            // Get a path from the user to save the asset
            string savepath = EditorUtility.SaveFilePanelInProject("Save Unsupported Shader List", "ShaderList", "asset", "Please enter a file name to save the shader list");

            // If the path is not empty, save the asset
            if (!string.IsNullOrEmpty(savepath))
            {
                try
                {
                    AssetDatabase.CreateAsset(asset, savepath);
                    AssetDatabase.SaveAssets();

                    Debug.Log($"[CheckShaders] Save {savepath} succeed!!!");
                }
                catch(System.Exception e)
                {
                    Debug.LogError(e);
                    Debug.LogError("Invaild path or filename");
                }
            }

            return false;
        }

        private const string UNITY_VERTEX_INPUT_INSTANCE_ID = "UNITY_VERTEX_INPUT_INSTANCE_ID";
        private const string UNITY_VERTEX_OUTPUT_STEREO = "UNITY_VERTEX_OUTPUT_STEREO";
        private const string UNITY_SETUP_INSTANCE_ID = "UNITY_SETUP_INSTANCE_ID";
        private const string UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO = "UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO";

        private static bool IsSinglePassInstancedSupported(Shader shader)
        {
            string[] symbols = {
            UNITY_VERTEX_INPUT_INSTANCE_ID,
            UNITY_VERTEX_OUTPUT_STEREO,
            UNITY_SETUP_INSTANCE_ID,
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO
        };

            // Check if the shader has these symbols
            var path = AssetDatabase.GetAssetPath(shader);
            var text = File.ReadAllText(path);

            if (path.Contains("Editor") //exclude editor shaders
            || path.Contains("probuilder") //exclude probuilder shaders
            || path.Contains("com.unity.render-pipelines.universal")) //exclude urp shaders
            {
                return true;
            }
            else
            {
                foreach (var symbol in symbols)
                {
                    if (text.Contains(symbol))
                        continue;
                    else
                        return false;
                }

                return true;
            }
        }
        #endregion

        [MenuItem("BuildTools/BuildAllBundles", false, 0)]
        public static void BuildAll()
        {
            ClearOldFiles();

            if (!AddressableHelper.ValidateAddressables()) {
                return;
            }

            /*if (!CheckAllShadersSupport())
            {
                Debug.LogWarning("Please make sure all shades support singlepassinstanced!!!");
                return;
            }*/

            //BuildWithProfile("Windows");
            //BuildWithProfile("Android");                             

            if (isBuiltAndroid)
            {
                SetPlayerSetting();
                isBuiltAndroid = true;
            }

            BuildWithProfile(BuildTargetGroup.Android, BuildTarget.Android);
            BuildWithProfile(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
        }


        [MenuItem("BuildTools/ClearOldFiles", false, 0)]
        static void ClearOldFiles()
        {
            var buildPath = Application.dataPath + "/Mods/";
            if (Directory.Exists(buildPath) == false)
                return;
            //File.Delete(buildPath);
            Directory.Delete(buildPath, true);
            AssetDatabase.Refresh();
            Debug.Log("ClearOldFiles");
        }


        static bool isBuiltAndroid = false;
        static void SetPlayerSetting()
        {
            UnityEditor.EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        }

        [MenuItem("BuildTools/InstallModOnWindows", false, 11)]
        static void InstallModOnWindows()
        {
            var targetpath = Application.persistentDataPath;
            
            var splitedPath = targetpath.Split('/');
            int len = splitedPath.Length;
            targetpath = "";
            for (int i=0; i<len-2; ++i)
            {
                targetpath += splitedPath[i] + "/";
            }
            targetpath += "CyDream/BattleTalent/Mods";

            var targetModPath = targetpath + "/" + Application.productName;

            if (Directory.Exists(targetModPath))
            {
                Directory.Delete(targetModPath, true);
            }
            Debug.Log("Deleted Old " + targetModPath);

            var sourcePath = Application.dataPath + "/Mods/" + Application.productName;

            DirectoryCopy(sourcePath, targetModPath, true);
            Debug.Log("Copyed New From " + sourcePath);
        }


        
        [MenuItem("BuildTools/InstallModOnAndroid", false, 11)]
        static void InstallModOnAndroid()
        {

            MS.Shell.Editor.EditorShell.Options options = null;
#if true   // here to define your encoding and env vars
            {
                options = new MS.Shell.Editor.EditorShell.Options()
                {
                    workDirectory = "./ADBTools/",
                    //encoding = System.Text.Encoding.GetEncoding("GBK"),
                    environmentVars = new Dictionary<string, string>(){
                        {"PATH","usr/bin"},
                    }
                };
            }

#endif

            {
                var operation = MS.Shell.Editor.EditorShell.Execute("InstallModToQuest.bat", options);
                operation.onLog += (MS.Shell.Editor.EditorShell.LogType LogType, string log) => {
                    Debug.Log(log);
                };

            }


            // execute adb push from unity always fail, I don't know why...
#if false
            {
                Debug.Log("trying to adb rm");
                var operation = MS.Shell.Editor.EditorShell.Execute(".\\ADBTools\\adb.exe shell rm -r /sdcard/Android/data/com.CrossLink.SAO/files/Mods/" + Application.productName, options);
                operation.onLog += (MS.Shell.Editor.EditorShell.LogType LogType, string log) => {
                    Debug.Log(log);
                };
                
            }
            {
                Debug.Log("trying to adb push");
                var operation = MS.Shell.Editor.EditorShell.Execute(
                    string.Format(".\\ADBTools\\adb.exe push .\\Assets\\Mods\\{0} /sdcard/Android/data/com.CrossLink.SAO/files/Mods/", Application.productName)
                    , options);
                operation.onLog += (MS.Shell.Editor.EditorShell.LogType LogType, string log) => {
                    Debug.Log(log);
                };
            }
#if false
            {
                Debug.Log("wait");
                var operation = MS.Shell.Editor.EditorShell.Execute(
                    "TIMEOUT /T 5"
                    , options);
                operation.onLog += (MS.Shell.Editor.EditorShell.LogType LogType, string log) => {
                    Debug.Log(log);
                };
            }
#endif
#endif
#if false
            string fullPath = Application.dataPath + "/../ADBTools/adb.exe";
            Debug.Log(fullPath);

            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(fullPath);
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                startInfo.Arguments = "shell rm -r /sdcard/Android/data/com.CrossLink.SAO/files/Mods/" + Application.productName;

                System.Diagnostics.Process.Start(startInfo);
            }

            {
                


                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(fullPath);
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                startInfo.Arguments = string.Format("push .\\Assets\\Mods\\{0} /sdcard/Android/data/com.CrossLink.SAO/files/Mods/", Application.productName);
                startInfo.WorkingDirectory = "./";

                System.Diagnostics.Process.Start(startInfo);
            }
#endif
        }


        static void BuildWithProfile(BuildTargetGroup buildGroup, BuildTarget buildTarget)
        {
            var aas = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");
            Debug.Log("AddressableAssetSettings Loading:" + aas);

            if (aas != null)
            {
                var settings = aas.profileSettings;
                aas.activeProfileId = settings.GetProfileId(buildTarget.ToString());
            }

            EditorUserBuildSettings.SwitchActiveBuildTarget(buildGroup, buildTarget);
            AddressableAssetSettings.CleanPlayerContent();
            AddressableAssetSettings.BuildPlayerContent();

            Debug.Log(buildTarget + " Built");
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }


    }

    // A scriptable object class to store a list of shaders as an asset
    [CreateAssetMenu(fileName = "ShaderList", menuName = "Shader List", order = 1)]
    public class ShaderListAsset : ScriptableObject
    {
        public List<Shader> shaders;
    }
}


