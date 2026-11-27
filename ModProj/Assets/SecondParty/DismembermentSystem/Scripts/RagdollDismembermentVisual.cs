using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;

public class RagdollDismembermentVisual : MonoBehaviour
{
    [Tooltip("List of body fragment configurations that can be dismembered. The first entry is usually the ROOT.")]
    public List<BodyFragment> Fragments;
    [Tooltip("Skinned mesh renderers that should not generate cap geometry on cut edges.")]
    public List<SkinnedMeshRenderer> ignoreEdgeCreation;
    [Tooltip("Skinned mesh renderers that should be skipped entirely during dismemberment.")]
    public List<SkinnedMeshRenderer> ignoreDismember;
    [Tooltip("Manually assigned skinned meshes to initialize. If empty, they are collected automatically from the root bone.")]
    public SkinnedMeshRenderer[] skinnedRenderers;

    public enum SelectionMode { Add, Replace }
    public enum SelectionStatus { Empty, Mixed, Full }

    [System.NonSerialized]
    public bool[] SelectionMask;
    [System.NonSerialized] 
    public int SelectionMaskLength;
    [System.NonSerialized]
    public Vector3[] verticesCached;
    [System.NonSerialized]
    public uint[] boneIndicesMask;
    [System.NonSerialized]
    public int[] triCached;

#if UNITY_EDITOR

    public BodyFragment FindFragment(string name)
    {
        for (int i = 0; i < Fragments.Count; ++i)
        {
            if (Fragments[i].Name == name)
                return Fragments[i];
        }
        return null;
    }

    void ClearSkinnedMeshCache()
    {
        boneIndicesMask = null;
        verticesCached = null;
        triCached = null;
    }
    [System.NonSerialized]
    static uint boneMask = 255;// b11111111
    void FillInMeshCahceData(Mesh mesh)
    {
        // fill in cache data.
        if (boneIndicesMask == null)
        {
            BoneWeight[] weights = mesh.boneWeights;
            boneIndicesMask = new uint[weights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                uint mask = (uint)weights[i].boneIndex0;
                mask |= (uint)weights[i].boneIndex1 << (1 * 8);
                mask |= (uint)weights[i].boneIndex2 << (2 * 8);
                mask |= (uint)weights[i].boneIndex3 << (3 * 8);
                boneIndicesMask[i] = mask;
            }
        }
        triCached = triCached == null ? mesh.triangles : triCached;
        verticesCached = verticesCached == null ? mesh.vertices : verticesCached;
    }

