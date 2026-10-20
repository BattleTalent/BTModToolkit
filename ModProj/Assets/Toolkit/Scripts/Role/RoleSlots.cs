using UnityEngine;
using UnityEditor;

namespace CrossLink
{
    public class RoleSlots : MonoBehaviour
    {

        public GameObject handSlotLeft;
        public GameObject handSlotRight;
        public GameObject shoulderSlotLeft;
        public GameObject shoulderSlotRight;

#if UNITY_EDITOR

        [EasyButtons.Button]
        public void GenerateSlots()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Please ensure that the fbxPrefab has an Animator " +
                    "and that the AnimationType option in the fbx file is Humanoid.");
                return;
            }

            if(!handSlotLeft) {
                Transform handTransformLeft = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                handSlotLeft = new GameObject("LWeapon Point");
                handSlotLeft.transform.parent = handTransformLeft;
                handSlotLeft.transform.localPosition = new Vector3(0,0,0);
                handSlotLeft.transform.rotation = Quaternion.LookRotation(transform.forward, -transform.right);
                handSlotLeft.transform.localScale = new Vector3(1,1,1);

                EditorUtility.SetDirty(this);
            }

            if(!handSlotRight) {
                Transform handTransformRight = animator.GetBoneTransform(HumanBodyBones.RightHand);
                handSlotRight = new GameObject("RWeapon Point");
                handSlotRight.transform.parent = handTransformRight;
                handSlotRight.transform.localPosition = new Vector3(0,0,0);
                handSlotRight.transform.rotation = Quaternion.LookRotation(transform.forward, transform.right);
                handSlotRight.transform.localScale = new Vector3(1,1,1);

                EditorUtility.SetDirty(this);
            }

            Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            if(!shoulderSlotLeft) {
                Transform shoulderTransformLeft = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
                shoulderSlotLeft = new GameObject("LWeapon Spine");
                shoulderSlotLeft.transform.parent = spine;
                shoulderSlotLeft.transform.position = shoulderTransformLeft.position;
                shoulderSlotLeft.transform.rotation = Quaternion.LookRotation(-transform.up, -transform.right);
                shoulderSlotLeft.transform.localScale = new Vector3(1,1,1);
                shoulderSlotLeft.transform.Translate(new Vector3(-0.2f,0,0));

                EditorUtility.SetDirty(this);
            }

            if(!shoulderSlotRight) {
                Transform shoulderTransformRight = animator.GetBoneTransform(HumanBodyBones.Spine);
                shoulderSlotRight = new GameObject("RWeapon Spine");
                shoulderSlotRight.transform.parent = spine;
                shoulderSlotRight.transform.localPosition = shoulderTransformRight.position;
                shoulderSlotRight.transform.rotation = Quaternion.LookRotation(-transform.up, transform.right);
                shoulderSlotRight.transform.localScale = new Vector3(1,1,1);
                shoulderSlotRight.transform.Translate(new Vector3(0.2f,0,0));

                EditorUtility.SetDirty(this);
            }


            Debug.LogWarning("Please check if this is correct after use. If not, " +
                "please assign the handTrans of the handPoseControl manually.");
        }

        
        [Header("Example Gizmo")]

        public bool showExampleRoleGizmo = false;
        public bool showRoleSlotsGizmos = true;
        public bool showExampleWeaponGizmo = true;

        private Mesh mesh;
        private Mesh weaponMesh;
        private Color wireColor = new Color(1, 0, 0, 0.1f);
        const float roleScale = 1.09f;
        private void OnDrawGizmos()
        {
            Gizmos.color = wireColor;

            if (showExampleRoleGizmo) {
                if (!mesh) {
                    mesh = AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/Toolkit/Gizmos/Avatar.fbx");
                }
                Gizmos.DrawWireMesh(mesh, transform.position, new Quaternion(0,0,0,0).normalized, new Vector3(roleScale, roleScale, roleScale));
            }


            if (showRoleSlotsGizmos) {
                if (handSlotLeft) {

                    if (showExampleWeaponGizmo)
                    {
                        if (!weaponMesh)
                        {
                            weaponMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/Toolkit/Gizmos/Sword.fbx");
                        }

                        Gizmos.DrawWireMesh(weaponMesh, handSlotLeft.transform.position, handSlotLeft.transform.rotation, new Vector3(100f, 100f, 100f));
                    }
                    else
                    {
                        Gizmos.DrawWireSphere(handSlotLeft.transform.position, 0.05f);
                    }
                }

                if (handSlotRight) {
                    if (showExampleWeaponGizmo)
                    {
                        if (!weaponMesh)
                        {
                            weaponMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/Toolkit/Gizmos/Sword.fbx");
                        }

                        Gizmos.DrawWireMesh(weaponMesh, handSlotRight.transform.position, handSlotRight.transform.rotation, new Vector3(100f, 100f, 100f));
                    }
                    else
                    {
                        Gizmos.DrawWireSphere(handSlotRight.transform.position, 0.05f);

                    }
                }

                if (shoulderSlotLeft) {
                    Gizmos.DrawWireSphere(shoulderSlotLeft.transform.position, 0.05f);
                }

                if (shoulderSlotRight) {
                    Gizmos.DrawWireSphere(shoulderSlotRight.transform.position, 0.05f);
                }
            }
        }
#endif
    }
}
