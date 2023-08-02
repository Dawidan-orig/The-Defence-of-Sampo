using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AliveBeing : Interactable_UtilityAI, IDamagable
{
    public float health = 100;

    public void Damage(float harm, IDamagable.DamageType type)
    {
        if (type == IDamagable.DamageType.sharp)
            health -= harm * 0.5f;
        else if (type == IDamagable.DamageType.blunt)
            health -= harm * 0.2f;
        else if (type == IDamagable.DamageType.thermal)
            health -= harm;
    }
}
