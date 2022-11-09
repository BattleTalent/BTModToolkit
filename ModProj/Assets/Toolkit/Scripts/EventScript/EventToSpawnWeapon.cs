using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{

    public class EventToSpawnWeapon : EventToBase
    {
        public string weaponName;
        public Transform spawnPos;
        //public Renderer tex;
        public TMPro.TextMeshPro text;
        public bool spawnFromRoguelite;
    }

}