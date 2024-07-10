using Alchemy.Inspector;
using Sampo.AI;
using Sampo.Weaponry.Ranged;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Building.Towers
{
    /// <summary>
    /// Ѕашн€, работающа€ относительно области действи€ - коллайдера.
    /// </summary>
    public class AttackingTower : MonoBehaviour
    {
        [Required]
        public BaseShooting weapon;
        [Required]
        public DestructableStructure structureBase;

        [ReadOnly]
        [SerializeField] List<Transform> targetsInRange;

        private void Update()
        {
            if (targetsInRange.Count == 0)
                return;

            Transform target = targetsInRange[0]; //TODO : —делать более сложный выбор целей
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

        private void OnTriggerEnter(Collider other)
        {
            TriggerCheck(other,
                () => { targetsInRange.Add(other.transform); });

            targetsInRange.RemoveAll(t => t == null);

            if (targetsInRange.Count > 2)
            targetsInRange.Sort((item1, item2) =>
            Vector3.Distance(transform.position, item1.position)
            .CompareTo(
                Vector3.Distance(transform.position, item2.position)
                )
            );
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerCheck(other,
                () => { targetsInRange.Remove(other.transform); });
        }

        private void TriggerCheck(Collider other, Action whatToDo)
        {
            if(other.TryGetComponent(out AITarget target)) 
            {
                if(target.IsWillingToAttack(structureBase.FactionType)) 
                {
                    whatToDo.Invoke();
                }
            }
        }
    }
}