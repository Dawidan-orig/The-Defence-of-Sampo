using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Action : UtilityAI_BaseState
// Будь то лечение или наложение баффа - в этом состоянии ИИ что-то делает со своими союзниками или с собой, в зависимости от цели.
{
    public AI_Action(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
    {
    }

    public override void CheckSwitchStates()
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
