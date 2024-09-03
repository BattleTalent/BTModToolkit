using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace CrossLink.Network
{
    public class NetworkIBState : NetworkObjBase
    {
        public NetworkInteractBase networkIB;

        public NetworkState<bool> isPendant;
        public NetworkState<bool> isPendantLeft;
        public NetworkState<bool> isThrowItem;
        public NetworkState<int> handIBIdx;
        public NetworkState<int> durability;
        public NetworkState<int> enhanceLevel;
        public NetworkState<bool> debugEnhance;

        public NetworkState<Vector3> initPos;
        public NetworkState<Quaternion> initRot;
    }
}