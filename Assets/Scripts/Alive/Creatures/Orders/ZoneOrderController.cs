using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.AI.Conditions.Orders
{
    /// <summary>
    /// Контроллирует приказы в соответствии с назначеной зоной коллайдером
    /// </summary>
    public class ZoneOrderController : MonoBehaviour, IOrderController
    {
        //TODO : Сделать систему, которая добавит сюда юнитов. Это должен быть, пожалуй, именно скрипт.

        /// <summary>
        /// Функция влияния в пределах зоны
        /// </summary>
        public Func<AITarget, int> orderPowerForTransform;

        [SerializeField]
        List<TargetingUtilityAI> unitsWithOrder;
        [SerializeField]
        List<AITarget> orderTargets;
        AITarget cashedTargetComp;

        public int GetOrderPower(AITarget @for)
        {
            return orderPowerForTransform.Invoke(@for);
        }

        public void AddUnitToOrder(TargetingUtilityAI newToAdd) 
        {
            unitsWithOrder.Add(newToAdd);
            newToAdd.ModifyAllActionsOf(transform, new StayNearOrder(transform));
        }

        #region IOrderController
        public List<TargetingUtilityAI> GetOrderedUnits()
        {
            return unitsWithOrder;
        }

        public bool GetOrderStatus(AITarget of)
        {
            return orderTargets.Contains(of);
        }
        #endregion

        #region unity
        private void Awake()
        {
            cashedTargetComp = GetComponent<AITarget>();
            unitsWithOrder = new();
            orderTargets = new();
            //TODO : Заменить на AnimationCurve
            orderPowerForTransform = ((@for) =>
            (int)(GetComponent<Collider>().bounds.extents.magnitude - Vector3.Distance(transform.position, @for.transform.position)) + cashedTargetComp.ai_weight);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.TryGetComponent(out AITarget entered)) 
            {
                foreach(var unit in unitsWithOrder) 
                {
                    //TODO : Переписать все вызовы фракций везде, чтобы получать кэшированный компонент, а не делать вызов
                    if(unit.GetComponent<AITarget>().IsWillingToAttack(entered.FactionType)) 
                    {
                        unit.ModifyAllActionsOf(entered.transform,
                            new PriorityActionOrder(GetOrderPower));
                    }
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out AITarget exited))
            {

            }
        }
        #endregion
    }
}