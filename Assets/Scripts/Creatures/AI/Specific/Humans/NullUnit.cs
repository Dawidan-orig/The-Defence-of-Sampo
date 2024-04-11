using Sampo.AI;
using System.Collections.Generic;
using UnityEngine;

public class NullUnit : TargetingUtilityAI
{
    //TODO : Вот все эти функции должны быть вынесены в отдельный абстрактный класс, независимый от TargetingUtlityAI. Это нужно преобразовать в DependencyInjection, чтобы не создавать новые Instanc'ы юнитов, а лишь менять поведение
    public override void ActionUpdate(Transform target)
    {
        //TODO : Проверка близости к цели.
        //TODO : Вызов функции преобразования у цели-дома
    }

    public override void AttackUpdate(Transform target)
    {
        
    }

    protected override Dictionary<Interactable_UtilityAI, int> GetActionsDictionary()
    {
        var res = UtilityAI_Manager.Instance.GetAllInteractions(GetComponent<Faction>());
        var toDel = new Dictionary<Interactable_UtilityAI, int>();
        
        foreach (var action in res) 
        {
            if(gameObject.GetComponent<Faction>().IsWillingToAttack(action.Key.GetComponent<Faction>().FactionType)) 
            {
                res.Remove(action.Key);
            }
        }

        return res;
    }

    protected override UtilityAI_BaseState TargetReaction(Transform target)
    {
        return _factory.Action();
    }

    public override Vector3 RelativeRetreatMovement()
    {
        // Этот юнит не отсутпает
        return Vector3.zero;
    }

    protected override Tool ToolChosingCheck(Transform target)
    {
        // У этого юнита нет оружия
        return null;
    }
}
