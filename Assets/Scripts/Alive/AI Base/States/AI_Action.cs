using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI
{
    /// <summary>
    /// � ���� ��������� �� �������� � �����, ������ ���������� ��� �� ����������
    /// </summary>
    public class AI_Action : UtilityAI_BaseState
    {
        //TODO : ������� Attack � ��� ���������, ����������� ���� �������
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

            /* TODO : ���� ��������� ��������� ������:
             * ������� � ������ ���������. ������ �����, �� ������ ����������� ����������� ���������� Action.
             * ���� ���� �� Interact, �� ���������� ��������� ���������� ����� ���� ��.
             * �� ��� ������ � ������ ������ � ���� �� �������?
             * ���� ����� �������� ������ AIAction, � ���� �������� ��� ���������.
             * ��������, ���������� Action � �������� ��� �� ���������� ������.
             * 
             * � ���� �� Action ��� ����� ���� ����� ��������.
             * ��������, ������� �������� ��������� �� ����.
            */

            // ������ �������: ������� ���� � ��� ����� ���� �� Interact.
            // ��������, ��� �� � ������ ������� (���� ������� ��� - ����� ������. ������� Attack)
            // ������ ����� ���-�� ����� ����� ������� Interact � ������� Attack.
            // ���������� ���������� ��� ������ ������������.
            // � ������ - ��� ��������� ��
            // ����� ����� ��������� ������� ����������� � ������� ��������, � ����� �������� �������.
            // ��, ��� �����. ��� ��������� ��� ��������� �����������

            MoveAlongPath(10);

            _ctx.ActionUpdate(_ctx.CurrentActivity.target);
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
    }
}