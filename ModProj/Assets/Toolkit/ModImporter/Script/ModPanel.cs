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

        private int curItemIdx;
        private List<ModItem> itemList;

        public void Start()
        {
            modItemPref.SetActive(false);

            itemList = new List<ModItem>();
        }

        public void UpdateScrollView(List<ModInfo> list)
        {
            foreach(ModInfo info in list)
            {
                AddModItem(info);
            }
        }

        void AddModItem(ModInfo info)
        {

            var item = Instantiate(modItemPref, GridTrans);
            item.SetActive(true);

            var modItem = item.GetComponent<ModItem>();
            itemList.Add(modItem);
            modItem.SetData(info, this, itemList.Count - 1);
        }

        public void RefleshScriptText(string title, string content)
        {
            contentText.text = content;
            titleText.text = title;
        }

        public void OnItemSelect(int idx)
        {
            if(curItemIdx >= 0)
            {
                itemList[curItemIdx].OnSelect(false);
            }

            curItemIdx = idx;
            itemList[idx].OnSelect(true);
        }
    }
}
