using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class HandDraw : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            DebugDraw.DrawBone(transform, true);
        }
    }
}

