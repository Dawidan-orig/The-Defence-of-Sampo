using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableStructure : Interactable_UtilityAI, IDamagable
{
    public float health = 10000;

    public GameObject remainsPrefab;

    public void Damage(float harm)
    {
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
