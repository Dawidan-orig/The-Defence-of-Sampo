using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableStructure : Interactable_UtilityAI, IDamagable
{
    public float health = 10000;
    public List<GameObject> connectedObjects = new List<GameObject>();
    public GameObject remainsPrefab;
    public Collider vital;
    public Transform parentToDestroy;

    public Collider Vital => vital;

    private void Start()
    {
        if (GetComponents<Collider>().Length == 1)
            vital = GetComponent<Collider>();

        if (!parentToDestroy)
            parentToDestroy = transform;
    }

    public void Damage(float harm, IDamagable.DamageType type)
    {
        if (type == IDamagable.DamageType.sharp)
            health -= harm * 0.2f;
        else if (type == IDamagable.DamageType.blunt)
            health -= harm;
        else if (type == IDamagable.DamageType.thermal)
            health -= harm;

        if (health < 0)
        {
            Destroy(parentToDestroy.gameObject);
            foreach(var obj in connectedObjects)
                Destroy(obj);
        }
    }

    private void OnDestroy()
    {
        if(remainsPrefab)
            Instantiate(remainsPrefab);
    }
}
