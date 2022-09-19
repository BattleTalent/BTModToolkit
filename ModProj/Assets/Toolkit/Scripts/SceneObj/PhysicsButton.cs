//#define PBTN_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class PhysicsButton : MonoBehaviour
    {
        public Rigidbody rb;

        public Renderer buttonRenderer;
        public Color unlockColor;
        public Color lockColor;

        [Tooltip("The (worldspace) distance from the initial position you have to push the button for it to register as pushed")]
        public float distanceToEngage = 0.0075f;

        [Tooltip("Is set to true when the button has been pressed down this update frame")]
        public bool buttonDown = false;

        [Tooltip("Is set to true when the button has been released from the down position this update frame")]
        public bool buttonUp = false;

        [Tooltip("Is set to true each frame the button is pressed down")]
        public bool buttonIsPushed = false;

        [Tooltip("Is set to true if the button was in a pushed state last frame")]
        public bool buttonWasPushed = false;

        public float lockAfterTriggerSec = 0;
        public ConfigurableJoint lockJoint;

        public event System.Action PressDownEvent;
        public event System.Action PressUpEvent;

        public SoundEffectInfo pressSound;

        float jointPopupOffset;
    }

}