using Sampo.AI;
using Sampo.Building;
using Sampo.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NullUnit : TargetingUtilityAI
{
    //TODO : ��� ��� ��� ������� ������ ���� �������� � ��������� ����������� �����, ����������� �� TargetingUtlityAI. ��� ����� ������������� � DependencyInjection, ����� �� ��������� ����� Instanc'� ������, � ���� ������ ���������
    public override void ActionUpdate(Transform target)
    {
        //TODO : �������� �������� � ����.
        //TODO : ����� ������� �������������� � ����-����
        //TODO : �������������� ������� �������������.
        // � ������ ������, ��������, ��� �������� ������ �������� ����� X3 � ���������� �����

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

    protected override Dictionary<Interactable_UtilityAI, int> GetActionsDictionary()
    {
        return UtilityAI_Manager.Instance.GetSameFactionInteractions(GetComponent<Faction>())
            .Where(kvp => kvp.Key.GetComponent<Faction>().isAvailableForSelfFaction)
            .Select(kvp => new { kvp.Key, val = (kvp.Key.TryGetComponent(out BuildableStructure _) ? kvp.Value * 3 : kvp.Value) })
            .ToDictionary(t => t.Key, t => t.val);
    }

    protected override bool IsTargetPassing(Transform target)
    {
        return true;
    }

    protected override UtilityAI_BaseState TargetReaction(Transform target)
    {
        return _factory.Action();
    }

    public override Vector3 RelativeRetreatMovement()
    {
        // ���� ���� �� ���������
        return Vector3.zero;
    }

    protected override Tool ToolChosingCheck(Transform target)
    {
        // � ����� ����� ��� ������
        return null;
    }
}
