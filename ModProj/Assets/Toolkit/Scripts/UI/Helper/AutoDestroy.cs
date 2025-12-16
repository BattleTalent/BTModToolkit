using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class AutoDestroy : MonoBehaviour
    {
        public float destroyDelay = 3f;

#if UNITY_EDITOR
        bool isActive = false;
        float currentTime = 0f;
        public void StartTimer()
        {
            isActive = true;
        }

        private void Update()
        {
            if (!isActive) return;

            currentTime += Time.deltaTime;

            if (currentTime >= destroyDelay)
            {
                Destroy(gameObject);
            }
        }
#endif
    }

}