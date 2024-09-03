using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace CrossLink.Network
{
    public class NetworkFlyObjectBase : NetworkObjBase
    {
        public NetworkState<string> flyObjectName;
        public NetworkState<int> ownerNetId;
        public NetworkState<Vector3> pos;
        public NetworkState<Vector3> dir;
        public NetworkState<float> vel;
        public NetworkState<string> spPath;

        #region Spawn Parent
        public NetworkState<int> parentRbNetId;
        public NetworkState<int> parentRbKey;
        public Transform parentTrans;
        #endregion

        public NetworkRigidbodySyncBase networkRB;
        public FlyObject flyObject;
    }
}