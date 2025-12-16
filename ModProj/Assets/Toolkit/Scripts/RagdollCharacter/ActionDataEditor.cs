#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
namespace CrossLink
{
    [CustomEditor(typeof(ActionData))]
    public class ActionDataEditor : Editor
    {
        private SerializedProperty performRangeMinProp;
        private SerializedProperty performRangeMaxProp;
        private SerializedProperty haveAtkIntentionProp;
        private SerializedProperty isRangeAtkProp;
        private SerializedProperty atkTimeLenProp;
        private SerializedProperty timeLenProp;
        private SerializedProperty ignoreDynamicSpeedProp;
        private SerializedProperty ignoreRagdollProfileProp;
        private SerializedProperty switchAtkOptionProp;
        private SerializedProperty combineListProp;
        private SerializedProperty timelinesProp;

        private bool showBasicSettings = true;
        private bool showAttackSettings = true;
        private bool showCombineList = true;
        private bool showTimelines = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Action Data", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            showBasicSettings = EditorGUILayout.Foldout(showBasicSettings, "base", true);
            if (showBasicSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(performRangeMinProp);
                EditorGUILayout.PropertyField(performRangeMaxProp);
                EditorGUILayout.PropertyField(timeLenProp);
                EditorGUILayout.PropertyField(ignoreDynamicSpeedProp);
                EditorGUILayout.PropertyField(ignoreRagdollProfileProp);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            showAttackSettings = EditorGUILayout.Foldout(showAttackSettings, "attack", true);
            if (showAttackSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(haveAtkIntentionProp);
                EditorGUILayout.PropertyField(isRangeAtkProp);
                EditorGUILayout.PropertyField(atkTimeLenProp);
                EditorGUILayout.PropertyField(switchAtkOptionProp);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            showCombineList = EditorGUILayout.Foldout(showCombineList, "ComblineList", true);
            if (showCombineList)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(combineListProp, true);
                EditorGUI.indentLevel--;
            }

            if (GUILayout.Button("Output Combine"))
            {
                ActionData actionData = (ActionData)target;
                actionData.OutputCombine();

                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.Space();

            showTimelines = EditorGUILayout.Foldout(showTimelines, "Timelines", true);
            if (showTimelines)
            {
                EditorGUI.indentLevel++;
                DrawTimelines();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            //DrawCustomButtons();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTimelines()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("timeline:", GUILayout.Width(80));
            int newSize = EditorGUILayout.IntField(timelinesProp.arraySize, GUILayout.Width(50));
            if (newSize != timelinesProp.arraySize)
            {
                timelinesProp.arraySize = newSize;
            }
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                timelinesProp.arraySize++;
            }

            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                if (timelinesProp.arraySize > 0)
                {
                    timelinesProp.arraySize--;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            for (int i = 0; i < timelinesProp.arraySize; i++)
            {
                SerializedProperty timelineProp = timelinesProp.GetArrayElementAtIndex(i);
                SerializedProperty prepareTimeProp = timelineProp.FindPropertyRelative("prepareTime");
                SerializedProperty startTimeProp = timelineProp.FindPropertyRelative("startTime");
                SerializedProperty endTimeProp = timelineProp.FindPropertyRelative("endTime");
                SerializedProperty actionDatasProp = timelineProp.FindPropertyRelative("actionDatas");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                bool isTimelineExpanded = IsTimelineExpanded(timelineProp);

                EditorGUILayout.BeginHorizontal();

                isTimelineExpanded = EditorGUILayout.Foldout(isTimelineExpanded, $"TimelineData {i}", true, EditorStyles.boldLabel);
                SetTimelineExpanded(timelineProp, isTimelineExpanded);

                if (GUILayout.Button("x", GUILayout.Width(25)))
                {
                    timelinesProp.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    return;
                }
                EditorGUILayout.EndHorizontal();

                if (isTimelineExpanded)
                {
                    EditorGUILayout.PropertyField(prepareTimeProp);
                    EditorGUILayout.PropertyField(startTimeProp);
                    EditorGUILayout.PropertyField(endTimeProp);

                    DrawActionSubDataArray(actionDatasProp);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (timelinesProp.arraySize == 0)
            {
                if (GUILayout.Button("Add Timeline"))
                {
                    timelinesProp.arraySize = 1;
                }
            }
        }

        private bool IsTimelineExpanded(SerializedProperty timelineProp)
        {
            string key = $"TimelineExpanded_{timelineProp.propertyPath}";
            return SessionState.GetBool(key, true);
        }

        private void SetTimelineExpanded(SerializedProperty timelineProp, bool expanded)
        {
            string key = $"TimelineExpanded_{timelineProp.propertyPath}";
            SessionState.SetBool(key, expanded);
        }

        private void DrawActionSubDataArray(SerializedProperty actionDatasProp)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("action:", GUILayout.Width(80));
            int newSize = EditorGUILayout.IntField(actionDatasProp.arraySize, GUILayout.Width(50));
            if (newSize != actionDatasProp.arraySize)
            {
                actionDatasProp.arraySize = newSize;
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                actionDatasProp.arraySize++;
            }

            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                if (actionDatasProp.arraySize > 0)
                {
                    actionDatasProp.arraySize--;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            for (int i = 0; i < actionDatasProp.arraySize; i++)
            {
                DrawActionSubDataElement(actionDatasProp, i);
            }
        }

        private void DrawActionSubDataElement(SerializedProperty arrayProp, int index)
        {
            SerializedProperty elementProp = arrayProp.GetArrayElementAtIndex(index);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string typeName = GetManagedReferenceTypeName(elementProp);

            EditorGUILayout.BeginHorizontal();
            bool isExpanded = IsActionSubDataExpanded(elementProp);
            isExpanded = EditorGUILayout.Foldout(isExpanded, $"{index}: {typeName}", true, EditorStyles.boldLabel);
            SetActionSubDataExpanded(elementProp, isExpanded);

            if (GUILayout.Button("¡Á", GUILayout.Width(20), GUILayout.Height(15)))
            {
                arrayProp.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();

            if (isExpanded)
            {
                bool isEmpty = IsElementEmpty(elementProp);

                if (isEmpty)
                {
                    DrawTypeSelection(elementProp);
                }
                else
                {
                    DrawElementProperties(elementProp, typeName);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private bool IsActionSubDataExpanded(SerializedProperty elementProp)
        {
            string key = $"ActionSubDataExpanded_{elementProp.propertyPath}";
            return SessionState.GetBool(key, true); 
        }

        private void SetActionSubDataExpanded(SerializedProperty elementProp, bool expanded)
        {
            string key = $"ActionSubDataExpanded_{elementProp.propertyPath}";
            SessionState.SetBool(key, expanded);
        }

        private bool IsElementEmpty(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ManagedReference:
                    return string.IsNullOrEmpty(property.managedReferenceFullTypename);

                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null;

                default:
                    return true;
            }
        }

        private string GetManagedReferenceTypeName(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
                {
                    string[] parts = property.managedReferenceFullTypename.Split(' ');
                    if (parts.Length >= 2)
                    {
                        string fullTypeName = parts[1];
                        string[] namespaceParts = fullTypeName.Split('.');
                        return namespaceParts[namespaceParts.Length - 1]; 
                    }
                    return "unknown";
                }
                return "empty";
            }
            else if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue != null)
                {
                    return property.objectReferenceValue.GetType().Name;
                }
                return "empty";
            }

            return $"unknown ({property.propertyType})";
        }

        private void DrawElementProperties(SerializedProperty property, string typeName)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                if (typeName == "ActionAnimData")
                {
                    DrawActionAnimDataProperties(property);
                }
                else if (typeName == "ActionFlyObjectData")
                {
                    DrawActionFlyObjectDataProperties(property);
                }
                else if (typeName == "ActionEffectData")
                {
                    DrawActionEffectDataProperties(property);
                }
                else if (typeName == "ActionSoundData")
                {
                    DrawActionSoundDataProperties(property);
                }
                else if (typeName == "ActionThrowData")
                {
                    DrawActionThrowDataProperties(property);
                }
                else if (typeName == "ActionCombatData")
                {
                    DrawActionCombatDataProperties(property);
                }
                else if (typeName == "ActionEventData")
                {
                    DrawActionEventDataProperties(property);
                }
                else
                {
                    try
                    {
                        EditorGUILayout.PropertyField(property, GUIContent.none, true);
                    }
                    catch
                    {
                        EditorGUILayout.HelpBox($"cannot paint {typeName} attr:", MessageType.Warning);
                    }
                }
            }
            else if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                EditorGUILayout.PropertyField(property, new GUIContent("ObjectReference"));
            }

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("switch type", GUILayout.Width(80)))
                {
                    ShowTypeSelectionMenu(property);
                }

                if (GUILayout.Button("reset", GUILayout.Width(60)))
                {
                    property.managedReferenceValue = null;
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndHorizontal();
            }
            else if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (GUILayout.Button("clear refernce", GUILayout.Width(80)))
                {
                    property.objectReferenceValue = null;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawTypeSelection(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("type:", GUILayout.Width(60));

                if (GUILayout.Button("select...", GUILayout.Width(80)))
                {
                    ShowTypeSelectionMenu(property);
                }

                EditorGUILayout.EndHorizontal();
            }
            else if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (GUILayout.Button("select object", GUILayout.Width(80)))
                {
                    // open object selector
                }
            }
        }

