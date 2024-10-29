using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{

    public class FlyObjectX : FlyObject
    {
        public LuaScript script;

        #region Life Cycle

        public void OnSpawn(InteractBase ow, Vector3 dir, float vel = -1)
        {
            // script call:OnSpawn
        }

        public void FlyStart(Vector3 vel, bool local = false)
        {
            // script call:FlyStart
        }

        public void OnDelay(float t, float s)
        {
            // script call:OnDelay
        }

        public void OnDelayEnd()
        {
            // script call:OnDelayEnd
        }
        public void FlyStop(bool local = false)
        {
            // script call:FlyStop
        }

        public void OnFinish(bool local = false)
        {
            // script call:OnFinish
        }


        public void OnTimeOut()
        {
            // script call:OnTimeOut
        }

        protected void ResetState()
        {
            // script call:ResetState
        }
        #endregion


        #region Collision
        public void OnCollision(Collision collision, PhysicsUnit pu)
        {
            // script call:OnCollision
        }


        public void OnCollisionUpdate(Rigidbody rb, Collider col, Vector3 point, Vector3 normal, Vector3 relaVel)
        {
            // script call:OnCollisionUpdate
        }

        public void OnCollisionWithHitScan(Rigidbody rb, Collider col, Vector3 point, Vector3 normal, Vector3 relaVel)
        {
            // script call:OnCollisionWithHitScan
        }

        //public void OnCollisionWithPlayer(Collision collision, InteractCharacter player)
        //{
        //    // script call:OnCollisionWithPlayer
        //}

        //public void OnCollisionWithPlayerHand(Collision collision, InteractHand hand)
        //{
        //    // script call:OnCollisionWithPlayerHand
        //}

        //public void OnCollisionWithRole(FullCharacterControl fc, RagdollMuscle mu, Rigidbody rb, Collider col, Vector3 point, Vector3 normal, Vector3 relaVel)
        //{
        //    // script call:OnCollisionWithRole
        //}

        protected void OnCollisionWithScene(Collision collision)
        {
            // script call:OnCollisionWithScene
        }

        #endregion


        #region Trigger
        public void OnTrigger(PhysicsUnit pu, Collider collider)
        {
            // script call:OnTrigger
        }
        //public void OnTriggerWithPlayerHand(Collider collider, InteractHand hand)
        //{
        //    // script call:OnTriggerWithPlayerHand
        //}

        public void OnTriggerWithStaticScene(Collider collider)
        {
            // script call:OnTriggerWithStaticScene
        }

        public void OnTriggerWithPlayer(PhysicsUnit pu, Collider collider)
        {
            // script call:OnTriggerWithPlayer
        }

        //public void OnTriggerWithRole(FullCharacterControl fc, Collider collider)
        //{
        //    // script call:OnTriggerWithRole
        //}
        #endregion

    }

}