    public void GetSelection(SkinnedMeshRenderer skinnedMesh, BodyFragment fragment, SelectionMode mode = SelectionMode.Replace)
    {
        var Mesh = skinnedMesh.sharedMesh;
        var Vertices = Mesh.vertices;
        if (SelectionMask == null || Vertices.Length > SelectionMask.Length)
        {
            SelectionMask = new bool[Vertices.Length];
            SelectionMaskLength = Vertices.Length;
        }
        else
        {
            SelectionMaskLength = Vertices.Length;
        }

        if (mode == SelectionMode.Replace) for (int i = 0; i < Vertices.Length; i++) SelectionMask[i] = false;
        if (fragment.bone == null || fragment.Size.x == 0 || fragment.Size.y == 0 || fragment.Size.z == 0) return;

        Transform[] bones = skinnedMesh.bones;

        var bindposeIndex = System.Array.FindIndex(skinnedMesh.bones, b => b == fragment.bone);
        if (bindposeIndex == -1) return;

        bool[] boneIndexFlags = new bool[bones.Length];
        for (int i = 0; i < bones.Length; i++)
        {
            boneIndexFlags[i] = bones[i].IsChildOf(fragment.bone);
        }

        FillInMeshCahceData(Mesh);

        var M = Matrix4x4.Inverse(Matrix4x4.TRS(fragment.Position, Quaternion.Euler(fragment.Rotation), fragment.Size)) * Mesh.bindposes[bindposeIndex];
        //bindposes == > bone.worldtolocal * model.localtoworld ==> model-->bone

        Vector3 point;

        for (int i = 0; i < triCached.Length; i += 3)
        {
            int indexA = triCached[i];
            int indexB = triCached[i + 1];
            int indexC = triCached[i + 2];

            // Avoids unrelated vertices geting calculated
            uint mask = boneIndicesMask[indexA];

            // If all four bones don't belong to the fragment bone group
            if (boneIndexFlags[(mask) & boneMask] == false
            && boneIndexFlags[(mask >> 8) & boneMask] == false
            && boneIndexFlags[(mask >> 16) & boneMask] == false)
            {
                continue;
            }
            // && boneIndexFlags[ (mask >> 24) & boneMask ] == false)

            Vector3 p;
            // Faster.. With no 'new' overheads
            p.x = (verticesCached[indexA].x + verticesCached[indexB].x + verticesCached[indexC].x) / 3.0f;
            p.y = (verticesCached[indexA].y + verticesCached[indexB].y + verticesCached[indexC].y) / 3.0f;
            p.z = (verticesCached[indexA].z + verticesCached[indexB].z + verticesCached[indexC].z) / 3.0f;

            point.x = M.m00 * p.x + M.m01 * p.y + M.m02 * p.z + M.m03;
            point.y = M.m10 * p.x + M.m11 * p.y + M.m12 * p.z + M.m13;
            point.z = M.m20 * p.x + M.m21 * p.y + M.m22 * p.z + M.m23;

            if (point.x > -0.5 && point.x < 0.5
                && point.y > -0.5 && point.y < 0.5 &&
                point.z > -0.5 && point.z < 0.5)

            {
                SelectionMask[indexA] = true;
                SelectionMask[indexB] = true;
                SelectionMask[indexC] = true;
            }
        }

    }
    public void UpdatePlaneByAvatar(string muName)
    {
        var frag = FindFragment(muName);
        if (frag == null || frag.bone == null)
            return;

        var animator = transform.root.GetComponentInChildren<Animator>(true);
        if (animator == null || !animator.isHuman)
            return;

        Transform avatarBone;
        Transform avatarParent;
        Transform avatarChild;
        if (!TryGetAvatarPlaneBones(animator, frag, out avatarBone, out avatarParent, out avatarChild))
            return;

        Vector3 normalWorld = Vector3.zero;
        if (avatarParent != null)
        {
            normalWorld = (avatarParent.position - avatarBone.position).normalized;
        }
        else if (avatarChild != null)
        {
            normalWorld = (avatarBone.position - avatarChild.position).normalized;
        }

        if (normalWorld.sqrMagnitude < 0.000001f)
            return;

        Vector3 characterUp = GetAvatarCharacterUp(animator);
        Vector3 characterRight = GetAvatarCharacterRight(animator);
        Vector3 characterForward = GetAvatarCharacterForward(characterUp, characterRight, animator.transform);
        Vector3 referenceWorld = GetAvatarReferenceAxis(muName, characterForward, characterUp);
        Vector3 projectedReference = Vector3.ProjectOnPlane(referenceWorld, normalWorld);

        if (projectedReference.sqrMagnitude < 0.000001f && avatarChild != null)
        {
            projectedReference = Vector3.ProjectOnPlane(avatarChild.position - avatarBone.position, normalWorld);
        }
        if (projectedReference.sqrMagnitude < 0.000001f)
        {
            projectedReference = Vector3.ProjectOnPlane(frag.bone.forward, normalWorld);
        }
        if (projectedReference.sqrMagnitude < 0.000001f)
        {
            projectedReference = Vector3.ProjectOnPlane(frag.bone.up, normalWorld);
        }
        if (projectedReference.sqrMagnitude < 0.000001f)
            return;

        frag.cutPlaneNormal = frag.bone.InverseTransformDirection(normalWorld);
        frag.cutPlaneBinormal = frag.bone.InverseTransformDirection(projectedReference.normalized);
    }

