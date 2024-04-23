using Sampo.Core;
using UnityEngine;

namespace Sampo.AI
{
    /// <summary>
    /// � ���� ��������� �� �������� � �����, ������ ���������� ��� �� ����������
    /// </summary>
    public class AI_Action : UtilityAI_BaseState
    {
        //TODO : ����� ��� �� ������� ����� Interact()
        public AI_Action(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
        {
        }

        public override bool CheckSwitchStates()
        {
            if (_ctx.IsDecidingStateRequired()
                || _ctx.CurrentActivity.target == null
                || path.status == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
            {
                SwitchStates(_factory.Deciding());
                return true;
            }

            return false;
        }

        public override void UpdateState()
        {
            Debug.DrawRay(_ctx.transform.position, Vector3.up * 2, Color.yellow);

            if (CheckSwitchStates())
                return;

            CheckRepath();

            float weaponRange = 1;
            if (_ctx.CurrentActivity.behaviour.BehaviourWeapon == null
                && _ctx.CurrentActivity.target.TryGetComponent(out IInteractable interact))
            {
                weaponRange = interact.GetInteractionRange();

                if (Vector3.Distance(_ctx.transform.position, _ctx.CurrentActivity.target.position)
                    < interact.GetInteractionRange())
                {
                    //TODO!!! : ��� ������ ������-�� � MultiweaponUnit
                    // �������� Logger'� ��� ���� ������ ���, �������!!
                    interact.Interact(_ctx.transform);
                    _ctx.MarkCurrentActionAsDone();
                    return;
                }
            }
            else if(_ctx.CurrentActivity.behaviour.BehaviourWeapon != null)
            {
                weaponRange = _ctx.CurrentActivity.behaviour.BehaviourWeapon.GetRange();
            }

            Vector3 closest = _ctx.BehaviourAI.GetClosestPoint(_ctx.CurrentActivity.target, _ctx.transform.position);

            float progress = 1 - (Vector3.Distance(closest, _ctx.transform.position)
                / weaponRange);

            if (progress > 0)
                RetreatReposition(progress);
            else
                MoveAlongPath(10);

            //TODO DESIGN : �����������. ��� ����� ��������� ���������, �� �� AIBehaviour.
            // ��� ��� ����������, ����� � ��� ����������� � ��� �� �����.
            // ��� ��������� Action'� � TargetingUtilityAI.

            // ��� ������ Action �������� � ���� AIBehaviour.
            // ������ ��� ����������� ������ AIBehaviour ��� ������������.
            // ������ ��� ����� ������ ������������ ������ �������������� ������ ���� Kit � AIBehaviour.
            // � �� ����� ������ ���� ����� ������������.
            // MultiweaponUnit ��� ������� �� ������ �� ���������� �����,
            // ������� ��������� ����������� ��� ������ �� ������������, � GetCurrentWeaponPoints() ������������ ��������
            // ��� ������ ����� ����������� ��� ActionUpdate - ����� ������������ ����������� �� ����� ���������� �� ����.
            // ����������� ����� ���� ���������� �������, �� ������� �� ����������,
            // � ������ ���� ����� ��� ������ ����� �������� �������� ����������.

            _ctx.BehaviourAI.ActionUpdate(_ctx.CurrentActivity.target);
        }

        public override string ToString()
        {
            return "Performing action";
        }

        public override void FixedUpdateState()
        {

        }

        public override void InitializeSubState()
        {

        }
        private void RetreatReposition(float retreatCurveTime)
        {
            //TODO : ������������� _ctx.BehaviourAI.RelativeRetreatMovement
            Vector3 newPos = _ctx.transform.position - _ctx.BehaviourAI.retreatInfluence.Evaluate(retreatCurveTime)
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