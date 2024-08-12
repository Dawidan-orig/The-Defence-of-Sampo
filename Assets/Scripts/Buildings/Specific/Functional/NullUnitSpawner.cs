using Alchemy.Inspector;
using WingedCore.Core;
using System.Collections;
using UnityEngine;
using Sampo.Economy;

namespace WingedCore.Building.Spawners
{
    /// <summary>
    /// Дом, из которого появляются юниты-пустышки.
    /// Эти юниты потом преобразуются во всех других, более конкретных
    /// </summary>
    public class NullUnitSpawner : BuildableStructure, IInteractable
    {
        //TODO? : Gizmo для отображение появляемого юнита
        public float frequency = 10;
        public int limitAddition = 10;
        public Vector3 spawnPosRelative = Vector3.up;
        public Quaternion spawnRotRelative = Quaternion.identity;

        public GameObject prefabToSpawn;

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
            MonoBehaviourSingleton<ResourcesRequestManager>.Instance.AddNewSpawner(this);
        }
        private void OnDestroy()
        {
            MonoBehaviourSingleton<ResourcesRequestManager>.Instance.RemoveSpawner(this);
            MonoBehaviourSingleton<ResourcesRequestManager>.Instance.NullUnitLimit -= limitAddition;
        }

        public void Interact(Transform interactor)
        {
            //???
        }

        public void PlayerInteract()
        {
            //TODO : Настройка через интерфейс
        }

        protected override void Build()
        {
            MonoBehaviourSingleton<ResourcesRequestManager>.Instance.NullUnitLimit += limitAddition;
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
                MonoBehaviourSingleton<ResourcesRequestManager>.Instance.CreateNewNullUnit(
                    transform.TransformPoint(spawnPosRelative), transform.rotation * spawnRotRelative);
                toSpawn--;
                yield return new WaitForSeconds(frequency);
            }
        }

        public float GetInteractionRange()
        {
            throw new System.NotImplementedException();
        }

        private void OnDrawGizmosSelected()
        {
            

            if (prefabToSpawn != null)
            {
                Mesh res = null;
                var sMeshRenderer = prefabToSpawn.GetComponentInChildren<SkinnedMeshRenderer>();
                if (sMeshRenderer.sharedMesh != null)
                    res = sMeshRenderer.sharedMesh;
                
                if(res == null)
                {
                    var meshFilter = prefabToSpawn.GetComponentInChildren<MeshFilter>();
                    if(meshFilter != null)
                        res = meshFilter.sharedMesh;
                }

                Gizmos.color = Color.blue;
                Gizmos.DrawWireMesh(res, transform.TransformPoint(spawnPosRelative), transform.rotation * spawnRotRelative);
            }
        }
    }
}