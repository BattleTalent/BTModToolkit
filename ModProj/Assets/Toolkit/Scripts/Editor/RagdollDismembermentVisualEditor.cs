using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(RagdollDismembermentVisual))]
public class RagdollDismembermentVisualEditor : Editor
{
    private static readonly HumanBodyBones[] HumanBodyBoneOptions =
        ((HumanBodyBones[])System.Enum.GetValues(typeof(HumanBodyBones)))
        .Where(bone => (bone == HumanBodyBones.Head
        || bone == HumanBodyBones.Spine || bone == HumanBodyBones.Chest || bone == HumanBodyBones.UpperChest
        || bone == HumanBodyBones.RightUpperArm || bone == HumanBodyBones.RightLowerArm
        || bone == HumanBodyBones.LeftUpperArm || bone == HumanBodyBones.LeftLowerArm
        || bone == HumanBodyBones.RightUpperLeg || bone == HumanBodyBones.RightLowerLeg
        || bone == HumanBodyBones.LeftUpperLeg || bone == HumanBodyBones.LeftLowerLeg))
        .ToArray();

    private static readonly string[] HumanBodyBoneLabels =
        new[] { "None" }
        .Concat(HumanBodyBoneOptions.Select(GetHumanBodyBoneLabel))
        .ToArray();

