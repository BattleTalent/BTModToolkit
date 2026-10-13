using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class SandBoxControlSetup : MonoBehaviour
    {
        public Transform[] enemySpawnBoards;
        public Transform[] allySpawnBoards;

        public Transform playerTrigger;
        public Transform weaponSpawnBoard;
        public Transform firePlace;
        public Transform campaignBoard;
        public Transform album;
        public Transform sceneObjSpawnPoint;

        public Transform StartBtn;
        public Transform nextSceneObjBtn;
        public Transform prevSceneObjBtn;
        public Transform clearSceneObjBtn;

        public Transform weaponSpawnPoint;

        public string sceneBlockDataName;

    }
}

