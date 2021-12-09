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
        #region Basic Info
        public Rigidbody rb;
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
        public float linearForce = 30000;
        public float linearDamper = 500;
        public float angularForce = 20000;
        public float angularDamper = 500;
        [Tooltip("lerp speed when single hand")]
        public float singleHandSpeed = 0.2f;
        [Tooltip("lerp speed when two hand")]
        public float twoHandSpeed = 0.3f;

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


#if UNITY_EDITOR
        [Button]
        void SwingLikePistol()
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 4;
                rb.drag = 0.5f;
                rb.angularDrag = 1f;
            }
            linearForce = 20000;
            linearDamper = 500;
            angularForce = 14000;
            angularDamper = 500;
            singleHandSpeed = 1f;
            twoHandSpeed = 1;
            Debug.Log("Done, please adjust CenterMass and InertiaTensor in RigidbodyInit as you need");
        }


        [Button]
        void SwingLikeDagger()
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 6;
                rb.drag = 0.5f;
                rb.angularDrag = 1f;
            }
            linearForce = 20000;
            linearDamper = 500;
            angularForce = 14000;
            angularDamper = 500;
            singleHandSpeed = 0.9f;
            twoHandSpeed = 1;
            Debug.Log("Done, please adjust CenterMass and InertiaTensor in RigidbodyInit as you need");
        }

        [Button]
        void SwingLikeShortSword()
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 8;
                rb.drag = 0.5f;
                rb.angularDrag = 1f;
            }
            linearForce = 20000;
            linearDamper = 500;
            angularForce = 15000;
            angularDamper = 500;
            singleHandSpeed = 0.45f;
            twoHandSpeed = 0.8f;
            Debug.Log("Done, please adjust CenterMass and InertiaTensor in RigidbodyInit as you need");
        }

        [Button]
        void SwingLikeRegularSword()
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 10;
                rb.drag = 0.8f;
                rb.angularDrag = 1.2f;
            }
            linearForce = 20000;
            linearDamper = 500;
            angularForce = 12000;
            angularDamper = 500;
            singleHandSpeed = 0.45f;
            twoHandSpeed = 0.6f;
            Debug.Log("Done, please adjust CenterMass and InertiaTensor in RigidbodyInit as you need");
        }

        [Button]
        void SwingLikeKatana()
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 10;
                rb.drag = 0.8f;
                rb.angularDrag = 1.2f;
            }
            linearForce = 20000;
            linearDamper = 500;
            angularForce = 9000;
            angularDamper = 500;
            singleHandSpeed = 0.5f;
            twoHandSpeed = 0.8f;
            Debug.Log("Done, please adjust CenterMass and InertiaTensor in RigidbodyInit as you need");
        }

        [Button]
        void SwingLikeClaymore()
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 14;
                rb.drag = 1.2f;
                rb.angularDrag = 1.5f;
            }
            linearForce = 30000;
            linearDamper = 500;
            angularForce = 10000;
            angularDamper = 500;
            singleHandSpeed = 0.25f;
            twoHandSpeed = 0.8f;
            Debug.Log("Done, please adjust CenterMass and InertiaTensor in RigidbodyInit as you need");
        }

        [Button]
        void SwingLikeSpear()
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 14;
                rb.drag = 1.2f;
                rb.angularDrag = 1.6f;
            }
            linearForce = 30000;
            linearDamper = 500;
            angularForce = 13000;
            angularDamper = 500;
            singleHandSpeed = 0.28f;
            twoHandSpeed = 0.8f;
            //enableAutoTickTwoHand = true;
            Debug.Log("Done, please adjust CenterMass and InertiaTensor in RigidbodyInit as you need");
        }

        [Button]
        void SwingLikeHammer()
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 20;
                rb.drag = 1.2f;
                rb.angularDrag = 2f;
            }
            linearForce = 30000;
            linearDamper = 500;
            angularForce = 13000;
            angularDamper = 500;
            singleHandSpeed = 0.18f;
            twoHandSpeed = 0.45f;
            Debug.Log("Done, please adjust CenterMass and InertiaTensor in RigidbodyInit as you need");
        }
#endif
    }
}

