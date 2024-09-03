using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink.Network
{
    public class EnvObj : MonoBehaviour
    {
        public System.Action envObjDestroyEvent;

        void OnDestroy()
        {
            envObjDestroyEvent?.Invoke();
        }
    }
}