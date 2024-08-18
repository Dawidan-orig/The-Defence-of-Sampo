using WingedCore.AI;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Waves
{
    public class WaveHandler : MonoBehaviour
    {
        [Header("Setup")]
        public Transform container;
        [Tooltip("¬олны, что используютс€ исключительно в Editor и €вл€ют собой заранее созданные палитры")]
        public List<WaveData> prefabPalletes = new List<WaveData>();
        [Tooltip("–аспределение очков дл€ создани€ юнитов.")]
        public AnimationCurve wavePointDistribution;

        [Header("Constraints")]
        [Tooltip("Ќорма очков на одного боеспособного юнита, каковым €вл€етс€, например, обычный мечник с такими себе показател€ми.")]
        public const int NORMAL_POINTS = 100;
        public float wave_power = 10000;
        public int units_amount = 100;

        public System.Action OnPreWave;

        [Header("Lookonly")]
        [SerializeField]
        private Stack<KeyValuePair<GameObject, int>> unitPrefabsToSpawn = new Stack<KeyValuePair<GameObject, int>>();

        public GameObject GetSpawnedUnit(Vector3 onPosition, Quaternion withRotation = default)
        {
            if (unitPrefabsToSpawn.Count == 0)
                return null;

            var kvp = unitPrefabsToSpawn.Pop();

            GameObject unit = Instantiate(kvp.Key, onPosition, withRotation, container);
            unit.GetComponent<IPointsDistribution>().AssignPoints(kvp.Value);
            return unit;
        }
        public int GetAmountOfUnitsToSpawn()
        {
            return unitPrefabsToSpawn.Count;
        }

        private void FormFromPallete(Pallete givenPallete)
        {
            OnPreWave?.Invoke();

            float remainedPower = wave_power;
            int toSpawn = units_amount;

            float middleValue_PointsForUnit = wave_power / units_amount;

            while (toSpawn > 0 && remainedPower > 0)
            {
                float generationValue = Random.value;

                int usedPoints = Mathf.RoundToInt(wavePointDistribution.Evaluate((units_amount - toSpawn) / units_amount) * middleValue_PointsForUnit);

                //Debug.Log(givenPallete.Pass(generationValue).GetType());
                GameObject newUnitPrefab = (GameObject)givenPallete.Pass(generationValue);
                unitPrefabsToSpawn.Push(KeyValuePair.Create(newUnitPrefab, usedPoints));

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
    }
}