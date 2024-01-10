using UnityEngine;
using UnityEngine.AI;

namespace Sampo.AI
{
    public class AI_LongReposition : UtilityAI_BaseState
    // ИИ двигается в какую-то точку с помощью NavMesh, при это не делая более ничего
    {        
        public AI_LongReposition(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
        {

        }

        public override bool CheckSwitchStates()
        {
            if (_ctx.DecidingStateRequired() || _ctx.CurrentActivity.target == null)
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
                if (shooting.AvilableToShoot(_ctx.CurrentActivity.target, out _))
                {
                    SwitchStates(_factory.Deciding());
                    return true;
                }

                return false;
            }

            if (_ctx.MeleeReachable())
            {
                SwitchStates(_factory.Deciding());
                return true;
            }

            return false;
        }

        public override void EnterState()
        {
            Rigidbody body = _ctx.GetComponent<Rigidbody>();

            if (_ctx.NMAgent)
            {
                body.isKinematic = true;
                _ctx.NMAgent.enabled = true;
            }
            base.EnterState();
        }

        public override void ExitState()
        {
            if (_ctx.NMAgent)
            {
                _ctx.NMAgent.enabled = false;
                _ctx.GetComponent<Rigidbody>().isKinematic = false;
            }
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