    private static bool TryGetAvatarPlaneBones(Animator animator, BodyFragment frag,
        out Transform bone, out Transform parent, out Transform child)
    {
        bone = null;
        parent = null;
        child = null;

        Transform hips = GetAvatarBone(animator, HumanBodyBones.Hips);
        Transform spine = GetAvatarBone(animator, HumanBodyBones.Spine);
        Transform chest = GetAvatarChestBone(animator);
        Transform head = GetAvatarBone(animator, HumanBodyBones.Head);
        Transform leftUpperArm = GetAvatarBone(animator, HumanBodyBones.LeftUpperArm);
        Transform leftLowerArm = GetAvatarBone(animator, HumanBodyBones.LeftLowerArm);
        Transform leftHand = GetAvatarBone(animator, HumanBodyBones.LeftHand);
        Transform rightUpperArm = GetAvatarBone(animator, HumanBodyBones.RightUpperArm);
        Transform rightLowerArm = GetAvatarBone(animator, HumanBodyBones.RightLowerArm);
        Transform rightHand = GetAvatarBone(animator, HumanBodyBones.RightHand);
        Transform leftUpperLeg = GetAvatarBone(animator, HumanBodyBones.LeftUpperLeg);
        Transform leftLowerLeg = GetAvatarBone(animator, HumanBodyBones.LeftLowerLeg);
        Transform leftFoot = GetAvatarBone(animator, HumanBodyBones.LeftFoot);
        Transform rightUpperLeg = GetAvatarBone(animator, HumanBodyBones.RightUpperLeg);
        Transform rightLowerLeg = GetAvatarBone(animator, HumanBodyBones.RightLowerLeg);
        Transform rightFoot = GetAvatarBone(animator, HumanBodyBones.RightFoot);

        switch (frag.Name)
        {
            case CrossLink.RagdollBoneInfo.Pelvis:
                bone = hips;
                parent = chest != null ? chest : spine;
                break;
            case CrossLink.RagdollBoneInfo.Spine:
            case CrossLink.RagdollBoneInfo.Chest:
                bone = chest != null ? chest : spine;
                parent = head;
                child = hips;
                break;
            case CrossLink.RagdollBoneInfo.Head:
                bone = head;
                parent = chest != null ? chest : spine;
                break;
            case CrossLink.RagdollBoneInfo.LUpArm:
                bone = leftUpperArm;
                parent = chest != null ? chest : spine;
                child = leftLowerArm;
                break;
            case CrossLink.RagdollBoneInfo.LForeArm:
                bone = leftLowerArm;
                parent = leftUpperArm;
                child = leftHand;
                break;
            case CrossLink.RagdollBoneInfo.LHand:
                bone = leftHand;
                parent = leftLowerArm;
                break;
            case CrossLink.RagdollBoneInfo.RUpArm:
                bone = rightUpperArm;
                parent = chest != null ? chest : spine;
                child = rightLowerArm;
                break;
            case CrossLink.RagdollBoneInfo.RForeArm:
                bone = rightLowerArm;
                parent = rightUpperArm;
                child = rightHand;
                break;
            case CrossLink.RagdollBoneInfo.RHand:
                bone = rightHand;
                parent = rightLowerArm;
                break;
            case CrossLink.RagdollBoneInfo.LThigh:
                bone = leftUpperLeg;
                parent = hips;
                child = leftLowerLeg;
                break;
            case CrossLink.RagdollBoneInfo.LCalf:
                bone = leftLowerLeg;
                parent = leftUpperLeg;
                child = leftFoot;
                break;
            case CrossLink.RagdollBoneInfo.LFoot:
                bone = leftFoot;
                parent = leftLowerLeg;
                break;
            case CrossLink.RagdollBoneInfo.RThigh:
                bone = rightUpperLeg;
                parent = hips;
                child = rightLowerLeg;
                break;
            case CrossLink.RagdollBoneInfo.RCalf:
                bone = rightLowerLeg;
                parent = rightUpperLeg;
                child = rightFoot;
                break;
            case CrossLink.RagdollBoneInfo.RFoot:
                bone = rightFoot;
                parent = rightLowerLeg;
                break;
        }

        if (bone == null && frag.bone != null)
        {
            bone = frag.bone;
            parent = frag.bone.parent;
            child = frag.bone.childCount > 0 ? frag.bone.GetChild(0) : null;
        }

        return bone != null;
    }

    public static Transform GetAvatarBone(Animator animator, HumanBodyBones humanBone)
    {
        if (animator == null || !animator.isHuman)
            return null;

        return animator.GetBoneTransform(humanBone);
    }

    private static Transform GetAvatarChestBone(Animator animator)
    {
        Transform chest = GetAvatarBone(animator, HumanBodyBones.UpperChest);
        if (chest != null)
            return chest;

        chest = GetAvatarBone(animator, HumanBodyBones.Chest);
        if (chest != null)
            return chest;

        return GetAvatarBone(animator, HumanBodyBones.Spine);
    }

    private static Vector3 GetAvatarCharacterUp(Animator animator)
    {
        Transform head = GetAvatarBone(animator, HumanBodyBones.Head);
        Transform hips = GetAvatarBone(animator, HumanBodyBones.Hips);
        if (head != null && hips != null)
        {
            Vector3 up = head.position - hips.position;
            if (up.sqrMagnitude > 0.000001f)
                return up.normalized;
        }

        return Vector3.up;
    }

