using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace CrossLink
{
    public class RagdollHitInfoRef : MonoBehaviour
    {
        public RagdollHitInfoObj[] refs;

        private void Reset()
        {
            refs = GetComponentsInChildren<RagdollHitInfoObj>();
        }
    }

}