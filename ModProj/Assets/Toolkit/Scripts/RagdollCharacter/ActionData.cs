
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System;
#endif


namespace CrossLink
{

    [System.Serializable]
    public class ActionCombineData
    {
        [Tooltip("Reference to the ActionData to combine")]
        public ActionData data;

        [Header("Timeline Options")]
        [Tooltip("Time offset applied to the entire timeline")]
        public float timeOffset = 0;
        [Tooltip("Time scaling factor for the timeline")]
        public float timeScale = 1;

        [Header("Anim Options")]
        [Tooltip("Motion scaling factor for animation")]
        public float motionScale = 0f;
        [Tooltip("Relative distance adjustment for animation")]
        public float relativeDis = 0f;

        [Header("Combat Options")]
        [Tooltip("Whether this action makes the character unknockable")]
        public bool unknockable = false;
        //public bool beBlockable = false;
        [Tooltip("How this attack reacts to being blocked")]
        public ActionCombatData.BlockAction blockAction;
        //public bool keepAttackEvenBlock = false;
        [Tooltip("Whether this attack can be heavy blocked")]
        public bool beHeavyBlock = false;
        [Tooltip("Whether this attack supports held block (continuous blocking)")]
        public bool supportHeldBlock = false;
        [Tooltip("How this attack reacts to being stuck/interrupted")]
        public ActionCombatData.StuckAction stuckAction;
        [Tooltip("Distance to push back enemies when hit")]
        public float hitbackDis = 0;
        [Tooltip("Probability of not causing dizziness when blocked (0-1)")]
        public float notDizzyProb = 0;
        [Tooltip("Probability of disarming the target when hit")]
        public float disarmPob = 0;

        [Header("Check Cancel")]
        [Tooltip("Distance threshold for checking cancel conditions")]
        public float checkCancelDis = 0;

        [Header("Sound Options")]
        [Tooltip("Type of role-specific sound to play")]
        public ActionSoundData.RoleSound roleSoundType = ActionSoundData.RoleSound.None;
        [Tooltip("Delay time before playing the role sound")]
        public float roleSoundDelayToPlay = 0;

        [Header("Windup Options")]
        [Tooltip("Partial animation name for windup/preparation phase")]
        public string windupPartialAnimName;
        [Tooltip("Duration of the windup/preparation phase")]
        public float windupTime;
    }


    //[Sirenix.OdinInspector.ShowOdinSerializedPropertiesInInspector]
    [CreateAssetMenu(fileName = "ActionData", menuName = "ActionData")]
    public class ActionData : ScriptableObject
        {
        [Tooltip("Minimum distance to trigger this action (distance to target)")]
        public float performRangeMin = 2;
        [Tooltip("Maximum distance to trigger this action (distance to target)")]
        public float performRangeMax = 5;
        [Tooltip("Has attack intention")]
        public bool haveAtkIntention = false;
        [Tooltip("Area attack")]
        public bool isRangeAtk = false;
        [Tooltip("Attack duration (determines how long the attack can be blocked)")]
        public float atkTimeLen = 0;
        [Tooltip("Total duration")]
        public float timeLen = 0;
        [Tooltip("Do not reset playback speed before playing")]
        public bool ignoreDynamicSpeed = false;
        [Tooltip("Ignore RagdollProfile during playback (e.g., hit reaction Profiles)")]
        public bool ignoreRagdollProfile = false;
        public SwitchAtkType switchAtkOption = SwitchAtkType.AllowSwitch;

        public enum SwitchAtkType
        {
            AllowSwitch,
            DisableSwitch,
            DisableSwitchToHere,
        }


        public ActionCombineData[] combineList;

