using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace CrossLink.Network
{
    public class NetworkEnv : NetworkObjBase
    {
        public GameObject envRoot;
        public EnvObj envObj;
        [SyncVar]
        public string envName = "";
        [SyncVar]
        public bool isGlobalMesh = false;

        public List<StabableObj> stabableObjs = new List<StabableObj>();
    }
}