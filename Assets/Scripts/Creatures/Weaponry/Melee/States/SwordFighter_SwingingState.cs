using System;
using UnityEngine;

namespace Sampo.Melee.Sword
{
    [Serializable]
    public class SwordFighter_SwingingState : SwordFighter_BaseState
    {

        public SwordFighter_SwingingState(SwordFighter_StateMachine currentContext, SwordFighter_StateFactory factory)
            : base(currentContext, factory) { }

        public override void EnterState()
        {
            _ctx.Blade.OnBladeCollision += BladeCollisionEnter;
            _ctx.Blade.OnBladeTrigger += BladeTriggerEnter;
        }

        public override void ExitState()
        {
            _ctx.Blade.OnBladeCollision -= BladeCollisionEnter;
            _ctx.Blade.OnBladeTrigger -= BladeTriggerEnter;
        }

        public override void FixedUpdateState()
        {
            ProcessSwingSword();
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
        }

        public override void CheckSwitchStates()
        {
            if (_ctx.CloseToDesire() && _ctx.CurrentActivity.target)
            {
                HandleCombo();
            }
        }

        private void ProcessSwingSword()
        {
            Vector3 savePos = _ctx.BladeHandle.position;

            float relativeHeightFrom = _ctx.MoveFrom.position.y - _ctx.transform.position.y;
            float relativeHeightTo = _ctx.DesireBlade.position.y - _ctx.transform.position.y;

            Vector3 relativeFrom = _ctx.MoveFrom.position - _ctx.transform.position;
            relativeFrom.y = 0;
            Vector3 relativeTo = _ctx.DesireBlade.position - _ctx.transform.position;
            relativeTo.y = 0;

            _ctx.BladeHandle.position = _ctx.transform.position
                + Vector3.SlerpUnclamped(relativeFrom, relativeTo, _ctx.MoveProgress)
                + new Vector3(0, Mathf.LerpUnclamped(relativeHeightFrom, relativeHeightTo, _ctx.MoveProgress), 0);

            _ctx.BladeHandle.LookAt(_ctx.BladeHandle.position + (_ctx.BladeHandle.position - _ctx.Vital.bounds.center).normalized,
                (_ctx.DesireBlade.position - _ctx.BladeHandle.position).normalized);
            _ctx.BladeHandle.RotateAround(_ctx.BladeHandle.position, _ctx.BladeHandle.right, 90);

            Debug.DrawLine(_ctx.BladeHandle.position, savePos, Color.white, 0.5f);
        }

        private void BladeCollisionEnter(object sender, Collision collision)
        {
            if ((!collision.gameObject.TryGetComponent<Rigidbody>(out _)) ||
                collision.gameObject.TryGetComponent<Blade>(out _))
            {
                HandleCombo();
                return;
            }

            //TODO? : Raycast для хорошей коллизии; Пули не отбиваются - слишком маленькие. Но с Continious Detection сбиваются нормально.
        }

        private void BladeTriggerEnter(object sender, Collider other)
        {
            if ((!other.gameObject.TryGetComponent<Rigidbody>(out _)) ||
                other.gameObject.TryGetComponent<Blade>(out _))
            {
                _ctx.SetInitialDesires();
                _ctx.CurrentToInitialAwait = _ctx.toInitialAwait;
                _ctx.InitiateNewBladeMove();

                SwitchStates(_factory.Repositioning());
                return;
            }
        }

        public override string ToString()
        {
            return "Swinging";
        }
    }
}