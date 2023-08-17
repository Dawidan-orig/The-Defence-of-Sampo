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

    public override bool CheckSwitchStates()
    {
        if (_ctx.DecidingStateRequired())
        {
            SwitchStates(_factory.Deciding());
            return true;
        }

        Tool weapon = _ctx.CurrentActivity.actWith;

        if (weapon is SimplestShooting)
        {
            if (!((SimplestShooting)weapon).AvilableToShoot(_ctx.CurrentActivity.target, out RaycastHit hit))
            {
                if (hit.rigidbody)
                {
                    Vector3 targetDirection = (_ctx.CurrentActivity.target.position - _ctx.transform.position).normalized;
                    Vector3 strafePoint = Utilities.NearestPointOnLine(_ctx.transform.position, targetDirection, hit.point);
                    Vector3 strafeDir = (strafePoint - hit.point).normalized;
                    Debug.DrawRay(_ctx.transform.position, strafeDir);
                    //_ctx.Body.AddForce(strafeDir * _ctx.retreatSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
                else
                {
                    SwitchStates(_factory.Deciding());
                    return true;
                }
            }

            return false;
        }

        if (!_ctx.NavMeshMeleeReachable())
        {
            SwitchStates(_factory.Deciding());
            return true;
        }

        return false;
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

        if (CheckSwitchStates())
            return;

        _ctx.AttackUpdate(_ctx.CurrentActivity.target);
    }
    public override void FixedUpdateState()
    {

    }

    public override string ToString()
    {
        return "Attacking";
    }
}
