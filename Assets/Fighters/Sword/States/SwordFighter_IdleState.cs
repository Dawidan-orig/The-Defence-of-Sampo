using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SwordFighter_IdleState : SwordFighter_BaseState
{
    public SwordFighter_IdleState(SwordFighter_StateMachine currentContext, SwordFighter_StateFactory factory)
        : base(currentContext, factory) { }

    Rigidbody _lastIncoming = null;

    public override void CheckSwitchStates()
    {
        if(_ctx.AttackRecharge >= _ctx.minimalTimeBetweenAttacks && _ctx.Enemy != null) 
        {
            Vector3 toPoint = _ctx.Enemy.transform.position;

            Vector3 bladeCenter = Vector3.Lerp(_ctx.Blade.upperPoint.position, _ctx.Blade.downerPoint.position, 0.5f);

            Plane transformXY = new Plane(_ctx.transform.forward, _ctx.transform.position);
            Vector3 toNewPosDir = (transformXY.ClosestPointOnPlane(_ctx.BladeHandle.position) - _ctx.transform.position).normalized;

            _ctx.SetDesires(_ctx.Vital.ClosestPointOnBounds(toNewPosDir * _ctx.swing_startDistance) + toNewPosDir*_ctx.swing_startDistance,
                   (bladeCenter - _ctx.Vital.bounds.center).normalized,
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
        _ctx.AttackCatcher.OnIncomingAttack += Incoming;
    }

    public override void ExitState()
    {
        _ctx.AttackCatcher.OnIncomingAttack -= Incoming;
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

    private void Incoming(object sender, AttackCatcher.AttackEventArgs e)
    {
        Rigidbody currentIncoming = e.body;
        _ctx.CurrentToInitialAwait = 0;

        if (e.free)
        {
            if (e.impulse < _ctx.criticalImpulse)
            {
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
                else 
                {
                    _ctx.SetDesires(toPoint + (bladeCenter - toPoint).normalized * _ctx.swing_startDistance,
                    (bladeCenter - _ctx.Vital.bounds.center).normalized,
                    (toPoint - _ctx.BladeHandle.position).normalized);                    
                    _ctx.NullifyProgress();
                    SwitchStates(_factory.Repositioning());
                }
            }
            else
            {
                //TODO : Evade() -- Должен быть в другом скрипте, тут - только вызов
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

            int ignored = _ctx.Blade.gameObject.layer; // Для игнора лезвий при проверке.
            ignored = ~ignored;

            BoxCollider bladeCollider = _ctx.Blade.GetComponent<BoxCollider>();
            Vector3 bladeHalfWidthLength = new Vector3((bladeCollider.size.x * bladeCollider.transform.lossyScale.x) / 2, 0.1f, (bladeCollider.size.z * bladeCollider.transform.lossyScale.z) / 2);

            if (Utilities.VisualisedBoxCast(bladeDown,
                bladeHalfWidthLength,
                (bladeUp - bladeDown).normalized,
                out _,
                Quaternion.FromToRotation(Vector3.up, (bladeUp - bladeDown).normalized),
                (bladeDown - bladeUp).magnitude,
                ignored,
                true,
                new Color(0.5f, 0.5f, 1f, 0.6f))
                ||
                Utilities.VisualisedBoxCast(bladeUp,
                bladeHalfWidthLength,
                (bladeDown - bladeUp).normalized,
                out _,
                Quaternion.FromToRotation(Vector3.up, (bladeDown - bladeUp).normalized),
                (bladeDown - bladeUp).magnitude,
                ignored,
                true,
                new Color(0.5f, 0.5f, 1f, 0.6f)))
            {
                return;
            }

            UnityEngine.Object.Destroy(bladePrediction);

            //IDEA : Усложнение, которое сделает лучше.
            // Сейчас очень много предсказаний аннулируются из-за коллизий. Есть альтернативное решение: Подбирать при коллизии ближайшие точки от меча до коллайдера такие,
            // Что вот буквально ещё шаг - и уже будет столкновение.

            _ctx.Block(bladeDown, bladeUp, toEnemyBlade_Dir);
            //if ((currentIncoming != _lastIncoming)) // Если новый объект летит - обновляем движение
                _ctx.NullifyProgress();

            SwitchStates(_factory.Repositioning());
        }

        _lastIncoming = currentIncoming;
    }

    public override string ToString()
    {
        return "Idle";
    }
}
