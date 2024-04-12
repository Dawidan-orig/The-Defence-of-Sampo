using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI
{
    /// <summary>
    /// В этом состоянии ИИ работает с собой, своими союзниками или со строениями
    /// </summary>
    public class AI_Action : UtilityAI_BaseState
    {
        //TODO : Перевод Attack в это состояние, объединение двух классов
        //TODO : Пусть тут по дефолту будет Interact()
        public AI_Action(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
        {
        }

        public override bool CheckSwitchStates()
        {
            if (_ctx.IsDecidingStateRequired()
                || _ctx.CurrentActivity.target == null
                || path.status == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
            {
                SwitchStates(_factory.Deciding());
                return true;
            }

            return false;
        }

        public override void UpdateState()
        {
            Debug.DrawRay(_ctx.transform.position, Vector3.up * 2, Color.yellow);

            if (CheckSwitchStates())
                return;

            CheckRepath();

            /* TODO : Надо придумать следующее теперь:
             * Переход в другое состояние. Прежде всего, он должен происходить посредством завершения Action.
             * Если речь об Interact, то достаточно проверять расстояние через него же.
             * Но что делать в случае оружий и того же лечения?
             * Есть смысл обновить логику AIAction, в него добавить эти параметры.
             * Например, завершение Action и удаление его из локального списка.
             * 
             * В этот же Action ещё много чего можно спрятать.
             * Например, функцию проверки дальности до цели.
            */

            // Способ решения: Сделать пока в лоб через этот же Interact.
            // Подумать, Как бы я сделал медиков (Хотя понятно как - через оружие. Копирка Attack)
            // Дальше найти что-то общее между здешним Interact и текущим Attack.
            // Объеденить функционал без потери возможностей.
            // В идеале - ещё увеличить их
            // Тогда будет состояние чистого перемещения и чистого действия, а также принятия решения.
            // То, что нужно. Оно покрывает все возможные вероятности

            MoveAlongPath(10);

            _ctx.ActionUpdate(_ctx.CurrentActivity.target);
        }

        public override string ToString()
        {
            return "Performing action";
        }

        public override void FixedUpdateState()
        {
            
        }

        public override void InitializeSubState()
        {
            
        }
    }
}