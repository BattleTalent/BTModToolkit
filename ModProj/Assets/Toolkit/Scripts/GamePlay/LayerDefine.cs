using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    static public class LayerDefine
    {
        static public LayerMask DefaultLayer = LayerMask.NameToLayer("Default");
        static public LayerMask EnvLayer = LayerMask.NameToLayer("CharacterObstacle");
        static public LayerMask EnvOnlyLayer = LayerMask.NameToLayer("ObstacleOnly");
        static public LayerMask CharacterLayer = LayerMask.NameToLayer("Character");
        static public LayerMask RagdollLayer = LayerMask.NameToLayer("Ragdoll");
        static public LayerMask RagdollOnlyLayer = LayerMask.NameToLayer("RagdollOnly");
        static public LayerMask RagdollFakeLayer = LayerMask.NameToLayer("RagdollFakePart");
        static public LayerMask InteractLayer = LayerMask.NameToLayer("Interact");
        static public LayerMask AirwallLayer = LayerMask.NameToLayer("AirWall");
        static public LayerMask CharacterOnlyLayer = LayerMask.NameToLayer("CharacterOnly");        
        static public LayerMask CharacterActLayer = LayerMask.NameToLayer("CharacterAct");
        static public LayerMask UILayer = LayerMask.NameToLayer("UI");
        static public LayerMask BodyParts = LayerMask.NameToLayer("BodyParts");
        static public LayerMask InvisibleLivLayer = LayerMask.NameToLayer("InvisibleLiv");
        static public LayerMask InvisibleFPSLayer = LayerMask.NameToLayer("InvisibleFPS");
        static public LayerMask HudFingerLayer = LayerMask.NameToLayer("HudFinger");

        static public LayerMask EnvLayerMask = 1 << LayerMask.NameToLayer("CharacterObstacle");
        static public LayerMask EnvOnlyLayerMask = 1 << LayerMask.NameToLayer("ObstacleOnly");
        static public LayerMask CharacterLayerMask = 1 << LayerMask.NameToLayer("Character");
        static public LayerMask RagdollLayerMask = 1 << LayerMask.NameToLayer("Ragdoll");
        static public LayerMask RagdollFakeLayerMask = 1 << LayerMask.NameToLayer("RagdollFakePart");
        static public LayerMask InteractLayerMask = 1 << LayerMask.NameToLayer("Interact");
        static public LayerMask BodyMask = RagdollLayerMask | RagdollFakeLayerMask | InteractLayerMask;
        static public LayerMask VisibleCollisionMask = BodyMask | EnvLayerMask;
        static public LayerMask AirwallLayerMask = 1 << LayerMask.NameToLayer("AirWall");
        static public LayerMask CharacterOnlyLayerMask = 1 << LayerMask.NameToLayer("CharacterOnly");
        static public LayerMask CharacterActLayerMask = 1 << LayerMask.NameToLayer("CharacterAct");
        static public LayerMask UILayerMask = 1 << LayerMask.NameToLayer("UI");
      

    }

}