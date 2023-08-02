using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class AI_Decide : UtilityAI_BaseState
// Если ИИ попал в патовую ситуацию, столкнулся с какой-то ошибкой или ещё по каким-то экстраординарным причинам не выполнил задачу -
// Он попадает в это состояние.
{
    public AI_Decide(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
    {
    }

    public override void CheckSwitchStates()
    {
        UtilityAI_BaseState newAcitivty = _ctx.SelectBestActivity();

        if(newAcitivty == null)
        {
            // Задач нет ВООБЩЕ.
            return;
        }

        switch(newAcitivty) 
        {
            case AI_LongReposition:
                SwitchStates(_factory.Reposition());
                break;
            default:
                // Проверяем, что мы далеко:
                NavMeshHit destination;
                NavMesh.Raycast(_ctx.transform.position, _ctx.CurrentActivity.data.target.position, out destination, NavMesh.AllAreas);
                NavMeshPath path = new NavMeshPath();
                NavMesh.CalculatePath(_ctx.transform.position, destination.position, NavMesh.AllAreas, path);
                if (Utilities.NavMeshPathLength(path) > _ctx.CurrentActivity.actWith.additionalMeleeReach + _ctx.baseReachDistance) {
                    SwitchStates(_factory.Reposition());
                    break;
                }
                // Если же близко: Меняемся на указанное состояние.
                SwitchStates(newAcitivty);
                break;
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
