using Sampo.Weaponry.Ranged;
using UnityEngine;

namespace Sampo.AI.Humans.Ranged
{
    public class UnitWithGun : AIBehaviourBase
    {
        public BaseShooting weapon;

        protected override void Awake()
        {
            base.Awake();

            _behaviourWeapon = weapon;
        }

        public override void ActionUpdate(Transform target)
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
            //TODO : ������� ��������� ���������� ����������� ��� � �������� � �����
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