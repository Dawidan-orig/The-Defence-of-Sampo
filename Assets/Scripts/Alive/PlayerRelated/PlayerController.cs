using Cinemachine;
using WingedCore.Player.CameraControls;
using UnityEngine;
using WingedCore.Core;
using Alchemy.Inspector;
using WingedCore.Core.Balance;

namespace WingedCore.Player
{

    [RequireComponent(typeof(Movement))]
    public class PlayerController : MonoBehaviour, IAnimationProvider
    {
        [Required]
        [SerializeField]
        CinemachineVirtualCamera FirstPerson;
        [Required]
        [SerializeField]
        CinemachineVirtualCamera ThirdPerson;

        Movement movement;

        public Transform usedMainHand;
        [Header("Weaponry")]
        [Header("MeleeWeapon")]
        public SwordControl swordControl;
        public float mouseDeltaForSwing = 80;
        public float reachLength = 1;
        public float castToWeaponSpaceK = 100;

        [Header("lookonly")]
        [SerializeField]
        Vector3 _prevMouse;
        [SerializeField]
        Transform _handTarget;
        private BalanceInfluencer _influence;

        private void Awake()
        {
            movement = GetComponent<Movement>();

            MonoBehaviourSingleton<CameraController>.Instance.ThirdPerson = FirstPerson;
            MonoBehaviourSingleton<CameraController>.Instance.FirstPerson = ThirdPerson;
            MonoBehaviourSingleton<CameraController>.Instance.player = this;
        }

        private void Update()
        {
            UpdateInput();

            if (TryGetComponent<SwordControl>(out var c))
            {
                swordControl = c;
                _handTarget = c.bladeHandle;
            }
        }

        private void UpdateInput()
        {
            Vector2 input;
            input.x = Input.GetAxisRaw("Vertical");
            input.y = Input.GetAxisRaw("Horizontal");

            Movement.SpeedType type = Movement.SpeedType.walk;
            if (movement.IsGrounded && Input.GetKey(KeyCode.LeftShift))
                type = Movement.SpeedType.sprint;
            else if (movement.IsGrounded)
                type = Movement.SpeedType.run;

            movement.PassInputDirect(input, type, Input.GetKeyDown(KeyCode.Space) && movement.IsGrounded && movement.JumpReady);

            #region sword fighting
            //TODO DESIGN :  ѕродумать систему, при которой можно будет использовать разное оружие, а не только ближний бой
            if (swordControl)
            {
                // ѕростое перемещение оружи€
                if (Input.GetMouseButton(0) && Vector3.Distance(Input.mousePosition, _prevMouse) < mouseDeltaForSwing)
                {
                    swordControl.ApplyNewDesire(CastMouseToSwordSpace(), transform.up, transform.forward);

                    _prevMouse = Input.mousePosition;
                }
                // ¬змах оружием
                else if (Input.GetMouseButton(0) && Vector3.Distance(Input.mousePosition, _prevMouse) > mouseDeltaForSwing)
                {
                    Vector3 screenDeltaDir = (Input.mousePosition - _prevMouse).normalized;

                    Vector3 swingPos = usedMainHand.position + transform.rotation * (reachLength * screenDeltaDir);

                    swordControl.Swing(swingPos);

                    _prevMouse = Input.mousePosition;
                }
                else if (Input.GetMouseButton(1))
                {
                    Vector3 start = CastMouseToSwordSpace();
                    Vector3 controlCenter = transform.position + transform.forward * reachLength;
                    Vector3 relativeEnd = (controlCenter - start).normalized * swordControl.blade.additionalMeleeReach/2;
                    Vector3 end = start + relativeEnd;
                    swordControl.Block(start, end, transform.forward);

                    _prevMouse = Input.mousePosition;
                }
                else
                {
                    swordControl.ReturnToInitial();
                    _prevMouse = new Vector3(Screen.width / 2, Screen.height / 2);
                }
            }
            #endregion
        }

        private Vector3 CastMouseToSwordSpace()
        {
            return CastMouseToSwordSpace(Input.mousePosition);
        }

        private Vector3 CastMouseToSwordSpace(Vector3 screenPos)
        {
            Vector3 centralize = (screenPos - new Vector3(Screen.width / 2, Screen.height / 2)) / castToWeaponSpaceK;
            Vector3 flatCast = transform.position + UnityEngine.Camera.main.transform.forward * reachLength + transform.rotation * centralize;

            Vector3 res = usedMainHand.position + reachLength * (flatCast - usedMainHand.position).normalized;

            return res;
        }

        public Vector3 GetLookTarget()
        {
            Physics.Raycast(UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition), UnityEngine.Camera.main.transform.forward, out var res, 100, LayerMask.NameToLayer("Default"));
            return res.point;
        }

        public bool IsGrounded()
        {
            return movement.IsGrounded;
        }

        public bool IsInJump()
        {
            return movement.InJump;
        }

        public Transform GetRightHandTarget()
        {
            return _handTarget;
        }
    }
}