using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class WaveHandler : MonoBehaviour
{
    private static WaveHandler _instance;
    [HideInInspector]
    public static WaveHandler Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<WaveHandler>();
            if (_instance == null)
            {
                GameObject go = new("Wave Controlling Singleton");
                _instance = go.AddComponent<WaveHandler>();
            }

            /*
            if (EditorApplication.isPlaying)
            {
                _instance.transform.parent = null;
                DontDestroyOnLoad(_instance.gameObject);
            }*/

            return _instance;
        }
        private set { }
    }

    [Header("Setup")]
    public Transform container;
    [Tooltip("Волны, что используются исключительно в Editor и являют собой заранее созданные палитры")]
    public List<WaveData> prefabPalletes = new List<WaveData>();
    [Tooltip("Распределение очков для создания юнитов.")]
    public AnimationCurve wavePointDistribution;

    [Header("Constraints")]
    public const int NORMAL_POINTS = 100; // Норма очков на одного боеспособного юнита, каковым является мечник с такими себе показателями.
    public float wave_power = 10000;
    public int units_amount = 100;

    [Header("Lookonly")]
    [SerializeField]
    private List<GameObject> unitPrefabsToSpawn = new List<GameObject>();

    public GameObject GetSpawnedUnit(Vector3 onPosition, Faction.FType ofFactionType, Quaternion withRotation = default) 
    {     
        if (unitPrefabsToSpawn.Count == 0)
            return null;

        bool activeSave = unitPrefabsToSpawn[0].activeSelf;
        unitPrefabsToSpawn[0].SetActive(false);
        GameObject unit = Instantiate(unitPrefabsToSpawn[0], onPosition, withRotation, container);
        unit.GetComponent<Faction>().ChangeFactionCompletely(ofFactionType);
        unit.SetActive(activeSave);
        unitPrefabsToSpawn[0].SetActive(activeSave);
        unitPrefabsToSpawn.RemoveAt(0);
        return unit;
    }
    public int GetAmountOfUnitsToSpawn() 
    {
        return unitPrefabsToSpawn.Count;
    }

    private void FormFromPallete(Pallete givenPallete) 
    {
        float remainedPower = wave_power;
        int toSpawn = units_amount;

        float middleValue_PointsForUnit = wave_power / units_amount;

        while (toSpawn > 0 && remainedPower > 0)
        {
            float generationValue = Random.value;

            int usedPoints = Mathf.RoundToInt(wavePointDistribution.Evaluate((units_amount - toSpawn) / units_amount) * middleValue_PointsForUnit);

            //Debug.Log(givenPallete.Pass(generationValue).GetType());
            GameObject newUnitPrefab = (GameObject)givenPallete.Pass(generationValue);
            newUnitPrefab.GetComponent<IPointsDistribution>().AssignPoints(usedPoints);
            unitPrefabsToSpawn.Add(newUnitPrefab);

            toSpawn--;
            remainedPower -= usedPoints;
        }
    }

    public void UsePrefabPallete() // используем заранее созданные палитры юнитов
    {
        unitPrefabsToSpawn.Clear();
        int chosenPalleteIndex = Random.Range(0, prefabPalletes.Count);

        Pallete former = prefabPalletes[chosenPalleteIndex].enemies;

        FormFromPallete(former);
    }

    public void FormProceduralPalette() // Создаём сбалансированную палитру юнитов процедурно
    {
        unitPrefabsToSpawn.Clear();

        //TODO (Когда будет много систем, пока что - ядерная бомба) : Процедурная палитра из наборов правил
    }
}