    private static Vector3 GetAvatarCharacterRight(Animator animator)
    {
        Transform rightUpperArm = GetAvatarBone(animator, HumanBodyBones.RightUpperArm);
        Transform leftUpperArm = GetAvatarBone(animator, HumanBodyBones.LeftUpperArm);
        if (rightUpperArm != null && leftUpperArm != null)
        {
            Vector3 right = rightUpperArm.position - leftUpperArm.position;
            if (right.sqrMagnitude > 0.000001f)
                return right.normalized;
        }

        Transform rightUpperLeg = GetAvatarBone(animator, HumanBodyBones.RightUpperLeg);
        Transform leftUpperLeg = GetAvatarBone(animator, HumanBodyBones.LeftUpperLeg);
        if (rightUpperLeg != null && leftUpperLeg != null)
        {
            Vector3 right = rightUpperLeg.position - leftUpperLeg.position;
            if (right.sqrMagnitude > 0.000001f)
                return right.normalized;
        }

        return Vector3.right;
    }

    private static Vector3 GetAvatarCharacterForward(Vector3 up, Vector3 right, Transform fallback)
    {
        Vector3 forward = Vector3.Cross(right, up);
        if (forward.sqrMagnitude < 0.000001f)
            forward = fallback.forward;
        else
            forward.Normalize();

        if (Vector3.Dot(forward, fallback.forward) < 0f)
            forward = -forward;

        return forward;
    }
    private static Vector3 GetAvatarReferenceAxis(string fragmentName, Vector3 characterForward, Vector3 characterUp)
    {
        switch (fragmentName)
        {
            case CrossLink.RagdollBoneInfo.Head:
            case CrossLink.RagdollBoneInfo.Pelvis:
            case CrossLink.RagdollBoneInfo.Spine:
            case CrossLink.RagdollBoneInfo.Chest:
            case CrossLink.RagdollBoneInfo.LUpArm:
            case CrossLink.RagdollBoneInfo.LForeArm:
            case CrossLink.RagdollBoneInfo.LHand:
            case CrossLink.RagdollBoneInfo.RUpArm:
            case CrossLink.RagdollBoneInfo.RForeArm:
            case CrossLink.RagdollBoneInfo.RHand:
            case CrossLink.RagdollBoneInfo.LThigh:
            case CrossLink.RagdollBoneInfo.LCalf:
            case CrossLink.RagdollBoneInfo.LFoot:
            case CrossLink.RagdollBoneInfo.RThigh:
            case CrossLink.RagdollBoneInfo.RCalf:
            case CrossLink.RagdollBoneInfo.RFoot:
                return characterForward;
            default:
                return characterUp;
        }
    }
    public class CapMeshData
    {
        public int[] triangleIndices;
        public Dictionary<int, Vector2> vertexUVMap; // Maps vertex index to UV coordinate
    }
#endif
}

[System.Serializable]
public class BodyFragment
{
    [Tooltip("Show this fragment's bounds, cut plane, effect previews, and mesh wireframe when their View settings toggles are enabled. Does not affect dismemberment.")]
    public bool ShowGizmos = true;
    [Tooltip("Show this fragment's selected mesh triangles when Show Gizmos and View settings > Draw wireframe are enabled. Does not control bounds or cut planes. Not used for the ROOT fragment.")]
    public bool ShowWireframe = true;
    public bool ShowProperties;
    [Tooltip("Fragment name. This is typically used at runtime to find and dismember the fragment.")]
    public string Name;
    [Tooltip("Bone transform assigned to this fragment. Cut planes and effect anchors are calculated from it.")]
    public Transform bone;

    [Tooltip("Radius of the cutting area used to determine which nearby vertices are affected.")]
    public float cutRadius = 0.1f;
    [Range(-360,360)]
    [Tooltip("Rotation angle around the cut binormal used to fine-tune the cut direction.")]
    public float cutAngleOnBinormal = 0;
    [Tooltip("Cut plane position in the local space of the fragment bone.")]
    public Vector3 cutPlanePos;
    [Tooltip("Cut plane normal direction in the local space of the fragment bone.")]
    public Vector3 cutPlaneNormal;
    [Tooltip("Cut plane binormal direction in the local space of the fragment bone, used as the rotation reference.")]
    public Vector3 cutPlaneBinormal;

