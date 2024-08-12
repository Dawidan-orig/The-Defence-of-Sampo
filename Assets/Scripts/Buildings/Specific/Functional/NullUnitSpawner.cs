using Alchemy.Inspector;
using WingedCore.Core;
using System.Collections;
using UnityEngine;
using Sampo.Economy;

namespace WingedCore.Building.Spawners
{
    /// <summary>
    /// Дом, из которого появляются юниты-пустышки.
    /// Эти юниты потом преобразуются во всех других, более конкретных
    /// </summary>
    public class NullUnitSpawner : BuildableStructure, IInteractable
    {
        //TODO? : Gizmo для отображение появляемого юнита
        public float frequency = 10;
        public int limitAddition = 10;
        [Required]
        public Transform transfromSpawnPos;

        [SerializeField]
        private int toSpawn = 0;

        public int ToSpawn {
            get => toSpawn;
            set
            {
                AddUnitsToSpawn(value - toSpawn);
            }
        }

        private void OnEnable()
        {
            MonoBehaviourSingleton<ResourcesRequestManager>.Instance.AddNewSpawner(this);
        }
        private void OnDestroy()
        {
            MonoBehaviourSingleton<ResourcesRequestManager>.Instance.RemoveSpawner(this);
            MonoBehaviourSingleton<ResourcesRequestManager>.Instance.NullUnitLimit -= limitAddition;
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
            MonoBehaviourSingleton<ResourcesRequestManager>.Instance.NullUnitLimit += limitAddition;
        }

        public void AddUnitsToSpawn(int amount) 
        {
            bool wasNoSpawns = toSpawn == 0;
            toSpawn += amount;
            if (wasNoSpawns)
                StartCoroutine(SpawnCycle());
        }

        private IEnumerator SpawnCycle()
        {
            while (toSpawn > 0)
            {
                MonoBehaviourSingleton<ResourcesRequestManager>.Instance.CreateNewNullUnit(transfromSpawnPos);
                toSpawn--;
                yield return new WaitForSeconds(frequency);
            }
        }

        public float GetInteractionRange()
        {
            throw new System.NotImplementedException();
        }
    }
}