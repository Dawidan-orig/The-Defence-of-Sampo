using Sampo.Core;
using System.Collections;
using UnityEngine;

namespace Sampo.Building.Spawners
{
    /// <summary>
    /// Дом, из которого появляются юниты-пустышки.
    /// Эти юниты потом преобразуются во всех других, более конкретных
    /// </summary>
    public class NullUnitSpawner : BuildableStructure, IInteractable
    {
        //TODO? : Сделать специальный атрибут самостоятельно,
        //Который выкидывает exception если поля не заполнены через Unity
        public float frequency = 10;
        public GameObject spawnPrefab;
        public Transform transfromSpawnPos;
        public Transform unitContainer;

        private void Awake()
        {
            if(spawnPrefab == null)
                spawnPrefab = (GameObject)Resources.Load("NullUnit");
        }

        public void Interact(Transform interactor)
        {
            //???
        }

        public void PlayerInteract()
        {
            //TODO : Настройка через интерфейс
        }

        protected override void Build()
        {
            StartCoroutine(nameof(SpawnCycle));

            unitContainer = Variable_Provider.Instance.unitsContainer;
        }

        private IEnumerator SpawnCycle() 
        {
            while (true)
            {
                yield return new WaitForSeconds(frequency);

                Instantiate(spawnPrefab, transfromSpawnPos.position, transfromSpawnPos.rotation, unitContainer);
            }
        }
    }
}