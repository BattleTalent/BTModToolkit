using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{
    [System.Serializable]
    public class HandFinger
    {
        public Transform fingerTip;
        public float fingerTipSize = 0.01f;
        public Transform[] fingerNodes;
        [SerializeField]
        Vector3[] fingerOpenPos;
        [SerializeField]
        Quaternion[] fingerOpenRot;
        [SerializeField]
        Vector3[] fingerClosePos;
        [SerializeField]
        Quaternion[] fingerCloseRot;

        [System.NonSerialized]
        public float latestWeight;

        public void CopyOpenClosePose(HandFinger handFinger)
        {
            handFinger.fingerOpenPos = (Vector3[])fingerOpenPos.Clone();
            handFinger.fingerOpenRot = (Quaternion[])fingerOpenRot.Clone();
            handFinger.fingerClosePos = (Vector3[])fingerClosePos.Clone();
            handFinger.fingerCloseRot = (Quaternion[])fingerCloseRot.Clone();
        }

        /// <summary>
        /// you should not call this directly, please check out SubmitFingerTarget api, the system will lerp to target for you
        /// </summary>
        /// <param name="weight"></param>
        public void SetPose(float weight)
        {
#if FINGER_WEIGHT_LOG
            Debug.Log("Finger " + fingerNodes[0].name + " : " + weight);
#endif

            latestWeight = weight;
            for (int i = 0; i < fingerNodes.Length; ++i)
            {
                fingerNodes[i].localPosition = Vector3.Lerp(fingerOpenPos[i], fingerClosePos[i], weight);
                fingerNodes[i].localRotation = Quaternion.Slerp(fingerOpenRot[i], fingerCloseRot[i], weight);
            }
        }

#if UNITY_EDITOR
        [EasyButtons.Button]
#endif
        public void SaveOpenPose()
        {
            fingerOpenPos = new Vector3[fingerNodes.Length];
            fingerOpenRot = new Quaternion[fingerNodes.Length];
            for (int i = 0; i < fingerNodes.Length; ++i)
            {
                fingerOpenPos[i] = fingerNodes[i].localPosition;
                fingerOpenRot[i] = fingerNodes[i].localRotation;
            }
        }

        public void PasteOpenPose()
        {
            for (int i = 0; i < fingerNodes.Length; ++i)
            {
                fingerNodes[i].localPosition = fingerOpenPos[i];
                fingerNodes[i].localRotation = fingerOpenRot[i];
            }
        }

#if UNITY_EDITOR
        [EasyButtons.Button]
#endif
        public void SaveClosePose()
        {
            fingerClosePos = new Vector3[fingerNodes.Length];
            fingerCloseRot = new Quaternion[fingerNodes.Length];
            for (int i = 0; i < fingerNodes.Length; ++i)
            {
                fingerClosePos[i] = fingerNodes[i].localPosition;
                fingerCloseRot[i] = fingerNodes[i].localRotation;
            }
        }

        public void PasteClosePose()
        {
            for (int i = 0; i < fingerNodes.Length; ++i)
            {
                fingerNodes[i].localPosition = fingerClosePos[i];
                fingerNodes[i].localRotation = fingerCloseRot[i];
            }
        }
    }

    public class HandPoseControl : MonoBehaviour
    {
        public Transform handTrans;
        public const float BoardPhaseRadius = 0.2f;//0.15f;//0.08f;
        public const int ThumbFinger = 0;
        public const int IndexFinger = 1;
        public const int MiddleFinger = 2;
        public const int RingFinger = 3;
        public const int PinkieFinger = 4;

        [Header("——————Saved can be undo——————")]
        public HandFinger[] fingers;
    
        public void SetHandPose(float weight)
        {
            for (int i = 0; i < fingers.Length; ++i)
            {
                fingers[i].SetPose(weight);
            }
        }

        public void SetHandPose(List<float> weightList)
        {
            for (int i = 0; i < fingers.Length; ++i)
            {
                fingers[i].SetPose(weightList[i]);
            }
        }

#if UNITY_EDITOR
        [EasyButtons.Button]
        void AutoConfigFingers()
        {
            if (handTrans == null)
            {
                Debug.LogError("Please assign the transform of the model's hand to \"handTrans\" first.");
                return;
            }

            List<HandFinger> fingerList = new List<HandFinger>();
            for (int i = 0; i < handTrans.childCount; ++i)
            {
                var finger = ConvertToFinger(handTrans.GetChild(i));
                if (finger.fingerNodes == null || finger.fingerNodes.Length == 0)
                    continue;
                fingerList.Add(finger);
            }
            fingers = fingerList.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }


        [EasyButtons.Button]
        void SaveFingersOpenPose()
        {
            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "open pose change") ;

            for (int i = 0; i < fingers.Length; ++i)
            {
                fingers[i].SaveOpenPose();
            }
        }

        [EasyButtons.Button]
        void ViewFingerOpenPose()
        {
            for (int i = 0; i < fingers.Length; ++i)
            {
                fingers[i].PasteOpenPose();
            }
        }

        [EasyButtons.Button]
        void SaveFingersClosePose()
        {
            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "close pose change");

            for (int i = 0; i < fingers.Length; ++i)
            {
                fingers[i].SaveClosePose();
            }
        }

        [EasyButtons.Button]
        void ViewFingerClosePose()
        {
            for (int i = 0; i < fingers.Length; ++i)
            {
                fingers[i].PasteClosePose();
            }
        }

        HandFinger ConvertToFinger(Transform fingerRoot, int nodeNum = 3)
        {
            HandFinger finger = new HandFinger();

            int fingerDepth = 1;
            Transform fingerNode = fingerRoot;
            while (fingerNode.childCount != 0)
            {
                ++fingerDepth;
                fingerNode = fingerNode.GetChild(0);
            }

            nodeNum = fingerDepth < nodeNum ? fingerDepth : nodeNum;

            finger.fingerNodes = new Transform[nodeNum];
            fingerNode = fingerRoot;
            for (int i = 0; i < nodeNum; ++i)
            {
                finger.fingerNodes[i] = fingerNode;
                if (fingerNode.childCount != 0)
                {
                    fingerNode = fingerNode.GetChild(0);
                }
            }

            //TransformHelper.FindAReasonableName            
            var tipTrans = FindChildRecursive(fingerRoot, "Tip");
            if (tipTrans != null)
            {
                finger.fingerTip = tipTrans;
                // }else
                // {
                //     finger.fingerTip = finger.fingerNodes[finger.fingerNodes.Length - 1];
            }

            return finger;
        }

        public static Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Contains(name))
                    return child;

                var result = FindChildRecursive(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        [EasyButtons.Button]
        void RemoveFingerTips()
        {
            for (int i = 0; i < fingers.Length; ++i)
            {
                if (fingers[i].fingerTip != null)
                {
                    Object.DestroyImmediate(fingers[i].fingerTip.gameObject);
                }
            }
        }


        [EasyButtons.Button]
        void AddFingerTips(float tipOffset = 0.01f, float tipSize = 0.01f)
        {
            for (int i = 0; i < fingers.Length; ++i)
            {
                if (fingers[i].fingerTip != null)
                {
                    continue;
                }
                var endNode = fingers[i].fingerNodes[fingers[i].fingerNodes.Length - 1];

                if (endNode == null || endNode.parent == null)
                    continue;

                var tip = new GameObject("Tip");
                tip.transform.parent = endNode;
                //tip.transform.localPosition = Vector3.zero;
                //tip.transform.localRotation = Quaternion.identity;
                //tip.transform.localScale = Vector3.one;
                tip.transform.position = (endNode.position - endNode.parent.position).normalized * tipOffset + endNode.position;
                fingers[i].fingerTip = tip.transform;
                fingers[i].fingerTipSize = tipSize;
            }
        }

        private void OnDrawGizmos()
        {
            if (fingers.Length <= 0)
                return;

            var color = Color.green;
            color.a = 0.3f;
            Gizmos.color = color;

            for (int i = 0; i < fingers.Length; ++i)
            {
                if (fingers[i].fingerTip == null)
                {
                    continue;
                }

                Gizmos.DrawWireSphere(fingers[i].fingerTip.position, fingers[i].fingerTipSize);
            }

            if (fingers[2].fingerNodes.Length > 0)
                Gizmos.DrawWireSphere(fingers[2].fingerNodes[0].position, BoardPhaseRadius);
        }
#endif
    }

}