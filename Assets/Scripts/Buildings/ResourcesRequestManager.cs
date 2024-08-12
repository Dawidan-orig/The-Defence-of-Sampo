using WingedCore.Building.Spawners;
using WingedCore.Core;
using System.Collections.Generic;
using UnityEngine;
using WingedCore.AI;

namespace Sampo.Economy
{
    public class ResourcesRequestManager : MonoBehaviour
    {
        //TODO : Сделать универсальной для любой из фракций, а не только для игрока

        [SerializeField]
        private GameObject nullUnitPrefab;

        //TODO? : Преобразовать в более универсальную систему, которая позволяет работать с любым типом юнитов
        [SerializeField]
        private int nullUnitLimit = 0;
        private int stashedRequestedAmount = 0;

        [SerializeField]
        List<NullUnitSpawner> _nullUnitSpawners;

        public int NullUnitLimit { get => nullUnitLimit; set => nullUnitLimit = value; }

        private void Awake()
        {
            nullUnitPrefab ??= Resources.Load<GameObject>("NullUnit");
            _nullUnitSpawners = new();
        }
        public void AddNewSpawner(NullUnitSpawner spawner)
        {
            if (_nullUnitSpawners.Count == 0 && stashedRequestedAmount > 0)
            {
                //TODO : Перераспределение значений спавна юнитов между всеми спавнерами при добавлении нового.
                //Вариант решения: Собрать все уже имеющиеся toSpawn'ы, сохранить в одно значение и пульнуть это в RequestNullUnits.
                spawner.AddUnitsToSpawn(stashedRequestedAmount);
                stashedRequestedAmount = 0;
            }

            _nullUnitSpawners.Add(spawner);
        }
        public void RemoveSpawner(NullUnitSpawner spawner)
        {
            _nullUnitSpawners.Remove(spawner);
        }

        public void CreateNewNullUnit(Transform spawnPos)
        {
            CreateNewNullUnit(spawnPos.position, spawnPos.rotation);
        }
        public void CreateNewNullUnit(Vector3 spawnPos, Quaternion rotation)
        {
            var unit = Instantiate(nullUnitPrefab, spawnPos, rotation, MonoBehaviourSingleton<WingedCore.DebugSystems.VariableProvider>.Instance.unitsContainer);
            unit.GetComponent<AITarget>().ChangeFactionCompletely(FactionType.sampo);
        }

        public void RequestNullUnits(IInteractable requestFor, int amount)
        {
            //TODO : Вывод из зданий-буферов (Приоритетнее)

            _nullUnitSpawners.Sort((spawner1, spawner2) => spawner2.ToSpawn.CompareTo(spawner1.ToSpawn));

            if (_nullUnitSpawners.Count == 0)
            {
                stashedRequestedAmount += amount;
                return;
            }
            if (_nullUnitSpawners.Count == 1)
            {
                _nullUnitSpawners[0].AddUnitsToSpawn(amount);
                return;
            }
            for (int i = 0; i < _nullUnitSpawners.Count - 1; i++)
            {
                int currentValue = _nullUnitSpawners[i].ToSpawn;
                int upTo = _nullUnitSpawners[i + 1].ToSpawn;

                if (currentValue == upTo)
                    continue;

                int diff = upTo - currentValue;
                int toAdd = Mathf.Clamp(amount, 0, diff);
                for (int j = 0; j < i + 1; j++)
                {
                    _nullUnitSpawners[j].AddUnitsToSpawn(toAdd);
                    amount -= toAdd;
                    if (amount == 0)
                        break;
                }
                if (amount == 0)
                    break;
            }
            //TODO? : Юниты должны быть распределены с приоритетным Action после своего создания. Хотя, впрочем, они и без этого сами всё нормально делают
        }
    }
}