using WingedCore.AI;
using System.Collections.Generic;
using UnityEngine;
using WingedCore.Core;
using WingedCore.AI.CounterSystem;

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

            InitializeTags();
        }

        private void InitializeTags()
        {
            const float BUILDING_HEALTH_BULK = 5000;
            const float BUILDING_HEALTH_WEAK = 500;

            CounterTagLocal tagSystem = GetComponent<CounterTagLocal>();
            if(tagSystem == null)
                tagSystem = gameObject.AddComponent<CounterTagLocal>();

            tagSystem.AddNewRole((CounterNode)Resources.Load("Building"));

            if (health > BUILDING_HEALTH_BULK)
                tagSystem.AddNewRole((CounterNode)Resources.Load("Tags/HealthBulk"));
            else if (health > BUILDING_HEALTH_WEAK)
                tagSystem.AddNewRole((CounterNode)Resources.Load("Tags/HealthWeak"));
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
                HanldedOnDestroy(parentToDestroy.gameObject);
                foreach (var obj in connectedObjects)
                    HanldedOnDestroy(obj);
            }
        }

        private void HanldedOnDestroy(GameObject obj)
        {
            if (remainsPrefab)
                Instantiate(remainsPrefab);

            Destroy(obj);
        }
    }
}