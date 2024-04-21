using Sampo.AI;
using Sampo.AI.Humans;
using Sampo.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using Alchemy.Inspector;

namespace Sampo.Building.Transformators
{
    /// <summary>
    /// Базовый класс для всех зданий-преобразователей пустых юнитов
    /// </summary>
    public class TransformationHouse : BuildableStructure, IInteractable
    {
        public float interactionRange = 1f;
        public int unitLimit = 5;
        [Required]
        public GameObject TransformationKitPrefab;
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

            if(interactor.TryGetComponent(out MultiweaponUnit multiweapon))
                multiweapon.AddNewBehaviour(TransformationKitPrefab);
            else 
            {
                Debug.Log("Преобразование константного юнита в MultiweaponUnit");
                AIBehaviourBase directBehaviour = interactor.GetComponent<AIBehaviourBase>();
                Destroy(directBehaviour);
                multiweapon = interactor.gameObject.AddComponent<MultiweaponUnit>();

                //TODO!!! : добавить NullUnitKit в глобальном виде. Он должен быть у всех
                //multiweapon.AddNewBehaviour((GameObject)Resources.Load("NullUnitKit"));
                multiweapon.AddNewBehaviour(TransformationKitPrefab);
            }

            createdUnits.Add(interactor.gameObject);

            if (!interactor.gameObject.TryGetComponent<OnDestroyNotifier>(out var comp))
                comp = interactor.gameObject.AddComponent<OnDestroyNotifier>();
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