        private void ShowTypeSelectionMenu(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
                return;

            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("ActionAnimData"), false, () =>
            {
                property.managedReferenceValue = new ActionAnimData();
                serializedObject.ApplyModifiedProperties();
            });

            menu.AddItem(new GUIContent("ActionFlyObjectData"), false, () =>
            {
                property.managedReferenceValue = new ActionFlyObjectData();
                serializedObject.ApplyModifiedProperties();
            });

            menu.AddItem(new GUIContent("ActionEffectData"), false, () =>
            {
                property.managedReferenceValue = new ActionEffectData();
                serializedObject.ApplyModifiedProperties();
            });

            menu.AddItem(new GUIContent("ActionSoundData"), false, () =>
            {
                property.managedReferenceValue = new ActionSoundData();
                serializedObject.ApplyModifiedProperties();
            });

            menu.AddItem(new GUIContent("ActionThrowData"), false, () =>
            {
                property.managedReferenceValue = new ActionThrowData();
                serializedObject.ApplyModifiedProperties();
            });

            menu.AddItem(new GUIContent("ActionCombatData"), false, () =>
            {
                property.managedReferenceValue = new ActionCombatData();
                serializedObject.ApplyModifiedProperties();
            });

            menu.AddItem(new GUIContent("ActionEventData"), false, () =>
            {
                property.managedReferenceValue = new ActionEventData();
                serializedObject.ApplyModifiedProperties();
            });


