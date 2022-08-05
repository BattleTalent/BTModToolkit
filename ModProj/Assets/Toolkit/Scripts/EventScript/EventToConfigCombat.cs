using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class EventToConfigCombat : EventToBase
    {
        public WaveGenerator waveDefine;
        public Transform[] pointList;

        public event System.Action StartCombatEvent;
        public event System.Action StopCombatEvent;
    }
}


