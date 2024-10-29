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

        public Injection[] GetObjList()
        {
            return objList;
        }

        public void SetObjList(Injection[] list)
        {
            objList = list;
        }
    }

    public class LuaBehaviour : MonoBehaviour
    {
        public LuaScript script = new LuaScript();

        public bool dontNeedUpdate = true;

        void Awake()
        {
            // script call:Awake
        }

        void Start()
        {
            // script call:Start
        }

        void OnDestroy()
        {
            // script call:OnDestroy
        }

        void OnEnable()
        {
            // script call:OnEnable
        }

        void Update()
        {
            if (dontNeedUpdate)
            {
                return;
            }

            // script call:Update
        }

        void OnDisable()
        {
            // script call:OnDisable
        }

        private void OnCollisionEnter(Collision collision)
        {
            // script call:OnColliderEnter

            // script call:OnCollisionEnter
        }

        private void OnCollisionStay(Collision collision)
        {
            // script call:OnColliderStay

            // script call:OnCollisionStay
        }
        private void OnCollisionExit(Collision collision)
        {
            // script call:OnColliderExit

            // script call:OnCollisionExit
        }

        private void OnTriggerEnter(Collider other)
        {
            // script call:OnTriggerEnter
        }

        private void OnTriggerExit(Collider other)
        {
            // script call:OnTriggerExit
        }

        private void OnTriggerStay(Collider other)
        {
            // script call:OnTriggerStay
        }
    }

}
