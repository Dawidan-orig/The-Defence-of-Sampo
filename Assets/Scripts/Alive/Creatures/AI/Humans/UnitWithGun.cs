using Sampo.Weaponry.Ranged;
using UnityEngine;

namespace Sampo.AI.Humans.Ranged
{
    public class UnitWithGun : AIBehaviourBase
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

        public override Tool ToolChosingCheck(Transform target)
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

        public override Vector3 RelativeRetreatMovement()
        {
            //TODO : Сделать медленное радиальное отсутпление как у скелетов в майне
            throw new System.NotImplementedException();
        }

        public override int GetCurrentWeaponPoints()
        {
            float range = weapon.GetRange();
            float dist = Vector3.Distance(transform.position, CurrentActivity.target.position);
            return Mathf.RoundToInt(-Mathf.Pow(dist - range, 2) + (dist - range) + range);
        }
    }
}