using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Player {
    public class CameraController : MonoBehaviour
    {
        public CinemachineVirtualCamera FirstPerson;
        public CinemachineVirtualCamera ThirdPerson;
        public CinemachineVirtualCamera Building;

        public PlayerController player;

        private CinemachineVirtualCamera[] cameras;
        private bool _buildMode = false;

        private static CameraController _instance;
        public static CameraController Instance {
            get {
                return _instance;
            }
        }
        private void Awake()
        {
            if(_instance == null)
                _instance = this;
        }

        private void Start()
        {
            cameras = new CinemachineVirtualCamera[] { FirstPerson, ThirdPerson, Building };
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.B)) 
            {
                EqualizePriorities();

                if (!_buildMode)
                {
                    Building.Priority = 15;
                    player.enabled = false;
                }
                else
                {
                    ThirdPerson.Priority = 15;
                    player.enabled = true;
                }

                _buildMode = !_buildMode;
            }
        }

        private void EqualizePriorities() 
        {
            foreach(var cam in cameras) 
            {
                cam.Priority = 10;
            }
        }
    }
}
