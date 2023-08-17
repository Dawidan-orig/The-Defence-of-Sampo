using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingLimb : MeleeTool
{
    public EventHandler<Collision> OnLimbCollisionEnter;
    private Rigidbody body;
    [SerializeField]
    private bool _isDamaging = true;

    public bool IsDamaging { get => _isDamaging; set => _isDamaging = value; }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnLimbCollisionEnter?.Invoke(this, collision);

        if (collision.collider.transform.TryGetComponent<AliveBeing>(out var alive) &&
            IsDamaging)
        {
            if (collision.transform.TryGetComponent<AttackingLimb>(out var otherLimb))
                if (otherLimb.host == host)
                    return;

            Utilities.DrawSphere(collision.GetContact(0).point, color: Color.red, duration: 3);
            alive.Damage(body.velocity.magnitude * body.mass * damageMultiplier, IDamagable.DamageType.sharp);
            _isDamaging = false;
        }
    }
}