        public TimelineData[] timelines;

#if UNITY_EDITOR
        [EasyButtons.Button]
        public void OutputCombine()
        {
            if (combineList == null || combineList.Length == 0)
            {
                UpdateMetaInfo();
                return;
            }


            int tlNum = 0;
            for (int i = 0; i < combineList.Length; ++i)
            {
                if (combineList[i].data == null)
                    continue;
                tlNum += combineList[i].data.timelines.Length;
            }
            timelines = new TimelineData[tlNum];


            // process
            // 1. clone data
            // 2. process options in clone data
            // 3. fill data into actual timelines according to timelineIndex
            int timelineIndex = 0;
            float timeOffset = 0;
            for (int i = 0; i < combineList.Length; ++i)
            {
                // special case: no data
                if (combineList[i].data == null)
                {
                    if (string.IsNullOrEmpty(combineList[i].windupPartialAnimName) == false)
                    {
                        // fill an empty timeline in timelines, and setup actiondata to it
                        Array.Resize(ref timelines, timelines.Length + 1);
                        var newTimeline = new TimelineData();
                        timelines[timelineIndex] = newTimeline;
                        timelines[timelineIndex].startTime = timeOffset;
                        timelines[timelineIndex].endTime = timeOffset + combineList[i].windupTime;

                        //timeOffset += combineList[i].windupTime;
                        ++timelineIndex;

                        var actAnim = new ActionAnimData();
                        actAnim.animName = combineList[i].windupPartialAnimName;
                        actAnim.animLayer = -1;
                        newTimeline.actionDatas = new ActionSubData[] { actAnim };
                        continue;
                    }

                    Debug.LogError("ActionData should not be empty at index:" + i);
                    return;

                }

                // for each combine data
                var cloneData = UnityEngine.Object.Instantiate(combineList[i].data);
                float offset = combineList[i].timeOffset + timeOffset;
                float timeLen = 0;


                // process role sound
                if (combineList[i].roleSoundType != ActionSoundData.RoleSound.None)
                {
                    for (int j = 0; j < cloneData.timelines.Length; ++j)
                    {
                        if (cloneData.timelines[j].startTime > 0)
                            continue;

                        var actSound = new ActionSoundData();
                        actSound.roleSound = combineList[i].roleSoundType;
                        if (combineList[i].roleSoundDelayToPlay > 0)
                        {
                            actSound.delayToPlay = combineList[i].roleSoundDelayToPlay;
                        }
                        var acts = new List<ActionSubData>(cloneData.timelines[j].actionDatas);
                        actSound.SetHeadAsSoundSource();
                        acts.Add(actSound);
                        cloneData.timelines[j].actionDatas = acts.ToArray();
                        break;
                    }
                }

                if (combineList[i].checkCancelDis > 0)
                {
                    for (int j = 0; j < cloneData.timelines.Length; ++j)
                    {
                        for (int k = 0; k < cloneData.timelines[j].actionDatas.Length; ++k)
                        {
                            var sb = cloneData.timelines[j].actionDatas[k] as ActionAnimData;
                            if (sb == null)
                                continue;

                            sb.checkCancelDis = combineList[i].checkCancelDis;
                        }
                    }
                }

                // specificly handle those by-passing combination
                if (combineList[i].supportHeldBlock
                    && combineList[i].data.combineList != null
#if true
                    && combineList[i].data.combineList.Length >= 1
#else
                    && combineList[i].data.combineList.Length == 1
                    && combineList[i].data.combineList[0].supportHeldBlock == false
#endif
                    )
                {
                    for (int j = 0; j < cloneData.timelines.Length; ++j)
                    {
                        for (int k = 0; k < cloneData.timelines[j].actionDatas.Length; ++k)
                        {
                            var sb = cloneData.timelines[j].actionDatas[k] as ActionSoundData;
                            if (sb == null || sb.sound == null)
                                continue;

                            sb.RecoverFromDifferentPitchWhoosh();
                        }
                    }
                }
                // process fight sound if heavy attack
                else
                if (combineList[i].supportHeldBlock == false

                    // but can not be the refencing
                    && (combineList[i].beHeavyBlock == false
                        && combineList[i].hitbackDis == 0
                        && combineList[i].notDizzyProb == 0
                        && combineList[i].disarmPob == 0) == false

                    // step around doesn't count
                    && (combineList[i].data.HaveAtkIntention())

                    )
                {
                    for (int j = 0; j < cloneData.timelines.Length; ++j)
                    {
                        for (int k = 0; k < cloneData.timelines[j].actionDatas.Length; ++k)
                        {
                            var sb = cloneData.timelines[j].actionDatas[k] as ActionSoundData;
                            if (sb == null || sb.sound == null)
                                continue;

                            sb.sound.MakeItSoundHeavier();
                        }
                    }
                }




                // if config is default, then we use as it is
                if (combineList[i].beHeavyBlock
                    || combineList[i].supportHeldBlock
                    || combineList[i].blockAction != ActionCombatData.BlockAction.NoBlock
                    || combineList[i].unknockable
                    || combineList[i].hitbackDis > 0
                    || combineList[i].stuckAction != ActionCombatData.StuckAction.Ignore
                    || combineList[i].disarmPob > 0
                    || combineList[i].motionScale > 0)
                {
                    for (int t = 0; t < cloneData.timelines.Length; ++t)
                    {
                        for (int a = 0; a < cloneData.timelines[t].actionDatas.Length; ++a)
                        {
                            var cb = cloneData.timelines[t].actionDatas[a] as ActionCombatData;
                            if (cb != null)
                            {
                                if (combineList[i].beHeavyBlock)
                                {
                                    cb.beHeavyBlockable = combineList[i].beHeavyBlock;
                                    // do not cover those original data                                              
                                }
                                if (combineList[i].notDizzyProb > 0)
                                {
                                    cb.notDizzyProb = combineList[i].notDizzyProb;
                                }
                                if (combineList[i].supportHeldBlock)
                                {
                                    cb.heldBlockSupport = combineList[i].supportHeldBlock;
                                }
#if false
                                if (combineList[i].keepAttackEvenBlock)
                                {
                                    cb.keepAttackEvenBlock = combineList[i].keepAttackEvenBlock;
                                }
#endif
                                if (combineList[i].blockAction != ActionCombatData.BlockAction.NoBlock)
                                {
                                    //cb.unknockable = false;
                                    //cb.beBlockable = true;
                                    cb.blockAction = combineList[i].blockAction == ActionCombatData.BlockAction.NoneBlock ? ActionCombatData.BlockAction.NoBlock
                                                    : combineList[i].blockAction;
                                }

#if false
                                if (combineList[i].unknockable)
                                {
                                    cb.unknockable = true;
                                }
#endif
                                if (combineList[i].hitbackDis > 0)
                                {
                                    cb.hitbackDis = combineList[i].hitbackDis;
                                }
                                if (combineList[i].stuckAction != ActionCombatData.StuckAction.Ignore)
                                {
                                    cb.stuckAction = combineList[i].stuckAction;
                                }
                                if (combineList[i].disarmPob > 0)
                                {
                                    cb.disarmAttackPob = combineList[i].disarmPob;
                                }
                            }

                            var ab = cloneData.timelines[t].actionDatas[a] as ActionAnimData;
                            if (ab != null)
                            {
                                if (combineList[i].motionScale > 0)
                                {
                                    ab.motionScale = combineList[i].motionScale;
                                }
                                if (combineList[i].relativeDis > 0)
                                {
                                    var lp = ab.trackLinearAnchorPoint;
                                    lp.z = combineList[i].relativeDis;
                                    ab.trackLinearAnchorPoint = lp;
                                }
                                if (combineList[i].unknockable)
                                {
                                    ab.unknockable = true;
                                }
                            }
                        }
                    }
                }

                if (combineList[i].timeScale > 0)
                {
                    cloneData.TimeScale(combineList[i].timeScale);
                }


                // fill data into actual timelines
                for (int j = 0; j < cloneData.timelines.Length; ++j)
                {

                    timelines[timelineIndex] = cloneData.timelines[j];
                    //if (combineList[i].timeOffset != 0)
                    {
                        timelines[timelineIndex].TimeOffset(offset);
                    }

                    // find the longest time
                    if (timelines[timelineIndex].endTime > timeLen)
                    {
                        timeLen = timelines[timelineIndex].endTime;
                    }

                    ++timelineIndex;
                }
                timeOffset = timeLen;
            }

            UpdateMetaInfo();
        }

