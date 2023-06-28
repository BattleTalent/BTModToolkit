using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
namespace CrossLink
{
    public class ModItem : MonoBehaviour
    {
        public Text nameText;
        public Image image;
        private ModPanel mp;

        private ModInfo modInfo;

        private int scpIdx = 0;
        private int maxCount = -1;

        public void SetData(ModInfo info, ModPanel modPanel, int idx)
        {
            modInfo = info;
            mp = modPanel;
            maxCount = modInfo.scripts.Count;

            itemIdx = idx;
            name = GetNameFromModPath(modInfo.path);
            nameText.text = name;
        }

        string GetNameFromModPath(string path)
        {
            int idx = path.LastIndexOf("\\Mods\\");
            path = path.Substring(idx, path.Length - idx);
            path = path.Replace("\\Mods\\", "");
            idx = path.IndexOf("\\");
            path = path.Substring(0, idx);

            return path;
        }

        private void ShowScripts()
        {
            if(maxCount > 0)
            {
                ShowScriptByIdx(scpIdx);
            }
        }

        public void ShowScriptByIdx(int idx)
        {
            string path = modInfo.scripts[idx];
            var obj = ResourceMgr.Instantiate(path) as TextAsset;
            if (obj == null)
                return;
            mp.RefleshScriptText(path, obj.text);
            
            Debug.Log(obj.text);
        }

        [EasyButtons.Button]
        public void NextScript()
        {
            if (maxCount <= 0)
                return;
            
            scpIdx = (scpIdx+1)% maxCount;
            ShowScriptByIdx(scpIdx);

            OnItemClick();
        }

        [EasyButtons.Button]
        public void BeforeScript()
        {
            if (maxCount <= 0)
                return;

            scpIdx = scpIdx - 1;
            if(scpIdx < 0)
            {
                scpIdx = maxCount - 1;
            }
            ShowScriptByIdx(scpIdx);
            
            OnItemClick();
        }

        [EasyButtons.Button]
        public void SpawnMods()
        {
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


            ShowScripts();

            OnItemClick();

        }

        private int itemIdx;
        void OnItemClick()
        {
            mp.OnItemSelect(itemIdx);
        }

        public void OnSelect(bool isSelect)
        {
            image.gameObject.SetActive(isSelect);
        }


        #region Save
        [EasyButtons.Button]
        public void SaveAsPrefabs()
        {
            foreach (string path in modInfo.gos)
            {
                var go = ResourceMgr.Instantiate(path) as GameObject;

                if (go != null)
                {
                    ScriptHelper.RefleshScripts(go);
                    CreatePrefabObj(go, ModImporter.Instance.prefabPath + "/" + name);
                }
                else
                {
                    Debug.Log("Save fail:" + path);
                }
            }
            //GameObject[] gos = SpawnMods();
            //ModDataMgr.Instance.CacheData(modInfo.path, gos);
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
                SaveScripts(path);
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, path + "/" + obj.name + ".prefab");
            }
            catch (System.Exception e)
            {
                //Debug.Log(e);
            }
#endif
        }
        #endregion

        public void SaveScripts(string exportPath)
        {
            string subFolder = "/Script";
            Directory.CreateDirectory(exportPath + subFolder);

            foreach (string scriptPath in modInfo.scripts)
            {
                var obj = ResourceMgr.Instantiate(scriptPath) as TextAsset;
                if (obj == null)
                    return;
                string[] words = scriptPath.Split('/');
                File.WriteAllText(exportPath + subFolder + "/" + words[words.Length - 1] + ".txt", obj.text);
            }
        }
    }
}
