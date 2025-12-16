using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class Actor : MonoBehaviour
    {
        public Transform trans => transform;
        [System.NonSerialized]
        public ActionEditor editor;

        Transform target;
        ActionData playAction;

        [System.NonSerialized]
        public float playTime;
        float lastPlayHead;
        float pauseAccTime = 0;


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
            Finish();

            if (ad.ignoreDynamicSpeed)
            {
                SetPlayTimeMlp(1);
            }

            playTime = Time.time;
            playAction = ad;
            lastPlayHead = -1;

            pauseAccTime = 0;
        }

        private void FixedUpdate()
        {
            UpdateAction();
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
