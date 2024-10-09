using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;
using System.Globalization;

namespace CrossLink.Network
{

    public class NetworkSceneObjStateX : NetworkSceneObjBase
    {
        public LuaBehaviour lua;
        [System.NonSerialized] 
        public LuaScript script;

        private Dictionary<string, NetworkState<bool>> boolStatesDic;
        private Dictionary<string, System.Action<bool>> boolStatesAction;
        private Dictionary<string, NetworkState<float>> floatStatesDic;
        private Dictionary<string, System.Action<float>> floatStatesAction;
        private Dictionary<string, NetworkState<int>> intStatesDic;
        private Dictionary<string, System.Action<int>> intStatesAction;
        private Dictionary<string, NetworkState<Vector2>> vec2StatesDic;
        private Dictionary<string, System.Action<Vector2>> vec2StatesAction;
        private Dictionary<string, NetworkState<Vector3>> vec3StatesDic;
        private Dictionary<string, System.Action<Vector3>> vec3StatesAction;
        private Dictionary<string, NetworkState<Quaternion>> quatStatesDic;
        private Dictionary<string, System.Action<Quaternion>> quatStatesAction;
        private Dictionary<string, NetworkState<string>> strStatesDic;
        private Dictionary<string, System.Action<string>> strStatesAction;

        #region state
        public void InjectBoolState(string name, System.Action<bool> changeCallback = null)
        {
        }

        public void SetBoolState(string name, bool value)
        {
        }

        //public bool GetBoolStateValue(string name)
        //{
        //    return GetBoolStateValue(boolStatesDic[name]);
        //}

        public void InjectFloatState(string name, System.Action<float> changeCallback = null)
        {
        }

        public void SetFloatStateValue(string name, float value)
        {
        }

        //public float GetFloatStateValue(string name)
        //{
        //    return GetFloatStateValue(floatStatesDic[name]);
        //}

        public void InjectIntState(string name, System.Action<int> changeCallback = null)
        {
        }

        public void SetIntStateValue(string name, int value)
        {
        }

        //public float GetIntStateValue(string name)
        //{
        //    return GetIntStateValue(intStatesDic[name]);
        //}

        public void InjectVec2State(string name, System.Action<Vector2> changeCallback = null)
        {
        }

        public void SetVec2StateValue(string name, Vector2 value)
        {
        }

        //public Vector2 GetVec2StateValue(string name)
        //{
        //    return GetVec2StateValue(vec2StatesDic[name]);
        //}

        public void InjectVec3State(string name, System.Action<Vector3> changeCallback = null)
        {
        }

        public void SetVec3StateValue(string name, Vector3 value)
        {
        }

        //public Vector3 GetVec3StateValue(string name)
        //{
        //    return GetVec3StateValue(vec3StatesDic[name]);
        //}

        public void InjectQuatState(string name, System.Action<Quaternion> changeCallback = null)
        {
        }

        public void SetQuatStateValue(string name, Quaternion value)
        {
        }

        //public Quaternion GetQuatStateValue(string name)
        //{
        //    return GetQuatStateValue(quatStatesDic[name]);
        //}

        public void InjectStrState(string name, System.Action<string> changeCallback = null)
        {
        }

        public void SetStrStateValue(string name, string value)
        {
        }
        //public string GetStrStateValue(string name)
        //{
        //    return GetStrStateValue(strStatesDic[name]);
        //}
        #endregion

        #region rpc
        [Server]
        public void ServerRpcFunc(
            string functionName,
            bool includeServer
        )
        {
            RpcFunc(functionName, includeServer);
        }

        [ClientRpc]
        public void RpcFunc(
            string functionName,
            bool includeServer
        )
        {
        }

        [Server]
        public void ServerRpcBoolFunc(
            string functionName,
            bool value,
            bool includeServer
        )
        {
            RpcBoolFunc(functionName, value, includeServer);
        }

        [ClientRpc]
        public void RpcBoolFunc(
            string functionName,
            bool value,
            bool includeServer
        )
        {
        }

        [Server]
        public void ServerRpcBoolListFunc(
            string functionName,
            List<bool> boolList,
            bool includeServer
        )
        {
            RpcBoolListFunc(functionName, boolList, includeServer);
        }

        [ClientRpc]
        public void RpcBoolListFunc(
            string functionName,
            List<bool> boolList,
            bool includeServer
        )
        {
        }

        [Server]
        public void ServerRpcFloatFunc(
            string functionName,
            float value,
            bool includeServer
        )
        {
            RpcFloatFunc(functionName, value, includeServer);
        }

        [ClientRpc]
        public void RpcFloatFunc(
            string functionName,
            float value,
            bool includeServer
        )
        {
        }

        [Server]
        public void ServerRpcFloatListFunc(
            string functionName,
            List<float> floatList,
            bool includeServer
        )
        {
            RpcFloatListFunc(functionName, floatList, includeServer);
        }

        [ClientRpc]
        public void RpcFloatListFunc(
            string functionName,
            List<float> floatList,
            bool includeServer
        )
        {
        }

        [Server]
        public void ServerRpcIntFunc(
            string functionName,
            int value,
            bool includeServer
        )
        {
            RpcIntFunc(functionName, value, includeServer);
        }

