using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class ActionEditor: MonoBehaviour
    {
        public Animator animator;
        public Actor actor;
        [Tooltip("AIPreset containing the exported root-motion layouts used by ActionData previews")]
        public AIPreset rootMotionPreset;
        public Dictionary<string, Transform> bones;

        Vector3 previewOriginPosition;
        Quaternion previewOriginRotation;
        bool previewOriginCaptured;

        private void Reset()
        {
            animator = GetComponent<Animator>();

            actor = GetComponent<Actor>();
            if (actor == null)
            {
                actor = gameObject.AddComponent<Actor>();
            }
            actor.editor = this;
        }

        private void Start()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            actor.editor = this;

            SetupBones();
            CapturePreviewOrigin();
        }

        void CapturePreviewOrigin()
        {
            previewOriginPosition = transform.position;
            previewOriginRotation = transform.rotation;
            previewOriginCaptured = true;
        }

        void SetupBones()
        {
            bones = new Dictionary<string, Transform>();
            bones.Add(RagdollBoneInfo.Pelvis, animator.GetBoneTransform(HumanBodyBones.Hips));
            bones.Add(RagdollBoneInfo.Spine, animator.GetBoneTransform(HumanBodyBones.Chest));
            bones.Add(RagdollBoneInfo.Head, animator.GetBoneTransform(HumanBodyBones.Head));

            bones.Add(RagdollBoneInfo.LThigh, animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg));
            bones.Add(RagdollBoneInfo.LCalf, animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
            bones.Add(RagdollBoneInfo.LFoot, animator.GetBoneTransform(HumanBodyBones.LeftFoot));

            bones.Add(RagdollBoneInfo.RThigh, animator.GetBoneTransform(HumanBodyBones.RightUpperLeg));
            bones.Add(RagdollBoneInfo.RCalf, animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
            bones.Add(RagdollBoneInfo.RFoot, animator.GetBoneTransform(HumanBodyBones.RightFoot));

            bones.Add(RagdollBoneInfo.LUpArm, animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
            bones.Add(RagdollBoneInfo.LForeArm, animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
            bones.Add(RagdollBoneInfo.LHand, animator.GetBoneTransform(HumanBodyBones.LeftHand));

            bones.Add(RagdollBoneInfo.RUpArm, animator.GetBoneTransform(HumanBodyBones.RightUpperArm));
            bones.Add(RagdollBoneInfo.RForeArm, animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
            bones.Add(RagdollBoneInfo.RHand, animator.GetBoneTransform(HumanBodyBones.RightHand));

            var roleSlots = GetComponent<RoleSlots>();
            if (roleSlots)
            {
                bones.Add(RagdollBoneInfo.LWeapon, roleSlots.handSlotLeft.transform);
                bones.Add(RagdollBoneInfo.RWeapon, roleSlots.handSlotRight.transform);
            }
        }

        public Transform GetBone(string name)
        {
            if (bones.ContainsKey(name))
                return bones[name];
            else
                return null;
        }

        public Transform GetWeaponTrans(string name)
        {
            if (bones.ContainsKey(name))
                return bones[name];
            else
                return null;
        }

        [EasyButtons.Button]
        public void PlayAction(ActionData actionData)
        {
            if (!previewOriginCaptured)
            {
                CapturePreviewOrigin();
            }

            actor.StopAction();
            transform.SetPositionAndRotation(previewOriginPosition, previewOriginRotation);
            actor.PlayAction(actionData);
        }

        public AnimLayoutDataItem GetRootMotionLayout(string rootMotionName)
        {
            if (rootMotionPreset == null)
            {
                Debug.LogWarning($"ActionEditor cannot preview root motion '{rootMotionName}': no AIPreset is assigned.", this);
                return null;
            }

            var layouts = rootMotionPreset.animLayoutDatas;
            if (layouts != null)
            {
                for (int i = 0; i < layouts.Length; i++)
                {
                    if (layouts[i] != null && layouts[i].Name == rootMotionName)
                    {
                        return layouts[i];
                    }
                }
            }

            Debug.LogWarning($"ActionEditor cannot preview root motion '{rootMotionName}': the assigned AIPreset does not contain that layout.", this);
            return null;
        }

        public void SetLayer(int layer, float weight)
        {
            if (animator.layerCount > layer)
                animator.SetLayerWeight(layer, weight);
        }
        public void SetAnimSpeed(float s)
        {
            animator.SetFloat("Speed", s);
        }
        public bool LayerIsOpened(int layer = 1)
        {
            return animator.layerCount > 1 && animator.GetLayerWeight(layer) > 0;
        }

    }

}

