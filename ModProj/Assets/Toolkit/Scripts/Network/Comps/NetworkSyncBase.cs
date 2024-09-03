using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace CrossLink.Network
{
    public class NetworkSyncBase : NetworkBehaviour
    {
        [Header("Send Interval Multiplier")]
        [Tooltip("Check/Sync every multiple of Network Manager send interval (= 1 / NM Send Rate), instead of every send interval.\n(30 NM send rate, and 3 interval, is a send every 0.1 seconds)\nA larger interval means less network sends, which has a variety of upsides. The drawbacks are delays and lower accuracy, you should find a nice balance between not sending too much, but the results looking good for your particular scenario.")]
        [Range(1, 120)]
        public uint sendIntervalMultiplier = 1;
    }
}