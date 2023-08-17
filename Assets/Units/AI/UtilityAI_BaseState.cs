using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class UtilityAI_BaseState
{
    protected TargetingUtilityAI _ctx;
    protected UtilityAI_Factory _factory;
    protected UtilityAI_BaseState _currentSubState;
    protected UtilityAI_BaseState _currentSuperState;
    public UtilityAI_BaseState(TargetingUtilityAI currentContext, UtilityAI_Factory factory)
    {
        _ctx = currentContext;
        _factory = factory;
    }

    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void FixedUpdateState();

    public abstract void ExitState();

    public abstract bool CheckSwitchStates();

    public abstract void InitializeSubState();


    protected void SwitchStates(UtilityAI_BaseState newState)
    {
        ExitState();
        newState.EnterState();
        _ctx.CurrentState = newState;
    }

    protected void SetSuperState(UtilityAI_BaseState newSuperState)
    {
        _currentSuperState = newSuperState;
    }

    protected void SetSubState(UtilityAI_BaseState newSubState)
    {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }
    
    public void ForceDecideState() 
    {
        SwitchStates(_factory.Deciding());
    }
}