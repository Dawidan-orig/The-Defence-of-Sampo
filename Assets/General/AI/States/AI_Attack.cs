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
        Vector3 closestToMe;
        if (_ctx.CurrentActivity.data.target.TryGetComponent<Collider>(out var c))
            closestToMe = c.ClosestPointOnBounds(_ctx.transform.position);
        else
            closestToMe = _ctx.CurrentActivity.data.target.position;


        if (Vector3.Distance(_ctx.transform.position, closestToMe) > _ctx.CurrentActivity.actDistance)
        {
            SwitchStates(_factory.Reposition());
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
        _ctx.AttackUpdate(_ctx.CurrentActivity.data.target);

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
