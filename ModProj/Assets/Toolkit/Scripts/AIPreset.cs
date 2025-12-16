using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace CrossLink
{
    [System.Serializable]
    public class CooldownSlot
    {
        public float skillsetInterval = 5;
        public int performNumInterval = 0;
        public string specialCondition;
        public ActionData[] actions;
    }

    [CreateAssetMenu(fileName = "AIPreset", menuName = "AIPreset")]
    [System.Serializable]
    public class AIPreset : ScriptableObject
    {
        public RuntimeAnimatorController controller;
        public ActionData[] aiActions;
        public ActionData[] dodgeActions;
        public ActionData[] wakeupActions;
        public CooldownSlot[] cooldownSlots;
        public string originalController;
        public AnimLayoutDataItem[] animLayoutDatas;
#if UNITY_EDITOR

        [EasyButtons.Button]
        public void AutoAddState(string folderPath)
        {
            var ac = controller as AnimatorController;
            AnimatorControllerLayer layer = ac.layers[0];
            var targetSubMachine = FindSubStateMachine(layer.stateMachine, "Actions");
            if (!targetSubMachine)
                return;

            if (aiActions != null)
                SetupAnimatorState(targetSubMachine, aiActions, folderPath);
            if (dodgeActions != null)
                SetupAnimatorState(targetSubMachine, dodgeActions, folderPath);
            if (wakeupActions != null)
                SetupAnimatorState(targetSubMachine, wakeupActions, folderPath);
        }

        void SetupAnimatorState(AnimatorStateMachine machine, ActionData[] actionDatas, string path)
        {
            if (actionDatas == null)
                return;

            var prefix = AddressableConfig.GetConfig().GetPrefix();
            for (int i = 0; i < actionDatas.Length; i++)
            {
                if (actionDatas[i].timelines != null)
                {
                    for (int j = 0; j < actionDatas[i].timelines.Length; j++)
                    {
                        if (actionDatas[i].timelines[j].actionDatas != null)
                        {
                            for (int k = 0; k < actionDatas[i].timelines[j].actionDatas.Length; k++)
                            {
                                if (actionDatas[i].timelines[j].actionDatas[k] is ActionAnimData animData)
                                {
                                    if (!string.IsNullOrEmpty(animData.animName))
                                    {
                                        bool stateExists = machine.states.Any(childState => childState.state.name == animData.animName);

                                        if (!stateExists)
                                        {
                                            var state = machine.AddState(animData.animName);
                                            var name = animData.animName.Replace(prefix, string.Empty);
                                            state.motion = ResourceMgr.Load(System.IO.Path.Combine(path, name)) as AnimationClip;
                                            state.speedParameterActive = true;
                                            state.speedParameter = "Speed";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

#if false
        [EasyButtons.Button]
        public void AutoSetPlaceholder()
        {
            var ac = controller as AnimatorController;
            AnimatorControllerLayer layer = ac.layers[0];
            var targetSubMachine = FindSubStateMachine(layer.stateMachine, "Actions");
            if (!targetSubMachine)
                return;

            string clipFolderPath = "Assets/Tool/CharacterAnimator/AnimationClip";
            if (!AssetDatabase.IsValidFolder(clipFolderPath))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/Tool/CharacterAnimator/AnimationClip");
                AssetDatabase.Refresh();
            }

            for (int i = 0; i < targetSubMachine.states.Length; i++)
            {
                if (targetSubMachine.states[i].state.motion == null)
                {
                    string stateName = targetSubMachine.states[i].state.name;
                    string clipPath = $"{clipFolderPath}/{stateName}.anim";
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);

                    if (clip == null)
                    {
                        clip = new AnimationClip();
                        clip.name = stateName;

                        AssetDatabase.CreateAsset(clip, clipPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    }
                    targetSubMachine.states[i].state.motion = clip;
                    EditorUtility.SetDirty(ac);
                }
            }
            AssetDatabase.SaveAssets();
        }

#endif



        private AnimatorStateMachine FindSubStateMachine(AnimatorStateMachine root, string name)
        {
            if (root.name == name)
                return root;

            foreach (ChildAnimatorStateMachine child in root.stateMachines)
            {
                var result = FindSubStateMachine(child.stateMachine, name);
                if (result != null)
                    return result;
            }

            return null;
        }

        [EasyButtons.Button]
        public void ExportRootMotions(AnimationClip animationClip)
        {
            var item = AnimTool.ExportRootMotion(animationClip);
            if (animLayoutDatas == null)
            {
                animLayoutDatas = new AnimLayoutDataItem[1];
                animLayoutDatas[animLayoutDatas.Length] = item;
            }
            {
                var array = new AnimLayoutDataItem[animLayoutDatas.Length + 1];
                System.Array.Copy(animLayoutDatas, array, animLayoutDatas.Length);
                array[animLayoutDatas.Length] = item;
                animLayoutDatas = array;
            }
        }

        [EasyButtons.Button]
        public void ExportAllRootMotions(string folderPath)
        {
            var items = AnimTool.ExportRootMotions(folderPath);
            if (animLayoutDatas == null)
                animLayoutDatas = items;
            else
            {
                var array = new AnimLayoutDataItem[animLayoutDatas.Length + items.Length];
                System.Array.Copy(animLayoutDatas, array, animLayoutDatas.Length);
                System.Array.Copy(items, 0, array, animLayoutDatas.Length, items.Length);
                animLayoutDatas = array;
            }
        }
#endif
    }
}