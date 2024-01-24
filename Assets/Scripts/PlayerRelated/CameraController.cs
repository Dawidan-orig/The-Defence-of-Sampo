using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Player {
    public class CameraController : MonoBehaviour
    {
        public CinemachineVirtualCamera FPS;
        public CinemachineVirtualCamera ThirdPerson;
        public CinemachineVirtualCamera Building;

        public PlayerController player;

        private CinemachineVirtualCamera[] cameras;
        private bool _buildMode = false;
        private void Start()
        {
            cameras = new CinemachineVirtualCamera[] { FPS, ThirdPerson, Building };
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
