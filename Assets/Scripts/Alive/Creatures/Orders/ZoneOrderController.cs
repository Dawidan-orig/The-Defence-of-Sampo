using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI.Conditions.Orders
{
    /// <summary>
    /// ������������� ������� � ������������ � ���������� ����� �����������
    /// </summary>
    public class ZoneOrderController : MonoBehaviour, IOrderController
    {
        //TODO : ������� �������, ������� ������� ���� ������. ��� ������ ����, �������, ������ ������.

        /// <summary>
        /// ������� ������� � �������� ����
        /// </summary>
        public Func<Interactable_UtilityAI, int> orderPowerForTransform;

        [SerializeField]
        List<TargetingUtilityAI> unitsWithOrder;
        [SerializeField]
        List<Interactable_UtilityAI> orderTargets;

        public int GetOrderPower(Interactable_UtilityAI @for)
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

        public bool GetOrderStatus(Interactable_UtilityAI of)
        {
            return orderTargets.Contains(of);
        }
        #endregion

        #region unity
        private void Awake()
        {
            unitsWithOrder = new();
            orderTargets = new();
            orderPowerForTransform = ((@for) =>
            (int)(GetComponent<Collider>().bounds.extents.magnitude - Vector3.Distance(transform.position, @for.transform.position)));
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.TryGetComponent(out Interactable_UtilityAI entered)) 
            {
                foreach(var unit in unitsWithOrder) 
                {
                    //TODO : �������� ������� � TargetingUAI, ����� �� ������ ��� ����� � ������
                    //TODO : �������� ������� � � Interactable, ��� �� ����� Require �����
                    //TODO : ���������� ��� ������ ������� �����, ����� �������� ������������ ���������, � �� ������ �����
                    if(unit.GetComponent<Faction>().IsWillingToAttack(entered.GetComponent<Faction>().FactionType)) 
                    {
                        unit.ModifyAllActionsOf(entered.transform,
                            new PriorityActionOrder(GetOrderPower));
                    }
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out TargetingUtilityAI exited))
            {

            }
        }
        #endregion
    }
}