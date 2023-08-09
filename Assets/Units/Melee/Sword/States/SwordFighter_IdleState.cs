using System;
using UnityEngine;

[Serializable]
public class SwordFighter_IdleState : SwordFighter_BaseState
{
    public SwordFighter_IdleState(SwordFighter_StateMachine currentContext, SwordFighter_StateFactory factory)
        : base(currentContext, factory) { }

    public override void CheckSwitchStates()
    {
        if (_ctx.CurrentActivity.target != null)
            if (_ctx.SwingReady && _ctx.ActionReachable())
            {
                Vector3 toPoint = _ctx.CurrentActivity.target.position;

                Vector3 bladeCenter = Vector3.Lerp(_ctx.Blade.upperPoint.position, _ctx.Blade.downerPoint.position, 0.5f);

                Plane transformXY = new Plane(_ctx.transform.forward, _ctx.transform.position);
                Vector3 toNewPosDir = (transformXY.ClosestPointOnPlane(_ctx.BladeHandle.position) - _ctx.transform.position).normalized;

                Vector3 newPos = _ctx.Vital.ClosestPointOnBounds(toNewPosDir * _ctx.swing_startDistance) + toNewPosDir * _ctx.swing_startDistance;
                _ctx.SetDesires(newPos,
                       (newPos - _ctx.Vital.bounds.center).normalized,
                       (toPoint - _ctx.BladeHandle.position).normalized);

                _ctx.NullifyProgress();
                SwitchStates(_factory.Repositioning());

                _ctx.AttackReposition = true;
                return;
            }

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
