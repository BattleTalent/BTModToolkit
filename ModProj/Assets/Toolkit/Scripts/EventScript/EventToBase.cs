using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrossLink
{

    // use for execute actions
    public class EventToBase : MonoBehaviour
    {
        public string eventName;

        //public string listenEvent;
        public bool actOnAwake = false;
        public float delayExecute = 0;

        public EventToBase[] events;
        public UnityEvent actions;
        public UnityEvent lateActions;

        public int executeLimitCount = -1;
    }
}