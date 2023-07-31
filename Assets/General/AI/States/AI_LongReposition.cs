using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_LongReposition : UtilityAI_BaseState
// ИИ двигается в какую-то точку с помощью NavMesh
{
    public AI_LongReposition(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
    {

    }

    public override void CheckSwitchStates()
    {
        if (_ctx.NMAgent.remainingDistance < _ctx.CurrentActivity.actDistance)
        { 
            SwitchStates(_ctx.CurrentActivity.whatDoWhenClose); // Как дошли - выполняем указанное действие
        }
    }

    public override void EnterState()
    {
        _ctx.GetComponent<Rigidbody>().isKinematic = true;

        _ctx.NMAgent.enabled = true;
        _ctx.NMAgent.path.ClearCorners();
        NavMeshHit destination;
        _ctx.NMAgent.Raycast(_ctx.CurrentActivity.data.target.position, out destination);
        _ctx.NMAgent.SetDestination(destination.position);
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
        CheckSwitchStates();
    }

    public override string ToString()
    {
        return "Moving";
    }
}
