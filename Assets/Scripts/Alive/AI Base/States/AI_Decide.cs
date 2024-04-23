using UnityEngine;

namespace Sampo.AI
{
    /// <summary>
    /// ���������-��������������. ������� ����, �� ������, ��� ��� ������ ������.
    /// </summary>
    public class AI_Decide : UtilityAI_BaseState
        //���������, � ������� �� ������������� � ������ ����� ������.
        //TODO : ����� �������������� ���� �����, �� ����� �� �����.
        //����� ����� ������ ����������� ��������, ��� ������ ��������
    {
        public AI_Decide(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
        {
        }

        public override bool CheckSwitchStates()
        {
            bool hasAction = _ctx.SelectBestActivityIfAny();

            if (hasAction)
                SwitchStates(_factory.Action());

            return hasAction;

        }

        public override void EnterState()
        {

        }

        public override void ExitState()
        {

        }

        public override void InitializeSubState()
        {

        }

        public override void UpdateState()
        {
            Debug.DrawRay(_ctx.transform.position, Vector3.up * 2, Color.black);

            CheckSwitchStates();
        }
        public override void FixedUpdateState()
        {

        }

        public override string ToString()
        {
            return "Thinking";
        }
    }
}