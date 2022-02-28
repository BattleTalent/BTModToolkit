using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class ModImporter : MonoBehaviour
    {
        public static ModImporter Instance = new ModImporter();

        public static ModPanel modPanel;
        void Start()
        {
            modPanel = GetComponentInChildren<ModPanel>();
            ImportMods();
        }

        public void ImportMods()
        {
            ModManager.Instance.Init();
        }


        [EasyButtons.Button]
        public void RefleshView()
        {
            modPanel.UpdateScrollView(ModManager.Instance.mods);
        }
    }
}