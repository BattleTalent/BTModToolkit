#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace CrossLink
{
    public class CleanupContent : ScriptableObject
    {
        [MenuItem("Tools/Destructive/Cleanup All Content")]
        static void CleanupAllContent()
        {
            var wantsToContinue = EditorUtility.DisplayDialog(
                "Are you sure?",
                "This action will remove all content under the Build and Resources folder? This task is irreversable so make sure you have a backup.", 
                "Yes, go ahead!",
                "cancel"
            );
            
            if (wantsToContinue)
            {
                AssetDatabase.CreateFolder("Assets","Build_new");
                AssetDatabase.CreateFolder("Assets","Resources_new");

                string[] importantBuildFiles = new string[] { "Entry.txt" };

                foreach (string importantBuildFile in importantBuildFiles)
                {
                    AssetDatabase.CopyAsset(
                        $"Assets/Build/{importantBuildFile}", 
                        $"Assets/Build_new/{importantBuildFile}"
                    );
                }

                string[] importantResources = new string[] { "AddressableConfig.asset", "CommonPP.asset", "TemplateWizard.asset" };

                foreach (string importantResource in importantResources)
                {
                    AssetDatabase.CopyAsset(
                        $"Assets/Resources/{importantResource}", 
                        $"Assets/Resources_new/{importantResource}"
                    );
                }

                AssetDatabase.DeleteAsset("Assets/Build");
                AssetDatabase.DeleteAsset("Assets/Resources");

                AssetDatabase.MoveAsset("Assets/Build_new", "Assets/Build");
                AssetDatabase.MoveAsset("Assets/Resources_new", "Assets/Resources");

                AddressableConfig.GetConfig().addressablePaths.Clear();
            }

            Debug.Log("All cleaned up!");
        }
    }
}


#endif