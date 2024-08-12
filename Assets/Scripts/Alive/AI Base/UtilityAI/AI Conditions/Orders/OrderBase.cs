using UnityEngine;

namespace WingedCore.AI.Conditions.Orders
{
    /// <summary>
    /// Абстрактная база для всех приказов.
    /// Контроллирует Condition (Условие) у ИИ с учётом данных извне,
    /// Модифицируется на ходу
    /// </summary>
    public abstract class OrderBase : BaseAICondition
    {
        private int currentPoints = WingedCore.DebugSystems.VariableProvider.orderPointsLayer;
        public override int WeightInfluence => currentPoints;

        protected Transform backlingTarget;
        protected AITarget backlingSelf;

        public void SetActionBackling(TargetingUtilityAI.AIAction actionBacklink) 
        {
            backlingTarget = actionBacklink.target;
            backlingSelf = actionBacklink.behaviour.GetMainTransform().GetComponent<AITarget>();
        }

        public void ExternalModify(int pointsAdded, IOrderController from) 
        {
            currentPoints += pointsAdded;
            
            if(!from.GetOrderStatus(backlingSelf))
                EndCondition();
        }
    }
}