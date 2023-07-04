using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class RoleSlots : MonoBehaviour
    {
        public GameObject handSlotLeft;
        public GameObject handSlotRight;
        public GameObject shoulderSlotLeft;
        public GameObject shoulderSlotRight;

        [EasyButtons.Button]
        public void AutoGetHandTransform()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Please ensure that the fbxPrefab has an Animator " +
                    "and that the AnimationType option in the fbx file is Humanoid.");
                return;
            }

            // if (handposeControlLeft == null||handposeControlRight == null)
            // {
            //     Debug.LogError("Please assign values to handPoseControlLeft and handPoseControlLeft");
            //     return;
            // }

            if(!handSlotLeft) {
                Transform handTransformLeft = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                handSlotLeft = new GameObject("LWeapon Point");
                handSlotLeft.transform.parent = handTransformLeft;
                handSlotLeft.transform.localPosition = new Vector3(0,0,0);
                handSlotLeft.transform.localRotation = new Quaternion(0,0,0,0);
                handSlotLeft.transform.localScale = new Vector3(1,1,1);
            }

            if(!handSlotRight) {
                Transform handTransformRight = animator.GetBoneTransform(HumanBodyBones.RightHand);
                handSlotRight = new GameObject("RWeapon Point");
                handSlotRight.transform.parent = handTransformRight;
                handSlotRight.transform.localPosition = new Vector3(0,0,0);
                handSlotRight.transform.localRotation = new Quaternion(0,0,0,0);
                handSlotRight.transform.localScale = new Vector3(1,1,1);
            }

            if(!shoulderSlotLeft) {
                Transform shoulderTransformLeft = animator.GetBoneTransform(HumanBodyBones.UpperChest);
                shoulderSlotLeft = new GameObject("LWeapon Spine");
                shoulderSlotLeft.transform.parent = shoulderTransformLeft;
                shoulderSlotLeft.transform.localPosition = new Vector3(-5,0,-5);
                shoulderSlotLeft.transform.localRotation = new Quaternion(0,0,0,0);
                shoulderSlotLeft.transform.localScale = new Vector3(1,1,1);
            }

            if(!shoulderSlotRight) {
                Transform shoulderTransformRight = animator.GetBoneTransform(HumanBodyBones.UpperChest);
                shoulderSlotRight = new GameObject("RWeapon Spine");
                shoulderSlotRight.transform.parent = shoulderTransformRight;
                shoulderSlotRight.transform.localPosition = new Vector3(5,0,-5);
                shoulderSlotRight.transform.localRotation = new Quaternion(0,0,0,0);
                shoulderSlotRight.transform.localScale = new Vector3(1,1,1);
            }

            Debug.LogWarning("Please check if this is correct after use. If not, " +
                "please assign the handTrans of the handPoseControl manually.");
        }

        private void OnDrawGizmos()
        {
            var color = Color.green;
            color.a = 0.3f;
            Gizmos.color = color;

            if (handSlotLeft) {
                Gizmos.DrawWireSphere(handSlotLeft.transform.position, 0.1f);
            }

            if (handSlotRight) {
                Gizmos.DrawWireSphere(handSlotRight.transform.position, 0.1f);
            }

            if (shoulderSlotLeft) {
                Gizmos.DrawWireSphere(shoulderSlotLeft.transform.position, 0.1f);
            }

            if (shoulderSlotRight) {
                Gizmos.DrawWireSphere(shoulderSlotRight.transform.position, 0.1f);
            }
        }
    }

}
