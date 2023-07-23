using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class RuntimeAvatarBuilder : MonoBehaviour
    {
        public GameObject fbxPrefab; // The FBX model prefab to spawn

        [Header("——————Settings——————")]
        public Vector3 handSlotLocalPositionLeft;
        public Quaternion handSlotLocalRotationLeft;
        public Vector3 handSlotLocalPositionRight;
        public Quaternion handSlotLocalRotationRight;
        public Vector3 backSlotsLocalPosition; 
        public Quaternion backSlotsLocalRotation;
        public Vector3 wristToPalmAxisLeft;
        public Vector3 wristToPalmAxisRight;
        public Vector3 palmUpAxisLeft;
        public Vector3 palmUpAxisRight;
        public HandPoseControl handposeControlLeft;
        public HandPoseControl handposeControlRight;
        public bool autoConfigHandPoseControl = false;
        public bool correctFingersOrder = false;

        public Transform handSlotLeft;
        public Transform handSlotRight;
        public Transform backSlots;

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

            PutSlots();

            AutoCorrectRenderer();

            Debug.LogWarning("Please check if this is correct after use. If not, " +
                "please assign the handTrans of the handPoseControl manually.");
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
                handSlotLeft.SetParent(handposeControlLeft.handTrans);
                handSlotLeft.localPosition = Vector3.zero;
            }
            if (handSlotRight)
            {
                handSlotRight.SetParent(handposeControlRight.handTrans);
                handSlotRight.localPosition = Vector3.zero;
            }
            if (backSlots)
            {
                backSlots.SetParent(animator.GetBoneTransform(HumanBodyBones.Chest));
                backSlots.localPosition = Vector3.zero;
            }
        }

        [EasyButtons.Button]
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
        public void AddSlotTool()
        {
            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "add slot tool");

            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Toolkit/AvatarBuilder/SlotTool.prefab");
            GameObject slots = Instantiate(prefab);

            if(!handSlotLeft){
                handSlotLeft = slots.transform.Find("HandSlot_Left");
            }

            if(!handSlotRight){
                handSlotRight = slots.transform.Find("HandSlot_Right");
            }

            if(!backSlots){
                backSlots = slots.transform.Find("BackSlots");
            }

            PutSlots();

            DestroyImmediate(slots);

            handSlotLeft.localPosition = handSlotLocalPositionLeft;
            handSlotLeft.localRotation = handSlotLocalRotationLeft;
            handSlotRight.localPosition = handSlotLocalPositionRight;
            handSlotRight.localRotation = handSlotLocalRotationRight;
            backSlots.localPosition = backSlotsLocalPosition;
            backSlots.localRotation = backSlotsLocalRotation;
        }

        [EasyButtons.Button]
        public void ApplySlotToolData()
        {
            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "apply slot tool data");


            if (handSlotLeft)
            {
                handSlotLocalRotationLeft = handSlotLeft.localRotation;
                handSlotLocalPositionLeft = handSlotLeft.localPosition;
            }
            if (handSlotRight)
            {
                handSlotLocalPositionRight = handSlotRight.localPosition;
                handSlotLocalRotationRight = handSlotRight.localRotation;
            }
            if (backSlots)
            {
                backSlotsLocalPosition = backSlots.localPosition;
                backSlotsLocalRotation = backSlots.localRotation;
            }
        }

        [EasyButtons.Button]
        public void RemoveSlotTool()
        {
            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "remove slot tool");
            if (handSlotLeft)
            {
                DestroyImmediate(handSlotLeft.gameObject);
                handSlotLeft = null;
            }
            if (handSlotRight)
            {
                DestroyImmediate(handSlotRight.gameObject);
                handSlotRight = null;
            }
            if (backSlots)
            {
                DestroyImmediate(backSlots.gameObject);
                backSlots = null;
            }
        }
#endif
    }
}
