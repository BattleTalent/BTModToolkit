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
            targetpath += "CrossLink/BattleTalent/Mods";

            return targetpath;
        }


        [MenuItem("BuildTools/FastBuildAndInstallForWindows")]
        public static void FastBuildAndInstallForWindows()
        {
            ClearOldFiles();

            BuildWithProfile(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
            
            InstallModOnWindows();
        }

        [MenuItem("BuildTools/FastBuildAndInstallForAndroid")]
        public static void FastBuildAndInstallForAndroid()
        {
            ClearOldFiles();

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

            BuildWithProfile(BuildTargetGroup.Android, BuildTarget.Android);

            //InstallModOnAndroid();
            Debug.Log("Click the InstallModToQuest.bat command to install");
        }
#endif

        [MenuItem("BuildTools/BuildAllBundles", false, 0)]
        public static void BuildAll()
        {
            ClearOldFiles();
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
            targetpath += "CrossLink/BattleTalent/Mods";

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
}


