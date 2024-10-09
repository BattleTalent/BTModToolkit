using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Reflection;
using UnityEngine.Events;

namespace CrossLink.Network
{
    [System.Serializable]
    public class NetworkStateLibrary
    {
        public List<Object> objList = new List<Object>();
        public List<Object> bindObjList = new List<Object>();
        public bool needSyncVelocity = false;
    }

    [System.Serializable]
    public class NetworkState<T>
    {
        public int index;
    }

    public class NetworkStateBase : NetworkSyncBase
    {
        [System.Flags]
        public enum DataType
        {
            NONE = 0, // 0
            BOOL = 1 << 0, // 1
            FLOAT = 1 << 1, // 2
            INT = 1 << 2, // 4
            VEC2 = 1 << 3, // 8
            VEC3 = 1 << 4, // 16
            QUAT = 1 << 5, // 32
            STR = 1 << 6, //64
        }

        public bool IsInit => isInit;
        private bool isInit = false;
        public System.Action serverInitAction;
        public System.Action clientInitAction;

        public NetworkStateLibrary stateLibrary;

        public List<UnityEvent<List<bool>, List<float>, List<int>, List<string>>> rpcSends;
        public List<UnityEvent<List<bool>, List<float>, List<int>, List<string>>> cmdSends;
        public List<UnityEvent<List<bool>, List<float>, List<int>, List<string>>> rpcCallbacks;
        public List<UnityEvent<List<bool>, List<float>, List<int>, List<string>>> cmdCallbacks;

        public DataType dataMask = DataType.NONE;

        public List<bool> localBoolStates = new List<bool>();
        public List<float> localFloatStates = new List<float>();
        public List<int> localIntStates = new List<int>();
        public List<Vector2> localVec2States = new List<Vector2>();
        public List<Vector3> localVec3States = new List<Vector3>();
        public List<Quaternion> localQuatStates = new List<Quaternion>();
        public List<string> localStrStates = new List<string>();

        public List<NetworkState<bool>> netBoolStates = new List<NetworkState<bool>>();
        public List<NetworkState<float>> netFloatStates = new List<NetworkState<float>>();
        public List<NetworkState<int>> netIntStates = new List<NetworkState<int>>();
        public List<NetworkState<Vector2>> netVec2States = new List<NetworkState<Vector2>>();
        public List<NetworkState<Vector3>> netVec3States = new List<NetworkState<Vector3>>();
        public List<NetworkState<Quaternion>> netQuatStates = new List<NetworkState<Quaternion>>();
        public List<NetworkState<string>> netStrStates = new List<NetworkState<string>>();

        public bool onlySyncOnChange = true;
        public float onlySyncOnChangeCorrectionMultiplier = 2;
        public bool compressQuat = true;
        public float floatSensitivity = 0.01f;
        public float vec2Sensitivity = 0.01f;
        public float vec3Sensitivity = 0.01f;
        public float quatSensitivity = 0.01f;
        public bool interpolateFloats = true;
        public bool interpolateVec2s = true;
        public bool interpolateVec3s = true;
        public bool interpolateQuats = true;

        public bool timelineOffset = false;

        public virtual void OnCommandFunc(
            int index,
            List<bool> boolList,
            List<float> floatList,
            List<int> intList,
            List<string> strList)
        {
        }
    }
}