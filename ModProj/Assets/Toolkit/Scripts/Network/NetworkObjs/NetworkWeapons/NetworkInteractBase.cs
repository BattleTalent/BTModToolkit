using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace CrossLink.Network
{
    public class NetworkInteractBase : NetworkBehaviour
    {
        public string ibName;
        public NetworkTransformFix networkTransform;
        public NetworkRigidbodySyncBase networkRB;
        public NetworkIBState networkIBState;
        public NetworkIBProperties networkIBProperties;
        public NetworkInteractTrigger networkIT;
        public NetworkStabObject networkStabObject;
        public NetworkBlessingWeapon networkBlessing;
        public InteractBase ib;
        public List<AttachObj> attachList = new List<AttachObj>();
        public List<StabObject.StabGeometry> stabGeoList = new List<StabObject.StabGeometry>();

        public bool isSceneProp = false;

        public bool simpleWeapon = false;
        public bool disabledClientJoint = false;
    }
}