using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Player.CameraControls
{
    public class PlayerCameraLockTarget : MonoBehaviour
    {
        private Rigidbody _alignedLock;

        public Rigidbody AlignedLock { get => _alignedLock; set => _alignedLock = value; }

        private void Awake()
        {
            gameObject.layer = 8; //CameraLock layer
            _alignedLock = GetComponentInParent<Rigidbody>();
        }
    }
}