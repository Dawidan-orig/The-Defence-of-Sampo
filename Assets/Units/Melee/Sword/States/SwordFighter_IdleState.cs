using System;
using UnityEngine;

[Serializable]
public class SwordFighter_IdleState : SwordFighter_BaseState
{
    public SwordFighter_IdleState(SwordFighter_StateMachine currentContext, SwordFighter_StateFactory factory)
        : base(currentContext, factory) { }

    public override void CheckSwitchStates()
    {
        if (_ctx.CurrentToInitialAwait < _ctx.toInitialAwait)
            _ctx.CurrentToInitialAwait += Time.deltaTime;
        else
        {
            if (_ctx.InitialBlade.position != _ctx.DesireBlade.position
                && _ctx.InitialBlade.up != _ctx.DesireBlade.up)
            {
                _ctx.SetDesires(_ctx.InitialBlade.position, _ctx.InitialBlade.up, _ctx.InitialBlade.forward);
                _ctx.NullifyProgress();
                _ctx.CurrentToInitialAwait = _ctx.toInitialAwait;

                SwitchStates(_factory.Repositioning());
            }
        }
    }

    public override void EnterState()
    {
        _ctx.OnRepositionIncoming += IncomingForRepos;
        _ctx.OnSwingIncoming += IncomingForSwing;
    }

    public override void ExitState()
    {
        _ctx.OnRepositionIncoming -= IncomingForRepos;
        _ctx.OnSwingIncoming -= IncomingForSwing;
    }

    public override void FixedUpdateState()
    {

    }

    public override void InitializeSubState()
    {

    }

    public override void UpdateState()
    {
        if (_ctx.CurrentCombo.Count > 0)
        {
            HandleCombo();
        }

        CheckSwitchStates();
    }

    private void IncomingForSwing(object sender, SwordFighter_StateMachine.IncomingSwingEventArgs e)
    {
        _ctx.Swing(e.toPoint);
        _ctx.NullifyProgress();
        SwitchStates(_factory.Swinging());
    }

    private void IncomingForRepos(object sender, SwordFighter_StateMachine.IncomingReposEventArgs e)
    {
        _ctx.Block(e.bladeDown, e.bladeUp, e.bladeDir);
        _ctx.NullifyProgress();
        SwitchStates(_factory.Repositioning());
    }

    public override string ToString()
    {
        return "Idle";
    }
}
