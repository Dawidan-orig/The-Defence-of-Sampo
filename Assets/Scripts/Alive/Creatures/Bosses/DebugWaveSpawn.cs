using Alchemy.Inspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WingedCore.Core.Timers;

namespace WingedCore.DebugSystems
{
    public class DebugWaveSpawn : MonoBehaviour
    {
        public Bounds SpawnZone;
        public float timeToNewWave = 100;

        [SerializeField, ReadOnly]
        [Range(0, 1)]
        private float progress;

        private CountdownTimer waveTimer;

        private void Awake()
        {
            waveTimer = new(timeToNewWave);
            waveTimer.OnTimerStop += waveTimer.Reset;
            waveTimer.OnTimerStop += SpawnWave;
        }

        private void Update()
        {
            progress = waveTimer.CurrentTime/timeToNewWave;
        }

        private void OnEnable()
        {
            SpawnWave();
            waveTimer.Start();
        }

        private void OnDisable()
        {
            waveTimer.Stop();
        }

        private void SpawnWave() 
        {
            WaveHandler waves = MonoBehaviourSingleton<WaveHandler>.Instance;
            waves.UseRandomPrefabPallete();
            for(int i = 0; i < waves.GetAmountOfUnitsToSpawn(); i++) 
            {
                Vector3 from = SpawnZone.min;
                Vector3 to = SpawnZone.max;

                Vector3 pos = new(Random.Range(from.x,to.x),
                    1,
                    Random.Range(from.z, to.z));

                waves.GetSpawnedUnit(pos, FactionType.enemy);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(SpawnZone.center, SpawnZone.size);
        }
    }
}