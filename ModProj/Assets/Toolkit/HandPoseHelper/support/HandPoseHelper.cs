using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CrossLink
{
    //[ExecuteInEditMode]
    public class HandPoseHelper : MonoBehaviour
    {
        public AttachPoint handAttach;

        private GameObject fitOffset_l;
        private GameObject fitOffset_r;

        private Transform bip_r_trans;
        private Transform bip_l_trans;

        private Quaternion qt_l;
        private Quaternion qt_r;

        [EasyButtons.Button]
        void AddDrawTool()
        {
            HandPoses handPoses = GetComponentInChildren<HandPoses>();
            RemoveDrawTool();

            if (handAttach == null)
            {
                Debug.LogError("Please set the HandAttach which you want to adject");
                return;
            }

            bip_r_trans = handAttach.transform.Find("Bip002 R Hand");
            bip_l_trans = handAttach.transform.Find("Bip002 L Hand");

            Quaternion c_qua_r = bip_r_trans.localRotation;
            Quaternion c_qua_l = bip_l_trans.localRotation;
            Vector3 c_vec_r = bip_r_trans.localPosition;
            Vector3 c_vec_l = bip_l_trans.localPosition;

            if (bip_l_trans != null)
            {
                bip_l_trans.localPosition = Vector3.zero;
                bip_l_trans.localRotation = Quaternion.identity;

                fitOffset_l = Object.Instantiate(handPoses.fitOffset_l, bip_l_trans) as GameObject;
                qt_l = fitOffset_l.transform.localRotation;
            }
            else
            {
                Debug.LogError("Please make sure your HandAttach has node Bip002 L Hand.");

                return;
            }
            bip_l_trans.localPosition = c_vec_l;
            bip_l_trans.localRotation = c_qua_l;


            if (bip_r_trans != null)
            {
                bip_r_trans.localPosition = Vector3.zero;
                bip_r_trans.localRotation = Quaternion.identity;

                fitOffset_r = Object.Instantiate(handPoses.fitOffset_r, bip_r_trans) as GameObject;
                qt_r = fitOffset_r.transform.localRotation;
            }
            else
            {
                Debug.LogError("Please make sure your HandAttach has node Bip002 R Hand.");
                return;
            }

            bip_r_trans.localPosition = c_vec_r;
            bip_r_trans.localRotation = c_qua_r;

            RefleshHandPose();
        }

        [EasyButtons.Button]

        private void ResetHandPositionAndRotation()
        {
            //reset transform
            if (bip_r_trans != null)
            {
                bip_r_trans.localPosition = Vector3.zero;
                bip_r_trans.localRotation = Quaternion.identity;
            }
            else
                Debug.LogError("Please make sure your weapon has handAttach already, and do not change Bip002 R Hand's name.");

            if (fitOffset_r)
            {
                fitOffset_r.transform.localPosition = Vector3.zero;
                fitOffset_r.transform.localRotation = qt_r;
            }
            if (bip_l_trans != null)
            {
                bip_l_trans.localPosition = Vector3.zero;
                bip_l_trans.localRotation = Quaternion.identity;
            } else
                Debug.LogError("Please make sure your weapon has handAttach already, and do not change Bip002 L Hand's name.");

            if (fitOffset_l)
            {
                fitOffset_l.transform.localPosition = new Vector3(0.0026f, -0.0123f, -0.002f);
                fitOffset_l.transform.localRotation = qt_l;
            }
        }

        [EasyButtons.Button]
        void RemoveDrawTool()
        {
            if (fitOffset_l != null)
                Object.DestroyImmediate(fitOffset_l);
            if (fitOffset_r != null)
                Object.DestroyImmediate(fitOffset_r);
        }




        private string handPose;
        private HandPoseControl hpc_l;
        private HandPoseControl hpc_r;

        private HandPosePreset handPoseConfig;

        private HandPosePreset GetHandPose(string handPose)
        {
            HandPoses handPoses = GetComponentInChildren<HandPoses>();
            foreach (Pose pose in handPoses.poses)
            {
                if(pose.id == handPose)
                {
                    return pose.asset;
                }
            }

            return handPoses.defaultPose;
        }

        [EasyButtons.Button]
        private void RefleshHandPose()
        {
            handPose = handAttach.handPose;
            if (handPose == null)
                return;

            handPoseConfig = GetHandPose(handPose);
            if (handPoseConfig == null)
                return;
            hpc_l = fitOffset_l.GetComponentInChildren<HandPoseControl>();

            hpc_r = fitOffset_r.GetComponentInChildren<HandPoseControl>();
            
            SetHandPose();
        }

        void SetHandPose()
        {
            if (hpc_l)
                hpc_l.SetHandPose(handPoseConfig.fingerWeight);
            if (hpc_r)
                hpc_r.SetHandPose(handPoseConfig.fingerWeight);
        }

        void LateUpdate()
        {
            SetHandPose();
        }
    }
}