using UnityEngine;
using UnityEngine.AI;

public class AI_LongReposition : UtilityAI_BaseState
// ИИ двигается в какую-то точку с помощью NavMesh, при это не делая более ничего
{
    public AI_LongReposition(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
    {

    }

    public override bool CheckSwitchStates()
    {
        if (_ctx.DecidingStateRequired())
        {
            SwitchStates(_factory.Deciding());
            return true;
        }

        if (_ctx.NMAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            SwitchStates(_factory.Deciding());
            return true;
        }

        if (_ctx.CurrentActivity.actWith is SimplestShooting)
        {
            if (((SimplestShooting)_ctx.CurrentActivity.actWith).AvilableToShoot(_ctx.CurrentActivity.target, out _))
            {
                SwitchStates(_factory.Attack());
                return true;
            }

            return false;
        }

        if (_ctx.MeleeReachable())
        {
            SwitchStates(_ctx.CurrentActivity.whatDoWhenClose); // Как дошли - выполняем указанное действие
            return true;
        }

        return false;
    }

    public override void EnterState()
    {
        _ctx.GetComponent<Rigidbody>().isKinematic = true;

        _ctx.NMAgent.enabled = true;
        Repath();
    }

    public override void ExitState()
    {
        _ctx.NMAgent.enabled = false;
        _ctx.GetComponent<Rigidbody>().isKinematic = false;
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

        if (_ctx.CurrentActivity.target.hasChanged)
            Repath();
    }

    private void Repath()
    {
        _ctx.NMAgent.path.ClearCorners();
        _ctx.NMAgent.SetDestination(_ctx.CurrentActivity.target.position);
    }

    public override string ToString()
    {
        return "Moving";
    }
}
