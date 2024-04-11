using Sampo.AI.Conditions;
using Sampo.Core;
using Sampo.Weaponry.Ranged;
using UnityEngine;
using UnityEngine.AI;

namespace Sampo.AI
{
    public abstract class UtilityAI_BaseState
    {
        protected TargetingUtilityAI _ctx { get; private set; }
        protected UtilityAI_Factory _factory { get; private set; }

        protected NavMeshPath path;
        protected Vector3 moveTargetPos { get; private set; }
        private Vector3 repathLastUnitPos;
        private Vector3 repathLastTargetPos;
        private const float RECALC_DIFF = 3;

        public UtilityAI_BaseState(TargetingUtilityAI currentContext, UtilityAI_Factory factory)
        {
            _ctx = currentContext;
            _factory = factory;
        }

        public virtual void EnterState()
        {
            CheckRepath();
        }

        public virtual void ExitState() { }

        public abstract void UpdateState();

        public abstract void FixedUpdateState();

        public abstract bool CheckSwitchStates();

        public abstract void InitializeSubState();

        protected void SwitchStates(UtilityAI_BaseState newState)
        {
            ExitState();
            newState.EnterState();
            _ctx.CurrentState = newState;
        }

        public void ForceDecideState()
        {
            SwitchStates(_factory.Deciding());
        }

        /// <summary>
        /// Проверяет, есть ли смысл перестраивать путь к цели
        /// </summary>
        protected void CheckRepath()
        {
            if (!Utilities.ValueInArea(repathLastTargetPos, _ctx.CurrentActivity.target.position, RECALC_DIFF))
            {
                Repath();
            }
        }
        /// <summary>
        /// Перестраивает путь к цели
        /// </summary>
        private void Repath()
        {
            Vector3 closestPos = _ctx.GetClosestPoint(_ctx.CurrentActivity.target, _ctx.navMeshCalcFrom.position);            
            moveTargetPos = closestPos;

            repathLastTargetPos = moveTargetPos;
            repathLastUnitPos = _ctx.transform.position;

            if (_ctx.CurrentActivity.actWith is BaseShooting shooting) // Ищем лучшую позицию для стрельбы
            {
                moveTargetPos = shooting.NavMeshClosestAviableToShoot(_ctx.CurrentActivity.target);
            }

            const float SEARCH_RADIUS = 100;

            if(NavMesh.SamplePosition(moveTargetPos, out var hit, SEARCH_RADIUS, NavMesh.AllAreas))
                moveTargetPos = hit.position;

            path = new NavMeshPath();
            NavMesh.CalculatePath(_ctx.navMeshCalcFrom.position, moveTargetPos, NavMesh.AllAreas, path);
            _ctx.MovingAgent.PassPath(path);

            MoveAlongPath(10);
        }

        protected void MoveAlongPath(float lookDist) 
        {
            if (path.status != NavMeshPathStatus.PathInvalid && path.corners.Length > 1)
            {
                if(Vector3.Distance(_ctx.transform.position, _ctx.CurrentActivity.target.position) > lookDist)
                    _ctx.MovingAgent.MoveIteration(path.corners[1]);
                else
                    _ctx.MovingAgent.MoveIteration(path.corners[1], _ctx.CurrentActivity.target.position);
            }
            else
            {
                _ctx.ModifyActionOf(_ctx.CurrentActivity.target, new NoPathCondition(10));                

                var closest = NavMeshCalculations.Instance.GetCell(_ctx.navMeshCalcFrom.position);
                moveTargetPos = closest.Center();
                _ctx.MovingAgent.MoveIteration(moveTargetPos);
            }
        }
    }
}