using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using EasyButtons;
#endif


namespace CrossLink
{

    public class InteractTriggerX : InteractTrigger
    {
        public LuaScript script = new LuaScript();


        protected void AwakeInit()
        {
            // script call:Awake
        }

        protected  void StartInit()
        {
            // script call:Start
        }

        protected void Destroy()
        {
            // script call:OnDestroy
        }



        public void OpenSkill(AttachObj attach)
        {
            // script call:OpenSkill
        }

        public void CloseSkill()
        {
            // script call:CloseSkill
        }

        public void UpdateSkill()
        {
            // script call:UpdateSkill
        }

        public void OnActivateBegin()
        {
            // script call:OnActivateBegin
        }
        public void OnActivateEnd()
        {
            // script call:OnActivateEnd
        }

        public void OnActivateCancel()
        {
            // script call:OnActivateCancel
        }


        public void OnChargeBegin()
        {
            // script call:OnChargeBegin
        }

        public void OnChargeCancel()
        {
            // script call:OnChargeCancel
        }

        public void OnChargeReady()
        {
            // script call:OnChargeReady
        }

        public void OnChargeRelease()
        {
            // script call:OnChargeRelease
        }

        public void OnChargeUpdate(float rate)
        {
            // script call:OnChargeUpdate
        }

        public void OnCoolDownBegin()
        {
            // script call:OnCoolDownBegin
        }
        public void OnCoolDownEnd()
        {
            // script call:OnCoolDownEnd
        }

        public void TriggerOnCoolDown()
        {
            // script call:TriggerOnCoolDown
        }

        public void OnChargeCoolDownBegin()
        {
            // script call:OnChargeCoolDownBegin
        }
        public void OnChargeCoolDownEnd()
        {
            // script call:OnChargeCoolDownEnd
        }

        public void TriggerOnChargeCoolDown()
        {
            // script call:TriggerOnChargeCoolDown
        }

        protected void OnGrab(AttachObj attach, bool t)
        {
            // script call:OnGrab
        }

        //protected void OnSlot(SlotTrigger slot, bool t)
        //{
        //    // script call:OnSlot
        //}

        protected void OnOverlap(bool t)
        {
            // script call:OnEquipmentOverlap
        }

        protected void OnEnhance(InteractBase ib)
        {
            // script call:OnEnhance
        }

        #region Shop

        public void OnSale()
        {
            // script call:OnSale
        }

        public void OnBuy()
        {
            // script call:OnBuy
        }

        #endregion


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
