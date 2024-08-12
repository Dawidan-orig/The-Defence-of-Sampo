using WingedCore.AI;
using WingedCore.Weaponry.Melee;
using System;
using UnityEngine;
using WingedCore.Core;
using WingedCore.Core.Utility;

namespace WingedCore.Weaponry.Special
{
    public class AttackingLimb : MeleeTool
    {
        public EventHandler<Collision> OnLimbCollisionEnter;
        [SerializeField]
        private bool _isDamaging = true;
        public bool IsDamaging { get => _isDamaging; set => _isDamaging = value; }


        private void OnCollisionEnter(Collision collision)
        {
            OnLimbCollisionEnter?.Invoke(this, collision);

            if (collision.collider.transform.TryGetComponent<AliveBeing>(out var alive) &&
                IsDamaging)
            {
                if (collision.transform.TryGetComponent<AttackingLimb>(out var otherLimb))
                    if (otherLimb.Host == _host)
                        return;

                alive.Damage(body.velocity.magnitude * body.mass * damageMultiplier, IDamagable.DamageType.sharp);
                _isDamaging = false;
            }
        }
    }
}