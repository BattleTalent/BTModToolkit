using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrossLink
{

    public class RagdollBoneInfo : MonoBehaviour
    {
        public const int RIGHT_HAND = 0;
        public const int LEFT_HAND = 1;

        public const string Pelvis = "Pelvis";
        public const string Head = "Head";
        public const string Spine = "Spine2";
        public const string Chest = "Chest";
        public const string UpArm = "UpperArm";
        public const string ForeArm = "ForeArm";
        public const string Hand = "Hand";
        public const string Thigh = "Thigh";
        public const string Calf = "Calf";
        public const string Foot = "Foot";
        public const string Arm = "Arm";

        public const string RUpArm = "RightUpperArm";
        public const string LUpArm = "LeftUpperArm";

        public const string RForeArm = "RightForeArm";
        public const string LForeArm = "LeftForeArm";

        public const string RHand = "RightHand";
        public const string LHand = "LeftHand";

        public const string RThigh = "RightThigh";
        public const string LThigh = "LeftThigh";

        public const string RCalf = "RightCalf";
        public const string LCalf = "LeftCalf";

        public const string RFoot = "RightFoot";
        public const string LFoot = "LeftFoot";

        public const string RWeapon = "RWeapon";
        public const string LWeapon = "LWeapon";

        static public string[] BasicCutableBones = new string[] {
            Head, Spine,
            RUpArm, RForeArm, LUpArm, LForeArm,
            RThigh, RCalf, LThigh, LCalf
        };

        public class MuscleDirection
        {
            public Vector3 forward;
            public Vector3 right;
            public Vector3 parent;
        }
    }
}