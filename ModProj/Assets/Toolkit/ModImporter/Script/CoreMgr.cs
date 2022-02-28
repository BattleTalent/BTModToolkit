using UnityEngine;
using System.Collections;

namespace CrossLink
{

    public abstract class CoreMgr
    {
        virtual public void OnInit() { }
        virtual public void OnRelease() { }
        virtual public void OnStateClear() { }
    }


}