using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableStructure : Interactable_UtilityAI, IDamagable
{
    public float health = 10000;

    public GameObject remainsPrefab;

    public void Damage(float harm, IDamagable.DamageType type)
    {
        if (type == IDamagable.DamageType.sharp)
            health -= harm * 0.2f;
        else if (type == IDamagable.DamageType.blunt)
            health -= harm;
        else if (type == IDamagable.DamageType.thermal)
            health -= harm;

        if (health < 0)
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if(remainsPrefab)
            Instantiate(remainsPrefab);
    }
}
