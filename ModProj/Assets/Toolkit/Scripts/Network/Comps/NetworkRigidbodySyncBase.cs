using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Runtime.CompilerServices;

namespace CrossLink.Network
{
    public class NetworkRigidbodySyncBase : NetworkSyncBase
    {
        public Rigidbody targetRig;
        public bool isClientLocalWeapon = false;
        public bool isOnSlot = false;

        [Header("Bandwidth Savings")]
        [Tooltip("When true, changes are not sent unless greater than sensitivity values below.")]
        public bool onlySyncOnChange = true;
        [Tooltip("If we only sync on change, then we need to correct old snapshots if more time than sendInterval * multiplier has elapsed." +
            "\n\nOtherwise the first move will always start interpolating from the last move sequence's time, which will make it stutter when starting every time.")]
        public float onlySyncOnChangeCorrectionMultiplier = 2;
        [Tooltip("Apply smallest-three quaternion compression. This is lossy, you can disable it if the small rotation inaccuracies are noticeable in your project.")]
        public bool compressRotation = true;
        public float rotationSensitivity = 0.01f;
        public bool syncLocalTrans;

        [Header("Timeline Offset")]
        [Tooltip("Add a small timeline offset to account for decoupled arrival of NetworkTime and NetworkRigidbody snapshots." +
            "\nfixes: https://github.com/MirrorNetworking/Mirror/issues/3427")]
        public bool timelineOffset = false;

        // delta compression is capable of detecting byte-level changes.
        // if we scale float position to bytes,
        // then small movements will only change one byte.
        // this gives optimal bandwidth.
        //   benchmark with 0.01 precision: 130 KB/s => 60 KB/s
        //   benchmark with 0.1  precision: 130 KB/s => 30 KB/s
        [Header("Precision")]
        [Tooltip("Position is rounded in order to drastically minimize bandwidth." +
            "\n\nFor example, a precision of 0.01 rounds to a centimeter. In other words, sub-centimeter movements aren't synced until they eventually exceeded an actual centimeter." +
            "\n\nDepending on how important the object is, a precision of 0.01-0.10 (1-10 cm) is recommended." +
            "\n\nFor example, even a 1cm precision combined with delta compression cuts the Benchmark demo's bandwidth in half, compared to sending every tiny change.")]
        [Range(0.00_01f, 1f)]                   // disallow 0 division. 1mm to 1m precision is enough range.
        public float positionPrecision = 0.01f; // 1 cm
        [Range(0.00_01f, 1f)]                   // disallow 0 division. 1mm to 1m precision is enough range.
        public float velocityPrecision = 0.01f;      // 1 cm
        [Range(0.00_01f, 1f)]                   
        public float angularVelocityPrecision = 0.01f;

        public bool physicsFollow = true;

        public Vector3 receivePosition;
        public Vector3 receiveVelocity;
        public Vector3 receiveAngularVelocity;

        public Vector3 lastPosition;
        public Vector3 lastVelocity;
        public Vector3 lastAngularVelocity;

        // selective sync //////////////////////////////////////////////////////
        [Header("Selective Sync. Don't change these at Runtime")]
        public bool syncPosition = true;  // do not change at runtime!
        public bool syncRotation = true;  // do not change at runtime!
        public bool syncVelocity = false; // do not change at runtime!
        public bool syncAngularVelocity = false; // do not change at runtime!

        // interpolation is on by default, but can be disabled to jump to
        // the destination immediately. some projects need this.
        [Header("Interpolation")]
        [Tooltip("Set to false to have a snap-like effect on position movement.")]
        public bool lerpPosition = true;
        [Tooltip("Set to false to have a snap-like effect on rotations.")]
        public bool lerpRotation = true;
        [Tooltip("Set to false to remove velocity smoothing.")]
        public bool lerpVelocity = true;
        [Tooltip("Set to false to remove angluar velocity smoothing.")]
        public bool lerpAngularVelocity = true;
    }
}