using Sampo.Player;
using System;
using UnityEngine;

namespace Sampo.Abilities
{
    [Serializable]
    public class ProceedingSlash : Ability
    {
        public GameObject _slashPrefab;
        public LayerMask layers;
        public const float SPEED = 25;
        public const float LIFETIME = 10;
        public const float RECHARGE = 10;
        public const int SLICES = 20;
        public const float UPFORCE_POWER = 4;
        SwordControl userActions;

        [SerializeField]
        private float _slicesLeft;
        [SerializeField]
        private float _currentRecharge;
        [SerializeField]
        private Rigidbody _body;

        public ProceedingSlash(Transform user) : base(user)
        {
            _slicesLeft = 0;
            _currentRecharge = RECHARGE;
            //TODO? : Использование Resorces - не самая лучшая идея. Может, можно придумать что-то получше?
            //Тем не менее, теперь этот класс способности изолирован и независим.
            _slashPrefab = Resources.Load<GameObject>("WindSlash");
            _body = user.GetComponent<Rigidbody>();
        }

        public override void Enable()
        {
            if (user.TryGetComponent<SwordControl>(out var control))
            {
                base.Enable();
                userActions = control;
                userActions.OnSlashEnd += PerformAbility;
            }
        }

        public override void Disable()
        {
            base.Disable();

            userActions.OnSlashEnd -= PerformAbility;
            userActions = null;
        }

        public override void Activate()
        {
            if (_currentRecharge >= RECHARGE)
            {
                base.Activate();
                _slicesLeft = SLICES;
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            _currentRecharge = 0;
        }

        public void PerformAbility(object sender, SwordControl.ActionData e)
        {
            if (!AbleToUse() || _slicesLeft <= 0)
                return;

            _slicesLeft--;

            _body.AddForce(!user.GetComponent<IAnimationProvider>().IsGrounded() ? Vector3.up * UPFORCE_POWER : Vector3.zero, ForceMode.VelocityChange);

            GameObject slash = GameObject.Instantiate(_slashPrefab, Vector3.Lerp(e.moveStart.position, e.desire.position, 0.5f),
                Quaternion.FromToRotation(Vector3.right,
                Vector3.ProjectOnPlane((e.desire.position - e.moveStart.position).normalized, user.forward)));

            slash.GetComponent<Tool>().Host = user;

            slash.GetComponent<Faction>().ChangeFactionCompletely(user.GetComponent<Faction>().FactionType);

            Rigidbody body = slash.GetComponent<Rigidbody>();

            Vector3 direction = user.forward;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var hit, 300, layers))
                direction = (hit.point - user.position).normalized;

            body.AddForce(direction * 10, ForceMode.VelocityChange);
            body.drag = 0;

            Physics.IgnoreCollision(slash.GetComponent<Collider>(), e.blade.GetComponent<Collider>());
            Physics.IgnoreCollision(slash.GetComponent<Collider>(), user.GetComponent<Collider>());

            GameObject.Destroy(slash, LIFETIME);

            if (_slicesLeft <= 0)
                Deactivate();
        }

        public override void Update()
        {
            if (_activated)
                return;

            if (_currentRecharge < RECHARGE)
                _currentRecharge += Time.deltaTime;
        }
    }
}