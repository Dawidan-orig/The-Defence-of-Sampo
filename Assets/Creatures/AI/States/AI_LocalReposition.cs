using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_LocalReposition : UtilityAI_BaseState
// Перемещение ИИ с помощью Navmesh, но на короткие расстояния,
// С применением параллельных систем.
// Пример использования: Перемещение со стрельбой на ходу.
{
    public AI_LocalReposition(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
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
        return "Local moving";
    }
}
