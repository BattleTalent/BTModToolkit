using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class ReSkinPreviewTool : MonoBehaviour
    {
        [Tooltip("Skin that needs to be previewed")]
        public GameObject SkinModGameObject;

        [Tooltip("name of the root node of all meshes in the prefab,the same value as MeshRoot in ItemInfoConfig.")]
        public string MeshRoot;

        private GameObject previewObj;

        [EasyButtons.Button]
        public void Preview()
        {
            if (string.IsNullOrEmpty(MeshRoot))
            {
                Debug.LogError("Please enter MeshRoot,name of the root node of all meshes in the prefab.");
                return;
            }
            var target = transform.Find("CalibrationNode");

            if (target == null)
            {
                Debug.LogError("The preview tool is missing the ReSkin target.");
                return;
            }
            Transform boneRoot = target.transform.Find("root");

            GameObject targetObj = GameObject.Instantiate(target.gameObject);
            targetObj.SetActive(true);
            targetObj.transform.SetParent(transform);
            targetObj.transform.localPosition = Vector3.zero;

            var previewObj = GameObject.Instantiate(SkinModGameObject);
            previewObj.SetActive(true);
            previewObj.transform.SetParent(transform);
            previewObj.transform.localPosition = Vector3.zero;

            Transform skinRoot = previewObj.transform.Find(MeshRoot);
            if (skinRoot == null)
            {
                Debug.LogError("The MeshRoot:"+ MeshRoot +" node is not found in SkinMod.");
                return;
            }

            var helper = skinRoot.gameObject.AddComponent<ReSkinHelper>();
            helper.excludeList = new List<Transform>();
            previewObj.transform.position = boneRoot.position;
            helper.PasteBonesTransformTo(boneRoot, previewObj.transform);


            var oldSkin = targetObj.transform.Find("Warrior_Boy");
            if(oldSkin != null)
                oldSkin.gameObject.SetActive(false);

            skinRoot.transform.SetParent(targetObj.transform);
            helper.ReskinToBones(boneRoot);
            

            DestroyImmediate(previewObj);
        }

        [EasyButtons.Button]
        public void RemovePreview()
        {
            var target = transform.Find("CalibrationNode");
            Transform cur = null;
            for(int i = transform.childCount - 1; i >= 0; i--)
            {
                cur = transform.GetChild(i);
                if (cur != target)
                    DestroyImmediate(cur.gameObject);
            }
        }
    }
}