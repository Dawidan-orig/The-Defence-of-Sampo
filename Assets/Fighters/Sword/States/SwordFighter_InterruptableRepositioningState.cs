using System;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class SwordFighter_InterruptableRepositioningState : SwordFighter_BaseState
{
    public SwordFighter_InterruptableRepositioningState(SwordFighter_StateMachine currentContext, SwordFighter_StateFactory factory)
        : base(currentContext, factory) { }

    Rigidbody _lastIncoming = null;

    public override void EnterState()
    {
        _ctx.AttackCatcher.OnIncomingAttack += Incoming;
    }

    public override void ExitState()
    {
        _ctx.AttackCatcher.OnIncomingAttack -= Incoming;
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
            SwitchStates(_factory.Idle());
        }
    }

    private void Incoming(object sender, AttackCatcher.AttackEventArgs e)
    {
        Rigidbody currentIncoming = e.body;
        _ctx.CurrentToInitialAwait = 0;

        if (e.free)
        {
            if (e.impulse < _ctx.criticalImpulse)
            {
                //IDEA: Вариант сделать замах: С помощью Curve.

                Vector3 toPoint = e.start;

                Vector3 bladeCenter = Vector3.Lerp(_ctx.Blade.upperPoint.position, _ctx.Blade.downerPoint.position, 0.5f);
                float bladeCenterLen = Vector3.Distance(bladeCenter, _ctx.Blade.downerPoint.position);
                float swingDistance = bladeCenterLen + _ctx.toBladeHandle_MaxDistance;

                if (Vector3.Distance(_ctx.Vital.bounds.center, toPoint) < swingDistance)
                {
                    _ctx.Swing(toPoint);
                    _ctx.NullifyProgress();
                    SwitchStates(_factory.Swinging());
                }
            }
        }
        else
        {
            Vector3 enemyBladeCenter = Vector3.Lerp(e.start, e.end, 0.5f);

            GameObject bladePrediction = new();
            bladePrediction.transform.position = enemyBladeCenter;

            GameObject start = new();
            start.transform.position = e.start;
            start.transform.parent = bladePrediction.transform;

            GameObject end = new();
            end.transform.position = e.end;
            end.transform.parent = bladePrediction.transform;

            //TODO : Это логика рапиры, из-за чего отбиваемое оружие "отражается".
            //bladePrediction.transform.Rotate(e.direction, 90);

            // Синхронизация для параллельности vital
            //bladePrediction.transform.rotation = Quaternion.FromToRotation((end.transform.position - start.transform.position).normalized, transform.up);

            Vector3 toEnemyBlade_Dir = (bladePrediction.transform.position - _ctx.Vital.bounds.center).normalized;
            bladePrediction.transform.Rotate(toEnemyBlade_Dir, 90); // Ставим перпендикулярно

            if (e.body.GetComponent<Blade>().host != null) // Притягиваем меч максимально близко к себе.
            {
                //TODO : Заменить на handle
                //TODO : Заменить на SDF; Во время блока меч влезает внутрь тела.  
                Vector3 boundsClosest = _ctx.Vital.ClosestPointOnBounds(bladePrediction.transform.position);
                bladePrediction.transform.position = boundsClosest
                    + (bladePrediction.transform.position - boundsClosest).normalized * _ctx.block_minDistance;
            }

            Vector3 bladeDown = start.transform.position;
            Vector3 bladeUp = end.transform.position;

            UnityEngine.Object.Destroy(bladePrediction);
            int ignored = _ctx.Blade.gameObject.layer; // Для игнора лезвий при проверке.
            ignored = ~ignored;

            //TODO : Поменять на BoxCast'ы
            if (Physics.Raycast(bladeDown, bladeUp - bladeDown, (bladeDown - bladeUp).magnitude, ignored) // Снизу вверх
                ||
                Physics.Raycast(bladeUp, bladeDown - bladeUp, (bladeDown - bladeUp).magnitude, ignored) // Сверху вниз
                )
            {
                //Debug.DrawLine(bladeUp,bladeDown,Color.gray, 2);
                return;
            }

            //IDEA : Усложнение, которое сделает лучше.
            // Сейчас очень много предсказаний аннулируются из-за коллизий. Есть альтернативное решение: Подбирать при коллизии ближайшие точки от меча до коллайдера такие,
            // Что вот буквально ещё шаг - и уже будет столкновение.

            _ctx.Block(bladeDown, bladeUp, toEnemyBlade_Dir);
            if ((currentIncoming != _lastIncoming)) // Если новый объект летит - обновляем движение
                _ctx.NullifyProgress();

            SwitchStates(_factory.Repositioning());
        }

        _lastIncoming = currentIncoming;
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

        // На будущее:
        //TODO : Переставлять desire, когда на пути до него есть препятствия
        //TODO : Добавить событий на начало движения меча, состояние в прогрессе и конец движения.
    }

    public override string ToString()
    {
        return "reposition";
    }
}
