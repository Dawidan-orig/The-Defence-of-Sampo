using System;
using UnityEngine;

namespace Sampo.Weaponry.Melee.Sword
{
    [Serializable]
    public abstract class SwordFighter_BaseState
    {
        protected SwordFighter_StateMachine _ctx;
        protected SwordFighter_StateFactory _factory;
        public SwordFighter_BaseState(SwordFighter_StateMachine currentContext, SwordFighter_StateFactory factory)
        {
            _ctx = currentContext;
            _factory = factory;
        }

        public abstract void EnterState();

        public abstract void UpdateState();

        public abstract void FixedUpdateState();

        public abstract void ExitState();

        public abstract void CheckSwitchStates();

        protected void SwitchStates(SwordFighter_BaseState newState)
        {
            ExitState();
            newState.EnterState();
            _ctx.CurrentSwordState = newState;
        }

        protected void HandleCombo()
        {
            SwordFighter_StateMachine.ActionJoint action;
            if (_ctx.CurrentCombo.TryPop(out action))
            {
                if (action.currentActionType == SwordFighter_StateMachine.ActionType.Reposition)
                {
                    _ctx.SetDesires(action.nextRelativeDesire + _ctx.transform.position, action.nextRotation * Vector3.up, action.nextRotation * Vector3.forward);
                    _ctx.InitiateNewBladeMove();
                    if (!(_ctx.CurrentSwordState is SwordFighter_RepositioningState))
                        SwitchStates(_factory.Repositioning());
                }
                else if (action.currentActionType == SwordFighter_StateMachine.ActionType.Swing)
                {
                    _ctx.Swing(action.nextRelativeDesire + _ctx.transform.position);
                    _ctx.InitiateNewBladeMove();
                    if (!(_ctx.CurrentSwordState is SwordFighter_SwingingState))
                        SwitchStates(_factory.Swinging());
                }
            }
            else
            {
                _ctx.SetInitialDesires();
                _ctx.InitiateNewBladeMove();
                _ctx.CurrentToInitialAwait = _ctx.toInitialAwait;
                SwitchStates(_factory.Repositioning());
            }
        }
    }
}