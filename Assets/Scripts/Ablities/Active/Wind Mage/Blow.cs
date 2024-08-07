using WingedCore.AI;
using System;
using UnityEngine;

namespace WingedCore.Abilities
{
    [Serializable]
    public class Blow : Ability
    {
        public const float RECHARGE = 10;
        public const float RADIUS = 5;
        public const float POWER = 20;

        [SerializeField]
        private float _currentRecharge = 0;

        public Blow(Transform user) : base(user)
        {

        }

        public override void Activate()
        {
            if (_currentRecharge <= RECHARGE)
                return;

            _activated = false;

            _currentRecharge = 0;

            Utilities.DrawSphere(user.position, RADIUS, Color.red, 3);

            foreach (Collider c in Physics.OverlapSphere(user.position, RADIUS))
            {
                if (c.transform == user)
                    continue;

                if (c.TryGetComponent<AITarget>(out _))
                    if (!c.transform.GetComponent<AITarget>().IsWillingToAttack(user.GetComponent<AITarget>().FactionType))
                        continue;

                if (c.TryGetComponent<IMovingAgent>(out var agent))
                    agent.ExternalForceMacros();

                if (c.TryGetComponent<Rigidbody>(out var otherBody))
                {
                    otherBody.AddForce((otherBody.position - user.position + Vector3.up).normalized * POWER, ForceMode.VelocityChange);
                }
            }
        }

        public override void Update()
        {
            if (_currentRecharge < RECHARGE)
                _currentRecharge += Time.deltaTime;
            else
                _activated = true;
        }
    }
}