    private static readonly Dictionary<string, HumanBodyBones> HumanBodyBoneSelections =
        new Dictionary<string, HumanBodyBones>();
    struct OrientedBoundsData
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 size;
    }

    struct CutRadiusSample
    {
        public float planeDistance;
        public float radialDistance;
    }

    public static bool ShowViewSettings = true;
    public static bool ShowFragmentsSettings = true;

    private const string DrawBoundsPreferenceKey = "CrossLink.RagdollDismembermentVisualEditor.DrawBounds";
    public static bool DrawBounds = true;
    public static bool DrawEffects = true;
    public static bool DrawWireframe = false;

    private RagdollDismembermentVisual Visual => (RagdollDismembermentVisual)target;

    void OnEnable()
    {
        DrawBounds = EditorPrefs.GetBool(DrawBoundsPreferenceKey, true);
        CheckMeshReadableStatus();
    }


    Bounds GetAutomaticBounds(Transform bone)
    {
        var collider = bone.GetComponent<Collider>();
        Bounds bounds = new Bounds();

        if (collider)
        {
            if (collider is BoxCollider)
            {
                var box = (BoxCollider)collider;
                bounds.center = box.center;
                bounds.size = box.size;
            }
            else if (collider is SphereCollider)
            {
                var sphere = (SphereCollider)collider;
                bounds.center = sphere.center;
                bounds.size = Vector3.one * sphere.radius * 2;
            }
            else if (collider is CapsuleCollider)
            {
                var capsule = (CapsuleCollider)collider;
                bounds.center = capsule.center;

                Vector3 Height = (capsule.direction == 0 ? Vector3.right : capsule.direction == 1 ? Vector3.up : Vector3.forward) * (capsule.height - capsule.radius * 2);
                bounds.size = Vector3.one * capsule.radius * 2 + Height;
            }
        }
        else
        {
            for (int n = 0; n < bone.childCount; n++) bounds.Encapsulate(bone.GetChild(n).localPosition);
        }
        return bounds;
    }
    bool TryCollectFragmentPoints(BodyFragment fragment, out List<Vector3> points)
    {
        points = new List<Vector3>(256);
        if (fragment == null || fragment.bone == null)
        {
            return false;
        }

        var renderers = new List<SkinnedMeshRenderer>();
        if (fragment.SkinnedMeshes != null)
        {
            for (int i = 0; i < fragment.SkinnedMeshes.Count; i++)
            {
                var renderer = fragment.SkinnedMeshes[i];
                if (renderer != null && renderers.Contains(renderer) == false)
                {
                    renderers.Add(renderer);
                }
            }
        }

        if (renderers.Count == 0)
        {
            renderers.AddRange(fragment.bone.root.GetComponentsInChildren<SkinnedMeshRenderer>(true));
        }

        for (int rendererIndex = 0; rendererIndex < renderers.Count; rendererIndex++)
        {
            var renderer = renderers[rendererIndex];
            if (renderer == null || renderer.sharedMesh == null)
            {
                continue;
            }

            var mesh = renderer.sharedMesh;
            var bones = renderer.bones;
            var bindposes = mesh.bindposes;
            var vertices = mesh.vertices;
            var boneWeights = mesh.boneWeights;

            if (bones == null || bindposes == null || vertices == null || boneWeights == null)
            {
                continue;
            }

            int fragmentBoneIndex = System.Array.FindIndex(bones, b => b == fragment.bone);
            if (fragmentBoneIndex < 0 || fragmentBoneIndex >= bindposes.Length)
            {
                continue;
            }

            bool[] subtreeBoneFlags = new bool[bones.Length];
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                subtreeBoneFlags[boneIndex] = bones[boneIndex] != null && bones[boneIndex].IsChildOf(fragment.bone);
            }

            int vertexCount = Mathf.Min(vertices.Length, boneWeights.Length);
            Matrix4x4 meshToBone = bindposes[fragmentBoneIndex];
            for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
            {
                if (VertexFullyDominatedBySubtree(boneWeights[vertexIndex], subtreeBoneFlags) == false)
                {
                    continue;
                }

                points.Add(meshToBone.MultiplyPoint3x4(vertices[vertexIndex]));
            }
        }

        return points.Count > 0;
    }

    bool GetAutomaticBoundsBySkinnedMesh(BodyFragment fragment, out OrientedBoundsData boundsData)
    {
        boundsData = default;
        if (fragment == null || fragment.bone == null)
        {
            return false;
        }

        var renderers = new List<SkinnedMeshRenderer>();
        if (fragment.SkinnedMeshes != null)
        {
            for (int i = 0; i < fragment.SkinnedMeshes.Count; i++)
            {
                var renderer = fragment.SkinnedMeshes[i];
                if (renderer != null && renderers.Contains(renderer) == false)
                {
                    renderers.Add(renderer);
                }
            }
        }

        if (renderers.Count == 0)
        {
            renderers.AddRange(fragment.bone.root.GetComponentsInChildren<SkinnedMeshRenderer>(true));
        }

        var points = new List<Vector3>(256);

        for (int rendererIndex = 0; rendererIndex < renderers.Count; rendererIndex++)
        {
            var renderer = renderers[rendererIndex];
            if (renderer == null || renderer.sharedMesh == null)
            {
                continue;
            }

            var mesh = renderer.sharedMesh;
            var bones = renderer.bones;
            var bindposes = mesh.bindposes;
            var vertices = mesh.vertices;
            var boneWeights = mesh.boneWeights;

            if (bones == null || bindposes == null || vertices == null || boneWeights == null)
            {
                continue;
            }

            int fragmentBoneIndex = System.Array.FindIndex(bones, b => b == fragment.bone);
            if (fragmentBoneIndex < 0 || fragmentBoneIndex >= bindposes.Length)
            {
                continue;
            }

            bool[] subtreeBoneFlags = new bool[bones.Length];
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                subtreeBoneFlags[boneIndex] = bones[boneIndex] != null && bones[boneIndex].IsChildOf(fragment.bone);
            }

            int vertexCount = Mathf.Min(vertices.Length, boneWeights.Length);
            Matrix4x4 meshToBone = bindposes[fragmentBoneIndex];
            for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
            {
                if (VertexFullyDominatedBySubtree(boneWeights[vertexIndex], subtreeBoneFlags) == false)
                {
                    continue;
                }

                points.Add(meshToBone.MultiplyPoint3x4(vertices[vertexIndex]));
            }
        }

        if (points.Count == 0)
        {
            return false;
        }

        Quaternion orientation = fragment.bone.localRotation;
        Quaternion inverseOrientation = Quaternion.Inverse(orientation);

        Vector3 min = inverseOrientation * points[0];
        Vector3 max = min;
        for (int i = 1; i < points.Count; i++)
        {
            Vector3 orientedPoint = inverseOrientation * points[i];
            min = Vector3.Min(min, orientedPoint);
            max = Vector3.Max(max, orientedPoint);
        }

        Vector3 centerInBoxSpace = (min + max) * 0.5f;
        Vector3 size = max - min;
        size = Vector3.Max(size * 1.02f, Vector3.one * 0.001f);

        boundsData.position = orientation * centerInBoxSpace;
        boundsData.size = size;
        return true;
    }

    bool GetAutomaticSizeBySkinnedMesh(BodyFragment fragment, Quaternion orientation, Vector3 center, out Vector3 size)
    {
        size = Vector3.zero;
        if (fragment == null || fragment.bone == null)
        {
            return false;
        }

        var renderers = new List<SkinnedMeshRenderer>();
        if (fragment.SkinnedMeshes != null)
        {
            for (int i = 0; i < fragment.SkinnedMeshes.Count; i++)
            {
                var renderer = fragment.SkinnedMeshes[i];
                if (renderer != null && renderers.Contains(renderer) == false)
                {
                    renderers.Add(renderer);
                }
            }
        }

        if (renderers.Count == 0)
        {
            renderers.AddRange(fragment.bone.root.GetComponentsInChildren<SkinnedMeshRenderer>(true));
        }

        var points = new List<Vector3>(256);
        for (int rendererIndex = 0; rendererIndex < renderers.Count; rendererIndex++)
        {
            var renderer = renderers[rendererIndex];
            if (renderer == null || renderer.sharedMesh == null)
            {
                continue;
            }

            var mesh = renderer.sharedMesh;
            var bones = renderer.bones;
            var bindposes = mesh.bindposes;
            var vertices = mesh.vertices;
            var boneWeights = mesh.boneWeights;

            if (bones == null || bindposes == null || vertices == null || boneWeights == null)
            {
                continue;
            }

            int fragmentBoneIndex = System.Array.FindIndex(bones, b => b == fragment.bone);
            if (fragmentBoneIndex < 0 || fragmentBoneIndex >= bindposes.Length)
            {
                continue;
            }

            bool[] subtreeBoneFlags = new bool[bones.Length];
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                subtreeBoneFlags[boneIndex] = bones[boneIndex] != null && bones[boneIndex].IsChildOf(fragment.bone);
            }

            int vertexCount = Mathf.Min(vertices.Length, boneWeights.Length);
            Matrix4x4 meshToBone = bindposes[fragmentBoneIndex];
            for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
            {
                if (VertexFullyDominatedBySubtree(boneWeights[vertexIndex], subtreeBoneFlags) == false)
                {
                    continue;
                }

                points.Add(meshToBone.MultiplyPoint3x4(vertices[vertexIndex]));
            }
        }

        if (points.Count == 0)
        {
            return false;
        }

        Quaternion inverseOrientation = Quaternion.Inverse(orientation);
        Vector3 min = inverseOrientation * (points[0] - center);
        Vector3 max = min;
        for (int i = 1; i < points.Count; i++)
        {
            Vector3 orientedPoint = inverseOrientation * (points[i] - center);
            min = Vector3.Min(min, orientedPoint);
            max = Vector3.Max(max, orientedPoint);
        }

        size = Vector3.Max((max - min) * 1.02f, Vector3.one * 0.001f);
        return true;
    }

    bool GetAutomaticCutRadiusBySkinnedMesh(BodyFragment fragment, out float cutRadius)
    {
        cutRadius = 0f;

        List<Vector3> points;
        if (TryCollectFragmentPoints(fragment, out points) == false)
        {
            return false;
        }

        Vector3 normal = fragment.cutPlaneNormal.normalized;
        if (normal.sqrMagnitude < 0.000001f)
        {
            return false;
        }

        Vector3 binormal = Vector3.ProjectOnPlane(fragment.cutPlaneBinormal, normal).normalized;
        if (binormal.sqrMagnitude < 0.000001f)
        {
            binormal = Vector3.ProjectOnPlane(Vector3.up, normal).normalized;
        }
        if (binormal.sqrMagnitude < 0.000001f)
        {
            binormal = Vector3.ProjectOnPlane(Vector3.right, normal).normalized;
        }
        if (binormal.sqrMagnitude < 0.000001f)
        {
            return false;
        }

        Vector3 tangent = Vector3.Cross(normal, binormal).normalized;
        Vector3 planePos = fragment.cutPlanePos;

        var samples = new List<CutRadiusSample>(points.Count);
        bool hasValidBounds = fragment.Size.x > 0f && fragment.Size.y > 0f && fragment.Size.z > 0f;
        Quaternion inverseBoundsRotation = Quaternion.Inverse(Quaternion.Euler(fragment.Rotation));
        Vector3 halfSize = fragment.Size * 0.5f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = points[i];
            if (hasValidBounds)
            {
                Vector3 boundsLocal = inverseBoundsRotation * (point - fragment.Position);
                if (Mathf.Abs(boundsLocal.x) > halfSize.x || Mathf.Abs(boundsLocal.y) > halfSize.y || Mathf.Abs(boundsLocal.z) > halfSize.z)
                {
                    continue;
                }
            }

            Vector3 relative = point - planePos;
            float planeDistance = Mathf.Abs(Vector3.Dot(relative, normal));
            float radialDistance = Mathf.Sqrt(
                Mathf.Pow(Vector3.Dot(relative, tangent), 2f) +
                Mathf.Pow(Vector3.Dot(relative, binormal), 2f));

            samples.Add(new CutRadiusSample
            {
                planeDistance = planeDistance,
                radialDistance = radialDistance
            });
        }

        if (samples.Count == 0)
        {
            return false;
        }

        samples.Sort((a, b) => a.planeDistance.CompareTo(b.planeDistance));
        int nearestCount = Mathf.Clamp(samples.Count / 10, 8, 64);
        nearestCount = Mathf.Min(nearestCount, samples.Count);

        float maxRadius = 0f;
        for (int i = 0; i < nearestCount; i++)
        {
            maxRadius = Mathf.Max(maxRadius, samples[i].radialDistance);
        }

        if (maxRadius <= 0.0001f)
        {
            float minPlaneDistance = samples[0].planeDistance;
            float fallbackThreshold = minPlaneDistance + 0.01f;
            for (int i = 0; i < samples.Count; i++)
            {
                if (samples[i].planeDistance > fallbackThreshold)
                {
                    break;
                }
                maxRadius = Mathf.Max(maxRadius, samples[i].radialDistance);
            }
        }

        if (maxRadius <= 0.0001f)
        {
            for (int i = 0; i < samples.Count; i++)
            {
                maxRadius = Mathf.Max(maxRadius, samples[i].radialDistance);
            }
        }

        cutRadius = Mathf.Max(maxRadius * 1.05f, 0.001f);
        return true;
    }

    static bool VertexFullyDominatedBySubtree(BoneWeight boneWeight, bool[] subtreeBoneFlags)
    {
        float subtreeWeight =
            GetBoneWeightIfInSubtree(boneWeight.boneIndex0, boneWeight.weight0, subtreeBoneFlags) +
            GetBoneWeightIfInSubtree(boneWeight.boneIndex1, boneWeight.weight1, subtreeBoneFlags) +
            GetBoneWeightIfInSubtree(boneWeight.boneIndex2, boneWeight.weight2, subtreeBoneFlags) +
            GetBoneWeightIfInSubtree(boneWeight.boneIndex3, boneWeight.weight3, subtreeBoneFlags);

        return subtreeWeight >= 1f;
    }

    static float GetBoneWeightIfInSubtree(int boneIndex, float weight, bool[] subtreeBoneFlags)
    {
        return weight > 0.0001f
            && boneIndex >= 0
            && boneIndex < subtreeBoneFlags.Length
            && subtreeBoneFlags[boneIndex]
            ? weight
            : 0f;
    }


    private PanelEvents DrawPanel(Rect rect, string content, Color backgroundColor, bool drawDeleteButton = false)
    {
        PanelEvents result = PanelEvents.NoEvent;
        Color oldColor = GUI.backgroundColor;
        backgroundColor.a = 0.3f;
        GUI.backgroundColor = backgroundColor;
        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
        GUI.skin.button.fontStyle = FontStyle.Bold;

        GUI.Box(rect, "");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(content))
        {
            result = PanelEvents.FoldUnfold;
        }

        GUI.skin.button.alignment = TextAnchor.MiddleCenter;
        GUI.skin.button.fontStyle = FontStyle.Normal;
        GUI.backgroundColor = oldColor;
        if (drawDeleteButton && GUILayout.Button("X", GUILayout.Width(30)))
        {
            result = PanelEvents.Delete;
        }

        EditorGUILayout.EndHorizontal();
        return result;
    }

    private void DrawViewSettings()
    {
        bool drawBounds = EditorGUILayout.Toggle("Draw bounds", DrawBounds);
        if (drawBounds != DrawBounds)
        {
            DrawBounds = drawBounds;
            EditorPrefs.SetBool(DrawBoundsPreferenceKey, DrawBounds);
        }
        DrawEffects = EditorGUILayout.Toggle("Draw effects", DrawEffects);
        DrawWireframe = EditorGUILayout.Toggle("Draw wireframe", DrawWireframe);
        SceneView.RepaintAll();
    }

    private void DrawFragmentDetails(SerializedProperty fragment, int fragmentIndex, bool isRoot = false)
    {
        EditorGUILayout.PropertyField(fragment.FindPropertyRelative("Name"));
        EditorGUILayout.PropertyField(fragment.FindPropertyRelative("color"));
        EditorGUILayout.PropertyField(fragment.FindPropertyRelative("bone"));
        if (!isRoot)
        {
            DrawHumanBodyBoneAutoConfig(fragment);
        }

        EditorGUILayout.PropertyField(fragment.FindPropertyRelative("ShowWireframe"));
        EditorGUILayout.PropertyField(fragment.FindPropertyRelative("SkinnedMeshes"));
        if (isRoot || fragment.FindPropertyRelative("bone").objectReferenceValue == null)
        {
            return;
        }

        if (DrawPanel(EditorGUILayout.BeginVertical(), "Bounds", Color.gray) == PanelEvents.FoldUnfold)
        {
            fragment.FindPropertyRelative("BoundsDetails").boolValue = !fragment.FindPropertyRelative("BoundsDetails").boolValue;
        }

        if (fragment.FindPropertyRelative("BoundsDetails").boolValue)
        {
            EditorGUILayout.PropertyField(fragment.FindPropertyRelative("cutRadius"));
            EditorGUILayout.PropertyField(fragment.FindPropertyRelative("cutAngleOnBinormal"));
            EditorGUILayout.PropertyField(fragment.FindPropertyRelative("cutPlanePos"));
            EditorGUILayout.PropertyField(fragment.FindPropertyRelative("cutPlaneNormal"));
            EditorGUILayout.PropertyField(fragment.FindPropertyRelative("cutPlaneBinormal"));
            EditorGUILayout.PropertyField(fragment.FindPropertyRelative("Position"));
            EditorGUILayout.PropertyField(fragment.FindPropertyRelative("Rotation"));
            EditorGUILayout.PropertyField(fragment.FindPropertyRelative("Size"));
            EditorGUILayout.PropertyField(fragment.FindPropertyRelative("allowBlowUp"));
            //EditorGUILayout.PropertyField(fragment.FindPropertyRelative("reactType"));
            EditorGUILayout.PropertyField(fragment.FindPropertyRelative("removeChildAfterDismember"));

            if (GUILayout.Button("Automatic plane by bone"))
            {
                var bone = (Transform)fragment.FindPropertyRelative("bone").objectReferenceValue;
                var visual = bone.root.GetComponentInChildren<RagdollDismembermentVisual>();
                visual.UpdatePlaneByAvatar(fragment.FindPropertyRelative("Name").stringValue);
                EditorUtility.SetDirty(visual);
            }
            if (GUILayout.Button("Automatic CutRadius"))
            {
                var dismember = target as RagdollDismembermentVisual;
                if (dismember != null && fragmentIndex >= 0 && fragmentIndex < dismember.Fragments.Count)
                {
                    float cutRadius;
                    if (GetAutomaticCutRadiusBySkinnedMesh(dismember.Fragments[fragmentIndex], out cutRadius))
                    {
                        fragment.FindPropertyRelative("cutRadius").floatValue = cutRadius;
                    }
                    else
                    {
                        Debug.LogWarning("Automatic CutRadius failed. Make sure the fragment has a valid cut plane and skinned mesh vertices weighted fully to the bone subtree.", dismember);
                    }
                }
            }
            if (GUILayout.Button("Automatic bounds"))
            {
                var dismember = target as RagdollDismembermentVisual;
                if (dismember != null && fragmentIndex >= 0 && fragmentIndex < dismember.Fragments.Count)
                {
                    OrientedBoundsData obb;
                    if (GetAutomaticBoundsBySkinnedMesh(dismember.Fragments[fragmentIndex], out obb))
                    {
                        fragment.FindPropertyRelative("Position").vector3Value = obb.position;
                        fragment.FindPropertyRelative("Size").vector3Value = obb.size;
                    }
                    else
                    {
                        Debug.LogWarning("Automatic bounds by skinned mesh failed. Make sure the fragment bone exists and the skinned meshes contain vertices weighted to the bone subtree.", dismember);
                    }
                }
            }
            if (GUILayout.Button("Automatic size"))
            {
                var dismember = target as RagdollDismembermentVisual;
                if (dismember != null && fragmentIndex >= 0 && fragmentIndex < dismember.Fragments.Count)
                {
                    var bodyFragment = dismember.Fragments[fragmentIndex];
                    Quaternion orientation = Quaternion.Euler(fragment.FindPropertyRelative("Rotation").vector3Value);
                    Vector3 center = fragment.FindPropertyRelative("Position").vector3Value;
                    Vector3 size;
                    if (GetAutomaticSizeBySkinnedMesh(bodyFragment, orientation, center, out size))
                    {
                        fragment.FindPropertyRelative("Size").vector3Value = size;
                    }
                    else
                    {
                        Debug.LogWarning("Automatic size by skinned mesh failed. Make sure the fragment bone exists and the skinned meshes contain vertices weighted to the bone subtree.", dismember);
                    }
                }
            }

        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Separator();
        DrawFragmentEffectEditor(fragment, "BoneEffect");
        DrawFragmentEffectEditor(fragment, "BoneParentEffect");
    }


    void DrawHumanBodyBoneAutoConfig(SerializedProperty Fragment)
    {
        string key = target.GetInstanceID() + ":" + Fragment.propertyPath;
        HumanBodyBones selectedBone;
        if (!HumanBodyBoneSelections.TryGetValue(key, out selectedBone))
        {
            selectedBone = GetHumanBodyBoneForFragment(Fragment);
        }

        int popupIndex = System.Array.IndexOf(HumanBodyBoneOptions, selectedBone) + 1;
        if (popupIndex <= 0)
        {
            popupIndex = 0;
        }

        EditorGUILayout.BeginHorizontal();
        popupIndex = EditorGUILayout.Popup("Human Body Bone", popupIndex, HumanBodyBoneLabels);
        selectedBone = popupIndex == 0
            ? HumanBodyBones.LastBone
            : HumanBodyBoneOptions[popupIndex - 1];
        HumanBodyBoneSelections[key] = selectedBone;

        using (new EditorGUI.DisabledScope(popupIndex == 0))
        {
            if (GUILayout.Button("Auto Config Bone", GUILayout.Width(140)))
            {
                AutoConfigFragmentBone(Fragment, selectedBone);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    HumanBodyBones GetHumanBodyBoneForFragment(SerializedProperty Fragment)
    {
        var boneProperty = Fragment.FindPropertyRelative("bone");
        var fragmentBone = boneProperty != null ? boneProperty.objectReferenceValue as Transform : null;
        if (fragmentBone == null)
        {
            return HumanBodyBones.LastBone;
        }

        var dismember = target as RagdollDismembermentVisual;
        if (dismember == null)
        {
            return HumanBodyBones.LastBone;
        }

        var animator = dismember.transform.root.GetComponentInChildren<Animator>(true);
        if (animator == null || !animator.isHuman)
        {
            return HumanBodyBones.LastBone;
        }

        for (int i = 0; i < HumanBodyBoneOptions.Length; i++)
        {
            if (animator.GetBoneTransform(HumanBodyBoneOptions[i]) == fragmentBone)
            {
                return HumanBodyBoneOptions[i];
            }
        }

        return HumanBodyBones.LastBone;
    }

    void AutoConfigFragmentBone(SerializedProperty Fragment, HumanBodyBones humanBodyBone)
    {
        var dismember = target as RagdollDismembermentVisual;
        if (dismember == null)
        {
            return;
        }

        var animator = dismember.transform.root.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            Debug.LogWarning("Auto Config Bone failed. No Animator found under the character root.", dismember);
            return;
        }

        if (!animator.isHuman)
        {
            Debug.LogWarning("Auto Config Bone failed. Animator is not humanoid.", animator);
            return;
        }

        Transform bone = null;
        try
        {
            bone = animator.GetBoneTransform(humanBodyBone);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"Auto Config Bone failed for {humanBodyBone}: {exception.Message}", animator);
            return;
        }

        if (bone == null)
        {
            Debug.LogWarning($"Auto Config Bone failed. Animator has no bone mapped for {humanBodyBone}.", animator);
            return;
        }

        Fragment.FindPropertyRelative("bone").objectReferenceValue = bone;

        string fragmentName;
        if (TryGetFragmentName(humanBodyBone, out fragmentName))
        {
            Fragment.FindPropertyRelative("Name").stringValue = fragmentName;
        }
        else
        {
            Debug.LogWarning($"Auto Config Bone found bone {humanBodyBone}, but no runtime fragment name mapping exists.", dismember);
        }

        EditorUtility.SetDirty(dismember);
        SceneView.RepaintAll();
    }

    static bool TryGetFragmentName(HumanBodyBones humanBodyBone, out string fragmentName)
    {
        switch (humanBodyBone)
        {
            case HumanBodyBones.Head:
                fragmentName = CrossLink.RagdollBoneInfo.Head;
                return true;
            case HumanBodyBones.Spine:
            case HumanBodyBones.Chest:
            case HumanBodyBones.UpperChest:
                fragmentName = CrossLink.RagdollBoneInfo.Spine;
                return true;
            case HumanBodyBones.LeftUpperArm:
                fragmentName = CrossLink.RagdollBoneInfo.LUpArm;
                return true;
            case HumanBodyBones.LeftLowerArm:
                fragmentName = CrossLink.RagdollBoneInfo.LForeArm;
                return true;
            case HumanBodyBones.RightUpperArm:
                fragmentName = CrossLink.RagdollBoneInfo.RUpArm;
                return true;
            case HumanBodyBones.RightLowerArm:
                fragmentName = CrossLink.RagdollBoneInfo.RForeArm;
                return true;
            case HumanBodyBones.LeftUpperLeg:
                fragmentName = CrossLink.RagdollBoneInfo.LThigh;
                return true;
            case HumanBodyBones.LeftLowerLeg:
                fragmentName = CrossLink.RagdollBoneInfo.LCalf;
                return true;
            case HumanBodyBones.RightUpperLeg:
                fragmentName = CrossLink.RagdollBoneInfo.RThigh;
                return true;
            case HumanBodyBones.RightLowerLeg:
                fragmentName = CrossLink.RagdollBoneInfo.RCalf;
                return true;
            default:
                fragmentName = null;
                return false;
        }
    }

    static string GetHumanBodyBoneLabel(HumanBodyBones humanBodyBone)
    {
        string fragmentName;
        return TryGetFragmentName(humanBodyBone, out fragmentName) ? fragmentName : humanBodyBone.ToString();
    }

    private void DrawFragmentEffectEditor(SerializedProperty fragment, string name)
    {
        if (DrawPanel(EditorGUILayout.BeginVertical(), name, Color.gray) == PanelEvents.FoldUnfold)
        {
            fragment.FindPropertyRelative(name + "Details").boolValue = !fragment.FindPropertyRelative(name + "Details").boolValue;
        }

        if (fragment.FindPropertyRelative(name + "Details").boolValue)
        {
            EditorGUILayout.PropertyField(fragment.FindPropertyRelative(name));
            if (fragment.FindPropertyRelative(name).objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(fragment.FindPropertyRelative(name + "Position"));
                EditorGUILayout.PropertyField(fragment.FindPropertyRelative(name + "Rotation"));
                EditorGUILayout.PropertyField(fragment.FindPropertyRelative(name + "Size"));
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void AddFragment(SerializedProperty fragments)
    {
        Color[] colors = { Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.yellow };
        fragments.InsertArrayElementAtIndex(fragments.arraySize);
        SerializedProperty newFragment = fragments.GetArrayElementAtIndex(fragments.arraySize - 1);
        newFragment.FindPropertyRelative("Name").stringValue = "Fragment" + fragments.arraySize;
        newFragment.FindPropertyRelative("color").colorValue = colors[fragments.arraySize % colors.Length];
        newFragment.FindPropertyRelative("bone").objectReferenceValue = null;
        newFragment.FindPropertyRelative("SkinnedMeshes").ClearArray();
        newFragment.FindPropertyRelative("BoneEffect").objectReferenceValue = null;
        newFragment.FindPropertyRelative("BoneParentEffect").objectReferenceValue = null;
    }

    private void DrawFragmentsSettings()
    {
        serializedObject.Update();
        SerializedProperty fragments = serializedObject.FindProperty("Fragments");
        int i = 0;
        while (i < fragments.arraySize)
        {
            SerializedProperty fragment = fragments.GetArrayElementAtIndex(i);
            switch (DrawPanel(EditorGUILayout.BeginVertical(),
                        fragment.FindPropertyRelative("Name").stringValue,
                        fragment.FindPropertyRelative("color").colorValue,
                        i > 0))
            {
                case PanelEvents.NoEvent:
                    if (fragment.FindPropertyRelative("ShowProperties").boolValue)
                    {
                        DrawFragmentDetails(fragment, i, i == 0);
                        EditorGUILayout.Separator();
                    }
                    i++;
                    break;
                case PanelEvents.FoldUnfold:
                    fragment.FindPropertyRelative("ShowProperties").boolValue =
                        !fragment.FindPropertyRelative("ShowProperties").boolValue;
                    break;
                case PanelEvents.Delete:
                    fragments.DeleteArrayElementAtIndex(i);
                    break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
        }

        if (GUILayout.Button("Add fragment"))
        {
            AddFragment(fragments);
        }

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        DrawReadableStatus();
        if (needsReadableFix)
        {
            EditorGUILayout.HelpBox(
                "Detected that SkinnedMeshRenderer uses meshes without \"Read/Write Enabled\" option.\n" +
                "This prevents the dismemberment system from properly reading mesh data (vertex positions, bone weights, etc.).\n" +
                "It is recommended to enable this option for full functionality.",
                MessageType.Warning
            );

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Auto-fix all meshes", GUILayout.Height(30)))
            {
                AutoFixMeshesReadable();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Note: Enabling Read/Write increases memory usage (approximately 2-3x the original mesh size).\n" +
                "If memory is a concern, consider disabling this at build time, or only enable it for the meshes that need it.",
                MessageType.Info
            );
        }

        // Check if there are changes (user might have modified the mesh in Inspector)
        if (GUI.changed)
        {
            CheckMeshReadableStatus();
        }
        EditorGUILayout.Space(10);

        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("ignoreEdgeCreation"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ignoreDismember"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skinnedRenderers"), true);

        serializedObject.ApplyModifiedProperties();

        if (DrawPanel(EditorGUILayout.BeginVertical(), "View settings", Color.white) == PanelEvents.FoldUnfold)
        {
            ShowViewSettings = !ShowViewSettings;
        }

        if (ShowViewSettings)
        {
            DrawViewSettings();
        }

        EditorGUILayout.EndVertical();

        if (DrawPanel(EditorGUILayout.BeginVertical(), "Fragments settings", Color.white) == PanelEvents.FoldUnfold)
        {
            ShowFragmentsSettings = !ShowFragmentsSettings;
        }

        if (ShowFragmentsSettings)
        {
            DrawFragmentsSettings();
        }

        EditorGUILayout.EndVertical();
    }

    private bool hasCheckedReadable = false;
    private bool needsReadableFix = false;
    private Mesh[] problemMeshes;
    private string[] problemMeshNames;
    void CheckMeshReadableStatus()
    {
        var targetComponent = (RagdollDismembermentVisual)target;
        var allSkinnedMeshes = targetComponent.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        // Collect all non-readable meshes
        var problematicMeshes = allSkinnedMeshes
            .Where(smr => smr.sharedMesh != null && !smr.sharedMesh.isReadable)
            .Select(smr => smr.sharedMesh)
            .Distinct()
            .ToList();

        needsReadableFix = problematicMeshes.Count > 0;
        problemMeshes = problematicMeshes.ToArray();
        problemMeshNames = problemMeshes.Select(m => m.name).ToArray();

        if (!needsReadableFix)
        {
            hasCheckedReadable = true;
        }
    }


    void DrawReadableStatus()
    {
        if (!hasCheckedReadable)
        {
            EditorGUILayout.LabelField("Checking mesh readability...");
            return;
        }

        EditorGUILayout.LabelField("Mesh Readability Status", EditorStyles.boldLabel);

        var targetComponent = (RagdollDismembermentVisual)target;
        var allSkinnedMeshes = targetComponent.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        if (allSkinnedMeshes.Length == 0)
        {
            EditorGUILayout.HelpBox("No SkinnedMeshRenderer components found", MessageType.Info);
            return;
        }

        foreach (var smr in allSkinnedMeshes)
        {
            if (smr.sharedMesh == null)
            {
                EditorGUILayout.LabelField($"• {smr.gameObject.name}: Mesh is null", EditorStyles.miniLabel);
                continue;
            }

            var mesh = smr.sharedMesh;
            bool isReadable = mesh.isReadable;
            string meshName = $"{smr.gameObject.name} -> {mesh.name}";

            EditorGUILayout.BeginHorizontal();

            if (isReadable)
            {
                EditorGUILayout.LabelField($"✓ {meshName}", EditorStyles.miniLabel);
                GUI.color = Color.green;
                EditorGUILayout.LabelField("Readable", EditorStyles.miniLabel, GUILayout.Width(60));
                GUI.color = Color.white;
            }
            else
            {
                EditorGUILayout.LabelField($"✗ {meshName}", EditorStyles.miniLabel);
                GUI.color = Color.red;
                EditorGUILayout.LabelField("Not readable", EditorStyles.miniLabel, GUILayout.Width(60));
                GUI.color = Color.white;
            }

            if (!isReadable && mesh != null)
            {
                if (GUILayout.Button("Fix", GUILayout.Width(50)))
                {
                    SetMeshReadable(mesh, true);
                    CheckMeshReadableStatus(); // Re-check
                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    bool SetMeshReadable(Mesh mesh, bool readable)
    {
        if (mesh == null) return false;

        // Get the asset path
        string assetPath = AssetDatabase.GetAssetPath(mesh);
        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogWarning($"Mesh {mesh.name} is not a project asset, cannot modify import settings", mesh);
            return false;
        }

        // Try ModelImporter (FBX/OBJ etc.)
        var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;

        if (modelImporter != null)
        {
            // This mesh was imported from a 3D model file
            if (modelImporter.isReadable != readable)
            {
                modelImporter.isReadable = readable;
                modelImporter.SaveAndReimport();
                return true;
            }
            return true; // Already at target state
        }

        // Try as a standalone mesh asset (.asset file)
        var meshImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        if (meshImporter != null)
        {
            // Unity's standalone mesh assets also use ModelImporter
            meshImporter.isReadable = readable;
            meshImporter.SaveAndReimport();
            return true;
        }

        Debug.LogWarning($"Cannot handle asset type: {assetPath}", mesh);
        return false;
    }

    void AutoFixMeshesReadable()
    {
        bool anyFixed = false;
        var failedMeshes = new System.Collections.Generic.List<string>();

        foreach (var mesh in problemMeshes)
        {
            if (SetMeshReadable(mesh, true))
            {
                anyFixed = true;
                Debug.Log($"Enabled Read/Write for {mesh.name}");
            }
            else
            {
                failedMeshes.Add(mesh.name);
            }
        }

        if (anyFixed)
        {
            // Refresh the asset database
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (failedMeshes.Count > 0)
            {
                EditorUtility.DisplayDialog("Fix Partially Complete",
                    $"Successfully fixed some meshes, but the following meshes could not be automatically fixed:\n{string.Join("\n", failedMeshes)}\n\n" +
                    "You may need to manually enable Read/Write in the Import Settings.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Fix Successful",
                    $"Successfully enabled Read/Write for {problemMeshes.Length} mesh(es).\n\n" +
                    "RagdollDismembermentVisual can now properly read mesh data.",
                    "OK");
            }

            CheckMeshReadableStatus();
            Repaint();
        }
        else
        {
            EditorUtility.DisplayDialog("Fix Failed",
                "Unable to automatically fix mesh readability settings.\n\n" +
                "Possible reasons:\n" +
                "- Mesh comes from read-only resources (e.g., Unity built-in packages)\n" +
                "- Asset file is locked\n" +
                "- You may need to manually enable Read/Write in the FBX or Mesh Import Settings",
                "OK");
        }
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    private static void DrawGizmos(RagdollDismembermentVisual script, GizmoType gizmoType)
    {
        if (script.Fragments == null)
        {
            return;
        }

        if (DrawBounds)
        {
            foreach (BodyFragment fragment in script.Fragments)
            {
                if (fragment == null || fragment.bone == null)
                {
                    continue;
                }

                Gizmos.color = fragment.color;
                Gizmos.matrix = fragment.bone.localToWorldMatrix *
                                Matrix4x4.TRS(fragment.Position, Quaternion.Euler(fragment.Rotation), fragment.Size);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

                Gizmos.matrix = fragment.bone.localToWorldMatrix;
                if (fragment.cutPlaneNormal == Vector3.zero)
                {
                    continue;
                }

                Gizmos.color = Color.red;
                Debug.DrawLine(fragment.GetCutPlanePos(), fragment.GetCutPlanePos() + fragment.GetCutPlaneBinormal() * (fragment.cutRadius * 3), Gizmos.color);
                CrossLink.DebugDraw.DrawPlane(fragment.GetCutPlanePos(), fragment.GetCutPlaneNormal(), fragment.cutRadius, Gizmos.color, 0f);
            }
        }

        if (DrawEffects)
        {
            foreach (BodyFragment fragment in script.Fragments)
            {
                if (fragment == null || fragment.bone == null)
                {
                    continue;
                }

                Gizmos.color = fragment.color;
                if (fragment.BoneEffect != null)
                {
                    MeshFilter filter = fragment.BoneEffect.GetComponentInChildren<MeshFilter>();
                    if (filter != null && filter.sharedMesh != null)
                    {
                        Gizmos.matrix =
                            Matrix4x4.TRS(fragment.bone.position, fragment.bone.rotation, fragment.bone.localScale) *
                            Matrix4x4.TRS(fragment.BoneEffectPosition, Quaternion.Euler(fragment.BoneEffectRotation), fragment.BoneEffectSize);
                        Gizmos.DrawMesh(filter.sharedMesh);
                    }
                }

                if (fragment.BoneParentEffect != null && fragment.bone.parent != null)
                {
                    MeshFilter filter = fragment.BoneParentEffect.GetComponentInChildren<MeshFilter>();
                    if (filter != null && filter.sharedMesh != null)
                    {
                        Gizmos.matrix =
                            Matrix4x4.TRS(fragment.bone.parent.position, fragment.bone.parent.rotation, fragment.bone.parent.localScale) *
                            Matrix4x4.TRS(fragment.BoneParentEffectPosition, Quaternion.Euler(fragment.BoneParentEffectRotation), fragment.BoneParentEffectSize);
                        Gizmos.DrawMesh(filter.sharedMesh);
                    }
                }
            }
        }

        if (DrawWireframe && script.Fragments.Count > 0 && script.Fragments[0].bone)
        {
            script.Fragments[0].SkinnedMeshes = script.Fragments[0].bone.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();

            foreach (SkinnedMeshRenderer skinnedMesh in script.Fragments[0].SkinnedMeshes)
            {
                if (skinnedMesh == null || skinnedMesh.sharedMesh == null)
                {
                    continue;
                }

                Gizmos.matrix = skinnedMesh.transform.localToWorldMatrix * Matrix4x4.Scale(skinnedMesh.sharedMesh.bindposes[0].lossyScale);
                Mesh mesh = skinnedMesh.sharedMesh;
                int[] tris = mesh.triangles;

                List<BodyFragment> sortedFragments = script.Fragments
                    .Where(f => f != null && f.bone != null)
                    .OrderBy(f => GetDepth(f.bone))
                    .Where(f => f != script.Fragments[0])
                    .ToList();

                foreach (BodyFragment fragment in sortedFragments)
                {
                    if (!fragment.ShowWireframe)
                    {
                        continue;
                    }

                    script.GetSelection(skinnedMesh, fragment);
                    List<int> triangles = new List<int>();
                    for (int i = 0; i < tris.Length; i += 3)
                    {
                        if (script.SelectionMask[tris[i]] &&
                            script.SelectionMask[tris[i + 1]] &&
                            script.SelectionMask[tris[i + 2]])
                        {
                            triangles.Add(tris[i]);
                            triangles.Add(tris[i + 1]);
                            triangles.Add(tris[i + 2]);
                        }
                    }

                    if (triangles.Count == 0)
                    {
                        continue;
                    }

                    Gizmos.color = fragment.color;
                    Mesh selectionMesh = mesh.Copy();
                    selectionMesh.subMeshCount = 1;
                    selectionMesh.SetTriangles(triangles, 0);
                    Gizmos.DrawWireMesh(selectionMesh);
                }
            }
        }
    }

    private static int GetDepth(Transform bone)
    {
        int depth = 0;
        Transform current = bone;
        while (current != null)
        {
            depth++;
            current = current.parent;
        }
        return depth;
    }

    private enum PanelEvents
    {
        NoEvent,
        FoldUnfold,
        Delete
    }
}
