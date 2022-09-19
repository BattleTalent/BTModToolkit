//#define HOOK_TRIGGER_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{

    public class InteractHook : InteractTrigger
    {
        public InteractHookRoot hookRoot;
        public LineRenderer hookline;

        public ConfigurableJoint hookLimitJoint;
        public float hookLimitOnCall;
        public float hookLimitOnGrab;
    }
}