        [EasyButtons.Button]
        void TimeScale(float s)
        {
            // for each timeline scale time begin&end
            // for anim, setup timescale
            // for delay sound, scale delay time
            for (int i = 0; i < timelines.Length; ++i)
            {
                var tl = timelines[i];
                tl.startTime *= s;
                tl.endTime *= s;
                for (int a = 0; a < tl.actionDatas.Length; ++a)
                {
                    var action = tl.actionDatas[a];
                    var anim = action as ActionAnimData;
                    if (anim != null)
                    {
                        anim.timeScale /= s;

                        anim.trackPosBeginTime *= s;
                        anim.trackPosTime *= s;
                        anim.ignorePosBeginTime *= s;
                        anim.ignorePosTime *= s;
                        anim.motionTimeOffset *= s;

                        anim.trackRotBeginTime *= s;
                        anim.trackRotTime *= s;
                        anim.ignoreRotTime *= s;
                    }
                    var sound = action as ActionSoundData;
                    if (sound != null)
                    {
                        sound.delayToPlay *= s;
                    }
                }
            }

            EditorUtility.SetDirty(this);
        }

        void UpdateMetaInfo()
        {
            atkTimeLen = FindAtkLenTime();
            haveAtkIntention = HaveAtkIntention();
            isRangeAtk = HaveAtkIntention(true);

            int slotId = 0;
            timeLen = 0;
            for (int i = 0; i < timelines.Length; ++i)
            {
                for (int j = 0; j < timelines[i].actionDatas.Length; ++j)
                {
                    timelines[i].actionDatas[j].slotId = slotId++;
                    if (timelines[i].endTime > timeLen)
                    {
                        timeLen = timelines[i].endTime;
                    }
                }
            }

            EditorUtility.SetDirty(this);
        }
        bool HaveAtkIntention(bool range = false)
        {
            bool haveWarning = false;
            for (int t = 0; t < timelines.Length; ++t)
            {
                for (int a = 0; a < timelines[t].actionDatas.Length; ++a)
                {
                    var action = timelines[t].actionDatas[a];
                    if (range == false && action is ActionCombatData)
                    {
                        haveWarning = true;
                        break;
                    }
                    if (action is ActionFlyObjectData)
                    {
                        haveWarning = true;
                        break;
                    }
                    if (action is ActionThrowData)
                    {
                        haveWarning = true;
                        break;
                    }
                }

                if (haveWarning)
                    break;
            }

            return haveWarning;
        }
        float FindAtkLenTime()
        {
            float atkLenTime = 0;

            for (int t = 0; t < timelines.Length; ++t)
            {
                for (int a = 0; a < timelines[t].actionDatas.Length; ++a)
                {
                    var action = timelines[t].actionDatas[a];
                    if (action is ActionCombatData)
                    {
                        if (timelines[t].endTime > atkLenTime)
                        {
                            atkLenTime = timelines[t].endTime;
                        }
#if false
                        if (timelines[t].endTime <= 0)
                        {
                            Debug.Log(name + " LenTime==0" + " T:" + t);
                            //continue;
                        }
#endif
                    }
                    if (action is ActionFlyObjectData)
                    {
                        if (timelines[t].startTime > atkLenTime)
                        {
                            atkLenTime = timelines[t].startTime;
                        }
                    }
                }
            }

            return atkLenTime;
        }
#endif
    }

    [System.Serializable]
    public class TimelineData
    {
        // action prepare time, before the number of seconds enter the timeline actions
        // Example: refers to the number of seconds between the strong attack point(show strongAtk weapon trail) and the actual strong attack
        public float prepareTime = 0f;
        public float startTime;
        public float endTime;


