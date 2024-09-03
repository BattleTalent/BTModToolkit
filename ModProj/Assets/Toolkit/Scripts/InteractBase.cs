using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using EasyButtons;
#endif



namespace CrossLink
{
    public class InteractBase : PhysicsUnit
    {
        #region Network
        public Network.NetworkStateLibrary stateLibrary;
        #endregion

        #region Basic Info
        public bool autoDisappearWhenDurableLow = true;
        public int durability = 25;
        public enum InteractType
        {
            Default,
            Fragile,
            Tough,
            Solid,
        }
        [Tooltip("Fragile is for range weapon, Solid is for shield, Tough is for environment trap")]
        public InteractType interactType;
        [Tooltip("guard points tell enemy where to defend")]
        public Transform[] guardPoints;
        #endregion

        #region Grab
        [Header("Grab")]
        [Tooltip("linear grab spring force")]
        public float linearForce = 30000;
        [Tooltip("linear grab damper")]
        public float linearDamper = 500;
        [Tooltip("angular grab spring force")]
        public float angularForce = 20000;
        [Tooltip("angular grab damper")]
        public float angularDamper = 500;
        [Tooltip("lerp speed when single hand")]
        public float singleHandSpeed = 0.2f;
        [Tooltip("lerp speed when two hand")]
        public float twoHandSpeed = 0.3f;

        [Tooltip("if allow grabbed by two hands")]
        public bool allowSecondHand = false;
        [Tooltip("when toogle to false, this obj can not be grabbed")]
        public bool beAbleToShowGrab = true;
        [Tooltip("sytem will calc a better hand tracking for spear, stick")]
        public bool enableAutoTickTwoHand = false;
        #endregion

        #region Hint
        [Header("Grab Hint")]
        public AttachObj[] attachList;

        [Tooltip("infomation")]
        public GazeObj gaze;

        [Tooltip("when toogle to true, this obj can not be grabbed from distance")]
        public bool grabDistanceLimit = false;
        #endregion

        #region Slot
        [Tooltip("define what type you are, it's used for match with slot")]
        public string[] itemTypes; // check a slot fit or not

        [Tooltip("attach point for your backpack")]
        public AttachObj[] mountAttachs;
        #endregion

        #region Paint
        [Tooltip("could this obj be painted with blood")]
        public bool canPaint = true;
        #endregion

        #region Enhance
        [Tooltip("level of enhance state")]
        public int enhanceLevel = -1;
        #endregion

        #region     Enchantment
        [Tooltip("Renderer for setting effects when enchanting. if empty then all Renderers on this prefab are selected to set the enchantment effect.")]
        public List<Renderer> enchantRenderers;
        public bool allowEnchantment = true;
        #endregion
    }
}

