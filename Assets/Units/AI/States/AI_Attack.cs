using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Attack : UtilityAI_BaseState
// ИИ двигается и атакует в этом состоянии.
{
    public AI_Attack(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
    {
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.CurrentActivity.actWith is SimplestShooting)
        {
            if (!((SimplestShooting)_ctx.CurrentActivity.actWith).AvilableToShoot(_ctx.CurrentActivity.target))
                SwitchStates(_factory.Deciding());

            return;
        }

        if (!_ctx.ActionReachable(_ctx.CurrentActivity.actWith.additionalMeleeReach + _ctx.baseReachDistance))
        {
            SwitchStates(_factory.Deciding());
            return;
        }
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
        Debug.DrawRay(_ctx.transform.position, Vector3.up * 2, Color.red);

        _ctx.AttackUpdate(_ctx.CurrentActivity.target);

        CheckSwitchStates();
    }
    public override void FixedUpdateState()
    {

    }

    public override string ToString()
    {
        return "Attacking";
    }
}
