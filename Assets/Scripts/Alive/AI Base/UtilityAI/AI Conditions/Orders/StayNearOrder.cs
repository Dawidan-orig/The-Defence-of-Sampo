using WingedCore.AI.Conditions;
using WingedCore.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.AI.Conditions.Orders
{
    /// <summary>
    /// Приказ держаться поблизости объекта
    /// </summary>
    public class StayNearOrder : OrderBase
    {
        Transform target;

        public StayNearOrder(Transform target)
        {
            this.target = target;
        }

        public override void Update()
        {
            if (target == null)
                EndCondition();
        }
    }
}