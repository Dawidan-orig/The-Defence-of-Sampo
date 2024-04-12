using Sampo.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Building
{
    //TODO : ��� ����� ������ ���-�� ��������, ����� ���� ��������� NullUnit'��,
    // ��� ����� ���� ����� ������� ������� ����������� ���������
    /// <summary>
    /// ���������, ��� �������� ���������� � ���� ���������� ������.
    /// ��� �������� �������� �� ��� ������ �����, � ��� �� �������������� ��� ��������
    /// </summary>
    public class BufferingHouse : BuildableStructure, IInteractable
    {
        public int bufferingAmount = 10;
        public float interactionRange = 5;
        public Transform releasePos;
        public Transform unitContainer;

        List<GameObject> contained;

        protected override void Build()
        {
            contained = new List<GameObject>();

            unitContainer = Variable_Provider.Instance.unitsContainer;
        }

        public void Contain(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.parent = transform;
            contained.Add(obj);

            if (contained.Count >= bufferingAmount)
                GetComponent<Interactable_UtilityAI>().enabled = false;
        }

        public void Release(GameObject obj)
        {
            obj.transform.position = releasePos.position;
            obj.transform.rotation = releasePos.rotation;
            obj.transform.parent = unitContainer;
            obj.SetActive(true);

            if (contained.Count <= bufferingAmount)
            {
                GetComponent<Interactable_UtilityAI>().enabled = true;
            }
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