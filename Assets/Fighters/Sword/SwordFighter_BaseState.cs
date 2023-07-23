using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class SwordFighter_BaseState
{
    protected SwordFighter_StateMachine _ctx;
    protected SwordFighter_StateFactory _factory;
    protected SwordFighter_BaseState _currentSubState;
    protected SwordFighter_BaseState _currentSuperState;
    public SwordFighter_BaseState(SwordFighter_StateMachine currentContext, SwordFighter_StateFactory factory)
    {
        _ctx = currentContext;
        _factory = factory;
    }

    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void FixedUpdateState();

    public abstract void ExitState();

    public abstract void CheckSwitchStates();

    public abstract void InitializeSubState();

    void UpdateStates() { }

    protected void SwitchStates(SwordFighter_BaseState newState) {
        ExitState();
        newState.EnterState();
        _ctx.CurrentState = newState;
    }

    protected void SetSuperState(SwordFighter_BaseState newSuperState)
    {
        _currentSuperState = newSuperState; 
    }

    protected void SetSubState(SwordFighter_BaseState newSubState) 
    {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }
}
