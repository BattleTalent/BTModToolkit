using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class SceneObj : PhysicsUnit
    {
        public SceneObj[] subObjs;
        public CharacterStat hp;
        public event System.Action HitEvent;
        //public bool freeFromSingleTile = false;

        public bool faceToTileDoor = false;
    }
}