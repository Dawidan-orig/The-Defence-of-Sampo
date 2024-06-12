using Sampo.AI;
using Sampo.Weaponry;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ��-����������, ��������������� ��� ������� �������,
/// �� ����� ������, ���� ������ � ���� ��,
/// �������� ����������� � �������� ������ ������� - ��������� �������� ������
/// </summary>
public class NullUnit : AIBehaviourBase
{
    public override Tool BehaviourWeapon => null;
    public override Dictionary<Interactable_UtilityAI, int> GetActionsDictionary()
    {
        var input = UtilityAI_Manager.Instance.GetSameFactionInteractions(GetMainTransform().gameObject.GetComponent<Faction>());

        var res = input
            .Where(kvp => kvp.Key.GetComponent<Faction>().IsAvailableForSelfFaction)
            .Select(kvp => new { kvp.Key, val = kvp.Value})
            //.Select(kvp.Key.TryGetComponent(out BuildableStructure _) ? kvp.val * 3 : kvp.val) 
            .ToDictionary(t => t.Key, t => t.val);

        return res;
    }
    public override bool IsTargetPassing(Transform target)
    {
        bool res = true;

        Faction other = target.GetComponent<Faction>();

        if (!other.IsAvailableForSelfFaction || target == transform)
            res = false;

        if (other.TryGetComponent(out AliveBeing b))
            if (b.mainBody == transform)
                res = false;

        return res;
    }
    public override Vector3 RelativeRetreatMovement()
    {
        // ���� ���� �� ���������
        return Vector3.zero;
    }
    public override int GetCurrentWeaponPoints()
    {
        int res = 0;
        if (IsTargetPassing(CurrentActivity.target))
            res = 100;

        return res;
    }
}
