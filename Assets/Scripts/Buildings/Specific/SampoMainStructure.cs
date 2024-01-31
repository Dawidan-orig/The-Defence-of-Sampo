using Cinemachine;
using Sampo.Player;
using UnityEngine;

namespace Sampo.Core
{
    public class SampoMainStructure : MonoBehaviour
    {
        public float playerRespawnTime = 10;
        public GameObject playerPrefab;
        public Transform playerContainer;
        public PlayerController connectedPlayer;

        private bool playerSpawnInvoked = false;

        private void Update()
        {
            if (connectedPlayer == null && !playerSpawnInvoked)
            {
                Invoke(nameof(RespawnPlayer), playerRespawnTime);
                playerSpawnInvoked = true;
            }
        }

        private void RespawnPlayer()
        {
            playerSpawnInvoked = false;

            const float FORWARD = 3;
            const float UPWARD = 1;

            Vector3 spawnPos = transform.position + transform.up * UPWARD + transform.forward * FORWARD + transform.position + GetComponent<Collider>().bounds.extents;
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity, playerContainer);
            connectedPlayer = player.GetComponent<PlayerController>();
            CameraController.Instance.ThirdPerson = player.GetComponentInChildren<ThirdPersonCameraPositioner>().gameObject.GetComponent<CinemachineVirtualCamera>();
            CameraController.Instance.ThirdPerson = player.GetComponentInChildren<FirstPersonCameraPositioner>().gameObject.GetComponent<CinemachineVirtualCamera>();
        }
    }
}
