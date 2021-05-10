using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using EasyButtons;
#endif




namespace CrossLink
{
    public class GazeMgr
    {
        public enum GazeType
        {
            NormalGaze,
            GoalGaze,
            DangerGaze,
            FriendGaze,
        }
    }


    public class GazeObj : MonoBehaviour
    {
        public string showName;
        public string showInfo;
        public GazeMgr.GazeType gazeType = GazeMgr.GazeType.NormalGaze;

#if UNITY_EDITOR
        [Button]
        void PullGazeNameFromGameObject()
        {
            showName = gameObject.name;
            showInfo = showName + "_Desc";
            var ib = GetComponent<InteractBase>();
            ib.gaze = this;
            //enabled = false;
            EditorUtility.SetDirty(gameObject);
        }
#endif
    }

}
