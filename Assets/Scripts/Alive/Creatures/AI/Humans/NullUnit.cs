using Sampo.AI;
using Sampo.Building;
using Sampo.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NullUnit : AIBehaviourBase
{
    //TODO : Вот все эти функции должны быть вынесены в отдельный абстрактный класс, независимый от TargetingUtlityAI. Это нужно преобразовать в DependencyInjection, чтобы не создавать новые Instanc'ы юнитов, а лишь менять поведение
    public override void ActionUpdate(Transform target)
    {
        //TODO : Проверка близости к цели.
        //TODO : Вызов функции преобразования у цели-дома
        //TODO : индивидуальная базовая приоритезация.
        // В данном случае, например, все строения должны получить бонус X3 к количеству очков

        if(target.TryGetComponent(out IInteractable interact)) 
        {
            if(Vector3.Distance(transform.position, target.position) < interact.GetInteractionRange()) 
            {
                interact.Interact(transform);
            }
        }
    }

    public override void AttackUpdate(Transform target)
    {
        
    }

    public override Dictionary<Interactable_UtilityAI, int> GetActionsDictionary()
    {
        var input = UtilityAI_Manager.Instance.GetSameFactionInteractions(GetComponent<Faction>());

        var res = input
            .Where(kvp => kvp.Key.GetComponent<Faction>().IsAvailableForSelfFaction)
            .Select(kvp => new { kvp.Key, val = kvp.Value})
            //(kvp.Key.TryGetComponent(out BuildableStructure _) ? kvp.Value * 3 : kvp.Value) 
            .ToDictionary(t => t.Key, t => t.val);

        return res;
    }

    public override bool IsTargetPassing(Transform target)
    {
        return true;
    }

    public override UtilityAI_BaseState TargetReaction(Transform target)
    {
        return _AITargeting.GetActionState();
    }

    public override Vector3 RelativeRetreatMovement()
    {
        // Этот юнит не отсутпает
        return Vector3.zero;
    }

    public override Tool ToolChosingCheck(Transform target)
    {
        // У этого юнита нет оружия
        return null;
    }
}