        [SerializeReference] public ActionSubData[] actionDatas;


#if UNITY_EDITOR
        [EasyButtons.Button]
        public void TimeOffset(float t)
        {
            startTime += t;
            if (endTime > 0)
            {
                endTime += t;
            }
            if (startTime < 0)
            {
                Debug.LogError("Offset Error");
            }
            for (int i = 0; i < actionDatas.Length; ++i)
            {
                actionDatas[i].TimeOffset(t);
            }
        }
#endif
    }

    [System.Serializable]
    public class ActionSubData
    {
        public int slotId;

        public virtual void OnPrepare(Actor actor)
        {

        }

        public virtual void OnEnter(Actor actor)
        {

        }
        public virtual void OnUpdate(Actor actor)
        {

        }
        public virtual void OnExit(Actor actor)
        {

        }

        public virtual void OnCancel(Actor actor, float remainSec)
        {
            OnExit(actor);
        }

        public virtual void OnClear(Actor actor, float remainSec)
        {

        }

        public virtual void TimeOffset(float t)
        {

        }
    }

    [System.Serializable]
    public class PosDefine
    {
        [Tooltip("Name of the bone to attach to. If empty, uses owner's transform")]
        public string boneName;
        // follow with the bone
        // in flyobj, means follow target
        [Tooltip("Whether to follow the bone/owner transform during movement")]
        public bool follow = false;
        // means baseonbone's local or owner's local
        [Tooltip("Whether the position and rotation are relative to the bone's local space or owner's local space")]
        public bool baseOnLocal = false;
        // pos offset
        [Tooltip("Position offset relative to the bone or owner")]
        public Vector3 pos = Vector3.zero;
        // rotation
        [Tooltip("Forward direction vector for rotation")]
        public Vector3 dir = Vector3.forward;
        [Tooltip("Up direction vector for rotation")]
        public Vector3 up = Vector3.up;

        public enum BaseOnWeaponPos
        {
            None,
            LWeapon,
            RWeapon,
        }

        // so that we could use weapon bone without having weapon muscle
        // it's useful for mega characters
        [Tooltip("Automatically use weapon bone positions without requiring weapon muscle")]
        public BaseOnWeaponPos weaponPos;
    }

    [System.Serializable]
    public class ActionAnimData : ActionSubData
    {
        public enum SpecialMoveType
        {
            Normal,
            Walk,
            Stand,
            Keep,
            Turn,
        }

        [Tooltip("Animation name")]
        public string animName;
        [Tooltip("animation layer, -1 means partial animation")]
        public int animLayer = 0; // -1 means partial anim
        [Tooltip("Cross-fade duration for smooth transition")]
        public float crossfadeTime = 0.1f;
        [Tooltip("Animation playback speed")]
        public float timeScale = 1.0f;
        [Tooltip("Root motion duration")]
        public float customLayoutLen = 0;
        [Tooltip("Root motion time offset")]
        public float motionTimeOffset = 0;
        //public float bodyScale = -1;

        [Tooltip("Root motion name")]
        public string rootMotion;
        [Tooltip("Loop root motion playback")]
        public bool loopMotion = false;
        [Tooltip("Cross-fade duration for smooth transition")]
        public SpecialMoveType moveType = SpecialMoveType.Normal;
        public float turnStartAngle = 10;
        public float turnStopAngle = 10;
        public float turnSpeedMlp = 1;
        public float rotateSpeed = -1;

        [Tooltip("Immune to knockout")]
        public bool unknockable = false;

        // scale path
        [Tooltip("Root motion scaling")]
        public float motionScale = 1;

        // track target
        [Tooltip("Start time for position tracking (animation progress 0-1)")]
        public float trackPosBeginTime = 0; // track begin time
        [Tooltip("End time for position tracking (animation progress 0-1)")]
        public float trackPosTime = -1;// track end time
        [Tooltip("Start time to ignore Root Motion position displacement")]
        public float ignorePosBeginTime = 0;
        [Tooltip("Start time to ignore position tracking (ignores both Root Motion displacement and tracking)")]
        public float ignorePosTime = 0;// ignore track if progress bigger than this

        [Tooltip("Start time for rotation tracking (animation progress 0-1)")]
        public float trackRotBeginTime = 0;
        [Tooltip("End time for rotation tracking (animation progress 0-1)")]
        public float trackRotTime = -1;
        [Tooltip("Start time to ignore rotation tracking (ignores both Root Motion rotation and tracking)")]
        public float ignoreRotTime = 0;

        [Tooltip("Controls the timing of Ragdoll configuration updates")]
        public float ignoreProfile = 0;
        [Tooltip("Position offset relative to the target point during position tracking phase")]
        public Vector3 trackLinearAnchorPoint;
        [Tooltip("Orientation relative to the target during rotation tracking phase")]
        public Vector3 trackAngularAnchorPoint;

        [Tooltip("add a floating range to trackLinearAnchorPoint\n" +
            "Vector3(x, y, z) means: trackLinearAnchorPoint + new Vector3(Random.Range(min, max), 0f, Random.Range(min, max))")]
        public Vector3 linearTrackPointFloatingRangeMin;
        [Tooltip("add a floating range to trackLinearAnchorPoint\n" +
            "Vector3(x, y, z) means: trackLinearAnchorPoint + new Vector3(Random.Range(min, max), 0f, Random.Range(min, max))")]
        public Vector3 linearTrackPointFloatingRangeMax;

