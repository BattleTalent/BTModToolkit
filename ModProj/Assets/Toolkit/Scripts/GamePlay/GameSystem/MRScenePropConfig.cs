using UnityEngine;

namespace CrossLink
{
    public class PropData
    {
        public PropInfo info;

        public Vector3 position;
        public Quaternion rotation;

        public Vector3[] relatedPos;
        public Quaternion[] relatedRot;

        [HideInInspector]
        public Bounds bounds;
    }

    [System.Serializable]
    public class PropInfo
    {
        [System.Serializable]
        public struct RelateInfo
        {
            public string name;
            public float embed;
        }

        public enum PropType
        {
            Kinematic,
            Interact,
            Light,
        }

        public PropType type;

        public string name;

        public bool attachToWall;
        public bool attachToFloor;
        public bool attachToCeiling;
        public Vector3 upAxis;
        public Vector3 scale;
        public float embed;
        public RelateInfo[] related;
    }
}