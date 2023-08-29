using System.Collections.Generic;
using UnityEngine;

public class UnitWithGun : TargetingUtilityAI
{
    public SimplestShooting weapon;

    public override void AttackUpdate(Transform target)
    {
        base.AttackUpdate(target);

        if (target.TryGetComponent(out Rigidbody body))
            weapon.transform.LookAt(weapon.GetPointToShoot(body));
        else
            weapon.transform.LookAt(target.position);

        weapon.Shoot(target.position);
    }

    protected override Tool ToolCheck(Transform target)
    {
        return weapon;
    }

    public override Transform GetRightHandTarget()
    {
        return weapon.transform;
    }
}