        [Tooltip("Controls the timing for enabling character physics joint updates, primarily affecting character physics simulation and ragdoll effects")]
        public float jointUpdateDisabledTime = 0;
        [Tooltip("Controls the timing for disabling character physics joint updates, primarily affecting character physics simulation and ragdoll effects")]
        public float jointUpdateDisabledStopTime = 0;
        [Tooltip("Checks distance to target when entering this action; cancels playback if distance exceeds this value (effective when value > 0)")]
        public float checkCancelDis = 0;

        public override void OnEnter(Actor actor)
        {
            var action = actor.GetActionData();
            if (action == null)
                return;

            actor.editor.SetAnimSpeed(timeScale / actor.GetPlayTimeMlp());

            if (string.IsNullOrEmpty(animName) == false)
            {
                actor.editor.animator.CrossFade(animName, crossfadeTime, animLayer, 0);
            }

        }
    }

    [System.Serializable]
    public class ActionFlyObjectData : ActionSubData
    {
        [Tooltip("Name of the fly object prefab to spawn")]
        public string flyObjectName;
        [Tooltip("Initial velocity of the fly object")]
        public float velocity;
        [Tooltip("Whether to automatically aim at the target")]
        public bool aimTarget = false;
        [Tooltip("Delay time before the fly object starts moving")]
        public float flyDelay = 0;
        [Tooltip("Whether to spawn the fly object directly at the target position")]
        public bool spawnOnAimTarget = false;
        [Tooltip("Offset from the target position when aiming")]
        public Vector3 aimTargetOffset;
        [Tooltip("Random deviation range for aiming accuracy")]
        public float aimRandom = 0;
        [Tooltip("Maximum range for aim assistance")]
        public float limitAimHelpRange = 0;
        [Tooltip("Position definition for fly object spawning")]
        public PosDefine pos;
        [Tooltip("Y-axis distance factor for trajectory calculation")]
        public float disYFactor = 0;
        [Tooltip("Starting distance for applying Y-axis distance factor")]
        public float disYFactorStartDis = 0;
        //public float maxRotateDegrees = 0;
        [Tooltip("Maximum rotation speed for aiming (degrees per second)")]
        public float maxAimSpeed = 0;
        [Tooltip("Delay before starting rotation towards target")]
        public float rotateDelay = 0;
        [Tooltip("Whether to destroy the fly object when exiting this action")]
        public bool deadOnExit = false;

#if UNITY_EDITOR
        GameObject fo;
        bool isSpawn = false;
        void DoSpawnFlyObj(Actor actor)
        {
            isSpawn = true;

            if (actor == null)
                return;


            var path = AddressableHelper.GetAddressableAssetPath(flyObjectName);
            if (string.IsNullOrEmpty(path))
                return;

            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
                return;
            fo = GameObject.Instantiate(asset);

            var des = fo.AddComponent<AutoDestroy>();
            var flyObject = fo.GetComponent<FlyObject>();
            if (flyObject)
            {
                des.destroyDelay = flyObject.maxFlyTime;
                des.StartTimer();
            }

            if (pos != null)
            {
                Vector3 basePos = actor.trans.position;
                Vector3 worldDir = Vector3.zero;
                Vector3 worldUp = Vector3.up;
                if (string.IsNullOrEmpty(pos.boneName) == false)
                {
                    var boneTrans = actor.GetBone(pos.boneName);
                    if (boneTrans != null)
                    {
                        if (pos.baseOnLocal)
                        {
                            basePos = boneTrans.TransformPoint(pos.pos);
                            worldDir = boneTrans.TransformDirection(pos.dir);
                            worldUp = boneTrans.TransformDirection(pos.up);
                        }
                        else
                        {
                            basePos = boneTrans.position;
                        }
                        if (pos.follow)
                        {
                            fo.transform.parent = boneTrans;
                        }
                    }
                }
                else
                {
                    if (pos.follow)
                    {
                        fo.transform.parent = actor.trans;
                    }
                    if (pos.baseOnLocal)
                    {
                        basePos = actor.trans.TransformPoint(pos.pos);
                        worldDir = actor.trans.TransformDirection(pos.dir);
                        worldUp = actor.trans.TransformDirection(pos.up);
                    }
                }
                if (worldDir == Vector3.zero)
                {
                    worldDir = actor.trans.TransformDirection(pos.dir);
                }

                if (pos.baseOnLocal)
                {
                    fo.transform.rotation = Quaternion.LookRotation(worldDir, worldUp);
                }
                else
                {
                    fo.transform.rotation = Quaternion.LookRotation(worldDir, pos.up);
                }

                if (pos.baseOnLocal == false)
                {
                    basePos += actor.trans.TransformVector(pos.pos);
                }
                fo.transform.position = basePos;

                fo.GetComponent<Rigidbody>().velocity = fo.transform.forward * velocity;
            }
        }

        public override void OnEnter(Actor actor)
        {
            if (flyDelay <= 0)
                DoSpawnFlyObj(actor);
        }

