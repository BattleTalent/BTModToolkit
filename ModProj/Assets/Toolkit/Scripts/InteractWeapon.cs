using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using EasyButtons;
#endif

namespace CrossLink
{
    public class InteractWeapon : InteractBase
    {


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
