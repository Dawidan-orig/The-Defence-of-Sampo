using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI
{
    public class AI_Action : UtilityAI_BaseState
    // ���� �� ������� ��� ��������� ����� - � ���� ��������� �� ���-�� ������ �� ������ ���������� ��� � �����, � ����������� �� ����.
    {
        //TODO : ��������� Dependency Injection, ����� ��� � �� �����������.
        //TODO : ������� Attack � ��� ���������
        //TODO : ����� ��� �� ������� ����� Interact()
        public AI_Action(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
        {
        }

        public override bool CheckSwitchStates()
        {
            throw new System.NotImplementedException();
        }

        public override void EnterState()
        {
            throw new System.NotImplementedException();
        }

        public override void ExitState()
        {
            throw new System.NotImplementedException();
        }

        public override void FixedUpdateState()
        {
            throw new System.NotImplementedException();
        }

        public override void InitializeSubState()
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateState()
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return "Action";
        }
    }
}