            menu.ShowAsContext();
        }

        private void DrawActionAnimDataProperties(SerializedProperty animDataProp)
        {
            try
            {
                SerializedProperty slotIdProp = animDataProp.FindPropertyRelative("slotId");
                SerializedProperty animNameProp = animDataProp.FindPropertyRelative("animName");
                SerializedProperty animLayerProp = animDataProp.FindPropertyRelative("animLayer");
                SerializedProperty crossfadeTimeProp = animDataProp.FindPropertyRelative("crossfadeTime");
                SerializedProperty timeScaleProp = animDataProp.FindPropertyRelative("timeScale");
                SerializedProperty customLayoutLenProp = animDataProp.FindPropertyRelative("customLayoutLen");
                SerializedProperty motionTimeOffsetProp = animDataProp.FindPropertyRelative("motionTimeOffset");
                SerializedProperty rootMotionProp = animDataProp.FindPropertyRelative("rootMotion");
                SerializedProperty loopMotionProp = animDataProp.FindPropertyRelative("loopMotion");
                SerializedProperty moveTypeProp = animDataProp.FindPropertyRelative("moveType");
                SerializedProperty turnStartAngleProp = animDataProp.FindPropertyRelative("turnStartAngle");
                SerializedProperty turnStopAngleProp = animDataProp.FindPropertyRelative("turnStopAngle");
                SerializedProperty turnSpeedMlpProp = animDataProp.FindPropertyRelative("turnSpeedMlp");
                SerializedProperty rotateSpeedProp = animDataProp.FindPropertyRelative("rotateSpeed");
                SerializedProperty unknockableProp = animDataProp.FindPropertyRelative("unknockable");
                SerializedProperty motionScaleProp = animDataProp.FindPropertyRelative("motionScale");
                SerializedProperty trackPosBeginTimeProp = animDataProp.FindPropertyRelative("trackPosBeginTime");
                SerializedProperty trackPosTimeProp = animDataProp.FindPropertyRelative("trackPosTime");
                SerializedProperty ignorePosBeginTimeProp = animDataProp.FindPropertyRelative("ignorePosBeginTime");
                SerializedProperty ignorePosTimeProp = animDataProp.FindPropertyRelative("ignorePosTime");
                SerializedProperty trackRotBeginTimeProp = animDataProp.FindPropertyRelative("trackRotBeginTime");
                SerializedProperty trackRotTimeProp = animDataProp.FindPropertyRelative("trackRotTime");
                SerializedProperty ignoreRotTimeProp = animDataProp.FindPropertyRelative("ignoreRotTime");
                SerializedProperty ignoreProfileProp = animDataProp.FindPropertyRelative("ignoreProfile");
                SerializedProperty trackLinearAnchorPointProp = animDataProp.FindPropertyRelative("trackLinearAnchorPoint");
                SerializedProperty trackAngularAnchorPointProp = animDataProp.FindPropertyRelative("trackAngularAnchorPoint");
                SerializedProperty linearTrackPointFloatingRangeMinProp = animDataProp.FindPropertyRelative("linearTrackPointFloatingRangeMin");
                SerializedProperty linearTrackPointFloatingRangeMaxProp = animDataProp.FindPropertyRelative("linearTrackPointFloatingRangeMax");
                SerializedProperty allowUnGravityProp = animDataProp.FindPropertyRelative("allowUnGravity");
                SerializedProperty jointUpdateDisabledTimeProp = animDataProp.FindPropertyRelative("jointUpdateDisabledTime");
                SerializedProperty jointUpdateDisabledStopTimeProp = animDataProp.FindPropertyRelative("jointUpdateDisabledStopTime");
                SerializedProperty checkCancelDisProp = animDataProp.FindPropertyRelative("checkCancelDis");

                if (slotIdProp != null) EditorGUILayout.PropertyField(slotIdProp);
                if (animNameProp != null) EditorGUILayout.PropertyField(animNameProp);
                if (animLayerProp != null) EditorGUILayout.PropertyField(animLayerProp);
                if (crossfadeTimeProp != null) EditorGUILayout.PropertyField(crossfadeTimeProp);
                if (timeScaleProp != null) EditorGUILayout.PropertyField(timeScaleProp);
                if (customLayoutLenProp != null) EditorGUILayout.PropertyField(customLayoutLenProp);
                if (motionTimeOffsetProp != null) EditorGUILayout.PropertyField(motionTimeOffsetProp);
                if (rootMotionProp != null) EditorGUILayout.PropertyField(rootMotionProp);
                if (loopMotionProp != null) EditorGUILayout.PropertyField(loopMotionProp);
                if (moveTypeProp != null) EditorGUILayout.PropertyField(moveTypeProp);

                if (moveTypeProp != null && moveTypeProp.enumValueIndex == (int)ActionAnimData.SpecialMoveType.Turn)
                {
                    EditorGUI.indentLevel++;
                    if (turnStartAngleProp != null) EditorGUILayout.PropertyField(turnStartAngleProp);
                    if (turnStopAngleProp != null) EditorGUILayout.PropertyField(turnStopAngleProp);
                    if (turnSpeedMlpProp != null) EditorGUILayout.PropertyField(turnSpeedMlpProp);
                    if (rotateSpeedProp != null) EditorGUILayout.PropertyField(rotateSpeedProp);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();

                if (unknockableProp != null) EditorGUILayout.PropertyField(unknockableProp);
                if (motionScaleProp != null) EditorGUILayout.PropertyField(motionScaleProp);

                if (trackPosBeginTimeProp != null) EditorGUILayout.PropertyField(trackPosBeginTimeProp);
                if (trackPosTimeProp != null) EditorGUILayout.PropertyField(trackPosTimeProp);
                if (ignorePosBeginTimeProp != null) EditorGUILayout.PropertyField(ignorePosBeginTimeProp);
                if (ignorePosTimeProp != null) EditorGUILayout.PropertyField(ignorePosTimeProp);

                EditorGUILayout.Space();
                if (trackRotBeginTimeProp != null) EditorGUILayout.PropertyField(trackRotBeginTimeProp);
                if (trackRotTimeProp != null) EditorGUILayout.PropertyField(trackRotTimeProp);
                if (ignoreRotTimeProp != null) EditorGUILayout.PropertyField(ignoreRotTimeProp);
                if (ignoreProfileProp != null) EditorGUILayout.PropertyField(ignoreProfileProp);

                EditorGUILayout.Space();

                if (trackLinearAnchorPointProp != null) EditorGUILayout.PropertyField(trackLinearAnchorPointProp);
                if (trackAngularAnchorPointProp != null) EditorGUILayout.PropertyField(trackAngularAnchorPointProp);

                EditorGUILayout.Space();
                
                if (linearTrackPointFloatingRangeMinProp != null) EditorGUILayout.PropertyField(linearTrackPointFloatingRangeMinProp);
                if (linearTrackPointFloatingRangeMaxProp != null) EditorGUILayout.PropertyField(linearTrackPointFloatingRangeMaxProp);

                EditorGUILayout.Space();

                if (allowUnGravityProp != null) EditorGUILayout.PropertyField(allowUnGravityProp);
                if (jointUpdateDisabledTimeProp != null) EditorGUILayout.PropertyField(jointUpdateDisabledTimeProp);
                if (jointUpdateDisabledStopTimeProp != null) EditorGUILayout.PropertyField(jointUpdateDisabledStopTimeProp);
                if (checkCancelDisProp != null) EditorGUILayout.PropertyField(checkCancelDisProp);

            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox($"draw ActionAnimData attr error: {e.Message}", MessageType.Error);
            }
        }

        private void DrawActionFlyObjectDataProperties(SerializedProperty flyObjectDataProp)
        {
            try
            {
                SerializedProperty slotIdProp = flyObjectDataProp.FindPropertyRelative("slotId");
                SerializedProperty flyObjectNameProp = flyObjectDataProp.FindPropertyRelative("flyObjectName");
                SerializedProperty velocityProp = flyObjectDataProp.FindPropertyRelative("velocity");
                SerializedProperty aimTargetProp = flyObjectDataProp.FindPropertyRelative("aimTarget");
                SerializedProperty flyDelayProp = flyObjectDataProp.FindPropertyRelative("flyDelay");
                SerializedProperty spawnOnAimTargetProp = flyObjectDataProp.FindPropertyRelative("spawnOnAimTarget");
                SerializedProperty aimTargetOffsetProp = flyObjectDataProp.FindPropertyRelative("aimTargetOffset");
                SerializedProperty aimRandomProp = flyObjectDataProp.FindPropertyRelative("aimRandom");
                SerializedProperty limitAimHelpRangeProp = flyObjectDataProp.FindPropertyRelative("limitAimHelpRange");
                SerializedProperty posProp = flyObjectDataProp.FindPropertyRelative("pos");
                SerializedProperty disYFactorProp = flyObjectDataProp.FindPropertyRelative("disYFactor");
                SerializedProperty disYFactorStartDisProp = flyObjectDataProp.FindPropertyRelative("disYFactorStartDis");
                SerializedProperty maxAimSpeedProp = flyObjectDataProp.FindPropertyRelative("maxAimSpeed");
                SerializedProperty rotateDelayProp = flyObjectDataProp.FindPropertyRelative("rotateDelay");
                SerializedProperty deadOnExitProp = flyObjectDataProp.FindPropertyRelative("deadOnExit");

                if (slotIdProp != null) EditorGUILayout.PropertyField(slotIdProp);
                if (flyObjectNameProp != null) EditorGUILayout.PropertyField(flyObjectNameProp);
                if (velocityProp != null) EditorGUILayout.PropertyField(velocityProp);
                if (aimTargetProp != null) EditorGUILayout.PropertyField(aimTargetProp);
                if (flyDelayProp != null) EditorGUILayout.PropertyField(flyDelayProp);
                if (spawnOnAimTargetProp != null) EditorGUILayout.PropertyField(spawnOnAimTargetProp);
                if (aimTargetOffsetProp != null) EditorGUILayout.PropertyField(aimTargetOffsetProp);
                if (aimRandomProp != null) EditorGUILayout.PropertyField(aimRandomProp);
                if (limitAimHelpRangeProp != null) EditorGUILayout.PropertyField(limitAimHelpRangeProp);


                DrawPosDefineProperties(posProp);

                EditorGUI.indentLevel--;

                if (disYFactorProp != null) EditorGUILayout.PropertyField(disYFactorProp);
                if (disYFactorStartDisProp != null) EditorGUILayout.PropertyField(disYFactorStartDisProp);
                if (maxAimSpeedProp != null) EditorGUILayout.PropertyField(maxAimSpeedProp);
                if (rotateDelayProp != null) EditorGUILayout.PropertyField(rotateDelayProp);
                if (deadOnExitProp != null) EditorGUILayout.PropertyField(deadOnExitProp);
            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox($"draw ActionFlyObjectData attr error: {e.Message}", MessageType.Error);
            }
        }

        private void DrawActionEffectDataProperties(SerializedProperty effectDataProp)
        {
            try
            {
                SerializedProperty slotIdProp = effectDataProp.FindPropertyRelative("slotId");
                SerializedProperty effectNameProp = effectDataProp.FindPropertyRelative("effectName");
                SerializedProperty posProp = effectDataProp.FindPropertyRelative("pos");
                SerializedProperty rotationBaseLocalProp = effectDataProp.FindPropertyRelative("rotationBaseLocal");
                SerializedProperty removeWhenActionFinishProp = effectDataProp.FindPropertyRelative("removeWhenActionFinish");
                SerializedProperty stopWhenActionFinishProp = effectDataProp.FindPropertyRelative("stopWhenActionFinish");
                SerializedProperty loopIdProp = effectDataProp.FindPropertyRelative("loopId");

                if (slotIdProp != null) EditorGUILayout.PropertyField(slotIdProp);
                if (effectNameProp != null) EditorGUILayout.PropertyField(effectNameProp);
                if (posProp != null) DrawPosDefineProperties(posProp);
                if (rotationBaseLocalProp != null) EditorGUILayout.PropertyField(rotationBaseLocalProp);
                if (removeWhenActionFinishProp != null) EditorGUILayout.PropertyField(removeWhenActionFinishProp);
                if (stopWhenActionFinishProp != null) EditorGUILayout.PropertyField(stopWhenActionFinishProp);
                if (loopIdProp != null) EditorGUILayout.PropertyField(loopIdProp);
            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox($"draw ActionEffectData attr error: {e.Message}", MessageType.Error);
            }
        }

        private void DrawActionSoundDataProperties(SerializedProperty soundDataProp)
        {
            try
            {
                SerializedProperty slotIdProp = soundDataProp.FindPropertyRelative("slotId");
                SerializedProperty posProp = soundDataProp.FindPropertyRelative("pos");
                SerializedProperty delayToPlayProp = soundDataProp.FindPropertyRelative("delayToPlay");
                SerializedProperty loopProp = soundDataProp.FindPropertyRelative("loop");
                SerializedProperty soundProp = soundDataProp.FindPropertyRelative("sound");
                SerializedProperty roleSoundProp = soundDataProp.FindPropertyRelative("roleSound");

                if (slotIdProp != null) EditorGUILayout.PropertyField(slotIdProp);

                if (posProp != null)
                {
                    DrawPosDefineProperties(posProp);
                }

                if (delayToPlayProp != null) EditorGUILayout.PropertyField(delayToPlayProp);
                if (loopProp != null) EditorGUILayout.PropertyField(loopProp);

                if (soundProp != null)
                {
                    DrawSoundEffectInfoProperties(soundProp);
                    EditorGUILayout.Space();
                }

                if (roleSoundProp != null) EditorGUILayout.PropertyField(roleSoundProp);
            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox($"draw ActionSoundData attr error: {e.Message}", MessageType.Error);
            }
        }

        private void DrawSoundEffectInfoProperties(SerializedProperty soundProp)
        {
            bool showSoundDetails = EditorGUILayout.Foldout(IsSoundExpanded(soundProp), "Sound", true);
            SetSoundExpanded(soundProp, showSoundDetails);

            if (showSoundDetails)
            {
                EditorGUI.indentLevel++;

                try
                {
                    SerializedProperty templateNameProp = soundProp.FindPropertyRelative("templateName");
                    SerializedProperty soundNamesProp = soundProp.FindPropertyRelative("soundNames");
                    SerializedProperty volProp = soundProp.FindPropertyRelative("vol");
                    SerializedProperty volRandomRangeProp = soundProp.FindPropertyRelative("volRandomRange");
                    SerializedProperty pitchMaxProp = soundProp.FindPropertyRelative("pitchMax");
                    SerializedProperty pitchMinProp = soundProp.FindPropertyRelative("pitchMin");

                    if (templateNameProp != null)
                        EditorGUILayout.PropertyField(templateNameProp);

                    if (soundNamesProp != null)
                        EditorGUILayout.PropertyField(soundNamesProp, true);

                    if (volProp != null)
                        EditorGUILayout.PropertyField(volProp);

                    if (volRandomRangeProp != null)
                        EditorGUILayout.PropertyField(volRandomRangeProp);

                    if (pitchMaxProp != null)
                        EditorGUILayout.PropertyField(pitchMaxProp);

                    if (pitchMinProp != null)
                        EditorGUILayout.PropertyField(pitchMinProp);
                }
                catch (Exception e)
                {
                    EditorGUILayout.HelpBox($"draw SoundEffectInfo attr error: {e.Message}", MessageType.Error);
                }

                EditorGUI.indentLevel--;
            }
        }

        private bool IsSoundExpanded(SerializedProperty soundProp)
        {
            string key = $"SoundExpanded_{soundProp.propertyPath}";
            return SessionState.GetBool(key, false);
        }

        private void SetSoundExpanded(SerializedProperty soundProp, bool expanded)
        {
            string key = $"SoundExpanded_{soundProp.propertyPath}";
            SessionState.SetBool(key, expanded);
        }

        private void DrawActionThrowDataProperties(SerializedProperty throwDataProp)
        {
            try
            {
                SerializedProperty slotIdProp = throwDataProp.FindPropertyRelative("slotId");
                SerializedProperty throwItemNameProp = throwDataProp.FindPropertyRelative("throwItemName");
                SerializedProperty posProp = throwDataProp.FindPropertyRelative("pos");
                SerializedProperty throwForceProp = throwDataProp.FindPropertyRelative("throwForce");
                
                if (slotIdProp != null) EditorGUILayout.PropertyField(slotIdProp);
                if (throwItemNameProp != null) EditorGUILayout.PropertyField(throwItemNameProp);
                if (posProp != null) DrawPosDefineProperties(posProp);
                if (throwForceProp != null) EditorGUILayout.PropertyField(throwForceProp);
            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox($"draw ActionThrowData attr error: {e.Message}", MessageType.Error);
            }
        }

        private void DrawActionCombatDataProperties(SerializedProperty combatDataProp)
        {
            try
            {
                SerializedProperty slotIdProp = combatDataProp.FindPropertyRelative("slotId");
                SerializedProperty hitbackDisProp = combatDataProp.FindPropertyRelative("hitbackDis");
                SerializedProperty blockActionProp = combatDataProp.FindPropertyRelative("blockAction");
                SerializedProperty beHeavyBlockableProp = combatDataProp.FindPropertyRelative("beHeavyBlockable");
                SerializedProperty heldBlockSupportProp = combatDataProp.FindPropertyRelative("heldBlockSupport");
                SerializedProperty notDizzyProbProp = combatDataProp.FindPropertyRelative("notDizzyProb");
                SerializedProperty disarmAttackPobProp = combatDataProp.FindPropertyRelative("disarmAttackPob");
                SerializedProperty stuckActionProp = combatDataProp.FindPropertyRelative("stuckAction");
                SerializedProperty hitDmgAddProp = combatDataProp.FindPropertyRelative("hitDmgAdd");
                SerializedProperty muscleNamesProp = combatDataProp.FindPropertyRelative("muscleNames");
                SerializedProperty blockAnimsRandomProp = combatDataProp.FindPropertyRelative("blockAnimsRandom");
                SerializedProperty blockAnimConfigProp = combatDataProp.FindPropertyRelative("blockAnimConfig");
                SerializedProperty heavyBlockAnimsRandomProp = combatDataProp.FindPropertyRelative("heavyBlockAnimsRandom");
                SerializedProperty useCustomProfileProp = combatDataProp.FindPropertyRelative("useCustomProfile");
                SerializedProperty triggerCombatEventProp = combatDataProp.FindPropertyRelative("triggerCombatEvent");
                SerializedProperty pushingWeaponEnabledProp = combatDataProp.FindPropertyRelative("pushingWeaponEnabled");

                if (slotIdProp != null) EditorGUILayout.PropertyField(slotIdProp);
                if (hitbackDisProp != null) EditorGUILayout.PropertyField(hitbackDisProp);
                if (blockActionProp != null) EditorGUILayout.PropertyField(blockActionProp);
                if (beHeavyBlockableProp != null) EditorGUILayout.PropertyField(beHeavyBlockableProp);
                if (heldBlockSupportProp != null) EditorGUILayout.PropertyField(heldBlockSupportProp);
                if (notDizzyProbProp != null) EditorGUILayout.PropertyField(notDizzyProbProp);
                if (disarmAttackPobProp != null) EditorGUILayout.PropertyField(disarmAttackPobProp);
                if (stuckActionProp != null) EditorGUILayout.PropertyField(stuckActionProp);
                if (hitDmgAddProp != null) EditorGUILayout.PropertyField(hitDmgAddProp);
                if (muscleNamesProp != null) EditorGUILayout.PropertyField(muscleNamesProp);
                if (blockAnimsRandomProp != null) EditorGUILayout.PropertyField(blockAnimsRandomProp);
                if (blockAnimConfigProp != null) EditorGUILayout.PropertyField(blockAnimConfigProp);
                if (heavyBlockAnimsRandomProp != null) EditorGUILayout.PropertyField(heavyBlockAnimsRandomProp);
                if (useCustomProfileProp != null) EditorGUILayout.PropertyField(useCustomProfileProp);
                if (triggerCombatEventProp != null) EditorGUILayout.PropertyField(triggerCombatEventProp);
                if (pushingWeaponEnabledProp != null) EditorGUILayout.PropertyField(pushingWeaponEnabledProp);
            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox($"draw ActionCombatData attr error: {e.Message}", MessageType.Error);
            }
        }

        private void DrawActionEventDataProperties(SerializedProperty eventDataProp)
        {
            try
            {
                SerializedProperty slotIdProp = eventDataProp.FindPropertyRelative("slotId");
                SerializedProperty eventNameProp = eventDataProp.FindPropertyRelative("eventName");
                SerializedProperty customValuesProp = eventDataProp.FindPropertyRelative("customValues");
                SerializedProperty isSceneEventProp = eventDataProp.FindPropertyRelative("isSceneEvent");
                SerializedProperty triggerOnEnterProp = eventDataProp.FindPropertyRelative("triggerOnEnter");

                if (slotIdProp != null) EditorGUILayout.PropertyField(slotIdProp);
                if (eventNameProp != null) EditorGUILayout.PropertyField(eventNameProp);
                if (customValuesProp != null) EditorGUILayout.PropertyField(customValuesProp, true);
                if (isSceneEventProp != null) EditorGUILayout.PropertyField(isSceneEventProp);

                if (isSceneEventProp != null && isSceneEventProp.boolValue)
                {
                    if (triggerOnEnterProp != null) EditorGUILayout.PropertyField(triggerOnEnterProp);
                }
            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox($"draw ActionEventData attr error: {e.Message}", MessageType.Error);
            }
        }

        private void DrawPosDefineProperties(SerializedProperty posProp)
        {
            bool showDetails = EditorGUILayout.Foldout(IsPosDefineExpanded(posProp), "Pos", true);
            SetPosDefineExpanded(posProp, showDetails);

            if (showDetails)
            {
                EditorGUI.indentLevel++;

                try
                {
                    EditorGUILayout.LabelField("PosDefine", EditorStyles.boldLabel);
                    SerializedProperty boneNameProp = posProp.FindPropertyRelative("boneName");
                    SerializedProperty followProp = posProp.FindPropertyRelative("follow");
                    SerializedProperty baseOnLocalProp = posProp.FindPropertyRelative("baseOnLocal");
                    SerializedProperty posOffsetProp = posProp.FindPropertyRelative("pos");
                    SerializedProperty dirProp = posProp.FindPropertyRelative("dir");
                    SerializedProperty upProp = posProp.FindPropertyRelative("up");
                    SerializedProperty weaponPosProp = posProp.FindPropertyRelative("weaponPos");

                    if (boneNameProp != null) EditorGUILayout.PropertyField(boneNameProp);
                    if (followProp != null) EditorGUILayout.PropertyField(followProp);
                    if (baseOnLocalProp != null) EditorGUILayout.PropertyField(baseOnLocalProp);
                    if (posOffsetProp != null) EditorGUILayout.PropertyField(posOffsetProp);
                    if (dirProp != null) EditorGUILayout.PropertyField(dirProp);
                    if (upProp != null) EditorGUILayout.PropertyField(upProp);
                    if (weaponPosProp != null) EditorGUILayout.PropertyField(weaponPosProp);
                }
                catch (Exception e)
                {
                    EditorGUILayout.HelpBox($"draw PosDefine attr error: {e.Message}", MessageType.Error);
                }

                EditorGUI.indentLevel--;
            }
        }

        private bool IsPosDefineExpanded(SerializedProperty posProp)
        {
            string key = $"PosDefineExpanded_{posProp.propertyPath}";
            return SessionState.GetBool(key, false);
        }

        private void SetPosDefineExpanded(SerializedProperty posProp, bool expanded)
        {
            string key = $"PosDefineExpanded_{posProp.propertyPath}";
            SessionState.SetBool(key, expanded);
        }

        private void OnEnable()
        {
            performRangeMinProp = serializedObject.FindProperty("performRangeMin");
            performRangeMaxProp = serializedObject.FindProperty("performRangeMax");
            haveAtkIntentionProp = serializedObject.FindProperty("haveAtkIntention");
            isRangeAtkProp = serializedObject.FindProperty("isRangeAtk");
            atkTimeLenProp = serializedObject.FindProperty("atkTimeLen");
            timeLenProp = serializedObject.FindProperty("timeLen");
            ignoreDynamicSpeedProp = serializedObject.FindProperty("ignoreDynamicSpeed");
            ignoreRagdollProfileProp = serializedObject.FindProperty("ignoreRagdollProfile");
            switchAtkOptionProp = serializedObject.FindProperty("switchAtkOption");
            combineListProp = serializedObject.FindProperty("combineList");
            timelinesProp = serializedObject.FindProperty("timelines");
        }
    }
}
#endif
