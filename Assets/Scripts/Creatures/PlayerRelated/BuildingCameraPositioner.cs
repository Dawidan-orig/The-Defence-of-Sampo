using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Player
{
    public class BuildingCameraPositioner : MonoBehaviour
    {
        public Transform position;
        public float movementSpeed = 20;
        public float groundHeight = 2;

        [SerializeField]
        private CinemachineVirtualCamera virtualCamera;

        private void Start()
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        private void Update()
        {
            if (!CinemachineCore.Instance.IsLive(virtualCamera))
                return;

            const float SNAP_DISTANCE = 100;

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector3 worldInput = new Vector3(input.x, 0, input.y);

            position.Translate(worldInput * movementSpeed * Time.deltaTime);

            if(Physics.Raycast(position.position + SNAP_DISTANCE * Vector3.up, Vector3.down, out var hit, SNAP_DISTANCE*2)) 
            {
                position.position = hit.point + Vector3.up * groundHeight;
            }
        }
    }
}