using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    public class WeaponFlyObjBase : InteractTrigger
    {
        // basic shooting
        public Transform shootPosition;
        public string flyobjName;
        public float shootVel = -1;

        // aim effect
        public AimWidget aimWidget;

        // recoil & anim
        public Animator shootAnim;
        public AnimationCurve recoilCurve;
        public Vector3 recoilVector = new Vector3(0, 0.2f, -1);
        public float recoilRotate = -3;
        public float recoilRandomBias = 3;

        // charge
        public bool dontShootOnInstant = false;
        public bool shootOnCharge = true;
        public bool shootOnActivateEnd = false;
        public bool autoSetupBulletIgnore = true;
    }

}