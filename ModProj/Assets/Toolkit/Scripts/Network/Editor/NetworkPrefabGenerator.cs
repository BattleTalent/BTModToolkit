using Mirror;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;
using UnityEditor.SceneManagement;

namespace CrossLink.Network
{
    public class NetworkPrefabGenerator : MonoBehaviour
    {
        private static readonly string buildPath = "Assets/Build";

        private static readonly string weaponGenPath = "WeaponState";

        [MenuItem("Network/Tools/GenerateSelectedNetworkWeapons")]
        static void GenerateSelectedWeapons()
        {
            var objs = Selection.gameObjects;
            var ite = objs.GetEnumerator();

            while (ite.MoveNext())
            {
                var obj = ite.Current as GameObject;
                var assetPath = AssetDatabase.GetAssetPath(obj);

                if (!assetPath.Contains(buildPath))
                {
                    Debug.LogError($"Selected obj: {obj} is not in {buildPath}");
                    continue;
                }
                if (!assetPath.Contains("Weapon/"))
                {
                    Debug.LogError($"Selected obj: {obj} is not in the Weapon folder.");
                    continue;
                }
                string modPath = assetPath.Substring(0, assetPath.LastIndexOf("/", assetPath.LastIndexOf("/") - 1));
                string statePath = Path.Combine(modPath, weaponGenPath);
                if (!Directory.Exists(statePath))
                {
                    Directory.CreateDirectory(statePath);
                }

                var genPath = statePath + "/" + "Network" + obj.name + ".prefab";
                if (File.Exists(genPath))
                {
                    File.Delete(genPath);
                }
                bool needSyncVelocity = obj.GetComponent<InteractBase>().stateLibrary.needSyncVelocity;

                GenerateWeapon(genPath, obj.name, needSyncVelocity);
            }

            AssetDatabase.SaveAssets();
        }


        static void GenerateWeapon(string genPath, string prefabName, bool needSyncVelocity, bool sceneEidtorSpawn = false)
        {
            Debug.Log($"Gen Network weapon prefab: {genPath}");

            var newGameObject = new GameObject();

            GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(newGameObject, genPath, InteractionMode.AutomatedAction);

            prefab.AddComponent<NetworkIdentity>();
            NetworkTransformFix networkTransform = prefab.AddComponent<NetworkTransformFix>();
            networkTransform.compressRotation = true;
            networkTransform.coordinateSpace = CoordinateSpace.World;
            networkTransform.enabled = false; // set sync comp disabed first
            networkTransform.target = prefab.transform;
            NetworkRigidbodySyncBase networkRB = prefab.AddComponent<NetworkRigidbodySyncBase>();
            networkRB.compressRotation = true;
            networkRB.syncLocalTrans = false;
            networkRB.syncVelocity = needSyncVelocity;
            networkRB.syncAngularVelocity = needSyncVelocity;
            networkRB.enabled = true;
            NetworkIBProperties networkIBProperites = prefab.AddComponent<NetworkIBProperties>();
            NetworkInteractTrigger networkIT = prefab.AddComponent<NetworkInteractTrigger>();

            NetworkInteractBase networkIB = prefab.AddComponent<NetworkInteractBase>();
            networkIB.ibName = AddressableConfig.GetConfig().GetPrefix() + prefabName;
            networkIB.networkTransform = networkTransform;
            networkIB.networkRB = networkRB;
            networkIB.networkIBProperties = networkIBProperites;
            networkIB.networkIT = networkIT;
            networkIB.networkIT.networkIB = networkIB;
            networkIB.isSceneProp = sceneEidtorSpawn;

            NetworkIBStateX networkIBState = prefab.AddComponent<NetworkIBStateX>();
            networkIBState.networkIB = networkIB;
            networkIB.networkIBState = networkIBState;

            var networkStabObject = prefab.AddComponent<NetworkStabObject>();
            networkStabObject.networkIB = networkIB;
            networkIB.networkStabObject = networkStabObject;

            networkTransform.syncDirection = SyncDirection.ServerToClient;
            networkRB.syncDirection = SyncDirection.ServerToClient;
            networkIBState.syncDirection = SyncDirection.ServerToClient;

            DestroyImmediate(newGameObject);
        }

        //[MenuItem("Network/Tools/GenerateSelectedNetworkFlyObjects")]
        //static void GenerateSelectedFlyObjects()
        //{
        //    var objs = Selection.gameObjects;
        //    var ite = objs.GetEnumerator();

        //    while (ite.MoveNext())
        //    {
        //        var obj = ite.Current as GameObject;
        //        var assetPath = AssetDatabase.GetAssetPath(obj);

        //        if (!assetPath.Contains(flyObjPath))
        //        {
        //            Debug.LogError($"Selected obj: {obj} is not a flyobject prefab~");
        //            continue;
        //        }

        //        var genPath = flyObjGenPath + "/" + "Network" + obj.name + ".prefab";
        //        GenerateFlyObject(genPath, obj.name);
        //    }

        //    AssetDatabase.SaveAssets();
        //}

        //static void GenerateFlyObject(string genPath, string prefabName)
        //{
        //    Debug.Log($"Gen Network flyObject prefab: {genPath}");

        //    var newGameObject = new GameObject();

        //    GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(newGameObject, genPath, InteractionMode.AutomatedAction);

        //    prefab.AddComponent<NetworkIdentity>();
        //    NetworkRigidbodySyncBase networkRB = prefab.AddComponent<NetworkRigidbodySyncBase>();
        //    networkRB.compressRotation = true;
        //    networkRB.syncLocalTrans = false;
        //    networkRB.syncVelocity = false;
        //    networkRB.syncAngularVelocity = false;
        //    networkRB.enabled = true;

        //    var typeSuffix = prefabName.Replace("_", "");
        //    Type t = Type.GetType($"CrossLink.Network.Network{typeSuffix}State, Assembly-CSharp");
        //    NetworkFlyObjectBase networkFOState = t == null ? prefab.AddComponent<NetworkFlyObjectBase>() : (prefab.AddComponent(t) as NetworkFlyObjectBase);
        //    networkFOState.networkRB = networkRB;
            
        //    networkRB.syncDirection = SyncDirection.ServerToClient;
        //    networkFOState.syncDirection = SyncDirection.ServerToClient;

        //    DestroyImmediate(newGameObject);
        //}
    }
}