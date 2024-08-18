using WingedCore.AI;
using WingedCore.Core;
using System.Collections.Generic;
using UnityEngine;
using Sampo.Economy;
using WingedCore.Building;

namespace Sampo.Building
{
    /*TODO : Эта штука должна как-то понимать, когда надо выпустить NullUnit'ов,
     * Для этого надо будет сделать систему менеджмента экономики
    */
    /// <summary>
    /// Структура, что способна удерживать в себе неактивных юнитов.
    /// Это позволит запасать их для разных задач, а так же манипулировать как группами
    /// </summary>
    public class BufferingHouse : BuildableStructure, IInteractable
    {
        public int bufferingAmount = 10;
        public float interactionRange = 5;
        public Transform releasePos;

        [SerializeField]
        List<GameObject> contained;

        protected override void Build()
        {
            contained = new List<GameObject>();
            MonoBehaviourSingleton<ResourcesRequestManager>.Instance.RequestNullUnits(this, bufferingAmount);
        }

        public void Contain(GameObject obj)
        {
            if (contained.Count >= bufferingAmount)
            {
                GetComponent<AITarget>().IsAvailableForSelfFaction = false;
                return;
            }

            obj.SetActive(false);
            obj.transform.parent = transform;
            contained.Add(obj);
        }

        public void Release(GameObject obj)
        {
            if(!contained.Contains(obj))
            {
                Debug.LogError("Попытка извлечь юнита, которого нет в этом здании", transform);
                return;
            }

            MonoBehaviourSingleton<ResourcesRequestManager>.Instance.RequestNullUnits(this, 1);

            obj.transform.SetPositionAndRotation(releasePos.position, releasePos.rotation);
            obj.transform.parent = MonoBehaviourSingleton<WingedCore.DebugSystems.VariableProvider>.Instance.unitsContainer;
            obj.SetActive(true);

            if (contained.Count <= bufferingAmount)            
                GetComponent<AITarget>().IsAvailableForSelfFaction = true;
        }

        public void ReleaseAll()
        {
            foreach (GameObject obj in contained)
            {
                Release(obj);
            }
        }

        private void OnDestroy()
        {
            ReleaseAll();
        }

        public void Interact(Transform interactor)
        {
            Contain(interactor.gameObject);
        }

        public void PlayerInteract()
        {
            //TODO : Настройка через интерфейс
        }

        public float GetInteractionRange()
        {
            return interactionRange;
        }
    }
}