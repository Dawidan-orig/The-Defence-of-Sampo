using WingedCore.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.AI.Conditions.Orders
{
    /// <summary>
    /// Приказ, изменяющий свою силу в runtime
    /// </summary>
    public class PriorityActionOrder : OrderBase
    {
        int additionalPrioritization;
        Func<AITarget,int> influenceLogic;
        public override int WeightInfluence => base.WeightInfluence + additionalPrioritization;

        /// <param name="influenceLogic">Функция, которая возвращает добавночную силу к очкам</param>
        public PriorityActionOrder(Func<AITarget, int> influenceLogic) 
        {
            additionalPrioritization = WeightInfluence;
            this.influenceLogic = influenceLogic;
        }

        public override void Update()
        {
            if(influenceLogic != null)
                additionalPrioritization = (int)Mathf.Clamp(
                    influenceLogic.Invoke(backlingTarget.GetComponent<AITarget>()),
                    Mathf.NegativeInfinity,
                    WingedCore.DebugSystems.VariableProvider.orderPointsLayer);
        }
    }
}