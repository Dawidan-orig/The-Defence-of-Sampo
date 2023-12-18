using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public enum DamageType 
    {
        sharp,
        blunt,
        thermal
    }

    public abstract void Damage(float harm, DamageType damage);
}
