//#define WALL_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{
    public class SlidingWall : SceneObj
    {

        public MultiTriggerHub hub;
        public Transform slideDirIndicator;

        public float width = 5;
    }
}