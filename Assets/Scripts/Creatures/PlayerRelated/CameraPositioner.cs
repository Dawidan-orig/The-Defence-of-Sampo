using Cinemachine;
using Sampo.Player;
using UnityEngine;

namespace Sampo.Player
{
    public class CameraPositioner : MonoBehaviour
    {
        //TODO : FPS Camera
        public PlayerController player;
        public Transform lockTransform;
        public Transform heightTransform;
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
                if (PenetratingRaycast(transform.position, transform.position + transform.forward * FAR_AWAY, out RaycastHit hit, 1))
                {
                    lockTransform.position = hit.point;

                    if (hit.rigidbody && hit.transform != player)
                    {
                        _currentLockRigidbodyTransfrom = hit.transform;
                    }
                }
                else
                    lockTransform.position = transform.position + transform.forward * FAR_AWAY;
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

                //Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                GameObject go = new GameObject();
                Transform probe = go.transform;
                probe.position = transform.position;
                probe.rotation = transform.rotation;

                probe.LookAt(lockTransform.position, Vector3.up);

                _rotation = probe.localEulerAngles;

                if (_rotation.x > 180)
                    _rotation.x -= 360;

                Destroy(go);
            }
            else
            {
                lockTransform.position = transform.position + transform.rotation * _initialLookAt;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity.x;
                float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity.y;

                _rotation.x -= mouseY;
                _rotation.y += mouseX;

                _rotation.x = Mathf.Clamp(_rotation.x, xAngleLimit.x, xAngleLimit.y);
            }

            player.transform.rotation = Quaternion.Euler(0, _rotation.y, 0);

            heightTransform.localPosition = Quaternion.Euler(_rotation.x,0,0) * Vector3.forward * heightTransfromDist;
        }

        public bool PenetratingRaycast(Vector3 from, Vector3 to, out RaycastHit hit, float duration = 0, Color? color = null)
        {
            if (color == null)
                color = Color.white;

            bool res = Utilities.VisualisedRaycast(from,
                    (to - from).normalized,
                    out hit,
                    (to - from).magnitude,
                    alive + structures, duration: duration, color: color);

            Vector3 dirAddition = (to - from).normalized * 0.05f; //„тобы реально пробивать коллайдеры, особенно кривые

            if (hit.collider)
                if (hit.collider.isTrigger) // ѕропускаем все trigger-collider'ы
                {
                    return PenetratingRaycast(hit.point + dirAddition,
                        to,
                        out hit, duration, color);
                }

            //TODO dep PlayerController : —делать эту проверку относительно любого оружи€ игрока, а не только меча
            if (hit.transform == player.transform || hit.transform == player.swordControl.blade.transform) // ѕропускаем тело игрока и его оружие
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