using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CrossLink
{
    public class ModPanel : MonoBehaviour
    {
        public Transform GridTrans;

        public Text contentText;
        public Text titleText;

        public GameObject modItemPref;

        private int curItemIdx = -1;
        private List<ModItem> itemList = new List<ModItem>();

        public void Awake()
        {
            if (modItemPref != null)
            {
                modItemPref.SetActive(false);
            }
        }

        public void UpdateScrollView(List<ModInfo> list)
        {
            ClearScrollView();

            if (list == null)
                return;

            foreach(ModInfo info in list)
            {
                AddModItem(info);
            }
        }

        void AddModItem(ModInfo info)
        {
            if (modItemPref == null || GridTrans == null)
            {
                Debug.LogWarning("ModPanel item prefab or grid transform is missing.");
                return;
            }

            var item = Instantiate(modItemPref, GridTrans);
            item.SetActive(true);

            var modItem = item.GetComponent<ModItem>();
            if (modItem == null)
            {
                Debug.LogWarning("Mod item prefab is missing ModItem component.");
                Destroy(item);
                return;
            }

            itemList.Add(modItem);
            modItem.SetData(info, this, itemList.Count - 1);
        }

        public void RefleshScriptText(string title, string content)
        {
            if (contentText == null || titleText == null)
            {
                Debug.LogWarning("ModPanel script title or content text is missing.");
                return;
            }

            contentText.text = content;
            titleText.text = title;
        }

        public void OnItemSelect(int idx)
        {
            if (idx < 0 || idx >= itemList.Count)
            {
                Debug.LogWarning("ModPanel item index out of range:" + idx);
                return;
            }

            if(curItemIdx >= 0 && curItemIdx < itemList.Count)
            {
                itemList[curItemIdx].OnSelect(false);
            }

            curItemIdx = idx;
            itemList[idx].OnSelect(true);
        }

        private void ClearScrollView()
        {
            for (int i = 0; i < itemList.Count; ++i)
            {
                if (itemList[i] != null)
                {
                    Destroy(itemList[i].gameObject);
                }
            }

            itemList.Clear();
            curItemIdx = -1;
        }
    }
}
