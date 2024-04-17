using Sampo.Building.Spawners;
using Sampo.Core;
using Sampo.Player.Economy;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildingsManager : MonoBehaviour
{
    private static BuildingsManager _instance;

    public static BuildingsManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<BuildingsManager>();

            if (_instance == null)
            {
                GameObject go = new("BuildingsManager");
                _instance = go.AddComponent<BuildingsManager>();
            }

            if (EditorApplication.isPlaying)
            {
                _instance.transform.parent = null;
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    GameObject nullUnitPrefab;

    //TODO? : Преобразовать в более универсальную систему, которая позволяет работать с любым типом юнитов
    [SerializeField]
    private int nullUnitLimit = 0;
    private int stashedRequestedAmount = 0;
    public int NullUnitLimit { get => nullUnitLimit; set => nullUnitLimit = value; }

    [SerializeField]
    List<NullUnitSpawner> nullUnitSpawners;
    public void AddNewSpawner(NullUnitSpawner spawner) 
    {
        if (nullUnitSpawners.Count == 0)
        {
            //TODO : Перераспределение значений спавна юнитов между всеми спавнерами при добавлении нового.
            //Вариант решения: Собрать все уже имеющиеся toSpawn'ы, сохранить в одно значение и пульнуть это в RequestNullUnits.
            spawner.AddUnitsToSpawn(stashedRequestedAmount);
            stashedRequestedAmount = 0;
        }

        nullUnitSpawners.Add(spawner);
    }
    public void RemoveSpawner(NullUnitSpawner spawner) 
    {
        nullUnitSpawners.Remove(spawner);
    }

    private void Awake()
    {
        nullUnitPrefab = Resources.Load<GameObject>("NullUnit");
        nullUnitSpawners = new();
    }

    public void CreateNewNullUnit(Transform spawnPos)
    {
        Instantiate(nullUnitPrefab, spawnPos.position, spawnPos.rotation, Variable_Provider.Instance.unitsContainer);
    }
    public void CreateNewNullUnit(Vector3 spawnPos, Quaternion rotation)
    {
        Instantiate(nullUnitPrefab, spawnPos, rotation, Variable_Provider.Instance.unitsContainer);
    }

    public void RequestNullUnits(IInteractable requestFor, int amount)
    {
        //TODO : Вывод из зданий-буферов 

        nullUnitSpawners.Sort((spawner1, spawner2) => spawner2.ToSpawn.CompareTo(spawner1.ToSpawn));

        if (nullUnitSpawners.Count == 0) {
            stashedRequestedAmount = amount;
            return;
                }
        if (nullUnitSpawners.Count == 1)
        {
            nullUnitSpawners[0].AddUnitsToSpawn(amount);
            return;
        }
        for(int i = 0; i <  nullUnitSpawners.Count-1; i++) 
        {
            int currentValue = nullUnitSpawners[i].ToSpawn;
            int upTo = nullUnitSpawners[i + 1].ToSpawn;

            if (currentValue == upTo)
                continue;

            int diff = upTo - currentValue;
            int toAdd = Mathf.Clamp(amount, 0, diff);
            for (int j = 0; j < i+1; j++) 
            {
                nullUnitSpawners[j].AddUnitsToSpawn(toAdd);
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
