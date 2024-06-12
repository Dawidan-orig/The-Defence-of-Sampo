using Sampo.Core;
using Sampo.Player.Economy;
using System.Collections;
using UnityEngine;

namespace Sampo.Building.Spawners
{
    /// <summary>
    /// ���, �� �������� ���������� �����-��������.
    /// ��� ����� ����� ������������� �� ���� ������, ����� ����������
    /// </summary>
    public class NullUnitSpawner : BuildableStructure, IInteractable
    {
        //TODO? : ������� ����������� ������� ��������������,
        //������� ���������� exception ���� ���� �� ��������� ����� Unity
        //���� Alchemy
        public float frequency = 10;
        public int limitAddition = 10;
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
            BuildingsManager.Instance.AddNewSpawner(this);
        }
        private void OnDestroy()
        {
            BuildingsManager.Instance.RemoveSpawner(this);
            BuildingsManager.Instance.NullUnitLimit -= limitAddition;
        }

        public void Interact(Transform interactor)
        {
            //???
        }

        public void PlayerInteract()
        {
            //TODO : ��������� ����� ���������
        }

        protected override void Build()
        {
            BuildingsManager.Instance.NullUnitLimit += limitAddition;
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
                BuildingsManager.Instance.CreateNewNullUnit(transfromSpawnPos);
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