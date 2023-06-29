#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace CrossLink
{
    public class TemplateWizardAsset : ScriptableObject
    {
        public enum ModType
        {
            Weapon,
            Song,
            Scene,
            Role,
            Skin
        }

        static TemplateWizardAsset wizard;

        public string newModFolderName = "";
        string previousNewModFolderName = "";

        public ModType selectedModType = ModType.Weapon;
        ModType previousselectedModType = ModType.Weapon;
        [ConditionalField("selectedModType", false, ModType.Weapon, ModType.Song)]public StoreItemInfo storeItemInfo;
        [ConditionalField("selectedModType", false, ModType.Scene)]public SceneModInfo sceneModInfo;
        [ConditionalField("selectedModType", false, ModType.Role)]public RoleModInfo roleModInfo;
        [ConditionalField("selectedModType", false, ModType.Skin)]public SkinInfo skinInfo;

        [EasyButtons.Button]
        void GenerateTemplate()
        {
            if (newModFolderName.Contains(" ")) {
                SetStatusMessage("Mod folder name should not contain spaces.", MessageType.Warning);
                return;
            }
            if (!char.IsUpper(newModFolderName[0])) {
                SetStatusMessage("Mod folder name should start with a capital.", MessageType.Warning);
                return;
            }

            if (AssetDatabase.IsValidFolder($"Assets/Build/{newModFolderName}")){
                SetStatusMessage("The mod already exists", MessageType.Warning);
                return;
            }

            string guid = AssetDatabase.CreateFolder("Assets/Build", newModFolderName);
            string newModFolderPath = AssetDatabase.GUIDToAssetPath(guid);

            if(selectedModType == ModType.Weapon) {
                GenerateWeaponTemplate(newModFolderPath);
            }

            if(selectedModType == ModType.Song) {
                GenerateSongTemplate(newModFolderPath);
            }

            if(selectedModType == ModType.Scene) {
                GenerateSceneTemplate(newModFolderPath);
            }

            if(selectedModType == ModType.Role) {
                GenerateRoleTemplate(newModFolderPath);
            }

            if(selectedModType == ModType.Skin) {
                GenerateSkinTemplate(newModFolderPath);
            }

            if(AddressableConfig.GetConfig().addressablePaths.Contains(newModFolderPath) == false) {
                AddressableConfig.GetConfig().addressablePaths.Add(newModFolderPath);
            }

            AddressableHelper.CreateAndRefreshAddressables();
            
            SetStatusMessage("Success!", MessageType.Info);
        }

        private ItemInfoConfig CreateItemInfoConfig(string newModFolderName)
        {
            ItemInfoConfig asset = CreateInstance<ItemInfoConfig>();
            AssetDatabase.CreateAsset(asset, $"Assets/Build/{newModFolderName}/Config/{newModFolderName}.asset");
            AssetDatabase.SaveAssets();

            return asset;
        }

        private void CreateWeaponPrefab(string newModFolderName)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Toolkit/Prefabs/RootWeaponNode.prefab");
            var instantiatedPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            PrefabUtility.SaveAsPrefabAssetAndConnect(instantiatedPrefab, $"Assets/Build/{newModFolderName}/Weapon/{newModFolderName}.prefab", InteractionMode.AutomatedAction);
            DestroyImmediate(instantiatedPrefab);
        }

        private void CreateIcon(string newModFolderName)
        {
            AssetDatabase.CopyAsset(
                $"Assets/Toolkit/TemplateWizard/Dummy/icon.png", 
                $"Assets/Build/{newModFolderName}/ICon/{newModFolderName}.png"
            );
        }

        private void CreateScene(string newModFolderName)
        {
            AssetDatabase.CopyAsset(
                $"Assets/Toolkit/TemplateWizard/Dummy/scene.unity", 
                $"Assets/Build/{newModFolderName}/Scene/{newModFolderName}.unity"
            );
        }

        static public TemplateWizardAsset GetWizard()
        {
            wizard = Resources.Load("TemplateWizard") as TemplateWizardAsset;
            return wizard;
        }

        [MenuItem("Tools/Template Wizard", false, 10)]
        static void SelectAddressablesConfig()
        {
            Selection.activeObject = GetWizard();
        }

        void OnValidate() {
            if(previousNewModFolderName != newModFolderName) {
                previousNewModFolderName = newModFolderName;
                OnImportantChange();
            }

            if(previousselectedModType != selectedModType) {
                previousselectedModType = selectedModType;
                OnImportantChange();
            }
        }
 
        void OnImportantChange() {
            if(selectedModType == ModType.Weapon){
                storeItemInfo.addStoreItemName = AddressableConfig.GetConfig().GetPrefix() + newModFolderName;
                storeItemInfo.itemType = ItemInfoConfig.ItemType.Weapon;
                return;
            }
            if(selectedModType == ModType.Song){
                storeItemInfo.addStoreItemName = AddressableConfig.GetConfig().GetPrefix() + newModFolderName;
                storeItemInfo.itemType = ItemInfoConfig.ItemType.Music;
                return;
            }
            if(selectedModType == ModType.Scene){
                sceneModInfo.sceneName = AddressableConfig.GetConfig().GetPrefix() + newModFolderName;
                return;
            }
            if(selectedModType == ModType.Role){
                roleModInfo.roleName = AddressableConfig.GetConfig().GetPrefix() + newModFolderName;
                return;
            }
            if(selectedModType == ModType.Skin){
                skinInfo.skinName = AddressableConfig.GetConfig().GetPrefix() + newModFolderName;
                return;
            }
        }

        void SetStatusMessage(string statusMessage, MessageType statusMessageType) {
            if(statusMessageType == MessageType.Info){
                Debug.Log(statusMessage);
            } else {
                Debug.LogError(statusMessage);
            }
        }

        void GenerateWeaponTemplate(string newModFolderPath){ 
            AssetDatabase.CreateFolder(newModFolderPath, "ICon");
            AssetDatabase.CreateFolder(newModFolderPath, "Config");
            AssetDatabase.CreateFolder(newModFolderPath, "Weapon");

            CreateIcon(newModFolderName);
            CreateWeaponPrefab(newModFolderName);
            
            var itemInfoConfig = CreateItemInfoConfig(newModFolderName);    
            itemInfoConfig.storeItemInfo = new StoreItemInfo[1];
            itemInfoConfig.storeItemInfo[0] = storeItemInfo;
        }

        void GenerateSongTemplate(string newModFolderPath){ 
            AssetDatabase.CreateFolder(newModFolderPath, "ICon");
            AssetDatabase.CreateFolder(newModFolderPath, "Config");
            AssetDatabase.CreateFolder(newModFolderPath, "Audio");

            CreateIcon(newModFolderName);

            var itemInfoConfig = CreateItemInfoConfig(newModFolderName);    
            itemInfoConfig.storeItemInfo = new StoreItemInfo[1];
            itemInfoConfig.storeItemInfo[0] = storeItemInfo;
        }

        void GenerateSceneTemplate(string newModFolderPath){ 
            AssetDatabase.CreateFolder(newModFolderPath, "ICon");
            AssetDatabase.CreateFolder(newModFolderPath, "Config");
            AssetDatabase.CreateFolder(newModFolderPath, "Scene");

            CreateIcon(newModFolderName);
            CreateScene(newModFolderName);

            var itemInfoConfig = CreateItemInfoConfig(newModFolderName);    
            itemInfoConfig.sceneModInfo = new SceneModInfo[1];
            itemInfoConfig.sceneModInfo[0] = sceneModInfo;
        }

        void GenerateRoleTemplate(string newModFolderPath){ 
            AssetDatabase.CreateFolder(newModFolderPath, "ICon");
            AssetDatabase.CreateFolder(newModFolderPath, "Config");
            AssetDatabase.CreateFolder(newModFolderPath, "Audio");
            AssetDatabase.CreateFolder(newModFolderPath, "Role");

            CreateIcon(newModFolderName);

            var itemInfoConfig = CreateItemInfoConfig(newModFolderName);    
            itemInfoConfig.roleModInfo = new RoleModInfo[1];
            itemInfoConfig.roleModInfo[0] = roleModInfo;
        }

        void GenerateSkinTemplate(string newModFolderPath){ 
            AssetDatabase.CreateFolder(newModFolderPath, "ICon");
            AssetDatabase.CreateFolder(newModFolderPath, "Config");
            AssetDatabase.CreateFolder(newModFolderPath, "Skin");

            CreateIcon(newModFolderName);

            var itemInfoConfig = CreateItemInfoConfig(newModFolderName);    
            itemInfoConfig.skinInfo = new SkinInfo[1];
            itemInfoConfig.skinInfo[0] = skinInfo;
        }
    }
}


#endif