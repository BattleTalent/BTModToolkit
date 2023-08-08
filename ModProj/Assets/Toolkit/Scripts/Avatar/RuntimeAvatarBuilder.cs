using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrossLink
{
    public class RuntimeAvatarBuilder : MonoBehaviour
    {
        public GameObject fbxPrefab; // The FBX model prefab to spawn

        [Header("——————Settings——————")]
        public Vector3 wristToPalmAxisLeft;
        public Vector3 wristToPalmAxisRight;
        public Vector3 palmUpAxisLeft;
        public Vector3 palmUpAxisRight;
        public HandPoseControl handposeControlLeft;
        public HandPoseControl handposeControlRight;
       // public bool correctFingersOrder = false;

        [Header("CharacterHeight recommanded range: 1~8")]
        [Range(0.1f, 10.0f)]
        public float characterHeight = 2.0f;
        public float cameraForwardOffset = 0.2f;
        [Range(0.0f, 1.0f)]
        public float naturalArmRotation = 0.5f;

        public Transform handSlotLeft;
        public Transform handSlotRight;
        public Transform backSlots;
        public Transform backSlotLeft;
        public Transform backSlotRight;
        public Transform sideSlotLeft;
        public Transform sideSlotRight;
        public Transform sideItemSlotLeft;
        public Transform sideItemSlotRight;


#if UNITY_EDITOR
        [EasyButtons.Button]
        public void AutoConfigBuilder()
        {
            if (fbxPrefab == null)
            {
                Debug.LogError("Please assign a value to \"FbxPrefab\".");
                return;
            }

            var animator = fbxPrefab.transform.root.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("Please ensure that the fbxPrefab has an Animator " +
                    "and that the AnimationType option in the fbx file is Humanoid.");
                return;
            }

            if (handposeControlLeft == null||handposeControlRight == null)
            {
                Debug.LogError("Please assign values to handPoseControlLeft and handPoseControlLeft");
                return;
            }

            handposeControlLeft.handTrans = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            handposeControlRight.handTrans = animator.GetBoneTransform(HumanBodyBones.RightHand);

            //check model
            if (animator.GetBoneTransform(HumanBodyBones.Hips) == null 
                || animator.GetBoneTransform(HumanBodyBones.Spine) == null
                || animator.GetBoneTransform(HumanBodyBones.Chest) == null
                || animator.GetBoneTransform(HumanBodyBones.Head) == null
                || animator.GetBoneTransform(HumanBodyBones.LeftUpperArm) == null
                || animator.GetBoneTransform(HumanBodyBones.LeftLowerArm) == null
                || animator.GetBoneTransform(HumanBodyBones.LeftHand) == null
                || animator.GetBoneTransform(HumanBodyBones.RightUpperArm) == null
                || animator.GetBoneTransform(HumanBodyBones.RightLowerArm) == null
                || animator.GetBoneTransform(HumanBodyBones.RightHand) == null
                || animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg) == null
                || animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg) == null
                || animator.GetBoneTransform(HumanBodyBones.LeftFoot) == null
                || animator.GetBoneTransform(HumanBodyBones.RightUpperLeg) == null
                || animator.GetBoneTransform(HumanBodyBones.RightLowerLeg) == null
                || animator.GetBoneTransform(HumanBodyBones.RightFoot) == null)
            {
                Debug.LogError("Some Bone has not been assigned, please check it by clicking \"ConfigureAvatar\" in the model.");
            }

            UnityEditor.EditorUtility.SetDirty(this);

            var type = UnityEditor.PrefabUtility.GetPrefabAssetType(this.gameObject);
            var status = UnityEditor.PrefabUtility.GetPrefabInstanceStatus(this.gameObject);
            if (!(type == UnityEditor.PrefabAssetType.NotAPrefab || status == UnityEditor.PrefabInstanceStatus.NotAPrefab))
                UnityEditor.PrefabUtility.UnpackPrefabInstance(this.gameObject, UnityEditor.PrefabUnpackMode.OutermostRoot, UnityEditor.InteractionMode.AutomatedAction);

            GenerateSlots(animator);
            PutSlots();

            AutoCorrectRenderer();


            Debug.LogWarning("Please check if this is correct after use. If not, " +
                "please assign the handTrans of the handPoseControl manually.");
        }

        void GenerateSlots(Animator animator)
        {
            var slotToolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Toolkit/AvatarBuilder/SlotTool.prefab");
            var instantiatedPrefab = Instantiate(slotToolPrefab, fbxPrefab.transform) as GameObject;
        
            if(!handSlotLeft) {
                Transform handTransformLeft = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                handSlotLeft = instantiatedPrefab.transform.Find("HandSlot_Left").transform;
                handSlotLeft.transform.parent = handTransformLeft;
                handSlotLeft.transform.localPosition = new Vector3(0,0,0);
                handSlotLeft.transform.localRotation = new Quaternion(0,0,0,0);
                handSlotLeft.transform.localScale = new Vector3(1,1,1);
            }

            if(!handSlotRight) {
                Transform handTransformRight = animator.GetBoneTransform(HumanBodyBones.RightHand);
                handSlotRight = instantiatedPrefab.transform.Find("HandSlot_Right").transform;
                handSlotRight.parent = handTransformRight;
                handSlotRight.localPosition = new Vector3(0,0,0);
                handSlotRight.localRotation = new Quaternion(0,0,0,0);
                handSlotRight.localScale = new Vector3(1,1,1);
            }

            if(!backSlots) {
                Transform shoulderTransformLeft = animator.GetBoneTransform(HumanBodyBones.Chest);
                backSlots = instantiatedPrefab.transform.Find("BackSlots").transform;
                backSlots.parent = shoulderTransformLeft;
                backSlots.localPosition = new Vector3(0,0,0);
                backSlots.localRotation = new Quaternion(0,0,0,0);
                backSlots.localScale = new Vector3(1,1,1);
                
                backSlotLeft = backSlots.Find("LeftBackSlot").transform;
                backSlotRight = backSlots.Find("RightBackSlot").transform;
                sideSlotLeft = backSlots.Find("LeftSideSlot").transform;
                sideSlotRight = backSlots.Find("RightSideSlot").transform;
                sideItemSlotLeft = backSlots.Find("LeftSideSlotItem").transform;
                sideItemSlotRight = backSlots.Find("RightSideSlotItem").transform; 
            }

            DestroyImmediate(instantiatedPrefab);
        }

        public void PutSlots()
        {
            var animator = fbxPrefab.transform.root.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("Please ensure that the fbxPrefab has an Animator " +
                    "and that the AnimationType option in the fbx file is Humanoid.");
                return;
            }

            if (handSlotLeft)
            {
                handSlotLeft.SetParent(transform);
                handSlotLeft.localScale = Vector3.one;
                handSlotLeft.SetParent(handposeControlLeft.handTrans);
            }
            if (handSlotRight)
            {
                handSlotRight.SetParent(transform);
                handSlotRight.localScale = Vector3.one;
                handSlotRight.SetParent(handposeControlRight.handTrans);
            }
            if (backSlots)
            {
                backSlots.SetParent(transform);
                backSlots.localScale = Vector3.one;
                backSlots.SetParent(animator.GetBoneTransform(HumanBodyBones.Chest));
            }
        }

        public void AutoCorrectRenderer()
        {
            if (fbxPrefab == null)
            {
                return;
            }

            var renderers = fbxPrefab.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.updateWhenOffscreen = true;
            }
        }

        [EasyButtons.Button]
        public void SetSlotTool()
        {
            if (fbxPrefab == null)
            {
                Debug.LogError("Please assign a value to \"FbxPrefab\".");
                return;
            }

            var animator = fbxPrefab.transform.root.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("Please ensure that the fbxPrefab has an Animator " +
                    "and that the AnimationType option in the fbx file is Humanoid.");
                return;
            }

            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "add slot tool");

            PutSlots();
        }
#endif
    }
}
