using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    [CreateAssetMenu(fileName = "WaveGroup", menuName = "WaveGroup")]
    public class WaveGroup:ScriptableObject
    {
        public string[] roles;

        //public RoleGroupTable.RolePackDesc desc;
    }



    public class RoleGroupTable
    {
        public WaveGroup[] waveGroups;

        public Dictionary<string, string[]> roleGroupMap = new Dictionary<string, string[]>();
        public class RolePackDesc
        {
            public WaveGroup[] groups;
        }

        public string[] roleList;
    }
}
