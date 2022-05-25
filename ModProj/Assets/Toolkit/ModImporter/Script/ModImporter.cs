using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CrossLink
{
    public class ModImporter : MonoBehaviour
    {
        public static ModImporter Instance = new ModImporter();

        public string prefabPath = "Assets/Resources/GenPrefab";

        public string loadModPath;

        public static ModPanel modPanel;

        private string defaultPath = "C:/Users[username]/AppData/LocalLow/CrossLink/BattleTalent/Mods";

        void Start()
        {
            modPanel = GetComponentInChildren<ModPanel>();
            ImportMods();
        }

        public void ImportMods()
        {
            ModManager.Instance.SetLoadModPath(loadModPath == defaultPath ? "" : loadModPath);
            ModManager.Instance.Init();
        }


        [EasyButtons.Button]
        public void RefleshView()
        {
            modPanel.UpdateScrollView(ModManager.Instance.mods);
        }

        [EasyButtons.Button]
        public void ResetLoadModPath()
        {
            loadModPath = defaultPath;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}