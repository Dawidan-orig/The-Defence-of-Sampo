using Sampo.Core;
using UnityEngine;

namespace Sampo.Building.Transformators
{
    /// <summary>
    /// Базовый класс для всех зданий-преобразователей пустых юнитов
    /// </summary>
    public class TransformationHouse : BuildableStructure, IInteractable
    {
        public float interactionRange = 5;
        public GameObject prefab_TransformTo;

        public void Interact(Transform interactor)
        {
            GameObject newBorn = Instantiate(prefab_TransformTo, interactor.position, interactor.rotation, Variable_Provider.Instance.unitsContainer);
            newBorn.GetComponent<Faction>().ChangeFactionCompletely(GetComponent<Faction>().FactionType);
            Destroy(interactor.gameObject);
        }

        public void PlayerInteract()
        {
            //TODO : Настройка через интерфейс
        }

        protected override void Build()
        {
            
        }

        public float GetInteractionRange()
        {
            return interactionRange;
        }
    }
}