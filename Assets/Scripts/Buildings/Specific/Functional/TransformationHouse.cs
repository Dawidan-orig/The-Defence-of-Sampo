using Sampo.AI;
using Sampo.Core;
using Sampo.Player.Economy;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Building.Transformators
{
    /// <summary>
    /// Базовый класс для всех зданий-преобразователей пустых юнитов
    /// </summary>
    public class TransformationHouse : BuildableStructure, IInteractable
    {
        public float interactionRange = 1f;
        public int unitLimit = 5;
        /* TODO (dep. TODO : Система классов) : Преобразование напрямую, а не через полное удаление GameObject
         * Для этого надо указывать тут тип, в который и будет происходить преобразование, а не Prefab.
         */
        public GameObject prefab_TransformTo;
        [SerializeField]
        private List<GameObject> createdUnits;

        private void OnValidate()
        {
            UpdateConnectedUnits(gameObject, null);
        }

        protected override void Start()
        {
            base.Start();

            createdUnits = new();
        }

        public void Interact(Transform interactor)
        {
            if (createdUnits.Count >= unitLimit-1)
            {
                GetComponent<Faction>().IsAvailableForSelfFaction = false;
            }

            //TODO!!!!! : Преобразование должно быть проще, и без удаления Gameobject. Нужно оставить тот же объект, и не менять ему фракцию. Это делает 100500 вызовов у менеджера
            //ломает всё! Необходимо убрать.
            GameObject newBorn = Instantiate(prefab_TransformTo, interactor.position, interactor.rotation, Variable_Provider.Instance.unitsContainer);
            newBorn.GetComponent<Faction>().ChangeFactionCompletely(GetComponent<Faction>().FactionType);
            Destroy(interactor.gameObject);
            createdUnits.Add(newBorn);

            if (!newBorn.TryGetComponent<OnDestroyNotifier>(out var comp))
                comp = newBorn.AddComponent<OnDestroyNotifier>();
            comp.onDestroy += UpdateConnectedUnits;
        }

        private void UpdateConnectedUnits(object sender, EventArgs _) 
        {
            int removed = createdUnits.RemoveAll(unit => unit == null);
            if (removed > 0)
            {
                GetComponent<Faction>().IsAvailableForSelfFaction = true;
                BuildingsManager.Instance.RequestNullUnits(this, removed);
            }
        }

        public void PlayerInteract()
        {
            //TODO : Настройка через интерфейс
        }

        protected override void Build()
        {
            BuildingsManager.Instance.RequestNullUnits(this, unitLimit);
        }

        public float GetInteractionRange()
        {
            return interactionRange;
        }
    }
}