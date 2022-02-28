using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CrossLink
{
    public class ModItem : MonoBehaviour
    {
        public Text name;
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

            RefleshName(modInfo.path);
        }

        void RefleshName(string path)
        {
            int idx = path.LastIndexOf("\\Mods\\");
            path = path.Substring(idx, path.Length - idx);
            path = path.Replace("\\Mods\\" ,"");
            idx = path.IndexOf("\\");
            path = path.Substring(0, idx);

            name.text = path;
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
            foreach(string path in modInfo.gos)
            {
                var weaponGO = ResourceMgr.Instantiate(path) as GameObject;

                if (weaponGO == null)
                    return;

                weaponGO.transform.position = Vector3.zero;
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
    }
}
