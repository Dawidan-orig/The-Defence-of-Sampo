using Cinemachine;
using Sampo.Building;
using UnityEngine;

namespace Sampo.Player
{
    public class BuildingCameraPositioner : MonoBehaviour
    {
        public float movementSpeed = 20;
        public float groundHeight = 2;

        [Header("Setup")]
        public Transform position;
        public Transform buildingsParent;
        public GameObject WallPylonPrefab;

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

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            const float SNAP_DISTANCE = 100;

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector3 worldInput = new Vector3(input.x, 0, input.y);

            position.Translate(worldInput * movementSpeed * Time.deltaTime);

            if (Physics.Raycast(position.position + SNAP_DISTANCE * Vector3.up, Vector3.down, out var hit, SNAP_DISTANCE * 2))
            {
                position.position = hit.point + Vector3.up * groundHeight;
            }

            CheckInputs();
        }

        private void CheckInputs()
        {
            if (Input.GetMouseButtonDown(0))
                if (Utilities.GetMouseInWorldCollision(out var point))
                {
                    Instantiate(BuildingSystem.Instance.ChosenStructureToBuild, point, Quaternion.identity, buildingsParent);
                }

            //TODO : Временное решение, без интерфейса
            for(int i = 0; i < 10; i++) 
            {
                if (Input.GetKey(KeyCode.Alpha0 + i))
                { 
                    BuildingSystem.Instance.ChosenStructureToBuild
                        = BuildingSystem.Instance.prefabs[i == 0 ? 9 : i-1].GetComponent<BuildableStructure>();
                }
            }            
        }
    }
}