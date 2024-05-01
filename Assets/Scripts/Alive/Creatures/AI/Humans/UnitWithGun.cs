using Sampo.Weaponry;
using Sampo.Weaponry.Ranged;
using UnityEngine;

namespace Sampo.AI.Humans.Ranged
{
    public class UnitWithGun : AIBehaviourBase
    {
        public BaseShooting weapon;

        public override Tool BehaviourWeapon => weapon;

        protected override void Awake()
        {
            base.Awake();
        }
        protected override void Update()
        {
            base.Update();
            Transform target = CurrentActivity.target;
            if (target == null)
                return;

            if (weapon.AvilableToShoot(target, out _))
            {
                if (target.TryGetComponent(out Rigidbody body))
                    weapon.transform.LookAt(weapon.PredictMovement(body));
                else
                    weapon.transform.LookAt(target.position);

                weapon.Shoot(target.position);
            }
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

        public override Vector3 RelativeRetreatMovement()
        {
            const float RANGE_EDGE_MODIFIER = 0.8f;

            float progress = weapon.GetRange()
                /Vector3.Distance(CurrentActivity.target.position, transform.position)
                * RANGE_EDGE_MODIFIER;

            Vector3 outDir = (CurrentActivity.target.position - transform.position).normalized;
            outDir *= progress;

            return outDir + Vector3.right;
        }

        public override int GetCurrentWeaponPoints()
        {
            float range = weapon.GetRange();
            float dist = Vector3.Distance(transform.position, CurrentActivity.target.position);
            return Mathf.RoundToInt(-Mathf.Pow(dist - range, 2) + (dist - range) + range);
        }
    }
}