        public override void OnExit(Actor actor)
        {
            if (fo != null && deadOnExit)
            {
                GameObject.DestroyImmediate(fo);
            }
            isSpawn = false;
        }

        public override void OnUpdate(Actor actor)
        {
            if (flyDelay > 0 && Time.time - actor.playTime >= flyDelay && !isSpawn)
            {
                DoSpawnFlyObj(actor);
            }
        }
#endif
    }

    [System.Serializable]
    public class ActionEffectData : ActionSubData
    {
        [Tooltip("Name of the effect prefab to play")]
        public string effectName;
        [Tooltip("Position and rotation definition for the effect")]
        public PosDefine pos;
        [Tooltip("Whether rotation is based on local space (bone/owner) or world space")]
        public bool rotationBaseLocal = false;
        [Tooltip("Whether to remove the effect when the action finishes")]
        public bool removeWhenActionFinish = false;
        [Tooltip("Whether to stop the effect when the action finishes")]
        public bool stopWhenActionFinish = false;
        [Tooltip("Loop ID for identifying loop effects. -1 means non-looping effect")]
        public int loopId = -1;

#if UNITY_EDITOR
        GameObject eo;
        public override void OnEnter(Actor actor)
        {
            var path = AddressableHelper.GetAddressableAssetPath(effectName);
            if (string.IsNullOrEmpty(path))
                return;

            DebugDraw.DrawLine(actor.trans.position, actor.trans.position + actor.trans.forward, Color.green, 2f);
            DebugDraw.DrawMarker(actor.trans.position, 0.1f, Color.green, 2f);

            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
                return;
            eo = GameObject.Instantiate(asset);

            var des = eo.AddComponent<AutoDestroy>();
            var effectObj = eo.GetComponent<EffectObj>();
            if (effectObj)
            {
                des.destroyDelay = effectObj.autoRemoveTime;
                des.StartTimer();
            }

            bool isChild = false;

            if (pos != null)
            {
                var boneTrans = actor.GetBone(pos.boneName);
                if (boneTrans != null)
                {
                    if (pos.follow)
                    {
                        effectObj.transform.parent = boneTrans;
                        isChild = true;
                    }
                    if (pos.baseOnLocal)
                    {
                        effectObj.transform.position = boneTrans.TransformPoint(pos.pos);
                        effectObj.transform.rotation = Quaternion.LookRotation(boneTrans.TransformDirection(pos.dir), rotationBaseLocal ? boneTrans.TransformDirection(pos.up) : Vector3.up);
                    }
                    else
                    {
                        effectObj.transform.position = actor.trans.TransformPoint(pos.pos);
                        effectObj.transform.rotation = Quaternion.LookRotation(actor.trans.TransformDirection(pos.dir), pos.up);
                    }
                }
                else
                {
                    effectObj.transform.position = actor.trans.TransformPoint(pos.pos);
                    effectObj.transform.rotation = Quaternion.LookRotation(actor.trans.TransformDirection(pos.dir));

                }
            }
            else
            {
                effectObj.transform.position = actor.trans.position;
                effectObj.transform.rotation = actor.trans.rotation;
            }
        }

        public override void OnUpdate(Actor actor)
        {
            if (eo != null && pos != null && pos.follow && !pos.baseOnLocal)
            {
                eo.transform.position = actor.trans.TransformPoint(pos.pos);
                eo.transform.rotation = Quaternion.LookRotation(actor.trans.TransformDirection(pos.dir), pos.up);
            }
        }

        public override void OnExit(Actor actor)
        {
            if (eo == null)
                return;

            if (stopWhenActionFinish || removeWhenActionFinish)
                GameObject.Destroy(eo);

            eo = null;
        }

#endif
    }

    [System.Serializable]
    public class ActionSoundData : ActionSubData
    {
        [Tooltip("Position definition for sound source. If null, uses global sound")]
        public PosDefine pos;
        [Tooltip("Delay time before playing the sound")]
        public float delayToPlay = 0;
        [Tooltip("Whether the sound should loop continuously")]
        public bool loop = false;
        //public bool keepSoundUntilCancel = false;
        [Tooltip("Sound effect information including clip, volume, pitch, etc.")]
        public SoundEffectInfo sound;
        public enum RoleSound
        {
            None,
            Warning,
            Attack,
            Taunt,
        }

        [Tooltip("Type of role-specific sound to play")]
        public RoleSound roleSound = RoleSound.None;


#if UNITY_EDITOR

        [EasyButtons.Button]
        public void SetSoundHeavier()
        {
            if (sound == null)
            {
                sound = new SoundEffectInfo();
            }
            sound.pitchMax -= 0.25f;
            sound.pitchMin -= 0.25f;
        }
        [EasyButtons.Button]
        public void SetSoundLighter()
        {
            if (sound == null)
            {
                sound = new SoundEffectInfo();
            }
            sound.pitchMax += 0.25f;
            sound.pitchMin += 0.25f;
        }


        [EasyButtons.Button]
        public void SetSmallWhoosh()
        {
            if (sound == null)
            {
                sound = new SoundEffectInfo();
            }
            sound.templateName = "WhooshSmall";
        }

        [EasyButtons.Button]
        public void SetNormalWhoosh()
        {
            if (sound == null)
            {
                sound = new SoundEffectInfo();
            }
            sound.templateName = "WhooshNormal";
        }

