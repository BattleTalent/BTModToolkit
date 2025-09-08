using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class ActionEditor: MonoBehaviour
    {
        public Animator animator;
        public Actor actor;
        public Dictionary<string, Transform> bones;

        private void Reset()
        {
            animator = GetComponent<Animator>();

            actor = GetComponent<Actor>();
            if (actor == null)
            {
                actor = gameObject.AddComponent<Actor>();
            }
        }

        private void Start()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            actor.editor = this;

            SetupBones();
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
            actor.PlayAction(actionData);
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

