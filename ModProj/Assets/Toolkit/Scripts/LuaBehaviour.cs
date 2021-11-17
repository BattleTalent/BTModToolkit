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
    public class InjectionString
    {
        public string name;
        public string value;
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
        [SerializeField]
        InjectionString[] stringList;

        public string GetLuaScript()
        {
            return luaScript;
        }

        public void SetLuaScript(string script)
        {
            luaScript = script;
        }

        public InjectionString[] GetStringList()
        {
            return stringList;
        }
    }

    public class LuaBehaviour : MonoBehaviour
    {
        public LuaScript script = new LuaScript();
    }

}
