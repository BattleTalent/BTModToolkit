using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CrossLink
{



    public class EventToDialogue : EventToBase
    {
        public DialogueData diaData;
        public UnityEvent finAction;
        public string specificVoiceOverFolder;
        public bool randomPick = false;
    }

}
