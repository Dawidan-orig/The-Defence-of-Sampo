using System;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class SwordFighter_RepositioningState : SwordFighter_BaseState
{
    public SwordFighter_RepositioningState(SwordFighter_StateMachine currentContext, SwordFighter_StateFactory factory)
        : base(currentContext, factory) { }

    public override void EnterState()
    {
        _ctx.OnRepositionIncoming += IncomingForRepos;
        _ctx.OnSwingIncoming += IncomingForSwing;
        MoveSword();
    }

    public override void ExitState()
    {
        _ctx.OnRepositionIncoming -= IncomingForRepos;
        _ctx.OnSwingIncoming -= IncomingForSwing;
    }

    public override void FixedUpdateState()
    {
        MoveSword();
    }

    public override void InitializeSubState()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.AlmostDesire()) 
        {
            if (_ctx.AttackReposition)
            {
                _ctx.Swing(_ctx.CurrentActivity.target.position);
                _ctx.NullifyProgress();
                SwitchStates(_factory.Swinging());
                _ctx.AttackReposition = false;
                return;
            }

            SwitchStates(_factory.Idle());
        }
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
        MoveSword();
        _ctx.NullifyProgress();
    }

    private void MoveSword()
    {
        float heightFrom = _ctx.MoveFrom.position.y;
        float heightTo = _ctx.DesireBlade.position.y;

        Vector3 from = new Vector3(_ctx.MoveFrom.position.x, 0, _ctx.MoveFrom.position.z);
        Vector3 to = new Vector3(_ctx.DesireBlade.position.x, 0, _ctx.DesireBlade.position.z);

        _ctx.BladeHandle.position = Vector3.Slerp(from, to, _ctx.MoveProgress) + new Vector3(0, Mathf.Lerp(heightFrom, heightTo, _ctx.MoveProgress), 0);

        #region rotationControl;
        GameObject go = new();
        Transform probe = go.transform;
        probe.position = _ctx.MoveFrom.position;
        probe.rotation = _ctx.DesireBlade.rotation;
        probe.parent = null;

        _ctx.BladeHandle.rotation = Quaternion.Lerp(_ctx.MoveFrom.rotation, probe.rotation, _ctx.MoveProgress);

        UnityEngine.Object.Destroy(go);
        #endregion

        /*
        Vector3 closestPos = vital.ClosestPointOnBounds(bladeHandle.position);
        const float TWO_DIVIDE_THREE = 2/3;
        
        if(moveProgress < TWO_DIVIDE_THREE) // Притягиваем максимально близко
        {
            desireBlade.position = closestPos + (desireBlade.position - closestPos).normalized * bladeMinDistance;

            GameObject upDirectioner = new();
            Vector3 toNearest = closestPos - desireBlade.position;
            upDirectioner.transform.up = toNearest;
            upDirectioner.transform.Rotate(0,0,90);
            desireBlade.up = upDirectioner.transform.up;
            Destroy(upDirectioner);
        }
        */
    }

    public override string ToString()
    {
        return "reposition";
    }
}
