using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class Actor : MonoBehaviour
    {
        public Transform trans => transform;
        public ActionEditor editor;

        Transform target;
        ActionData playAction;

        [System.NonSerialized]
        public float playTime;
        float lastPlayHead;
        float pauseAccTime = 0;

        AnimLayoutDataItem rootMotionLayout;
        ActionAnimData rootMotionData;
        float rootMotionDuration;
        float rootMotionElapsed;
        Vector3 rootMotionPreviousPosition;
        Quaternion rootMotionPreviousRotation;
        bool rootMotionAnimatorStateCaptured;
        bool rootMotionPreviousAnimatorApply;


        public float GetTimeline() { return Time.time - (playTime + pauseAccTime); }
        float playTimeMlp = 1;
        public float GetPlayTimeMlp() { return playTimeMlp; }
        public void SetPlayTimeMlp(float sp)
        {
            playTimeMlp = sp;
        }

        public ActionData GetActionData() { return playAction; }

        public Transform GetParamterTarget() { return target; }

        public void PlayAction(ActionData ad)
        {
            StopAction();

            if (ad == null)
                return;

            if (ad.ignoreDynamicSpeed)
            {
                SetPlayTimeMlp(1);
            }

            playTime = Time.time;
            playAction = ad;
            lastPlayHead = -1;
            isFinished = false;

            pauseAccTime = 0;
        }

        private void FixedUpdate()
        {
            UpdateRootMotion();
            UpdateAction();
        }

        private void OnDisable()
        {
            StopAction();
        }

        public void StopAction()
        {
            Finish();
            StopRootMotion();
        }

        public float ApplyRootMotion(ActionAnimData data)
        {
            StopRootMotion();

            if (data == null || string.IsNullOrEmpty(data.rootMotion) || editor == null)
                return 0;

            var layout = editor.GetRootMotionLayout(data.rootMotion);
            if (layout == null)
                return 0;

            if (data.timeScale <= 0)
            {
                Debug.LogWarning($"ActionEditor cannot preview root motion '{data.rootMotion}': animation timeScale must be greater than 0.", this);
                return 0;
            }

            var layoutDuration = data.customLayoutLen > 0 ? data.customLayoutLen : layout.Len;
            rootMotionDuration = layoutDuration / data.timeScale * playTimeMlp;
            if (rootMotionDuration <= 0 || layout.Len <= 0)
            {
                Debug.LogWarning($"ActionEditor cannot preview root motion '{data.rootMotion}': its duration must be greater than 0.", this);
                rootMotionDuration = 0;
                return 0;
            }

            rootMotionLayout = layout;
            rootMotionData = data;
            rootMotionElapsed = 0;
            rootMotionPreviousPosition = layout.EvaluatePos(0);
            rootMotionPreviousRotation = layout.EvaluateRot(0);

            if (editor.animator != null)
            {
                rootMotionPreviousAnimatorApply = editor.animator.applyRootMotion;
                rootMotionAnimatorStateCaptured = true;
                editor.animator.applyRootMotion = false;
            }
            return rootMotionDuration;
        }

        public void StopRootMotion(ActionAnimData owner = null)
        {
            if (owner != null && rootMotionData != owner)
                return;

            if (rootMotionAnimatorStateCaptured && editor != null && editor.animator != null)
            {
                editor.animator.applyRootMotion = rootMotionPreviousAnimatorApply;
            }

            rootMotionLayout = null;
            rootMotionData = null;
            rootMotionDuration = 0;
            rootMotionElapsed = 0;
            rootMotionAnimatorStateCaptured = false;
        }

        void UpdateRootMotion()
        {
            if (rootMotionLayout == null || rootMotionData == null)
                return;

            rootMotionElapsed += Time.fixedUnscaledDeltaTime;
            var sampleTime = Mathf.Clamp(rootMotionElapsed + rootMotionData.motionTimeOffset, 0, rootMotionDuration);
            var timeProgress = sampleTime / rootMotionDuration * rootMotionLayout.Len;
            var nextPosition = rootMotionLayout.EvaluatePos(timeProgress);
            var nextRotation = rootMotionLayout.EvaluateRot(timeProgress);

            bool ignorePosition = timeProgress < rootMotionData.ignorePosBeginTime
                || (rootMotionData.ignorePosTime > 0 && timeProgress > rootMotionData.ignorePosTime);
            bool ignoreRotation = rootMotionData.ignoreRotTime > 0 && timeProgress > rootMotionData.ignoreRotTime;

            if (!ignorePosition)
            {
                var positionDelta = nextPosition - rootMotionPreviousPosition;
                positionDelta.y = 0;
                positionDelta *= rootMotionData.motionScale;
                transform.position += transform.rotation * positionDelta;
            }

            if (!ignoreRotation)
            {
                var rotationDelta = Quaternion.Inverse(rootMotionPreviousRotation) * nextRotation;
                var rotationY = rotationDelta.eulerAngles.y;
                if (rotationY > 180)
                    rotationY -= 360;

                if (rootMotionData.rotateSpeed > 0)
                {
                    rotationY = Mathf.Clamp(rotationY, -rootMotionData.rotateSpeed, rootMotionData.rotateSpeed);
                }

                rotationDelta = Quaternion.Euler(0, rotationY, 0);
                if (rootMotionData.motionScale < 1)
                {
                    rotationDelta = Quaternion.Slerp(Quaternion.identity, rotationDelta, rootMotionData.motionScale);
                }

                transform.rotation = Quaternion.Euler(0,
                    transform.rotation.eulerAngles.y + rotationDelta.eulerAngles.y, 0);
            }

            rootMotionPreviousPosition = nextPosition;
            rootMotionPreviousRotation = nextRotation;

            if (rootMotionElapsed >= rootMotionDuration)
            {
                if (rootMotionData.loopMotion)
                {
                    rootMotionElapsed = 0;
                    rootMotionPreviousPosition = rootMotionLayout.EvaluatePos(0);
                    rootMotionPreviousRotation = rootMotionLayout.EvaluateRot(0);
                }
                else
                {
                    StopRootMotion();
                }
            }
        }

        public void UpdateAction()
        {
            if (playAction == null)
                return;

            float playhead = GetTimeline(); //Time.time - playTime;
            int overTimelineNum = 0;

            for (int t = 0; t < playAction.timelines.Length; ++t)
            {
                var timeline = playAction.timelines[t];

                var startTime = timeline.startTime;
                startTime *= playTimeMlp;

                var prepareTime = timeline.prepareTime;

                // action prepare
                if (lastPlayHead < startTime - prepareTime && playhead >= startTime - prepareTime)
                {
                    for (int i = 0; i < timeline.actionDatas.Length; ++i)
                    {
                        timeline.actionDatas[i].OnPrepare(this);
                    }
                }

                if (playhead < startTime)
                    continue;



                // just enter
                if (lastPlayHead < startTime && playhead >= startTime)
                {
                    for (int i = 0; i < timeline.actionDatas.Length; ++i)
                    {
                        timeline.actionDatas[i].OnEnter(this);
                    }
                    if (playAction == null)
                    {
                        return;
                    }
                }




                float endTime = timeline.endTime > 0 ? timeline.endTime : playAction.timeLen;
                endTime *= playTimeMlp;

                // just exit
                if (lastPlayHead < endTime && playhead >= endTime)
                {
                    for (int i = 0; i < timeline.actionDatas.Length; ++i)
                    {
                        timeline.actionDatas[i].OnExit(this);
                    }

                    // this action is stopped in Exit
                    if (playAction == null)
                    {
                        return;
                    }
                }





                if (playhead > endTime)
                {
                    ++overTimelineNum;
                    continue;
                }


                // update
                {
                    for (int i = 0; i < timeline.actionDatas.Length; ++i)
                    {
                        timeline.actionDatas[i].OnUpdate(this);
                    }
                }



            }

            lastPlayHead = playhead;

            // is over
            if (overTimelineNum >= playAction.timelines.Length)
            {
                Finish();
            }
        }

        bool isFinished = true;
        float lastAtkFinishTime;
        public float GetLastAtkFinishTime() { return lastAtkFinishTime; }
        float lastFinishTime;
        public float GetLastFinishTime() { return lastAtkFinishTime; }
        void Finish(bool resetAnim = true)
        {
            if (isFinished)
                return;
            isFinished = true;
            StopRootMotion();

            lastFinishTime = Time.time;
            if (playAction.haveAtkIntention)
            {
                lastAtkFinishTime = lastFinishTime;
            }

            float playhead = GetTimeline(); //Time.time - playTime;
            for (int t = 0; t < playAction.timelines.Length; ++t)
            {
                var endTime = playAction.timelines[t].endTime;
                endTime = endTime == 0 ? playAction.timeLen : endTime;
                endTime *= playTimeMlp;

                var ads = playAction.timelines[t].actionDatas;
                for (int i = 0; i < ads.Length; ++i)
                {
                    var remain = endTime - playhead;
                    // cancel those not finished action
                    if (playhead <= endTime
                        // some time, we may just pass the time
                        //&& playhead >= playAction.timelines[t].startTime
                        )
                    {
                        ads[i].OnCancel(this, remain);
                    }
                    ads[i].OnClear(this, remain);
                }
            }
            playAction = null;
        }

        public Transform GetBone(string boneName)
        {
            var bone = editor.GetBone(boneName);

            if (bone == null)
            {
                bone = editor.GetWeaponTrans(boneName);
            }

            return bone;
        }
    }
}
