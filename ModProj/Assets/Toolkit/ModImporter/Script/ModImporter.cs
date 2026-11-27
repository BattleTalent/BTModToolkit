using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace CrossLink
{
    public class ModImporter : MonoBehaviour
    {
        public static ModImporter Instance;

        public string prefabPath = "Assets/Resources/GenPrefab";

        public string loadModPath;

        public static ModPanel modPanel;

        private string defaultPath = "C:/Users[username]/AppData/LocalLow/CrossLink/BattleTalent/Mods";

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            modPanel = GetComponentInChildren<ModPanel>(true);
            ImportMods();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ImportMods()
        {
            ModManager.Instance.SetLoadModPath(loadModPath == defaultPath ? "" : loadModPath);
            ModManager.Instance.Init();
        }

        public void SpawnMods(ModInfo modInfo)
        {
            if (modInfo == null || modInfo.gos == null)
            {
                Debug.LogWarning("ModImporter has no mod data, cannot spawn prefabs.");
                return;
            }

            foreach (string path in modInfo.gos)
            {
                var go = ResourceMgr.Instantiate(path) as GameObject;
                if (go != null)
                {
                    go.transform.position = Vector3.zero;
                }
                else
                {
                    Debug.Log("Spawn fail:" + path);
                }
            }
        }

        public void SaveAsPrefabs(ModInfo modInfo)
        {
            if (modInfo == null || modInfo.gos == null)
            {
                Debug.LogWarning("ModImporter has no mod data, cannot save prefabs.");
                return;
            }

            string exportPath = prefabPath + "/" + GetSafeFileName(GetNameFromModPath(modInfo.path));
            SaveScripts(modInfo, exportPath);

            foreach (string path in modInfo.gos)
            {
                var go = ResourceMgr.Instantiate(path) as GameObject;
                if (go != null)
                {
                    try
                    {
                        ScriptHelper.RefleshScripts(go);
                        CreatePrefabObj(go, exportPath);
                    }
                    finally
                    {
                        Destroy(go);
                    }
                }
                else
                {
                    Debug.Log("Save fail:" + path);
                }
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        internal static string GetNameFromModPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "UnknownMod";

            try
            {
                var directory = new DirectoryInfo(path.TrimEnd('\\', '/'));
                if (directory.Parent != null)
                    return directory.Parent.Name;
                return directory.Name;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Mod path could not be parsed:" + path + "\n" + e.Message);
                return "UnknownMod";
            }
        }

        private string GetSafeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "Unnamed";

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }

            return fileName;
        }

        private void CreatePrefabObj(GameObject obj, string path)
        {
            Debug.Log(obj.name + " saved to: " + path);
#if UNITY_EDITOR
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            try
            {
                ClearNonProjectAssetReferences(obj);
                string assetPath = GetPrefabAssetPath(path, obj.name);
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, assetPath);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
#endif
        }

        private void SaveScripts(ModInfo modInfo, string exportPath)
        {
            if (modInfo == null || modInfo.scripts == null)
            {
                Debug.LogWarning("ModImporter has no mod data, cannot save scripts.");
                return;
            }

            string subFolder = "/Script";
            Directory.CreateDirectory(exportPath + subFolder);

            foreach (string scriptPath in modInfo.scripts)
            {
                var obj = ResourceMgr.Instantiate(scriptPath) as TextAsset;
                if (obj == null)
                {
                    Debug.LogWarning("Save script failed:" + scriptPath);
                    continue;
                }

                string[] words = scriptPath.Split('/');
                File.WriteAllText(exportPath + subFolder + "/" + GetSafeFileName(words[words.Length - 1]) + ".txt", obj.text);
            }
        }

#if UNITY_EDITOR
        private string GetPrefabAssetPath(string folderPath, string prefabName)
        {
            string assetPath = folderPath + "/" + GetSafeFileName(prefabName) + ".prefab";
            if (File.Exists(assetPath) && UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) == null)
            {
                string uniquePath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
                Debug.LogWarning("Existing prefab could not be loaded, saving to a new path instead:" + uniquePath);
                return uniquePath;
            }

            return assetPath;
        }

        private void ClearNonProjectAssetReferences(GameObject root)
        {
            ClearBundledSerializedObjectReferences(root);

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null && !IsAssetDatabaseObject(materials[i]))
                    {
                        materials[i] = null;
                        Debug.LogWarning("Cleared bundled material reference before saving prefab:" + renderer.name);
                    }
                }
                renderer.sharedMaterials = materials;
            }

            foreach (var meshFilter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (meshFilter.sharedMesh != null && !IsAssetDatabaseObject(meshFilter.sharedMesh))
                {
                    meshFilter.sharedMesh = null;
                    Debug.LogWarning("Cleared bundled mesh reference before saving prefab:" + meshFilter.name);
                }
            }

            foreach (var skinnedRenderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (skinnedRenderer.sharedMesh != null && !IsAssetDatabaseObject(skinnedRenderer.sharedMesh))
                {
                    skinnedRenderer.sharedMesh = null;
                    Debug.LogWarning("Cleared bundled skinned mesh reference before saving prefab:" + skinnedRenderer.name);
                }
            }

            foreach (var audioSource in root.GetComponentsInChildren<AudioSource>(true))
            {
                if (audioSource.clip != null && !IsAssetDatabaseObject(audioSource.clip))
                {
                    audioSource.clip = null;
                    Debug.LogWarning("Cleared bundled audio clip reference before saving prefab:" + audioSource.name);
                }
            }

            foreach (var animator in root.GetComponentsInChildren<Animator>(true))
            {
                if (animator.runtimeAnimatorController != null && !IsAssetDatabaseObject(animator.runtimeAnimatorController))
                {
                    animator.runtimeAnimatorController = null;
                    Debug.LogWarning("Cleared bundled animator controller reference before saving prefab:" + animator.name);
                }
            }

            foreach (var spriteRenderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (spriteRenderer.sprite != null && !IsAssetDatabaseObject(spriteRenderer.sprite))
                {
                    spriteRenderer.sprite = null;
                    Debug.LogWarning("Cleared bundled sprite reference before saving prefab:" + spriteRenderer.name);
                }
            }

            foreach (var image in root.GetComponentsInChildren<Image>(true))
            {
                if (image.sprite != null && !IsAssetDatabaseObject(image.sprite))
                {
                    image.sprite = null;
                    Debug.LogWarning("Cleared bundled UI sprite reference before saving prefab:" + image.name);
                }
            }

            foreach (var rawImage in root.GetComponentsInChildren<RawImage>(true))
            {
                if (rawImage.texture != null && !IsAssetDatabaseObject(rawImage.texture))
                {
                    rawImage.texture = null;
                    Debug.LogWarning("Cleared bundled UI texture reference before saving prefab:" + rawImage.name);
                }
            }
        }

        private void ClearBundledSerializedObjectReferences(GameObject root)
        {
            foreach (var component in root.GetComponentsInChildren<Component>(true))
            {
                if (component == null)
                    continue;

                var serializedObject = new UnityEditor.SerializedObject(component);
                var property = serializedObject.GetIterator();
                bool changed = false;

                while (property.NextVisible(true))
                {
                    if (property.propertyType != UnityEditor.SerializedPropertyType.ObjectReference)
                        continue;

                    var value = property.objectReferenceValue;
                    if (value == null || IsAssetDatabaseObject(value) || value is GameObject || value is Component)
                        continue;

                    property.objectReferenceValue = null;
                    changed = true;
                    Debug.LogWarning("Cleared bundled object reference before saving prefab:" + component.name + "." + property.propertyPath);
                }

                if (changed)
                {
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private bool IsAssetDatabaseObject(Object obj)
        {
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
            return !string.IsNullOrEmpty(assetPath);
        }
#endif


        [EasyButtons.Button]
        public void RefleshView()
        {
            if (modPanel == null)
            {
                Debug.LogWarning("ModPanel is missing, cannot refresh ModImporter view.");
                return;
            }

            modPanel.UpdateScrollView(ModManager.Instance.mods);
        }

        [EasyButtons.Button]
        public void ResetLoadModPath()
        {
            loadModPath = defaultPath;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
