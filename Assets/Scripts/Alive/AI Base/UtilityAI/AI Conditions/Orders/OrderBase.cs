using Sampo.AI.Conditions;
using Sampo.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI.Conditions.Orders
{
    /// <summary>
    /// Абстрактная база для всех приказов.
    /// Контроллирует Condition (Условие) у ИИ с учётом данных извне,
    /// Модифицируется на ходу
    /// </summary>
    public abstract class OrderBase : BaseAICondition
    {
        private int currentPoints = Variable_Provider.orderPointsLayer;
        public override int WeightInfluence => currentPoints;

        protected Transform backlingTarget;
        protected Interactable_UtilityAI backlingSelf;

        public void SetActionBackling(TargetingUtilityAI.AIAction actionBacklink) 
        {
            backlingTarget = actionBacklink.target;
            backlingSelf = actionBacklink.behaviour.GetMainTransform().GetComponent<Interactable_UtilityAI>();
        }

        public void ExternalModify(int pointsAdded, IOrderController from) 
        {
            currentPoints += pointsAdded;
            
            if(!from.GetOrderStatus(backlingSelf))
                EndCondition();
        }
    }
}