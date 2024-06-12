using Sampo.AI;
using System;
using UnityEngine;

namespace Sampo.Abilities
{
    [Serializable]
    public class FixedAscention : Ability
    {
        public const float RADIUS = 30;
        public const float RECHARGE = 1;

        [SerializeField]
        private float _currentRecharge = 0;

        public FixedAscention(Transform user) : base(user)
        {

        }

        public override void Update()
        {
            if (_currentRecharge < RECHARGE)
                _currentRecharge += Time.deltaTime;
            else
                _activated = true;
        }

        public override void Activate()
        {
            if (_currentRecharge <= RECHARGE)
                return;

            _activated = false;

            _currentRecharge = 0;

            Utilities.DrawSphere(user.position, RADIUS, Color.blue, 3);

            foreach (Collider c in Physics.OverlapSphere(user.position, RADIUS))
            {
                if (c.transform == user)
                    continue;

                if (c.TryGetComponent<Faction>(out _))
                    if (!c.transform.GetComponent<Faction>().IsWillingToAttack(user.GetComponent<Faction>().FactionType))
                        continue;

                if (c.TryGetComponent<BuffSystem>(out var other))
                {
                    other.AddEffect(new Ascended_Effect(7, 10, other.GetComponent<Rigidbody>()));
                }
            }
        }
    }
}