        [ClientRpc]
        public void RpcIntFunc(
            string functionName,
            int value,
            bool includeServer
        )
        {
        }

        [Server]
        public void ServerRpcIntListFunc(
            string functionName,
            List<int> intList,
            bool includeServer
        )
        {
            RpcIntListFunc(functionName, intList, includeServer);
        }

        [ClientRpc]
        public void RpcIntListFunc(
            string functionName,
            List<int> intList,
            bool includeServer
        )
        {
        }

        [Server]
        public void ServerRpcStringFunc(
            string functionName,
            string value,
            bool includeServer
        )
        {
            RpcStringFunc(functionName, value, includeServer);
        }

        [ClientRpc]
        public void RpcStringFunc(
            string functionName,
            string value,
            bool includeServer
        )
        {
        }

        [Server]
        public void ServerRpcStringListFunc(
            string functionName,
            List<string> strList,
            bool includeServer
        )
        {
            RpcStringListFunc(functionName, strList, includeServer);
        }

        [ClientRpc]
        public void RpcStringListFunc(
            string functionName,
            List<string> strList,
            bool includeServer
        )
        {
        }

        [Server]
        public void ServerRpcVec3Func(
            string functionName,
            Vector3 value,
            bool includeServer
        )
        {
            RpcVec3Func(functionName, value, includeServer);
        }

        [ClientRpc]
        public void RpcVec3Func(
            string functionName,
            Vector3 value,
            bool includeServer
        )
        {
        }

        [Server]
        public void ServerRpcVec3ListFunc(
            string functionName,
            List<Vector3> vecList,
            bool includeServer
        )
        {
            RpcVec3ListFunc(functionName, vecList, includeServer);
        }

        [ClientRpc]
        public void RpcVec3ListFunc(
            string functionName,
            List<Vector3> strList,
            bool includeServer
        )
        {
        }

        [Server]
        public void ServerRpcComplFunc(
            string functionName,
            bool boolValue,
            float floatValue,
            int intValue,
            string strValue,
            Vector3 vecValue,
            bool includeServer
        )
        {
            RpcComplFunc(functionName, boolValue, floatValue, intValue, strValue, vecValue, includeServer);
        }

        [ClientRpc]
        public void RpcComplFunc(
            string functionName,
            bool boolValue,
            float floatValue,
            int intValue,
            string strValue,
            Vector3 vecValue,
            bool includeServer
        )
        {
        }

        [Server]
        public void ServerRpcComplListFunc(
            string functionName,
            List<bool> boolList,
            List<float> floatList,
            List<int> intList,
            List<string> strList,
            List<Vector3> vecList,
            bool includeServer
        )
        {
            RpcComplListFunc(functionName, boolList, floatList, intList, strList, vecList, includeServer);
        }

        [ClientRpc]
        public void RpcComplListFunc(
            string functionName,
            List<bool> boolList,
            List<float> floatList,
            List<int> intList,
            List<string> strList,
            List<Vector3> vecList,
            bool includeServer
        )
        {
        }
        #endregion
        #region command
        [Command(requiresAuthority = false)]
        public void CommandFunc(string functionName)
        {
        }

        [Command(requiresAuthority = false)]
        public void CommandBoolFunc(
            string functionName,
            bool value
        )
        {
        }

        [Command(requiresAuthority = false)]
        public void CommandBoolListFunc(
            string functionName,
            List<bool> boolList
        )
        {
        }

        [Command(requiresAuthority = false)]
        public void CommandFloatFunc(
            string functionName,
            float value
        )
        {
        }

        [Command(requiresAuthority = false)]
        public void CommandFloatListFunc(
            string functionName,
            List<float> floatList
        )
        {
        }


        [Command(requiresAuthority = false)]
        public void CommandIntFunc(
            string functionName,
            int value
        )
        {
        }

        [Command(requiresAuthority = false)]
        public void CommandIntListFunc(
            string functionName,
            List<int> intList
        )
        {
        }


        [Command(requiresAuthority = false)]
        public void CommandStringFunc(
            string functionName,
            string value
        )
        {
        }


        [Command(requiresAuthority = false)]
        public void CommandStringListFunc(
            string functionName,
            List<string> strList
        )
        {
        }


        [Command(requiresAuthority = false)]
        public void CommandVec3Func(
            string functionName,
            Vector3 value
        )
        {
        }

        [Command(requiresAuthority = false)]
        public void CommandVec3ListFunc(
            string functionName,
            List<Vector3> vecList
        )
        {
        }


        [Command(requiresAuthority = false)]
        public void CommandComplFunc(
            string functionName,
            bool boolValue,
            float floatValue,
            int intValue,
            string strValue,
            Vector3 vecValue
        )
        {
        }


        [Command(requiresAuthority = false)]
        public void CommandComplListFunc(
            string functionName,
            List<bool> boolList,
            List<float> floatList,
            List<int> intList,
            List<string> strList,
            List<Vector3> vecList
        )
        {
        }

        [Client]
        public void CommandFunc(
            string function,
            List<bool> boolList,
            List<float> floatList,
            List<int> intList,
            List<string> strList)
        {
        }
        #endregion

    }
}