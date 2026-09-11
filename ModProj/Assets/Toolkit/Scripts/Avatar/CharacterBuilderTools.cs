#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrossLink
{
    internal class CharacterBuilderTools : EditorWindow
    {
        private const string WindowTitle = "Character Builder Tools";
        private const string DefaultAvatarPath = "Assets/Toolkit/Prefabs/Skin/Warrior_Rig_TPose Variant.prefab";
        private const string HandPoseLibraryPath = "Assets/Toolkit/HandPoseHelper/HandPoseLib.asset";
        private const string TargetLayerName = "InvisibleFPS";
        private const string AndroidPlatformName = "Android";
        private const string SpellScrystalTexturePath = "assets/toolkit/spellscrystal/textures/";
        private const string HandPoseHelperTexturePath = "assets/toolkit/handposehelper/res/";
        private const int MaxBaseTextureSize = 2048;
        private const int RecommendedTextureSize = 1024;
        private const int AndroidCompressionQuality = 50;
        private const long TotalTexturePixelWarningThreshold = 16L * 1024L * 1024L;
        private const int MeshVertexWarningThreshold = 50000;
        private const int MeshTriangleWarningThreshold = 100000;
        private const int TotalMeshVertexWarningThreshold = 100000;
        private const int TotalMeshTriangleWarningThreshold = 200000;
        private static readonly int[] TextureMaxSizeOptions = { 512, 1024, 2048 };
        private static readonly string[] TextureMaxSizeOptionLabels = { "512", "1024", "2048" };
        private const float HeadVertexWeightThreshold = 0.65f;
        private const float HeadRendererVertexRatioThreshold = 0.75f;
        private const float MaxHeadBoundsHeight = 1.2f;
        private const float MaxHeadBoundsCenterDistance = 0.75f;
        private const float MinHeightScale = 0.5f;
        private const float MaxHeightScale = 4.5f;
        private readonly List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
        private readonly List<TextureCheckResult> textureResults = new List<TextureCheckResult>();
        private readonly List<string> meshWarnings = new List<string>();
        private GameObject avatarPrefab;
        private GameObject characterTPose;
        private ItemInfoConfig itemInfoConfig;
        private float characterHeight = 2f;
        private int currentIndex;
        private int currentTextureIndex;
        private int textureReviewRevision;
        private Vector2 scrollPosition;
        private Vector2 meshScrollPosition;
        private string checkMessage;
        private string textureCheckMessage;
        private string meshCheckMessage;
        private string handPoseOffsetMessage;
        private bool textureCheckHasWarnings;
        private bool meshCheckHasWarnings;

        private class TextureCheckResult
        {
            public Texture2D texture;
            public TextureImporter importer;
            public string assetPath;
            public string propertyName;
            public int width;
            public int height;
            public int androidMaxTextureSize;
            public int recommendedMaxTextureSize;
            public int selectedMaxTextureSizeIndex;
        }

        [MenuItem("Tools/Character Builder Tools")]
        public static void Open()
        {
            CharacterBuilderTools window = GetWindow<CharacterBuilderTools>(WindowTitle);
            window.minSize = new Vector2(380f, 210f);
            window.Show();
        }

        public static void Open(GameObject prefab, float height)
        {
            CharacterBuilderTools window = GetWindow<CharacterBuilderTools>(WindowTitle);
            window.minSize = new Vector2(380f, 210f);
            window.avatarPrefab = prefab;
            window.characterTPose = null;
            window.itemInfoConfig = null;
            window.characterHeight = Mathf.Max(0.1f, height);
            window.ClearAllResults();
            window.Show();
        }

        public static void Open(GameObject prefab, float height, ItemInfoConfig config)
        {
            CharacterBuilderTools window = GetWindow<CharacterBuilderTools>(WindowTitle);
            window.minSize = new Vector2(380f, 210f);
            window.avatarPrefab = prefab;
            window.characterTPose = null;
            window.itemInfoConfig = config;
            window.characterHeight = Mathf.Max(0.1f, height);
            window.ClearAllResults();
            window.Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void SetHeadRendererCandidates(List<SkinnedMeshRenderer> candidates)
        {
            renderers.Clear();
            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    SkinnedMeshRenderer renderer = candidates[i];
                    if (renderer != null
                        && renderer.gameObject != null
                        && renderer.gameObject.layer != LayerDefine.InvisibleFPSLayer)
                    {
                        renderers.Add(renderer);
                    }
                }
            }

            currentIndex = 0;
            FocusCurrentRenderer(true);
            Repaint();
        }

        private void ClearHeadRendererReview()
        {
            renderers.Clear();
            currentIndex = 0;
            SceneView.RepaintAll();
            Repaint();
        }

        private void ClearAllResults()
        {
            ClearHeadRendererReview();
            textureResults.Clear();
            meshWarnings.Clear();
            currentTextureIndex = 0;
            checkMessage = null;
            textureCheckMessage = null;
            meshCheckMessage = null;
            handPoseOffsetMessage = null;
            textureCheckHasWarnings = false;
            meshCheckHasWarnings = false;
            Repaint();
        }

        private void OnGUI()
        {
            SkipInvalidRenderers();
            SkipInvalidTextures();

            EditorGUILayout.LabelField("Character Builder Tools", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            GameObject newAvatarPrefab = (GameObject)EditorGUILayout.ObjectField("Character Prefab", avatarPrefab, typeof(GameObject), true);
            if (newAvatarPrefab != avatarPrefab)
            {
                avatarPrefab = newAvatarPrefab;
                ClearAllResults();
            }
            characterHeight = EditorGUILayout.FloatField("Character Height", characterHeight);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Common Checks", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Check Material Textures"))
            {
                RunMaterialTextureCheck();
            }

            if (GUILayout.Button("Check Mesh Complexity"))
            {
                RunMeshComplexityCheck();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Avatar Checks", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("For Avatar mods only.", MessageType.Info);
            if (GUILayout.Button("Check Camera Occlusion"))
            {
                RunHeadOcclusionCheck();
            }

            if (!string.IsNullOrEmpty(checkMessage))
            {
                EditorGUILayout.HelpBox(checkMessage, MessageType.Info);
            }

            if (!string.IsNullOrEmpty(textureCheckMessage))
            {
                EditorGUILayout.HelpBox(textureCheckMessage, textureCheckHasWarnings ? MessageType.Warning : MessageType.Info);
            }

            if (!string.IsNullOrEmpty(meshCheckMessage))
            {
                EditorGUILayout.HelpBox(meshCheckMessage, meshCheckHasWarnings ? MessageType.Warning : MessageType.Info);
            }

            if (renderers.Count == 0 || currentIndex >= renderers.Count)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("No active camera occlusion review. Assign an avatar prefab and run a check.", MessageType.Info);
            }
            else
            {
                DrawHeadOcclusionReview();
            }

            DrawTextureReview();
            DrawMeshWarnings();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Configure hand offset", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Select the Avatar in the FBX, click ConfigureAvatar in the Inspector, then drag the model root node to Character T-Pose.",
                MessageType.Info);
            characterTPose = (GameObject)EditorGUILayout.ObjectField(
                "Character T-Pose",
                characterTPose,
                typeof(GameObject),
                true);
            itemInfoConfig = (ItemInfoConfig)EditorGUILayout.ObjectField("Item Info Config", itemInfoConfig, typeof(ItemInfoConfig), false);

            if (GUILayout.Button("Calculate Avatar Hand Pose Offset"))
            {
                CalculateAvatarHandPoseOffset();
            }

            if (GUILayout.Button("Generate Avatar Handposes From Offset"))
            {
                GenerateAvatarHandposesFromOffset();
            }

            if (!string.IsNullOrEmpty(handPoseOffsetMessage))
            {
                EditorGUILayout.HelpBox(handPoseOffsetMessage, MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
        }

        private void DrawHeadOcclusionReview()
        {
            SkinnedMeshRenderer renderer = renderers[currentIndex];
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Head Occlusion Check", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Candidate {currentIndex + 1} / {renderers.Count}", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.ObjectField("Renderer", renderer, typeof(SkinnedMeshRenderer), true);
            EditorGUILayout.ObjectField("GameObject", renderer.gameObject, typeof(GameObject), true);
            EditorGUILayout.LabelField("Path", GetTransformPath(renderer.transform));
            EditorGUILayout.LabelField("Current Layer", GetLayerDisplayName(renderer.gameObject.layer));
            EditorGUILayout.LabelField("Target Layer", TargetLayerName);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This renderer may block the first-person camera. Inspect the highlighted object in the Scene view, then choose an action.", MessageType.Info);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set InvisibleFPS"))
            {
                SetCurrentRendererLayer();
                MoveNext();
            }

            if (GUILayout.Button("Skip"))
            {
                MoveNext();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTextureReview()
        {
            SkipInvalidTextures();
            if (textureResults.Count == 0 || currentTextureIndex >= textureResults.Count)
            {
                return;
            }

            TextureCheckResult result = textureResults[currentTextureIndex];
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Material Texture Check", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Candidate {currentTextureIndex + 1} / {textureResults.Count}", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField("Texture", result.texture, typeof(Texture2D), false);
            EditorGUILayout.LabelField("Path", result.assetPath);
            EditorGUILayout.LabelField("Material Property", result.propertyName);
            EditorGUILayout.LabelField("Source Size", $"{result.width} x {result.height}");
            EditorGUILayout.LabelField("Android Max Size", result.androidMaxTextureSize.ToString());
            EditorGUILayout.LabelField("Recommended Max Size", result.recommendedMaxTextureSize.ToString());
            GUI.SetNextControlName(GetTexturePopupControlName(result));
            int selectedMaxTextureSizeIndex = EditorGUILayout.Popup("Target Android Max Size", result.selectedMaxTextureSizeIndex, TextureMaxSizeOptionLabels);
            if (selectedMaxTextureSizeIndex != result.selectedMaxTextureSizeIndex)
            {
                result.selectedMaxTextureSizeIndex = selectedMaxTextureSizeIndex;
                ApplySelectedTextureAndroidMaxSize(result);
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.HelpBox("BaseMap and NormalMap are recommended to use 2K, while other textures are recommended to use 1K or below. This setting only affects the Max Size import override for the Android platform.", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Use Recommended"))
            {
                ApplyRecommendedTextureAndroidMaxSize(result);
                MoveNextTexture();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("Next Texture"))
            {
                MoveNextTexture();
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CalculateAvatarHandPoseOffset()
        {
            if (itemInfoConfig == null)
            {
                Debug.LogError("CalculateAvatarHandPoseOffset failed: assign the target ItemInfoConfig first.");
                return;
            }

            if (characterTPose == null)
            {
                Debug.LogError("CalculateAvatarHandPoseOffset failed: assign Character T-Pose in CharacterBuilderTools first.", itemInfoConfig);
                return;
            }

            GameObject defaultAvatarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultAvatarPath);
            if (defaultAvatarPrefab == null)
            {
                Debug.LogError($"CalculateAvatarHandPoseOffset failed: default avatar prefab was not found at {DefaultAvatarPath}.");
                return;
            }

            GameObject defaultAvatarInstance = null;
            GameObject targetAvatarInstance = null;
            try
            {
                defaultAvatarInstance = PrefabUtility.InstantiatePrefab(defaultAvatarPrefab) as GameObject;
                if (defaultAvatarInstance == null)
                {
                    Debug.LogError("CalculateAvatarHandPoseOffset failed: could not instantiate the default avatar prefab.");
                    return;
                }

                defaultAvatarInstance.hideFlags = HideFlags.HideAndDontSave;
                GameObject targetAvatarObject = characterTPose;
                if (EditorUtility.IsPersistent(characterTPose))
                {
                    targetAvatarInstance = PrefabUtility.InstantiatePrefab(characterTPose) as GameObject;
                    if (targetAvatarInstance == null)
                    {
                        Debug.LogError("CalculateAvatarHandPoseOffset failed: could not instantiate Character T-Pose.", itemInfoConfig);
                        return;
                    }

                    targetAvatarInstance.hideFlags = HideFlags.HideAndDontSave;
                    targetAvatarObject = targetAvatarInstance;
                }

                Animator defaultAnimator = GetRequiredHumanoidAnimator(defaultAvatarInstance, "default avatar");
                Animator avatarAnimator = GetRequiredHumanoidAnimator(targetAvatarObject, "target avatar T-Pose");
                if (defaultAnimator == null || avatarAnimator == null)
                {
                    return;
                }

                Transform defaultLeftHand = GetRequiredHandBone(defaultAnimator, HumanBodyBones.LeftHand, "default avatar");
                Transform defaultRightHand = GetRequiredHandBone(defaultAnimator, HumanBodyBones.RightHand, "default avatar");
                Transform avatarLeftHand = GetRequiredHandBone(avatarAnimator, HumanBodyBones.LeftHand, "target avatar T-Pose");
                Transform avatarRightHand = GetRequiredHandBone(avatarAnimator, HumanBodyBones.RightHand, "target avatar T-Pose");
                if (defaultLeftHand == null || defaultRightHand == null || avatarLeftHand == null || avatarRightHand == null)
                {
                    return;
                }

                AvatarInfo avatarInfo = FindAvatarInfo(itemInfoConfig, characterTPose.name, "CalculateAvatarHandPoseOffset");
                if (avatarInfo == null)
                {
                    return;
                }

                Undo.RecordObject(itemInfoConfig, "Calculate Avatar Hand Pose Offset");
                if (avatarInfo.handposeOffset == null)
                {
                    avatarInfo.handposeOffset = new HandPoseOffset();
                }

                Quaternion defaultLeftHandRootRotation = GetRootRelativeRotation(defaultAnimator.transform, defaultLeftHand);
                Quaternion avatarLeftHandRootRotation = GetRootRelativeRotation(avatarAnimator.transform, avatarLeftHand);
                Quaternion defaultRightHandRootRotation = GetRootRelativeRotation(defaultAnimator.transform, defaultRightHand);
                Quaternion avatarRightHandRootRotation = GetRootRelativeRotation(avatarAnimator.transform, avatarRightHand);

                avatarInfo.handposeOffset.leftHandRotationOffset =
                    Quaternion.Inverse(defaultLeftHandRootRotation) * avatarLeftHandRootRotation;
                avatarInfo.handposeOffset.rightHandRotationOffset =
                    Quaternion.Inverse(defaultRightHandRootRotation) * avatarRightHandRootRotation;

                EditorUtility.SetDirty(itemInfoConfig);
                AssetDatabase.SaveAssets();
                handPoseOffsetMessage = $"Updated hand pose offset for {avatarInfo.avatarName}.";
                Debug.Log($"CalculateAvatarHandPoseOffset succeeded: updated {avatarInfo.avatarName} in {AssetDatabase.GetAssetPath(itemInfoConfig)}.", itemInfoConfig);
                Repaint();
            }
            finally
            {
                if (targetAvatarInstance != null)
                {
                    DestroyImmediate(targetAvatarInstance);
                }

                if (defaultAvatarInstance != null)
                {
                    DestroyImmediate(defaultAvatarInstance);
                }
            }
        }

        private void GenerateAvatarHandposesFromOffset()
        {
            if (itemInfoConfig == null)
            {
                Debug.LogError("GenerateAvatarHandposesFromOffset failed: assign the target ItemInfoConfig first.");
                return;
            }

            if (characterTPose == null)
            {
                Debug.LogError("GenerateAvatarHandposesFromOffset failed: assign Character T-Pose in CharacterBuilderTools first.", itemInfoConfig);
                return;
            }

            AvatarInfo avatarInfo = FindAvatarInfo(itemInfoConfig, characterTPose.name, "GenerateAvatarHandposesFromOffset");
            if (avatarInfo == null)
            {
                return;
            }

            if (avatarInfo.handposeOffset == null)
            {
                Debug.LogError("GenerateAvatarHandposesFromOffset failed: AvatarInfo has no HandPoseOffset.", itemInfoConfig);
                return;
            }

            if (!IsValidQuaternion(avatarInfo.handposeOffset.leftHandRotationOffset)
                || !IsValidQuaternion(avatarInfo.handposeOffset.rightHandRotationOffset))
            {
                Debug.LogError("GenerateAvatarHandposesFromOffset failed: HandPoseOffset contains an invalid hand rotation.", itemInfoConfig);
                return;
            }

            HandPoseLib handPoseLibrary = AssetDatabase.LoadAssetAtPath<HandPoseLib>(HandPoseLibraryPath);
            if (handPoseLibrary == null || handPoseLibrary.poseDefines == null)
            {
                Debug.LogError($"GenerateAvatarHandposesFromOffset failed: HandPoseLib was not found or has no pose definitions at {HandPoseLibraryPath}.");
                return;
            }

            HashSet<string> existingPoseNames = new HashSet<string>();
            if (avatarInfo.handposes != null)
            {
                for (int i = 0; i < avatarInfo.handposes.Length; i++)
                {
                    HandPoseModifier existingPose = avatarInfo.handposes[i];
                    if (existingPose != null && !string.IsNullOrEmpty(existingPose.name))
                    {
                        existingPoseNames.Add(existingPose.name);
                    }
                }
            }

            List<HandPoseModifier> generatedPoses = new List<HandPoseModifier>();
            for (int i = 0; i < handPoseLibrary.poseDefines.Length; i++)
            {
                HandPoseLib.HandPoseDefine poseDefine = handPoseLibrary.poseDefines[i];
                if (poseDefine == null || string.IsNullOrEmpty(poseDefine.poseName))
                {
                    Debug.LogError($"GenerateAvatarHandposesFromOffset failed: HandPoseLib contains an empty pose definition at index {i}.");
                    return;
                }

                if (existingPoseNames.Contains(poseDefine.poseName))
                {
                    continue;
                }

                if (!TryBuildHandPoseModifier(poseDefine, avatarInfo.handposeOffset, out HandPoseModifier generatedPose, out string error))
                {
                    Debug.LogError($"GenerateAvatarHandposesFromOffset failed for {poseDefine.poseName}: {error}.", itemInfoConfig);
                    return;
                }

                generatedPoses.Add(generatedPose);
                existingPoseNames.Add(poseDefine.poseName);
            }

            if (generatedPoses.Count == 0)
            {
                handPoseOffsetMessage = $"No missing handposes found for {avatarInfo.avatarName}; existing handposes were preserved.";
                Debug.Log($"GenerateAvatarHandposesFromOffset skipped: all default handposes already exist for {avatarInfo.avatarName}.", itemInfoConfig);
                Repaint();
                return;
            }

            Undo.RecordObject(itemInfoConfig, "Generate Avatar Handposes From Offset");
            List<HandPoseModifier> mergedPoses = new List<HandPoseModifier>();
            if (avatarInfo.handposes != null)
            {
                mergedPoses.AddRange(avatarInfo.handposes);
            }

            mergedPoses.AddRange(generatedPoses);
            avatarInfo.handposes = mergedPoses.ToArray();
            EditorUtility.SetDirty(itemInfoConfig);
            AssetDatabase.SaveAssets();
            handPoseOffsetMessage = $"Added {generatedPoses.Count} handpose(s) for {avatarInfo.avatarName}; existing handposes were preserved.";
            Debug.Log($"GenerateAvatarHandposesFromOffset succeeded: added {generatedPoses.Count} handpose(s) for {avatarInfo.avatarName}.", itemInfoConfig);
            Repaint();
        }

        private static bool TryBuildHandPoseModifier(HandPoseLib.HandPoseDefine poseDefine, HandPoseOffset handPoseOffset, out HandPoseModifier modifier, out string error)
        {
            modifier = null;
            error = null;
            if (poseDefine.handPoses == null || poseDefine.handPoses.Length <= RagdollBoneInfo.LEFT_HAND)
            {
                error = "the pose does not contain both right and left hand presets";
                return false;
            }

            HandPoseSetup rightSetup = GetHandPoseSetup(poseDefine.handPoses[RagdollBoneInfo.RIGHT_HAND]);
            HandPoseSetup leftSetup = GetHandPoseSetup(poseDefine.handPoses[RagdollBoneInfo.LEFT_HAND]);
            if (rightSetup == null || leftSetup == null)
            {
                error = "the pose is missing a HandPoseSetup";
                return false;
            }

            if (rightSetup.preset == null || leftSetup.preset == null)
            {
                error = "the pose is missing a HandPosePreset";
                return false;
            }

            if (rightSetup.preset.fingerWeight == null || leftSetup.preset.fingerWeight == null)
            {
                error = "the pose is missing finger weights";
                return false;
            }

            Quaternion rightRotation = NormalizeQuaternion(rightSetup.transform.localRotation * handPoseOffset.rightHandRotationOffset);
            Quaternion leftRotation = NormalizeQuaternion(leftSetup.transform.localRotation * handPoseOffset.leftHandRotationOffset);
            if (!IsValidQuaternion(rightRotation) || !IsValidQuaternion(leftRotation))
            {
                error = "the generated hand rotation is invalid";
                return false;
            }

            modifier = new HandPoseModifier
            {
                name = poseDefine.poseName,
                leftHandPosition = leftSetup.transform.localPosition,
                leftHandRotation = leftRotation,
                rightHandPosition = rightSetup.transform.localPosition,
                rightHandRotation = rightRotation,
                fingerWeight = CopyFingerWeightList(leftSetup.preset.fingerWeight)
            };
            return true;
        }

        private static HandPoseSetup GetHandPoseSetup(UnityEngine.Object handPoseObject)
        {
            GameObject handPosePrefab = handPoseObject as GameObject;
            return handPosePrefab != null ? handPosePrefab.GetComponentInChildren<HandPoseSetup>(true) : null;
        }

        private static float[] CopyFingerWeightList(List<float> source)
        {
            float[] copy = new float[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }

        private static Quaternion GetRootRelativeRotation(Transform root, Transform source)
        {
            if (root == null || source == null)
            {
                return Quaternion.identity;
            }

            return Quaternion.Inverse(root.rotation) * source.rotation;
        }

        private static bool IsValidQuaternion(Quaternion value)
        {
            float squaredMagnitude = value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w;
            return !float.IsNaN(value.x)
                && !float.IsNaN(value.y)
                && !float.IsNaN(value.z)
                && !float.IsNaN(value.w)
                && !float.IsInfinity(value.x)
                && !float.IsInfinity(value.y)
                && !float.IsInfinity(value.z)
                && !float.IsInfinity(value.w)
                && squaredMagnitude > 0.000001f;
        }

        private static Quaternion NormalizeQuaternion(Quaternion value)
        {
            float squaredMagnitude = value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w;
            if (squaredMagnitude <= 0.000001f)
            {
                return Quaternion.identity;
            }

            float inverseMagnitude = 1f / Mathf.Sqrt(squaredMagnitude);
            return new Quaternion(
                value.x * inverseMagnitude,
                value.y * inverseMagnitude,
                value.z * inverseMagnitude,
                value.w * inverseMagnitude);
        }

        private static Animator GetRequiredHumanoidAnimator(GameObject avatarObject, string label)
        {
            Animator[] animators = avatarObject.GetComponentsInChildren<Animator>(true);
            if (animators == null || animators.Length == 0)
            {
                Debug.LogError($"CalculateAvatarHandPoseOffset failed: {label} is missing an Animator.");
                return null;
            }

            for (int i = 0; i < animators.Length; i++)
            {
                Animator animator = animators[i];
                if (animator != null && animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman)
                {
                    return animator;
                }
            }

            Debug.LogError($"CalculateAvatarHandPoseOffset failed: {label} has no valid Humanoid Animator.");
            return null;
        }

        private static Transform GetRequiredHandBone(Animator animator, HumanBodyBones bone, string label)
        {
            Transform hand = animator.GetBoneTransform(bone);
            if (hand == null)
            {
                Debug.LogError($"CalculateAvatarHandPoseOffset failed: {label} is missing HumanBodyBones.{bone}.");
            }

            return hand;
        }

        private static AvatarInfo FindAvatarInfo(ItemInfoConfig config, string avatarObjectName, string operationName)
        {
            if (config.avatarInfo == null || config.avatarInfo.Length == 0)
            {
                Debug.LogError($"{operationName} failed: ItemInfoConfig has no AvatarInfo entries.", config);
                return null;
            }

            if (config.avatarInfo.Length == 1)
            {
                if (config.avatarInfo[0] == null)
                {
                    Debug.LogError($"{operationName} failed: ItemInfoConfig contains an empty AvatarInfo entry.", config);
                    return null;
                }

                return config.avatarInfo[0];
            }

            string targetName = TrimCloneSuffix(avatarObjectName);
            string targetNameWithoutPrefix = StripAddressablePrefix(targetName);
            for (int i = 0; i < config.avatarInfo.Length; i++)
            {
                AvatarInfo info = config.avatarInfo[i];
                if (info == null || string.IsNullOrEmpty(info.avatarName))
                {
                    continue;
                }

                string infoName = TrimCloneSuffix(info.avatarName);
                if (string.Equals(infoName, targetName, System.StringComparison.Ordinal)
                    || string.Equals(StripAddressablePrefix(infoName), targetNameWithoutPrefix, System.StringComparison.Ordinal))
                {
                    return info;
                }
            }

            Debug.LogError($"{operationName} failed: no AvatarInfo matches target avatar {avatarObjectName}.", config);
            return null;
        }

        private static string StripAddressablePrefix(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            AddressableConfig addressableConfig = AddressableConfig.GetConfig();
            string prefix = addressableConfig != null ? addressableConfig.GetPrefix() : string.Empty;
            return !string.IsNullOrEmpty(prefix) && value.StartsWith(prefix, System.StringComparison.Ordinal)
                ? value.Substring(prefix.Length)
                : value;
        }

        private static string TrimCloneSuffix(string value)
        {
            const string cloneSuffix = "(Clone)";
            if (!string.IsNullOrEmpty(value) && value.EndsWith(cloneSuffix, System.StringComparison.Ordinal))
            {
                return value.Substring(0, value.Length - cloneSuffix.Length);
            }

            return value;
        }

        private void DrawMeshWarnings()
        {
            if (meshWarnings.Count == 0)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mesh Complexity Warnings", EditorStyles.boldLabel);
            meshScrollPosition = EditorGUILayout.BeginScrollView(meshScrollPosition, GUILayout.MinHeight(90f), GUILayout.MaxHeight(180f));
            for (int i = 0; i < meshWarnings.Count; i++)
            {
                EditorGUILayout.HelpBox(meshWarnings[i], MessageType.Warning);
            }
            EditorGUILayout.EndScrollView();
        }

        private void RunHeadOcclusionCheck()
        {
            if (avatarPrefab == null)
            {
                ClearHeadRendererReview();
                checkMessage = "Assign an avatar prefab before running the camera occlusion check.";
                return;
            }

            Animator animator = avatarPrefab.transform.root.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                ClearHeadRendererReview();
                checkMessage = "No Animator found under the assigned avatar prefab.";
                return;
            }

            List<SkinnedMeshRenderer> candidates = new List<SkinnedMeshRenderer>();
            var skinnedMeshRenderers = avatarPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var renderer in skinnedMeshRenderers)
            {
                if (renderer == null || renderer.gameObject == null || renderer.sharedMesh == null)
                {
                    continue;
                }

                if (renderer.gameObject.layer == LayerDefine.InvisibleFPSLayer)
                {
                    continue;
                }

                if (IsLikelyHeadRenderer(renderer, animator))
                {
                    candidates.Add(renderer);
                }
            }

            SetHeadRendererCandidates(candidates);
            checkMessage = candidates.Count > 0
                ? $"Found {candidates.Count} possible camera occlusion renderer(s)."
                : "No possible camera occlusion renderers found.";
        }

        private void RunMaterialTextureCheck()
        {
            textureResults.Clear();
            currentTextureIndex = 0;
            textureReviewRevision++;
            textureCheckHasWarnings = false;

            if (avatarPrefab == null)
            {
                textureCheckMessage = "Assign a character prefab before running the material texture check.";
                Repaint();
                return;
            }

            HashSet<Texture2D> checkedTextures = new HashSet<Texture2D>();
            long totalTexturePixels = 0L;
            Renderer[] avatarRenderers = avatarPrefab.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < avatarRenderers.Length; i++)
            {
                Renderer avatarRenderer = avatarRenderers[i];
                if (avatarRenderer == null || avatarRenderer.sharedMaterials == null)
                {
                    continue;
                }

                Material[] materials = avatarRenderer.sharedMaterials;
                for (int j = 0; j < materials.Length; j++)
                {
                    AddMaterialTextures(materials[j], checkedTextures, ref totalTexturePixels);
                }
            }

            textureCheckHasWarnings = totalTexturePixels > TotalTexturePixelWarningThreshold;
            textureCheckMessage = textureCheckHasWarnings
                ? $"Found {textureResults.Count} material texture(s), total source size is about {FormatMegapixels(totalTexturePixels)}MP. Too many avatar textures may cause stutter in game."
                : $"Found {textureResults.Count} material texture(s), total source size is about {FormatMegapixels(totalTexturePixels)}MP.";
            Repaint();
        }

        private void AddMaterialTextures(Material material, HashSet<Texture2D> checkedTextures, ref long totalTexturePixels)
        {
            if (material == null || material.shader == null)
            {
                return;
            }

            int propertyCount = ShaderUtil.GetPropertyCount(material.shader);
            for (int i = 0; i < propertyCount; i++)
            {
                if (ShaderUtil.GetPropertyType(material.shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    continue;
                }

                string propertyName = ShaderUtil.GetPropertyName(material.shader, i);
                Texture2D texture = material.GetTexture(propertyName) as Texture2D;
                if (texture == null || checkedTextures.Contains(texture))
                {
                    continue;
                }

                checkedTextures.Add(texture);
                AddTextureCheckResult(texture, propertyName, ref totalTexturePixels);
            }
        }

        private void AddTextureCheckResult(Texture2D texture, string propertyName, ref long totalTexturePixels)
        {
            string assetPath = AssetDatabase.GetAssetPath(texture);
            if (IsIgnoredToolTexturePath(assetPath))
            {
                return;
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            TryGetSourceTextureSize(importer, texture, out int width, out int height);
            totalTexturePixels += (long)Mathf.Max(0, width) * Mathf.Max(0, height);

            TextureImporterPlatformSettings androidSettings = importer.GetPlatformTextureSettings(AndroidPlatformName);
            int androidMaxTextureSize = androidSettings.overridden ? androidSettings.maxTextureSize : importer.maxTextureSize;
            int recommendedMaxTextureSize = GetRecommendedTextureMaxSize(propertyName, importer, width, height);

            textureResults.Add(new TextureCheckResult
            {
                texture = texture,
                importer = importer,
                assetPath = assetPath,
                propertyName = propertyName,
                width = width,
                height = height,
                androidMaxTextureSize = androidMaxTextureSize,
                recommendedMaxTextureSize = recommendedMaxTextureSize,
                selectedMaxTextureSizeIndex = GetTextureMaxSizeOptionIndex(recommendedMaxTextureSize)
            });
        }

        private void ApplySelectedTextureAndroidMaxSize(TextureCheckResult result)
        {
            if (result == null || result.importer == null)
            {
                return;
            }

            int selectedMaxTextureSize = TextureMaxSizeOptions[Mathf.Clamp(result.selectedMaxTextureSizeIndex, 0, TextureMaxSizeOptions.Length - 1)];
            ApplyTextureAndroidMaxSize(result, selectedMaxTextureSize);
        }

        private void ApplyRecommendedTextureAndroidMaxSize(TextureCheckResult result)
        {
            if (result == null || result.importer == null)
            {
                return;
            }

            int recommendedMaxTextureSize = result.recommendedMaxTextureSize;
            result.selectedMaxTextureSizeIndex = GetTextureMaxSizeOptionIndex(recommendedMaxTextureSize);
            ApplyTextureAndroidMaxSize(result, recommendedMaxTextureSize);
        }

        private static void ApplyTextureAndroidMaxSize(TextureCheckResult result, int maxTextureSize)
        {
            TextureImporter importer = AssetImporter.GetAtPath(result.assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            TextureImporterPlatformSettings androidSettings = importer.GetPlatformTextureSettings(AndroidPlatformName);
            androidSettings.name = AndroidPlatformName;
            androidSettings.overridden = true;
            androidSettings.maxTextureSize = maxTextureSize;
            androidSettings.format = TextureImporterFormat.ASTC_6x6;
            androidSettings.compressionQuality = AndroidCompressionQuality;
            importer.SetPlatformTextureSettings(androidSettings);
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            result.importer = importer;
            result.androidMaxTextureSize = maxTextureSize;
        }

        private void MoveNextTexture()
        {
            currentTextureIndex++;
            textureReviewRevision++;
            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = 0;
            if (currentTextureIndex >= textureResults.Count)
            {
                textureResults.Clear();
                currentTextureIndex = 0;
                textureReviewRevision++;
                textureCheckMessage = "Material texture review complete.";
            }

            Repaint();
        }

        private string GetTexturePopupControlName(TextureCheckResult result)
        {
            int textureId = result != null && result.texture != null ? result.texture.GetInstanceID() : 0;
            return $"CharacterBuilderTools_TextureMaxSize_{textureReviewRevision}_{currentTextureIndex}_{textureId}";
        }

        private void SkipInvalidTextures()
        {
            while (currentTextureIndex < textureResults.Count)
            {
                TextureCheckResult result = textureResults[currentTextureIndex];
                if (result != null
                    && result.texture != null
                    && result.importer != null)
                {
                    return;
                }

                currentTextureIndex++;
            }
        }

        private void RunMeshComplexityCheck()
        {
            meshWarnings.Clear();
            meshCheckHasWarnings = false;

            if (avatarPrefab == null)
            {
                meshCheckMessage = "Assign a character prefab before running the mesh complexity check.";
                Repaint();
                return;
            }

            HashSet<Mesh> checkedMeshes = new HashSet<Mesh>();
            int totalVertexCount = 0;
            int totalTriangleCount = 0;

            MeshFilter[] meshFilters = avatarPrefab.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter meshFilter = meshFilters[i];
                if (meshFilter != null)
                {
                    AddMeshComplexityResult(meshFilter.sharedMesh, meshFilter.transform, checkedMeshes, ref totalVertexCount, ref totalTriangleCount);
                }
            }

            SkinnedMeshRenderer[] skinnedMeshRenderers = avatarPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                SkinnedMeshRenderer renderer = skinnedMeshRenderers[i];
                if (renderer != null)
                {
                    AddMeshComplexityResult(renderer.sharedMesh, renderer.transform, checkedMeshes, ref totalVertexCount, ref totalTriangleCount);
                }
            }

            if (totalVertexCount > TotalMeshVertexWarningThreshold || totalTriangleCount > TotalMeshTriangleWarningThreshold)
            {
                meshWarnings.Add($"Avatar total mesh complexity is high: {totalVertexCount} vertices, {totalTriangleCount} triangles. This may cause stutter in game.");
            }

            for (int i = 0; i < meshWarnings.Count; i++)
            {
                Debug.LogWarning(meshWarnings[i], avatarPrefab);
            }

            meshCheckMessage = meshWarnings.Count > 0
                ? $"Found {meshWarnings.Count} mesh complexity warning(s)."
                : $"Mesh complexity looks OK: {totalVertexCount} vertices, {totalTriangleCount} triangles.";
            meshCheckHasWarnings = meshWarnings.Count > 0;
            Repaint();
        }

        private void AddMeshComplexityResult(Mesh mesh, Transform owner, HashSet<Mesh> checkedMeshes, ref int totalVertexCount, ref int totalTriangleCount)
        {
            if (mesh == null || checkedMeshes.Contains(mesh))
            {
                return;
            }

            checkedMeshes.Add(mesh);
            int vertexCount = mesh.vertexCount;
            int triangleCount = GetTriangleCount(mesh);
            totalVertexCount += vertexCount;
            totalTriangleCount += triangleCount;

            if (vertexCount > MeshVertexWarningThreshold || triangleCount > MeshTriangleWarningThreshold)
            {
                string ownerPath = owner != null ? GetTransformPath(owner) : mesh.name;
                meshWarnings.Add($"Mesh \"{mesh.name}\" on \"{ownerPath}\" is high complexity: {vertexCount} vertices, {triangleCount} triangles. This may cause stutter in game.");
            }
        }

        private static int GetTriangleCount(Mesh mesh)
        {
            if (mesh == null)
            {
                return 0;
            }

            int triangleCount = 0;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                triangleCount += (int)mesh.GetIndexCount(i) / 3;
            }

            return triangleCount;
        }

        private static int GetRecommendedTextureMaxSize(string propertyName, TextureImporter importer, int width, int height)
        {
            int preferredSize = IsHighPriorityTexture(propertyName, importer) ? MaxBaseTextureSize : RecommendedTextureSize;
            int maxSourceSize = Mathf.Max(width, height);

            for (int i = 0; i < TextureMaxSizeOptions.Length; i++)
            {
                if (TextureMaxSizeOptions[i] >= preferredSize)
                {
                    if (TextureMaxSizeOptions[i] <= maxSourceSize || i == 0)
                    {
                        return TextureMaxSizeOptions[i];
                    }

                    return TextureMaxSizeOptions[Mathf.Max(0, i - 1)];
                }
            }

            return TextureMaxSizeOptions[TextureMaxSizeOptions.Length - 1];
        }

        private static bool IsHighPriorityTexture(string propertyName, TextureImporter importer)
        {
            if (importer != null && importer.textureType == TextureImporterType.NormalMap)
            {
                return true;
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            string lowerName = propertyName.ToLowerInvariant();
            return lowerName.Contains("basemap")
                || lowerName.Contains("maintex")
                || lowerName.Contains("albedo")
                || lowerName.Contains("diffuse")
                || lowerName.Contains("normal")
                || lowerName.Contains("bump");
        }

        private static int GetTextureMaxSizeOptionIndex(int maxTextureSize)
        {
            int closestIndex = 0;
            int closestDistance = Mathf.Abs(TextureMaxSizeOptions[0] - maxTextureSize);
            for (int i = 1; i < TextureMaxSizeOptions.Length; i++)
            {
                int distance = Mathf.Abs(TextureMaxSizeOptions[i] - maxTextureSize);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private static string FormatMegapixels(long pixels)
        {
            return (pixels / 1000000f).ToString("0.0");
        }

        private static bool IsIgnoredToolTexturePath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            string normalizedPath = assetPath.Replace('\\', '/').ToLowerInvariant();
            return normalizedPath.StartsWith(SpellScrystalTexturePath)
                || normalizedPath.StartsWith(HandPoseHelperTexturePath);
        }

        private static void TryGetSourceTextureSize(TextureImporter importer, Texture2D texture, out int width, out int height)
        {
            width = texture != null ? texture.width : 0;
            height = texture != null ? texture.height : 0;

            if (importer == null)
            {
                return;
            }

            System.Reflection.MethodInfo method = typeof(TextureImporter).GetMethod(
                "GetSourceTextureWidthAndHeight",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (method == null)
            {
                return;
            }

            try
            {
                object[] parameters = { width, height };
                method.Invoke(importer, parameters);
                width = parameters[0] is int ? (int)parameters[0] : width;
                height = parameters[1] is int ? (int)parameters[1] : height;
            }
            catch
            {
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            SkinnedMeshRenderer renderer = GetCurrentRenderer();
            if (renderer == null)
            {
                return;
            }

            Handles.color = Color.yellow;
            Bounds bounds = renderer.bounds;
            Handles.DrawWireCube(bounds.center, bounds.size);
        }

        private SkinnedMeshRenderer GetCurrentRenderer()
        {
            if (currentIndex < 0 || currentIndex >= renderers.Count)
            {
                return null;
            }

            return renderers[currentIndex];
        }

        private void SetCurrentRendererLayer()
        {
            SkinnedMeshRenderer renderer = GetCurrentRenderer();
            if (renderer == null || renderer.gameObject == null)
            {
                return;
            }

            Undo.RegisterCompleteObjectUndo(renderer.gameObject, "Set head renderer InvisibleFPS layer");
            renderer.gameObject.layer = LayerDefine.InvisibleFPSLayer;
            EditorUtility.SetDirty(renderer.gameObject);
        }

        private void MoveNext()
        {
            currentIndex++;
            if (currentIndex >= renderers.Count)
            {
                renderers.Clear();
                currentIndex = 0;
                checkMessage = "Head occlusion review complete.";
                SceneView.RepaintAll();
                Repaint();
                return;
            }

            FocusCurrentRenderer(true);
            Repaint();
        }

        private void FocusCurrentRenderer(bool frameSelected)
        {
            SkinnedMeshRenderer renderer = GetCurrentRenderer();
            if (renderer == null || renderer.gameObject == null)
            {
                return;
            }

            Selection.activeGameObject = renderer.gameObject;
            EditorGUIUtility.PingObject(renderer.gameObject);

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (frameSelected && sceneView != null)
            {
                sceneView.FrameSelected();
            }

            SceneView.RepaintAll();
        }

        private void SkipInvalidRenderers()
        {
            while (currentIndex < renderers.Count)
            {
                SkinnedMeshRenderer renderer = renderers[currentIndex];
                if (renderer != null
                    && renderer.gameObject != null
                    && renderer.gameObject.layer != LayerDefine.InvisibleFPSLayer)
                {
                    return;
                }

                currentIndex++;
            }
        }

        private bool IsLikelyHeadRenderer(SkinnedMeshRenderer renderer, Animator animator)
        {
            HashSet<Transform> headBoneRoots = new HashSet<Transform>();
            HashSet<int> headBoneIndices = new HashSet<int>();
            Transform headReference = null;

            if (!TryBuildHeadBoneIndices(renderer, animator, headBoneRoots, headBoneIndices, ref headReference))
            {
                return false;
            }

            BoneWeight[] boneWeights = renderer.sharedMesh.boneWeights;
            if (boneWeights == null || boneWeights.Length == 0)
            {
                return false;
            }

            int headDominantVertexCount = 0;
            for (int i = 0; i < boneWeights.Length; i++)
            {
                if (GetHeadBoneWeight(boneWeights[i], headBoneIndices) >= HeadVertexWeightThreshold)
                {
                    headDominantVertexCount++;
                }
            }

            float headVertexRatio = (float)headDominantVertexCount / boneWeights.Length;
            if (headVertexRatio < HeadRendererVertexRatioThreshold)
            {
                return false;
            }

            return PassHeadBoundsCheck(renderer, headReference);
        }

        private bool TryBuildHeadBoneIndices(SkinnedMeshRenderer renderer, Animator animator, HashSet<Transform> headBoneRoots, HashSet<int> headBoneIndices, ref Transform headReference)
        {
            Transform[] bones = renderer.bones;
            if (bones == null || bones.Length == 0)
            {
                return false;
            }

            AddHumanoidHeadBones(animator, headBoneRoots, ref headReference);
            AddNamedHeadBones(bones, headBoneRoots, ref headReference);

            for (int i = 0; i < bones.Length; i++)
            {
                Transform bone = bones[i];
                if (bone == null || !IsHeadBone(bone, headBoneRoots))
                {
                    continue;
                }

                headBoneIndices.Add(i);
            }

            if (headReference == null && headBoneIndices.Count > 0)
            {
                foreach (int boneIndex in headBoneIndices)
                {
                    if (boneIndex >= 0 && boneIndex < bones.Length)
                    {
                        headReference = bones[boneIndex];
                        break;
                    }
                }
            }

            return headBoneIndices.Count > 0;
        }

        private void AddHumanoidHeadBones(Animator animator, HashSet<Transform> headBoneRoots, ref Transform headReference)
        {
            if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.isHuman)
            {
                return;
            }

            AddHumanoidHeadBone(animator, HumanBodyBones.Head, true, headBoneRoots, ref headReference);
            AddHumanoidHeadBone(animator, HumanBodyBones.Neck, false, headBoneRoots, ref headReference);
            AddHumanoidHeadBone(animator, HumanBodyBones.Jaw, false, headBoneRoots, ref headReference);
            AddHumanoidHeadBone(animator, HumanBodyBones.LeftEye, false, headBoneRoots, ref headReference);
            AddHumanoidHeadBone(animator, HumanBodyBones.RightEye, false, headBoneRoots, ref headReference);
        }

        private void AddHumanoidHeadBone(Animator animator, HumanBodyBones boneId, bool isPrimary, HashSet<Transform> headBoneRoots, ref Transform headReference)
        {
            Transform bone = animator.GetBoneTransform(boneId);
            if (bone == null)
            {
                return;
            }

            headBoneRoots.Add(bone);
            if (isPrimary || headReference == null)
            {
                headReference = bone;
            }
        }

        private void AddNamedHeadBones(Transform[] bones, HashSet<Transform> headBoneRoots, ref Transform headReference)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                Transform bone = bones[i];
                if (bone == null || !IsHeadBoneName(bone.name))
                {
                    continue;
                }

                headBoneRoots.Add(bone);
                if (headReference == null || (IsPrimaryHeadBoneName(bone.name) && !IsPrimaryHeadBoneName(headReference.name)))
                {
                    headReference = bone;
                }
            }
        }

        private bool IsHeadBone(Transform bone, HashSet<Transform> headBoneRoots)
        {
            if (headBoneRoots.Contains(bone))
            {
                return true;
            }

            foreach (Transform root in headBoneRoots)
            {
                if (root != null && bone.IsChildOf(root))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsHeadBoneName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            string lowerName = name.ToLowerInvariant();
            return lowerName.Contains("head")
                || lowerName.Contains("neck")
                || lowerName.Contains("jaw")
                || lowerName.Contains("eye")
                || lowerName.Contains("face");
        }

        private static bool IsPrimaryHeadBoneName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name.ToLowerInvariant().Contains("head");
        }

        private float GetHeadBoneWeight(BoneWeight boneWeight, HashSet<int> headBoneIndices)
        {
            float weight = 0f;

            if (headBoneIndices.Contains(boneWeight.boneIndex0))
                weight += boneWeight.weight0;
            if (headBoneIndices.Contains(boneWeight.boneIndex1))
                weight += boneWeight.weight1;
            if (headBoneIndices.Contains(boneWeight.boneIndex2))
                weight += boneWeight.weight2;
            if (headBoneIndices.Contains(boneWeight.boneIndex3))
                weight += boneWeight.weight3;

            return weight;
        }

        private bool PassHeadBoundsCheck(SkinnedMeshRenderer renderer, Transform headReference)
        {
            float heightScale = Mathf.Clamp(characterHeight / 2f, MinHeightScale, MaxHeightScale);
            Bounds bounds = renderer.bounds;
            if (bounds.size.y > MaxHeadBoundsHeight * heightScale)
            {
                return false;
            }

            if (headReference == null)
            {
                return true;
            }

            float maxDistance = MaxHeadBoundsCenterDistance * heightScale;
            return (bounds.center - headReference.position).sqrMagnitude <= maxDistance * maxDistance;
        }

        private static string GetLayerDisplayName(int layer)
        {
            string layerName = LayerMask.LayerToName(layer);
            if (string.IsNullOrEmpty(layerName))
            {
                return layer.ToString();
            }

            return layerName;
        }

        private static string GetTransformPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            string path = transform.name;
            Transform parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
#endif
