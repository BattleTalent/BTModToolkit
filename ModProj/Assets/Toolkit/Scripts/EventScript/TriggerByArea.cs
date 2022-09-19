//#define TRIGGER_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CrossLink
{

    public class TriggerByArea : TriggerByBase
    {

        public float stayTime = 0.2f;

        public LayerMask checkMask;
        public bool playerOnlyAndReady = false;

        public bool triggerWhenLeave = false;

        public UnityEvent enterActions;
    }
}