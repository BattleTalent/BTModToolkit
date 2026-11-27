using UnityEngine;
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
        private int maxCount = 0;

        public void SetData(ModInfo info, ModPanel modPanel, int idx)
        {
            modInfo = info;
            mp = modPanel;
            maxCount = modInfo == null || modInfo.scripts == null ? 0 : modInfo.scripts.Count;

            itemIdx = idx;
            name = ModImporter.GetNameFromModPath(modInfo == null ? null : modInfo.path);
            if (nameText != null)
            {
                nameText.text = name;
            }
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
            if (modInfo == null || modInfo.scripts == null)
            {
                Debug.LogWarning("ModItem has no mod data, cannot show script.");
                return;
            }

            if (idx < 0 || idx >= modInfo.scripts.Count)
            {
                Debug.LogWarning("ModItem script index out of range:" + idx);
                return;
            }

            string path = modInfo.scripts[idx];
            var obj = ResourceMgr.Instantiate(path) as TextAsset;
            if (obj == null)
                return;

            if (mp == null)
            {
                Debug.LogWarning("ModPanel is missing, cannot show script text.");
                return;
            }

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
            if (ModImporter.Instance == null)
            {
                Debug.LogWarning("ModImporter is missing, cannot spawn prefabs.");
                return;
            }

            ModImporter.Instance.SpawnMods(modInfo);
            if (modInfo == null || modInfo.gos == null)
                return;

            ShowScripts();

            OnItemClick();

        }

        private int itemIdx;
        void OnItemClick()
        {
            if (mp == null)
            {
                Debug.LogWarning("ModPanel is missing, cannot select ModItem.");
                return;
            }

            mp.OnItemSelect(itemIdx);
        }

        public void OnSelect(bool isSelect)
        {
            if (image != null)
            {
                image.gameObject.SetActive(isSelect);
            }
        }


        #region Save
        [EasyButtons.Button]
        public void SaveAsPrefabs()
        {
            if (ModImporter.Instance == null)
            {
                Debug.LogWarning("ModImporter is missing, cannot save prefabs.");
                return;
            }

            ModImporter.Instance.SaveAsPrefabs(modInfo);
        }
        #endregion
    }
}
