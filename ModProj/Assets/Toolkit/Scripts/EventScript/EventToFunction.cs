using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    public class EventToFunction : EventToBase
    {
        [Tooltip("Calling luaOnReceiveEvent in this lua script when event is triggered")]
        public LuaScript script = new LuaScript();

        //public LuaFunction luaOnReceiveEvent;
    }
}