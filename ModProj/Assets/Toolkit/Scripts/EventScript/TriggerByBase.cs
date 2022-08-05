using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace CrossLink
{
    // use for as trigger, dosen't execate action
    public class TriggerByBase : MonoBehaviour
    {
        public int allowTriggerCount = -1;
        protected int triggerCount = 0;
        public float minTriggerInterval = 0;

        public EventToBase[] eventList;
        public UnityEvent actions;
    }
}