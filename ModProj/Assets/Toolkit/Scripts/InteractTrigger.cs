using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using EasyButtons;
#endif



namespace CrossLink
{
    public class InteractTrigger : MonoBehaviour
    {
        public InteractBase interact;

        [Tooltip("-1 means no skill, it used in one-shot or activate")]
        public float manaCost = -1; 
        [Tooltip("it used in charging per sec")]
        public float manaCostOnCharge = -1;
        [Tooltip("just for showup in UI")]
        public float skillDamage = 0;

        [Tooltip("instant or continue")]
        public bool instantSkill = false;


        // charge or not charge
        [Tooltip("-1 means forever, otherwise charge to this end")]
        public float skillChargeEndTime = -1; 
        public string chargeEffect;
        public string chargeEndEffect;

        // activate
        [Tooltip("after charged, it'll be activated, this controls how long will it last")]
        public float activateTime = -1;
        public string activateEffect;

        [Tooltip("don't tick this logic, better performance")]
        public bool dontNeedUpdate = true;

        [Tooltip("which renders will turn colors")]
        public Renderer[] skillChangeColors;
        public string weaponColorField = "_EmissionColor";

        protected void Reset()
        {
            interact = GetComponent<InteractBase>();
        }

#if UNITY_EDITOR
        [Button]
        void PullSkillChangeColors()
        {
            skillChangeColors = GetComponentsInChildren<Renderer>();
            //enabled = false;
            EditorUtility.SetDirty(gameObject);
        }
#endif
    }

}