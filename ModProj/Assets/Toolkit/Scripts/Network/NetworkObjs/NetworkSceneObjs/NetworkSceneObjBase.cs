using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace CrossLink.Network
{
    public class NetworkSceneObjBase : NetworkObjBase
    {
        public NetworkState<Vector3> pos;
        public NetworkState<Vector3> dir;
        public NetworkRigidbodySyncBase networkRB;
        public SceneObj so;
    }
}