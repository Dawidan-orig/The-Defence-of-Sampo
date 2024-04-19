using Sampo.AI;
using Sampo.Building;
using Sampo.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NullUnit : AIBehaviourBase
{
    public override void ActionUpdate(Transform target)
    {
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
        var input = UtilityAI_Manager.Instance.GetSameFactionInteractions(GetMainTransform().gameObject.GetComponent<Faction>());

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

    public override int GetCurrentWeaponPoints()
    {
        return 0;
    }
}
