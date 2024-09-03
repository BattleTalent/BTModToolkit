using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace CrossLink.Network
{
    public class NetworkFlyObjectStateX : NetworkFlyObjectBase
    {
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


        [Client]
        public void CommandFunc(
            string function,
            List<bool> boolList,
            List<float> floatList,
            List<int> intList,
            List<string> strList)
        {
        }
    }
}
