using WingedCore.AI;
using System.Collections.Generic;
using UnityEngine;
using WingedCore.Core;

namespace WingedCore.Building
{
    public class DestructableStructure : AITarget, IDamagable
    {
        public float health = 10000;
        public List<GameObject> connectedObjects = new List<GameObject>();
        public GameObject remainsPrefab;
        public Collider vital;
        public Transform parentToDestroy;

        public Collider Vital => vital;

        private void Awake()
        {
            var colliders = GetComponents<Collider>();
            if (colliders.Length == 1)
                vital = colliders[0];
        }

        private void Start()
        {
            if (!parentToDestroy)
                parentToDestroy = transform;
        }

        public void Damage(float harm, IDamagable.DamageType type)
        {
            //TODO : Перенести это в систему урона и элементов.
            if (type == IDamagable.DamageType.sharp)
                health -= harm * 0.2f;
            else if (type == IDamagable.DamageType.blunt)
                health -= harm;
            else if (type == IDamagable.DamageType.thermal)
                health -= harm;

            if (health < 0)
            {
                Destroy(parentToDestroy.gameObject);
                foreach (var obj in connectedObjects)
                    Destroy(obj);
            }
        }

        private void OnDestroy()
        {
            if (remainsPrefab)
                Instantiate(remainsPrefab);
        }
    }
}