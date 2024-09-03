using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace CrossLink.Network
{
    public class NetworkIBProperties : NetworkBehaviour
    {
        public InteractBase ib;
        public int durability;

        public List<NetworkPlayer> grabbers;
    }
}