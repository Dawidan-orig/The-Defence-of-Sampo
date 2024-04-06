using Cinemachine;
using Sampo.AI;
using Sampo.GUI;
using Sampo.Player;
using Sampo.Player.CameraControls;
using Sampo.Player.Economy;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace Sampo.Core
{
    public class SampoMainStructure : MonoBehaviour, IInteractable
    {
        [Header("setup")]
        public Collider sizingCollider;
        [Header("Player-related")]
        public float playerRespawnTime = 10;
        public GameObject playerPrefab;
        public PlayerController connectedPlayer;
        [Header("GUI")]
        public VisualTreeAsset spawnOption;
        [Header("Possible to spawn prefabs")]
        [SerializeField]
        List<SpawnOption> spawnables = new();

        private bool playerSpawnInvoked = false;

        private UIDocument menuDocument;
        private SpawnMenuController menu;
        private bool isListeningGUI = false;

        private void Start()
        {
            menuDocument = GetComponent<UIDocument>();
        }

        private void Update()
        {
            if (connectedPlayer == null && !playerSpawnInvoked)
            {
                Invoke(nameof(RespawnPlayer), playerRespawnTime);
                playerSpawnInvoked = true;
            }

            if (isListeningGUI && Input.GetMouseButtonUp(1))
            {
                isListeningGUI = false;

                IPanel panel = menuDocument.rootVisualElement.panel;
                Vector2 converted = Input.mousePosition;
                converted.y = Screen.height - converted.y;
                Vector2 pointerUI = RuntimePanelUtils.ScreenToPanel(panel, converted);
                VisualElement result = panel.Pick(pointerUI);

                GameObject prefab = menu.ConnectUIToObject(result);
                if (prefab && EconomySystem.Instance.Spend(prefab.GetComponent<TargetingUtilityAI>().VisiblePowerPoints))
                    SpawnGameObject(prefab);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                menu.ClearVisuals();
            }
        }

        private void RespawnPlayer()
        {
            playerSpawnInvoked = false;

            GameObject player = SpawnGameObject(playerPrefab);
            connectedPlayer = player.GetComponentInChildren<PlayerController>();

            CameraController.Instance.ThirdPerson = player.GetComponentInChildren<ThirdPersonCameraPositioner>().gameObject.GetComponent<CinemachineVirtualCamera>();
            CameraController.Instance.FirstPerson = player.GetComponentInChildren<FirstPersonCameraPositioner>().gameObject.GetComponent<CinemachineVirtualCamera>();
        }

        private GameObject SpawnGameObject(GameObject prefab)
        {
            const float FORWARD = 3;
            const float UPWARD = 1;

            Vector3 spawnPos = transform.position
                + transform.up * UPWARD
                + transform.forward * FORWARD * sizingCollider.bounds.extents.z ;
            GameObject res = Instantiate(prefab, spawnPos, Quaternion.identity);

            if(res.TryGetComponent<Faction>(out var f)) //TODO : Сделать short-hand для этого, одну функцию, что принимает два параметра. Применить везде. Слишком часто используется (DRY).
                f.ChangeFactionCompletely(sizingCollider.transform.GetComponent<Faction>().FactionType);

            return res;
        }

        public void Interact(Transform interactor)
        {
            if (interactor.gameObject.TryGetComponent<PlayerController>(out var player))
            {
                //TODO : Меню появляется даже тогда, когда взаимодействие происходит не правой кнопкой мыши. Это надо убрать.
                menu = new SpawnMenuController();
                menu.InitializeCharacterList(menuDocument.rootVisualElement, spawnOption, spawnables);

                isListeningGUI = true;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                /*
                 * Это всё для кругового окна. Может, и не воспользуюсь.
                List<string> menus = new();

                foreach (var option in spawnables)
                {
                    string menu = option.Path.Split("/")[0];
                    if (!menus.Contains(menu))                    
                        menus.Add(menu);
                }

                float angleDifference = 360 / menus.Count * Mathf.Deg2Rad;
                int i = 0;
                foreach (var menu in menus) {
                    VisualElement elem = new VisualElement();
                    resultMenu.Add(elem);
                    Rect sample = new Rect(Vector2.up * Mathf.Sin(angleDifference) + Vector2.right * Mathf.Cos(angleDifference), new Vector2(10,10));
                    elem. = sample;
                    i++;
                }*/
            }
        }
    }
}
