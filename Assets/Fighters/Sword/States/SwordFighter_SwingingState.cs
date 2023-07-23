using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SwordFighter_SwingingState : SwordFighter_BaseState
{

    public SwordFighter_SwingingState(SwordFighter_StateMachine currentContext, SwordFighter_StateFactory factory)
        : base(currentContext, factory) { }

    public override void EnterState()
    {
        _ctx.Blade.OnBladeCollision += BladeCollisionEnter;
    }

    public override void ExitState()
    {
        _ctx.Blade.OnBladeCollision -= BladeCollisionEnter;
    }

    public override void FixedUpdateState()
    {
        ProcessSwingSword();
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
        if (_ctx.CloseToDesire())
        {
            _ctx.NullifyProgress();
            SwitchStates(_factory.Repositioning());
        }
    }

    private void ProcessSwingSword()
    {
        float heightFrom = _ctx.MoveFrom.position.y;
        float heightTo = _ctx.DesireBlade.position.y;

        Vector3 from = new Vector3(_ctx.MoveFrom.position.x, 0, _ctx.MoveFrom.position.z);
        Vector3 to = new Vector3(_ctx.DesireBlade.position.x, 0, _ctx.DesireBlade.position.z);

        _ctx.BladeHandle.position = Vector3.Slerp(from, to, _ctx.MoveProgress) + new Vector3(0, Mathf.Lerp(heightFrom, heightTo, _ctx.MoveProgress), 0);

        _ctx.BladeHandle.LookAt(_ctx.BladeHandle.position + (_ctx.BladeHandle.position - _ctx.Vital.bounds.center).normalized, (_ctx.DesireBlade.position - _ctx.BladeHandle.position).normalized);
        _ctx.BladeHandle.RotateAround(_ctx.BladeHandle.position, _ctx.BladeHandle.right, 90);
    }

    private void BladeCollisionEnter(object sender, Collision collision)
    {
        if ((!collision.gameObject.TryGetComponent<Rigidbody>(out _)) ||
            collision.gameObject.TryGetComponent<Blade>(out _))
        {
            _ctx.SetDesires(_ctx.InitialBlade.position, _ctx.InitialBlade.up, _ctx.InitialBlade.forward);
            _ctx.NullifyProgress();
            _ctx.CurrentToInitialAwait = _ctx.toInitialAwait;

            SwitchStates(_factory.Repositioning());
            return;
        }

        //TODO : Raycast для хорошей коллизии; Пули не отбиваются - слишком маленькие. Но с Continious Detection сбиваются нормально.

        //TODO : При отбивании меча блоком ->
        // Это отбивание происходит не мгновенно, первые несколько кадров после отбивания летящий меч колбасит.
        // Вариант: Сделать время "Контузии" после столкновения, при котором не происходит никакой реакции (Incoming игнорируется)
    }

    public override string ToString()
    {
        return "Swinging";
    }
}