        [EasyButtons.Button]
        public void SetHeavyWhoosh()
        {
            if (sound == null)
            {
                sound = new SoundEffectInfo();
            }
            sound.templateName = "WhooshHeavy";
        }
        [EasyButtons.Button]
        public void RecoverFromDifferentPitchWhoosh()
        {
            if (sound.templateName == "WhooshHeavy")
            {
                SetHeavyWhoosh();
            }
            else if (sound.templateName == "WhooshNormal")
            {
                SetNormalWhoosh();
            }
            else if (sound.templateName == "WhooshSmall")
            {
                SetSmallWhoosh();
            }
        }

        [EasyButtons.Button]
        public void SetRollSound()
        {
            if (sound == null)
            {
                sound = new SoundEffectInfo();
            }
            sound.templateName = "StepRollSound";
        }

        [EasyButtons.Button]
        public void SetStepSound()
        {
            if (sound == null)
            {
                sound = new SoundEffectInfo();
            }
            sound.templateName = "StepSound";
        }

        [EasyButtons.Button]
        public void SetNullWeapon()
        {
            pos = null;
        }

        [EasyButtons.Button]
        public void SetRWeaponAsSoundSource()
        {
            pos = new PosDefine();
            pos.boneName = RagdollBoneInfo.RWeapon;
            pos.follow = true;
        }

        [EasyButtons.Button]
        public void SetLWeaponAsSoundSource()
        {
            pos = new PosDefine();
            pos.boneName = RagdollBoneInfo.LWeapon;
            pos.follow = true;
        }

        [EasyButtons.Button]
        public void SetHeadAsSoundSource()
        {
            pos = new PosDefine();
            pos.boneName = RagdollBoneInfo.Head;
            pos.follow = true;
        }

        [EasyButtons.Button]
        public void SetPelvisAsSoundSource()
        {
            pos = new PosDefine();
            pos.boneName = RagdollBoneInfo.Pelvis;
            pos.follow = true;
        }

        AudioSource ao;
        bool isPlaying = false;
        void DoPlaySound(Actor actor)
        {
            if (actor == null)
                return;

            var obj = new GameObject("soundObj");
            var des = obj.AddComponent<AutoDestroy>();
            ao = obj.AddComponent<AudioSource>();
            ao.loop = loop;

            var path = AddressableHelper.GetAddressableAssetPath(sound.soundNames[0]);
            if (string.IsNullOrEmpty(path))
                return;

            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
                return;
            ao.clip = clip;
            ao.volume = sound.vol;
            des.destroyDelay = clip.length;
            des.StartTimer();

            float pitchMax = sound.pitchMax == -1 ? 1 : sound.pitchMax;
            ao.pitch = UnityEngine.Random.Range(sound.pitchMin, pitchMax);
            ao.Play();

            if (pos != null)
            {
                if (pos.follow)
                {
                    var boneTrans = actor.GetBone(pos.boneName);
                    if (boneTrans != null)
                    {
                        ao.transform.parent = boneTrans;
                        ao.transform.localPosition = Vector3.zero;
                    }
                }
            }

            isPlaying = true;
        }

        public override void OnEnter(Actor actor)
        {
            if (delayToPlay <= 0)
                DoPlaySound(actor);
        }

        public override void OnExit(Actor actor)
        {
            if (ao != null)
            {
                if (loop)
                {
                    GameObject.Destroy(ao.gameObject);
                }
                else if (ao.clip == null)
                {
                    GameObject.Destroy(ao.gameObject);
                }
            }

            isPlaying = false;
        }

        public override void OnUpdate(Actor actor)
        {
            if (delayToPlay > 0 && Time.time - actor.playTime >= delayToPlay && !isPlaying)
            {
                DoPlaySound(actor);
            }
        }
#endif
    }

    [System.Serializable]
    public class ActionThrowData : ActionSubData
    {
        [Tooltip("Name of the throwable item prefab")]
        public string throwItemName;
        [Tooltip("Position and rotation definition for throwing")]
        public PosDefine pos;
        [Tooltip("Force vector applied to the thrown item")]
        public Vector3 throwForce;

#if UNITY_EDITOR
        public override void OnExit(Actor actor)
        {
            if (pos != null)
            {
                Vector3 basePos = actor.trans.position;
                Quaternion baseRot = Quaternion.identity;
                Transform bone = null;
                if (string.IsNullOrEmpty(pos.boneName) == false)
                {
                    bone = actor.editor.GetBone(pos.boneName);
                    if (bone == null)
                        return;

                    if (pos.weaponPos != PosDefine.BaseOnWeaponPos.None)
                    {
                        bone = pos.weaponPos == PosDefine.BaseOnWeaponPos.LWeapon ?
                                actor.editor.GetWeaponTrans("LWeapon")
                                : actor.editor.GetWeaponTrans("RWeapon");
                    }

                    if (bone == null)
                        return;

                    basePos = bone.TransformPoint(pos.pos);
                    baseRot = bone.rotation * Quaternion.Euler(pos.dir);
                }

                var dir = actor.trans.TransformDirection(throwForce);
                DebugDraw.DrawLine(basePos, basePos + dir.normalized, Color.red, 2f);
                DebugDraw.DrawMarker(basePos, 0.1f, Color.red, 2f);
            }
        }
#endif

    }

