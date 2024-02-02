using Cinemachine;
using UnityEngine;

namespace Sampo.Player.CameraControls
{
    public class ThirdPersonCameraPositioner : MonoBehaviour
    {
        public PlayerController player;
        public Transform lockTransform;
        public Transform heightTransform;
        public CinemachineTargetGroup targetGroup;
        public float heightTransfromDist = 10;
        public Vector2 sensitivity;
        public Vector2 xAngleLimit = new Vector2(-75, 75);
        public LayerMask alive;
        public LayerMask structures;

        [Header("lookonly")]
        private Vector2 _rotation;
        [SerializeField]
        private Vector3 _initialLookAt;
        [SerializeField]
        private Transform _currentLockRigidbodyTransfrom;
        [SerializeField]
        private CinemachineVirtualCamera virtualCamera;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            virtualCamera = GetComponent<CinemachineVirtualCamera>();

            _initialLookAt = lockTransform.position - transform.position;
        }

        private void LateUpdate()
        {
            if (!CinemachineCore.Instance.IsLive(virtualCamera))
                return;

            const float FAR_AWAY = 300;

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                Ray world_ScreenCenter = UnityEngine.Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

                if (PenetratingRaycast(world_ScreenCenter.origin, world_ScreenCenter.origin + world_ScreenCenter.direction * FAR_AWAY, out RaycastHit hit, 1))
                {
                    lockTransform.position = hit.point;
                    if (hit.rigidbody)
                        _currentLockRigidbodyTransfrom = hit.transform;
                }
                else
                    lockTransform.position = world_ScreenCenter.origin + world_ScreenCenter.direction * FAR_AWAY;
            }
            else if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                if (PenetratingRaycast(transform.position, lockTransform.position, out RaycastHit hit))
                {
                    if (!hit.rigidbody)
                    {
                        _currentLockRigidbodyTransfrom = null;
                        lockTransform.position = hit.point;
                    }
                }

                if (_currentLockRigidbodyTransfrom != null)
                    lockTransform.position = _currentLockRigidbodyTransfrom.position;

                GameObject go = new();
                Transform probe = go.transform;

                probe.position = lockTransform.position;
                probe.position = new Vector3(probe.transform.position.x, player.transform.position.y, probe.transform.position.z);
                player.transform.LookAt(probe);
                _rotation.y = player.transform.rotation.eulerAngles.y;

                Destroy(go);

                //Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                _currentLockRigidbodyTransfrom = null;
                lockTransform.position = transform.position + transform.rotation * _initialLookAt;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity.x;
                float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity.y;

                _rotation.x -= mouseY;
                _rotation.y += mouseX;

                _rotation.x = Mathf.Clamp(_rotation.x, xAngleLimit.x, xAngleLimit.y);

                heightTransform.localPosition = Quaternion.Euler(_rotation.x, 0, 0) * Vector3.forward * heightTransfromDist;

                player.transform.rotation = Quaternion.Euler(0, _rotation.y, 0);
            }
        }

        public bool PenetratingRaycast(Vector3 from, Vector3 to, out RaycastHit hit, float duration = 0, Color? color = null)
        {
            LayerMask CameraLock = 256;

            if (color == null)
                color = Color.white;

            bool res = Utilities.VisualisedRaycast(from,
                    (to - from).normalized,
                    out hit,
                    (to - from).magnitude,
                    alive + structures + CameraLock, duration: duration, color: color);

            Vector3 dirAddition = (to - from).normalized * 0.05f; //Чтобы реально пробивать коллайдеры, особенно кривые

            if (hit.collider)
                if (hit.collider.isTrigger) // Пропускаем все trigger-collider'ы
                {

                    PlayerCameraLockTarget locker = hit.transform.GetComponent<PlayerCameraLockTarget>();
                    if (!locker)
                        locker = hit.transform.GetComponentInChildren<PlayerCameraLockTarget>();

                    if (locker)
                    {
                        //TODO : Не менять, если уже есть lock-объект.
                        //TODO : Если пропал lock-объект, сделать проверку области, чтобы найти и переключится на ближайшую цель.
                        // Тогда управление будет - просто конфетка.
                        _currentLockRigidbodyTransfrom = locker.AlignedLock.transform;
                        return true;
                    }

                    return PenetratingRaycast(hit.point + dirAddition,
                        to,
                        out hit, duration, color);
                }

            //TODO dep PlayerController : Сделать эту проверку относительно любого оружия игрока, а не только меча
            if (hit.transform == player.transform || hit.transform == player.swordControl.blade.transform) // Пропускаем тело игрока и его оружие
            {
                return PenetratingRaycast(hit.point + dirAddition,
                        to,
                        out hit, duration, color);
            }

            if (hit.transform)
                if (hit.transform.TryGetComponent(out Tool tool) && tool is not AttackingLimb)
                {
                    return PenetratingRaycast(hit.point + dirAddition,
                            to,
                            out hit, duration, color);
                }

            return res;
        }
    }
}