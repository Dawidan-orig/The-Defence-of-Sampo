using Sampo.AI;
using Sampo.AI.Humans;
using Sampo.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using Alchemy.Inspector;
using System.Linq;
using Sampo.Core.JournalLogger;

namespace Sampo.Building.Transformators
{
    /// <summary>
    /// ������� ����� ��� ���� ������-���������������� ������ ������
    /// </summary>
    public class TransformationHouse : BuildableStructure, IInteractable
    {
        public bool requestUnits = false;
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
            string dataToDebugLog = "�������������� �� " + interactor.name + "\n";

            if (createdUnits.Contains(interactor.gameObject))
            {
                dataToDebugLog += "������� �������� ����� �� ���������";
                LoggerSingleton.DebugLog(dataToDebugLog, gameObject, interactor.gameObject);
                return;
            }

            if (createdUnits.Count > unitLimit-1)            
                GetComponent<Faction>().IsAvailableForSelfFaction = false;            

            if(interactor.TryGetComponent(out MultiweaponUnit multiweapon))
                multiweapon.AddNewBehaviour(TransformationKitPrefab);
            else 
            {
                dataToDebugLog += "�������������� ������������ ����� � MultiweaponUnit\n";
                AIBehaviourBase directBehaviour = interactor.GetComponent<AIBehaviourBase>();
                Destroy(directBehaviour);
                multiweapon = interactor.gameObject.AddComponent<MultiweaponUnit>();

                //TODO!!! : �������� NullUnitKit � ���������� ����. �� ������ ���� � ����,
                //��� ��� �������� �� ��� ������ ��������������
                //� ����������� ��������
                //multiweapon.AddNewBehaviour((GameObject)Resources.Load("NullUnitKit"));
                multiweapon.AddNewBehaviour(TransformationKitPrefab);
            }

            createdUnits.Add(interactor.gameObject);

            if (!interactor.gameObject.TryGetComponent<OnDestroyNotifier>(out var comp))
                comp = interactor.gameObject.AddComponent<OnDestroyNotifier>();
            comp.onDestroy += UpdateConnectedUnits;

            LoggerSingleton.DebugLog(dataToDebugLog, gameObject, interactor.gameObject);
        }

        //TODO : ������� ��������� ��� �����
        private void UpdateConnectedUnits(object sender, EventArgs _) 
        {
            createdUnits.Remove((GameObject)sender);
            int removed = createdUnits.RemoveAll(unit => unit == null) + 1;
            if (removed > 0 && requestUnits)
            {
                GetComponent<Faction>().IsAvailableForSelfFaction = true;
                BuildingsManager.Instance.RequestNullUnits(this, removed);
            }
        }

        public void PlayerInteract()
        {
            //TODO : ��������� ����� ���������
        }

        protected override void Build()
        {
            if(requestUnits)
                BuildingsManager.Instance.RequestNullUnits(this, unitLimit);
        }

        public float GetInteractionRange()
        {
            return interactionRange;
        }
    }
}