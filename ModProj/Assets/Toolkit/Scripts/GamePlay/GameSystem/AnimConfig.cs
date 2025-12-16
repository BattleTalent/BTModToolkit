using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace CrossLink
{

    [System.Serializable]
    public class AnimLayoutDataItem
    {
        public string Name;
        public float Len;

        public AnimationCurve RootTx;
        public AnimationCurve RootTy;
        public AnimationCurve RootTz;

        public AnimationCurve RootQx;
        public AnimationCurve RootQy;
        public AnimationCurve RootQz;
        public AnimationCurve RootQw;

        public Vector3 EvaluatePos(float t)
        {
            return new Vector3(
                RootTx.Evaluate(t),
                RootTy.Evaluate(t),
                RootTz.Evaluate(t)
                );
        }

        public Quaternion EvaluateRot(float t)
        {
            return new Quaternion(
                RootQx.Evaluate(t),
                RootQy.Evaluate(t),
                RootQz.Evaluate(t),
                RootQw.Evaluate(t)
                );
        }
    }

}
