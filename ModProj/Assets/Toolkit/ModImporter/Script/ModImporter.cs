using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
            EditorUtility.SetDirty(this);
#endif
        }
    }
}
