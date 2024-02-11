using Sampo.AI;
using UnityEngine;

public class UnitWithGun : TargetingUtilityAI
{
    public BaseShooting weapon;

    public override void AttackUpdate(Transform target)
    {
        if (weapon.AvilableToShoot(target, out _))
        {
            if (target.TryGetComponent(out Rigidbody body))
                weapon.transform.LookAt(weapon.PredictMovement(body));
            else
                weapon.transform.LookAt(target.position);

            weapon.Shoot(target.position);
        }
    }

    protected override Tool ToolChosingCheck(Transform target)
    {
        return weapon;
    }

    public override Transform GetRightHandTarget()
    {
        return weapon.transform;
    }

    public override void AssignPoints(int points)
    {
        base.AssignPoints(points);

        int remaining = points;

        //TODO DESIGN
    }

    public override void ActionUpdate(Transform target)
    {

    }
}
