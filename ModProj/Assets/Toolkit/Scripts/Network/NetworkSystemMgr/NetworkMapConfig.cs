using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CrossLink.Network
{
    [System.Serializable]
    public class NetworkMapConfigData
    {
        public int maxPlayers;
        //public NetworkMapType mapType;
        public float rebornTime;
        public float playerHp;
        public float playerMp;
        public float playerMpRecoverRate;
    }

    public enum NetworkMapType
    {
        Sandbox,
        Arena,
        Dungeon,
        NewMode,
    }
}