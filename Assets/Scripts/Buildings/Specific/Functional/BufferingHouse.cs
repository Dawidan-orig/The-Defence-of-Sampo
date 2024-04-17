using Codice.Client.BaseCommands.BranchExplorer;
using Sampo.AI;
using Sampo.Core;
using Sampo.Player.Economy;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Building
{
    /*TODO : ��� ����� ������ ���-�� ��������, ����� ���� ��������� NullUnit'��,
     * ��� ����� ���� ����� ������� ������� ����������� ���������
    */
    /// <summary>
    /// ���������, ��� �������� ���������� � ���� ���������� ������.
    /// ��� �������� �������� �� ��� ������ �����, � ��� �� �������������� ��� ��������
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
            BuildingsManager.Instance.RequestNullUnits(this, bufferingAmount);
        }

        public void Contain(GameObject obj)
        {
            if (contained.Count >= bufferingAmount)
            {
                GetComponent<Faction>().IsAvailableForSelfFaction = false;
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
                Debug.LogError("������� ������� �����, �������� ��� � ���� ������", transform);
                return;
            }

            BuildingsManager.Instance.RequestNullUnits(this, 1);

            obj.transform.SetPositionAndRotation(releasePos.position, releasePos.rotation);
            obj.transform.parent = Variable_Provider.Instance.unitsContainer;
            obj.SetActive(true);

            if (contained.Count <= bufferingAmount)            
                GetComponent<Faction>().IsAvailableForSelfFaction = false;
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
            //TODO : ��������� ����� ���������
        }

        public float GetInteractionRange()
        {
            return interactionRange;
        }
    }
}