    [System.Serializable]
    public class ActionCombatData : ActionSubData
    {
        public enum StuckAction
        {
            [Tooltip("nothing happen, just keep playing")]
            Ignore = 0,       // nothing happen, just keep playing
            [Tooltip("skip anim for a while and keep playing action")]
            StuckAWhile = 1,  // skip anim for a while and keep playing action
            [Tooltip("play stuck(block if handle) and keep playing action")]
            StuckAndNext = 2, // play stuck(block if handle) and keep playing action
            [Tooltip("play block anim and keep playing action")]
            BlockAndNext = 3, // play block anim and keep playing action
            [Tooltip("stuck or block away")]
            StuckNextOrBlockAway = 4, // stuck or block away
            //StuckAWhileByTimeing = 3,
        }

        public enum BlockAction
        {
            [Tooltip("this attack action wouldn't be cancelled, the StuckAction will start to work")]
            NoBlock,// this attack action wouldn't be cancelled, the StuckAction will start to work
            // the rest options will stop the action you're playing
            [Tooltip("block away")]
            BlockAndAway, // block away
            [Tooltip("stuck or block away")]
            BlockAndStop, // stuck or away
            [Tooltip("block away or continue")]
            BlockByTiming, // block away or continue
            [Tooltip("(stuck or block away) or continue")]
            StopByTiming, // (stuck or block away) or continue
            [Tooltip("this use to reset the data to noblock when combining")]
            NoneBlock = 100, // this use to reset the data to noblock when combining
        }

        public enum BlockAnimConfig
        {
            Dynamic,
            Config,
            AlwaysToLeft,
            AlwaysToRight,
        }
        // hitback others, 
        [Tooltip("Distance to push back enemies when hit")]
        public float hitbackDis = 0;
        // if touch player's weapon, then play block anim or into dizzy
        [Tooltip("How this attack reacts to being blocked")]
        public BlockAction blockAction = BlockAction.NoBlock;

        [Tooltip("Whether this attack can be heavy blocked")]
        public bool beHeavyBlockable = false;
        // held block means enemy's action still performance after colliding, 
        // but no damage for a while
        // if turns off, then need at least heavy atk to save you
        [Tooltip("Whether this attack supports held block (continuous blocking)")]
        public bool heldBlockSupport = false;
        // only perfect block can save you from this atk, 
        // need heldBlockSupport opened
        //public bool perfectBlockRequire = false;
        [Tooltip("Probability of not causing dizziness when blocked (0-1)")]
        public float notDizzyProb = 0;
        // can disarm player
        [Tooltip("Probability of disarming the target when hit")]
        public float disarmAttackPob = 0;

        [Tooltip("How this attack reacts to being stuck/interrupted")]
        public StuckAction stuckAction = StuckAction.Ignore;

        // add dmg to target hitinfo
        [Tooltip("Additional damage to apply on hit")]
        public int hitDmgAdd = 0;

        // activate muscle
        [Tooltip("Names of muscles/weapons used in this attack")]
        public string[] muscleNames;
        // anim to play after block
        [Tooltip("Random block animations to play when blocked")]
        public string[] blockAnimsRandom;
        [Tooltip("Configuration for block animation selection")]
        public BlockAnimConfig blockAnimConfig = BlockAnimConfig.Dynamic;
        //public string blockEffect;
        [Tooltip("Random heavy block animations to play when heavily blocked")]
        public string[] heavyBlockAnimsRandom;
        //public string heavyBlockEffect;
        //public HeavyBlockAnimConfig heavyBlockAnimConfig = HeavyBlockAnimConfig.Dynamic;

        //apply atk ragdollProfile or custom use actionMuscleStateData
        [Tooltip("Whether to use custom ragdoll profile instead of attack profile")]
        public bool useCustomProfile = false;
        [Tooltip("Whether to trigger combat events during this action")]
        public bool triggerCombatEvent = false;

        [Tooltip("Whether weapons can be pushed away by player during this action")]
        public bool pushingWeaponEnabled = false;

#if UNITY_EDITOR
        public override void OnUpdate(Actor actor)
        {
            if (muscleNames != null)
            {
                for (int i = 0; i < muscleNames.Length; i++)
                {
                    var trans = actor.editor.GetWeaponTrans(muscleNames[i]);
                    if (trans)
                    {
                        DebugDraw.DrawLine(trans.position, trans.position + trans.forward, Color.red, 0.2f);
                        DebugDraw.DrawMarker(trans.position, 0.1f, Color.red, 0.2f);
                    }
                }

            }
        }
#endif
    }

    [System.Serializable]
    public class ActionEventData : ActionSubData
    {
        public string eventName;
        public float[] customValues;
        [Tooltip("Trigger EventToBase if isSceneEvent is true, otherwise trigger actor.TriggerEventXX.")]
        public bool isSceneEvent = false;
        [Tooltip("Only works if isSceneEvent is true.")]
        public bool triggerOnEnter = true;
    }

#if false
    public class ActionMuscleStateData : ActionSubData
    {
        public RagdollProfile ragdollProfile;
        public RagdollProfile restoreProfile;
    }
#endif
}