    public bool BoundsDetails;
    [Tooltip("Center of the fragment bounds in bone local space, used for setup and visualization.")]
    public Vector3 Position;
    [Tooltip("Euler rotation of the fragment bounds, used for editor visualization.")]
    public Vector3 Rotation;
    [Tooltip("Size of the fragment bounds, used for setup and visualization.")]
    public Vector3 Size;

    [Tooltip("Effect prefab spawned on this fragment's bone after the dismemberment cut.")]
    public GameObject BoneEffect;
    public bool BoneEffectDetails;
    [Tooltip("Local position offset of BoneEffect relative to the fragment bone.")]
    public Vector3 BoneEffectPosition;
    [Tooltip("Local rotation offset of BoneEffect relative to the fragment bone.")]
    public Vector3 BoneEffectRotation;
    [Tooltip("Local scale applied to BoneEffect.")]
    public Vector3 BoneEffectSize;

    [Tooltip("Effect prefab spawned on the remaining parent side of the severed bone.")]
    public GameObject BoneParentEffect;
    public bool BoneParentEffectDetails;
    [Tooltip("Local position offset of BoneParentEffect relative to the parent-side bone.")]
    public Vector3 BoneParentEffectPosition;
    [Tooltip("Local rotation offset of BoneParentEffect relative to the parent-side bone.")]
    public Vector3 BoneParentEffectRotation;
    [Tooltip("Local scale applied to BoneParentEffect.")]
    public Vector3 BoneParentEffectSize;

    [Tooltip("Editor display color used to distinguish this fragment.")]
    public Color color;
    [Tooltip("Skinned meshes assigned to this fragment. After dismemberment they are transferred from the parent fragment to this one.")]
    public List<SkinnedMeshRenderer> SkinnedMeshes;
    [Tooltip("Only configure the skinned meshes explicitly listed here instead of automatically expanding or inheriting others.")]
    public bool configSkinnedMeshesOnly = false;

    [Tooltip("Whether this fragment is allowed to trigger the Blow Up behavior when that logic is used.")]
    public bool allowBlowUp = true;
    [Tooltip("Child skinned meshes that should be additionally removed after this fragment is dismembered.")]
    public List<SkinnedMeshRenderer> removeChildAfterDismember;
    //[Tooltip("Reaction state the character should enter after this fragment has been dismembered.")]
    //public ReactType reactType = ReactType.Dead;
    public enum ReactType
    {
        None = 0, // no react
        Dead = 1, // dead
        Dizzy = 2, //enter dizzy
        Stiff = 3, // do stiff
    }

#if UNITY_EDITOR
    public BodyFragment GetNearestParent(params BodyFragment[] Parents)
    {
        var Parent = bone.parent;
        while (Parent != null)
        {
            for (int i = 0; i < Parents.Length; i++) if (Parent == Parents[i].bone) return Parents[i];
            Parent = Parent.parent;
        }
        return null;
    }


    public Vector3 GetCutPlanePos()
    {
        return bone.TransformPoint(cutPlanePos);
    }

    public Vector3 GetCutPlaneNormal()
    {
        //return bone.TransformVector(cutPlaneNormal);//.normalized);
        return bone.worldToLocalMatrix.transpose.MultiplyVector(cutPlaneNormal).normalized;
    }

    public Vector3 GetCutPlaneBinormal()
    {
        //return bone.TransformDirection(cutPlaneBinormal);
        return bone.localToWorldMatrix.MultiplyVector(cutPlaneBinormal).normalized;
    }

    public Vector3 CalcPointOnPlane(Vector3 pos)
    {
        var cutPos = GetCutPlanePos();
        var cutNormal = GetCutPlaneNormal();
        var dir = pos - cutPos;
        var dirNormalFrag = cutNormal * Vector3.Dot(cutNormal, dir);
        return pos - dirNormalFrag;
    }


    public float CalcPointDisOnPlane(Vector3 planePos, Vector3 pos)
    {
        //var planePos = GetCutPlanePos();
        var cutNormal = GetCutPlaneNormal();
        var dir = pos - planePos;
        return Vector3.Dot(cutNormal, dir);
    }

    public float CalcPointDisOnPlane(Vector3 pos)
    {
        var planePos = GetCutPlanePos();
        return CalcPointDisOnPlane(planePos, pos);
    }
#endif
}
