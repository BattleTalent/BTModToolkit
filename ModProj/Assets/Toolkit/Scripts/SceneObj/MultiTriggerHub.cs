using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{
    public class MultiTriggerHub : MonoBehaviour
    {
        public event System.Action<Collider> TriggerEnterEvent;
        public event System.Action TriggerLeaveEvent;

        public MultiTriggerChecker[] checkers;
    }

}
