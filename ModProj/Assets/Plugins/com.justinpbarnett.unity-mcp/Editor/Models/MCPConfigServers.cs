using System;
using Newtonsoft.Json;

namespace UnityMcpBridge.Editor.Models
{
    [Serializable]
    public class McpConfigServers
    {
        [JsonProperty("unityMCP")]
        public McpConfigServer unityMCP;
    }
}
