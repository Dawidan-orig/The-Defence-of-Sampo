using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitWithGun : TargetingUtilityAI
{
    public SimplestShooting weapon;

    public override void AttackUpdate(Transform target)
    {
        base.AttackUpdate(target);

        weapon.transform.LookAt(target.position);

        weapon.Shoot();
    }

    protected override void DistributeActivityFromManager(object sender, UtilityAI_Manager.UAIData e)
    {
        _possibleActions.Clear();

        var activities = e.interactables;
        foreach (KeyValuePair<GameObject, int> activity in activities)
        {
            GameObject target = activity.Key;
            int weight = activity.Value;

            if (target.TryGetComponent<Interactable_UtilityAI>(out _))
            {
                AddNewPossibleAction(target.transform, weight, target.transform.name, weapon, _factory.Attack());
            }
        }
    }

    public override Vector3 GetRightHandTarget()
    {
        return weapon.transform.position;
    }
}
