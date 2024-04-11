using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public abstract Collider Vital {get; }
    public enum DamageType 
    {
        sharp,
        blunt,
        thermal
    }

    public abstract void Damage(float harm, DamageType damage);
}
