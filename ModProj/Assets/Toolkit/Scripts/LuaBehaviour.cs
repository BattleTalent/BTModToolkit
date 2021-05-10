using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{


    [System.Serializable]
    public class Injection
    {
        public string name;
        public UnityEngine.Object value;
    }


    [System.Serializable]
    public class InjectionNumber
    {
        public string name;
        public float value;
    }


    [System.Serializable]
    public class LuaScript
    {

        [SerializeField]
        string luaScript;
        [SerializeField]
        Injection[] objList;
        [SerializeField]
        InjectionNumber[] numberList;
    }

    public class LuaBehaviour : MonoBehaviour
    {
        public LuaScript script = new LuaScript();
    }

}
