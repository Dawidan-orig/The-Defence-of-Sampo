using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AliveBeing : Interactable_UtilityAI, IDamagable
{
    public float health = 100;

    public void Damage(float harm)
    {
        health -= harm;
    }
}
