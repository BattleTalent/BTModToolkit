using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink.Network
{
    public class NetworkObjBase : NetworkStateBase
    {
        public NetworkState<bool> needIns;
        public NetworkState<int> bindNetId;
        public NetworkState<int> bindIndex;
    }
}