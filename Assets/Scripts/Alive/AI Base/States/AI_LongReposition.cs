using Sampo.Melee;
using Sampo.Weaponry.Ranged;
using UnityEngine;

namespace Sampo.AI
{
    public class AI_LongReposition : UtilityAI_BaseState
    // ИИ двигается в какую-то точку, при это не делая более ничего
    {        
        public AI_LongReposition(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
        {

        }

        public override bool CheckSwitchStates()
        {
            if (_ctx.IsDecidingStateRequired() || _ctx.CurrentActivity.target == null)
            {
                SwitchStates(_factory.Deciding());
                return true;
            }

            /*if (path.status == NavMeshPathStatus.PathInvalid)
            {
                SwitchStates(_factory.Deciding());
                return true;
            }*/

            if (_ctx.CurrentActivity.actWith is BaseShooting shooting)
            {
                bool available = shooting.AvilableToShoot(_ctx.CurrentActivity.target, out _);
                if (available)
                    SwitchStates(_factory.Deciding());

                return available;
            }

            //TODO? : Выглядит мерзковато, слишком сильная привязка к разделению между Дальним боем и Ближним.
            bool outOfRange = _ctx.CurrentActivity.actWith.GetRange() + (_ctx.BehaviourAI is MeleeFighter fighter ? fighter.baseReachDistance : 0) >
                Vector3.Distance(_ctx.transform.position, _ctx.CurrentActivity.target.position);
            if (outOfRange)
                SwitchStates(_factory.Deciding());

            return outOfRange;
        }

        public override void FixedUpdateState()
        {

        }

        public override void InitializeSubState()
        {

        }

        public override void UpdateState()
        {
            Debug.DrawRay(_ctx.transform.position, Vector3.up * 2, Color.blue);

            if (CheckSwitchStates())
                return;

            _ctx.MovingAgent.MoveIteration(moveTargetPos);

            CheckRepath();
        }        

        public override string ToString()
        {
            return "Moving";
        }
    }
}