using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    [CreateAssetMenu(fileName = "ArmorProfile", menuName = "ArmorProfile")]
    public class ArmorProfile : ScriptableObject
    {

        public enum ArmorTag
        {
            ShoulderRight = 101,
            ShoulderLeft = 102,
            ForeArmRight = 103,
            ForeArmLeft = 104,
            HandRight = 105,
            HandLeft = 106,

            LegRight = 201,
            LegLeft = 202,
            LegThighRight = 203,
            LegThighLeft = 204,
            FootRight = 205,
            FootLeft = 206,

            Helm = 301,
            Torso = 302,
            Pelvis = 303,
            Eye = 304,
            Jaw = 305,
            Hair = 306,

            ShoulderRight2 = 101 + 2000,
            ShoulderLeft2 = 102 + 2000,
            ForeArmRight2 = 103 + 2000,
            ForeArmLeft2 = 104 + 2000,
            HandRight2 = 105 + 2000,
            HandLeft2 = 106 + 2000,

            LegRight2 = 201 + 2000,
            LegLeft2 = 202 + 2000,
            LegThighRight2 = 203 + 2000,
            LegThighLeft2 = 204 + 2000,
            FootRight2 = 205 + 2000,
            FootLeft2 = 206 + 2000,

            Helm2 = 301 + 2000,
            Torso2 = 302 + 2000,
            Pelvis2 = 303 + 2000,
            Eye2 = 304 + 2000,
            Jaw2 = 305 + 2000,
            Hair2 = 306 + 2000,



            ShoulderRight3 = 101 + 3000,
            ShoulderLeft3 = 102 + 3000,
            ForeArmRight3 = 103 + 3000,
            ForeArmLeft3 = 104 + 3000,
            HandRight3 = 105 + 3000,
            HandLeft3 = 106 + 3000,

            LegRight3 = 201 + 3000,
            LegLeft3 = 202 + 3000,
            LegThighRight3 = 203 + 3000,
            LegThighLeft3 = 204 + 3000,
            FootRight3 = 205 + 3000,
            FootLeft3 = 206 + 3000,

            Helm3 = 301 + 3000,
            Torso3 = 302 + 3000,
            Pelvis3 = 303 + 3000,
            Eye3 = 304 + 3000,
            Jaw3 = 305 + 3000,
            Hair3 = 306 + 3000,




            ShoulderRight4 = 101 + 4000,
            ShoulderLeft4 = 102 + 4000,
            ForeArmRight4 = 103 + 4000,
            ForeArmLeft4 = 104 + 4000,
            HandRight4 = 105 + 4000,
            HandLeft4 = 106 + 4000,

            LegRight4 = 201 + 4000,
            LegLeft4 = 202 + 4000,
            LegThighRight4 = 203 + 4000,
            LegThighLeft4 = 204 + 4000,
            FootRight4 = 205 + 4000,
            FootLeft4 = 206 + 4000,

            Helm4 = 301 + 4000,
            Torso4 = 302 + 4000,
            Pelvis4 = 303 + 4000,
            Eye4 = 304 + 4000,
            Jaw4 = 305 + 4000,
            Hair4 = 306 + 4000,







            ShoulderRight5 = 101 + 5000,
            ShoulderLeft5 = 102 + 5000,
            ForeArmRight5 = 103 + 5000,
            ForeArmLeft5 = 104 + 5000,
            HandRight5 = 105 + 5000,
            HandLeft5 = 106 + 5000,

            LegRight5 = 201 + 5000,
            LegLeft5 = 202 + 5000,
            LegThighRight5 = 203 + 5000,
            LegThighLeft5 = 204 + 5000,
            FootRight5 = 205 + 5000,
            FootLeft5 = 206 + 5000,

            Helm5 = 301 + 5000,
            Torso5 = 302 + 5000,
            Pelvis5 = 303 + 5000,
            Eye5 = 304 + 5000,
            Jaw5 = 305 + 5000,
            Hair5 = 306 + 5000,






            X1 = 1001,
            X2 = 1002,
            X3 = 1003,
            X4 = 1004,
            X5 = 1005,
            X6 = 1006,
            X7 = 1007,
            X8 = 1008,
            X9 = 1009,

            Y1 = 2001,
            Y2 = 2002,
            Y3 = 2003,
            Y4 = 2004,
            Y5 = 2005,
            Y6 = 2006,
            Y7 = 2007,
            Y8 = 2008,
            Y9 = 2009,

        }

        [System.Serializable]
        public class ArmorConfig
        {
            public ArmorTag tag;
            // if respath is empty, means we will use bonepath's res as instaited res
            public string resBonePath;
            public string resBoneDetachColPath;
            public string bonePath;
            public Vector3 flipLocalScale = Vector3.zero;
            public float hpPercent = 0.25f;
            public bool fullCover = false;
            public string beHitEffect;
            public float stiffSec = -1;
        }

        public List<ArmorConfig> armorConfigList;
    }

}