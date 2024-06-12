using Sampo.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI.Conditions.Orders
{
    /// <summary>
    /// Приказ, изменяющий свою силу в runtime
    /// </summary>
    public class PriorityActionOrder : OrderBase
    {
        int additionalPrioritization;
        Func<Interactable_UtilityAI,int> influenceLogic;
        public override int WeightInfluence => base.WeightInfluence + additionalPrioritization;

        /// <param name="influenceLogic">Функция, которая возвращает добавночную силу к очкам</param>
        public PriorityActionOrder(Func<Interactable_UtilityAI, int> influenceLogic) 
        {
            additionalPrioritization = WeightInfluence;
            this.influenceLogic = influenceLogic;
        }

        public override void Update()
        {
            if(influenceLogic != null)
                additionalPrioritization = (int)Mathf.Clamp(
                    influenceLogic.Invoke(backlingTarget.GetComponent<Interactable_UtilityAI>()),
                    Mathf.NegativeInfinity,
                    Variable_Provider.emotionalPointsLayer);
        }
    }
}