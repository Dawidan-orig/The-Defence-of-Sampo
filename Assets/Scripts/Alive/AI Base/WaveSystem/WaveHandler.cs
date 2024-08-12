using WingedCore.AI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WingedCore
{
    public class WaveHandler : MonoBehaviour
    {
        [Header("Setup")]
        public Transform container;
        [Tooltip("Волны, что используются исключительно в Editor и являют собой заранее созданные палитры")]
        public List<WaveData> prefabPalletes = new List<WaveData>();
        [Tooltip("Распределение очков для создания юнитов.")]
        public AnimationCurve wavePointDistribution;

        [Header("Constraints")]
        [Tooltip("Норма очков на одного боеспособного юнита, каковым является, например, обычный мечник с такими себе показателями.")]
        public const int NORMAL_POINTS = 100;
        public float wave_power = 10000;
        public int units_amount = 100;

        [Header("Lookonly")]
        [SerializeField]
        private List<GameObject> unitPrefabsToSpawn = new List<GameObject>();

        public GameObject GetSpawnedUnit(Vector3 onPosition, FactionType ofFactionType, Quaternion withRotation = default)
        {
            if (unitPrefabsToSpawn.Count == 0)
                return null;

            GameObject unit = Instantiate(unitPrefabsToSpawn[0], onPosition, withRotation, container);
            unit.GetComponent<AITarget>().ChangeFactionCompletely(ofFactionType);
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

        public void UseRandomPrefabPallete() // используем заранее созданные палитры юнитов
        {
            unitPrefabsToSpawn.Clear();
            int chosenPalleteIndex = Random.Range(0, prefabPalletes.Count);

            Pallete former = prefabPalletes[chosenPalleteIndex].enemies;

            FormFromPallete(former);
        }

        public void FormProceduralPalette() // Создаём сбалансированную палитру юнитов процедурно
        {
            unitPrefabsToSpawn.Clear();
        }
    }
}