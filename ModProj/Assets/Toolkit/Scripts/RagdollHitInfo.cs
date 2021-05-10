using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{
    [System.Serializable]
    public class RagdollHitInfo : ValueProvider
    {
        #region HitInfo
        [Tooltip("define the damage settings, it's from hitinfo table")]
        public string templateName;
        [Tooltip("the larger the more important, so when weapon upgrade, it'll have more chance to be picked up")]
        public float weightAmongMultiple = 1;
        #endregion
    }

}
