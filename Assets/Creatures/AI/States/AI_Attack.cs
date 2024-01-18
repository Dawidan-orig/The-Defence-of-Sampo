using UnityEngine;

namespace Sampo.AI
{
    public class AI_Attack : UtilityAI_BaseState
    // ИИ двигается и атакует в этом состоянии.
    {
        public AI_Attack(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
        {
        }

        public override bool CheckSwitchStates()
        {
            if (_ctx.DecidingStateRequired() || _ctx.CurrentActivity.target == null)
            {
                SwitchStates(_factory.Deciding());
                return true;
            }

            return false;
        }

        public override void InitializeSubState()
        {

        }

        public override void UpdateState()
        {
            Debug.DrawRay(_ctx.transform.position, Vector3.up * 2, Color.red);

            if (CheckSwitchStates())
                return;

            CheckRepath();

            // Отходим назад
            
                float weaponRange = _ctx.CurrentActivity.actWith.GetRange();

                float progress = 1 - (Vector3.Distance(_ctx.CurrentActivity.target.position, _ctx.transform.position)
                    / weaponRange);

                if (progress > 0)
                {
                    RetreatReposition(progress);
                }
                else
                {
                    _ctx.MovingAgent.MoveIteration(moveTargetPos, _ctx.CurrentActivity.target.position);
                }
            

            _ctx.AttackUpdate(_ctx.CurrentActivity.target);
        }
        public override void FixedUpdateState()
        {

        }

        public override string ToString()
        {
            return "Attacking";
        }

        private void RetreatReposition(float retreatCurveTime)
        {
            Vector3 newPos = _ctx.transform.position - _ctx.retreatInfluence.Evaluate(retreatCurveTime)
                    * (_ctx.CurrentActivity.target.position - _ctx.transform.position);

            if (_ctx.MovingAgent.IsNearObstacle(newPos - _ctx.transform.position, out Vector3 normal))
            {
                Vector3 dir = Vector3.ProjectOnPlane(
                    (_ctx.CurrentActivity.target.position - _ctx.transform.position).normalized,
                    normal);
                dir.Normalize();

                newPos = _ctx.transform.position - dir
                    * (_ctx.CurrentActivity.target.position - _ctx.transform.position).magnitude;
            }

            _ctx.MovingAgent.MoveIteration(newPos, _ctx.CurrentActivity.target.position);
        }
    }
}