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

        //const int FINGER_COUNT = 5;
        public int numOfFinger = 5;

        [Header("——————Saved can be undo——————")]
        [Tooltip("Finger data follows the hierarchy order of Humanoid finger roots, ignoring non-finger nodes. Use Restore Finger Hierarchy Order to reorder saved data without rebuilding poses.")]
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
        void AutoConfigFingers(int fingerNodeDepth = 3)
        {
            if (fingerNodeDepth <= 0)
            {
                Debug.LogError("AutoConfigFingers failed: fingerNodeDepth must be greater than zero.", this);
                return;
            }

            if (!TryGetFingerRootsInHierarchyOrder("AutoConfigFingers", out Transform[] fingerRoots))
            {
                return;
            }

            HandFinger[] generatedFingers = new HandFinger[fingerRoots.Length];
            for (int i = 0; i < fingerRoots.Length; i++)
            {
                generatedFingers[i] = ConvertToFingerRunTime(fingerRoots[i], fingerNodeDepth);
            }

            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "Auto Config Fingers");
            fingers = generatedFingers;
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }

        [EasyButtons.Button("Restore Finger Hierarchy Order")]
        void RestoreFingerHierarchyOrder()
        {
            if (!TryGetFingerRootsInHierarchyOrder("RestoreFingerHierarchyOrder", out Transform[] fingerRoots))
            {
                return;
            }

            if (fingers == null || fingers.Length != numOfFinger)
            {
                Debug.LogError("RestoreFingerHierarchyOrder failed: Fingers must contain exactly Num Of Finger entries. Existing data was preserved.", this);
                return;
            }

            Dictionary<Transform, HandFinger> fingersByRoot = new Dictionary<Transform, HandFinger>();
            for (int i = 0; i < fingers.Length; i++)
            {
                HandFinger finger = fingers[i];
                if (finger == null || finger.fingerNodes == null
                    || finger.fingerNodes.Length == 0 || finger.fingerNodes[0] == null)
                {
                    Debug.LogError($"RestoreFingerHierarchyOrder failed: Fingers[{i}] has no finger root. Existing data was preserved.", this);
                    return;
                }

                Transform fingerRoot = finger.fingerNodes[0];
                if (fingersByRoot.ContainsKey(fingerRoot))
                {
                    Debug.LogError($"RestoreFingerHierarchyOrder failed: duplicate finger root {fingerRoot.name}. Existing data was preserved.", this);
                    return;
                }

                fingersByRoot.Add(fingerRoot, finger);
            }

            HandFinger[] reorderedFingers = new HandFinger[fingerRoots.Length];
            bool orderChanged = false;
            for (int i = 0; i < fingerRoots.Length; i++)
            {
                if (!fingersByRoot.TryGetValue(fingerRoots[i], out HandFinger finger))
                {
                    Debug.LogError($"RestoreFingerHierarchyOrder failed: finger root {fingerRoots[i].name} has no matching saved finger. Check the saved finger roots. Existing data was preserved.", this);
                    return;
                }

                // Keep the complete saved finger, including poses, tips, and bone references.
                reorderedFingers[i] = finger;
                orderChanged |= finger != fingers[i];
            }

            if (!orderChanged)
            {
                Debug.Log("RestoreFingerHierarchyOrder skipped: Fingers already follow the hand's child order.", this);
                return;
            }

            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "Restore Finger Hierarchy Order");
            fingers = reorderedFingers;
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            Debug.Log("RestoreFingerHierarchyOrder succeeded: restored finger hierarchy order and preserved all saved finger data. Save the prefab to keep the change.", this);
        }

        private bool TryGetFingerRootsInHierarchyOrder(string operationName, out Transform[] fingerRoots)
        {
            fingerRoots = null;
            if (handTrans == null)
            {
                Debug.LogError($"{operationName} failed: assign Hand Trans first.", this);
                return false;
            }

            if (numOfFinger <= 0)
            {
                Debug.LogError($"{operationName} failed: Num Of Finger must be greater than zero.", this);
                return false;
            }

            Animator animator = handTrans.GetComponentInParent<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.isHuman)
            {
                Debug.LogError($"{operationName} failed: Hand Trans must belong to a valid Humanoid Animator.", this);
                return false;
            }

            HumanBodyBones[] fingerRootBones;
            if (handTrans == animator.GetBoneTransform(HumanBodyBones.LeftHand))
            {
                fingerRootBones = new HumanBodyBones[]
                {
                    HumanBodyBones.LeftThumbProximal,
                    HumanBodyBones.LeftIndexProximal,
                    HumanBodyBones.LeftMiddleProximal,
                    HumanBodyBones.LeftRingProximal,
                    HumanBodyBones.LeftLittleProximal,
                };
            }
            else if (handTrans == animator.GetBoneTransform(HumanBodyBones.RightHand))
            {
                fingerRootBones = new HumanBodyBones[]
                {
                    HumanBodyBones.RightThumbProximal,
                    HumanBodyBones.RightIndexProximal,
                    HumanBodyBones.RightMiddleProximal,
                    HumanBodyBones.RightRingProximal,
                    HumanBodyBones.RightLittleProximal,
                };
            }
            else
            {
                Debug.LogError($"{operationName} failed: Hand Trans is not this Animator's LeftHand or RightHand bone.", this);
                return false;
            }

            HashSet<Transform> mappedFingerRoots = new HashSet<Transform>();
            foreach (HumanBodyBones fingerRootBone in fingerRootBones)
            {
                Transform fingerRoot = animator.GetBoneTransform(fingerRootBone);
                if (fingerRoot != null)
                    mappedFingerRoots.Add(fingerRoot);
            }

            List<Transform> orderedRoots = new List<Transform>();
            foreach (Transform child in handTrans)
            {
                if (mappedFingerRoots.Contains(child))
                    orderedRoots.Add(child);
            }

            if (orderedRoots.Count != numOfFinger)
            {
                Debug.LogError($"{operationName} failed: found {orderedRoots.Count} mapped finger roots directly under Hand Trans, expected {numOfFinger}. Check Humanoid mapping and Num Of Finger. Existing data was preserved.", this);
                return false;
            }

            fingerRoots = orderedRoots.ToArray();
            return true;
        }

        HandFinger ConvertToFingerRunTime(Transform fingerRoot, int fingerNodeDepth = 3)
        {
            HandFinger finger = new HandFinger();

            int fingerDepth = 1;
            Transform fingerNode = fingerRoot;
            while (fingerNode.childCount != 0)
            {
                ++fingerDepth;
                fingerNode = fingerNode.GetChild(0);
            }

            fingerNodeDepth = fingerDepth < fingerNodeDepth ? fingerDepth : fingerNodeDepth;

            finger.fingerNodes = new Transform[fingerNodeDepth];
            fingerNode = fingerRoot;
            for (int i = 0; i < fingerNodeDepth; ++i)
            {
                finger.fingerNodes[i] = fingerNode;
                if (fingerNode.childCount != 0)
                {
                    fingerNode = fingerNode.GetChild(0);
                }
            }

            var tipTrans = FindChildRecursive(fingerRoot, "Tip");
            if (tipTrans != null)
            {
                finger.fingerTip = tipTrans;
            }

            return finger;
        }

        [EasyButtons.Button("View - Finger Open Pose", Spacing = EasyButtons.ButtonSpacing.Before)]
        void ViewFingerOpenPose()
        {
            for (int i = 0; i < fingers.Length; ++i)
            {
                fingers[i].PasteOpenPose();
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }

        
        [EasyButtons.Button("View - Finger Close Pose")]
        void ViewFingerClosePose()
        {
            for (int i = 0; i < fingers.Length; ++i)
            {
                fingers[i].PasteClosePose();
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [EasyButtons.Button("Save - Finger Open Pose", Spacing = EasyButtons.ButtonSpacing.Before)]
        void SaveFingersOpenPose()
        {
            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "open pose change") ;

            for (int i = 0; i < fingers.Length; ++i)
            {
                fingers[i].SaveOpenPose();
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }       

        [EasyButtons.Button("Save - Finger Close Pose", Spacing = EasyButtons.ButtonSpacing.After)]
        void SaveFingersClosePose()
        {
            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "close pose change");

            for (int i = 0; i < fingers.Length; ++i)
            {
                fingers[i].SaveClosePose();
            }
            UnityEditor.EditorUtility.SetDirty(this);
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
            UnityEditor.EditorUtility.SetDirty(this);
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
            UnityEditor.EditorUtility.SetDirty(this);
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
