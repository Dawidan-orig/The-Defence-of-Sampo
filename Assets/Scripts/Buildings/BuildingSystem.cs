using Sampo;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sampo.Building
{
    
    public class BuildingSystem : MonoBehaviour
    {
        private static BuildingSystem _instance;
        public static BuildingSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<BuildingSystem>();
                if (_instance == null)
                {
                    GameObject go = new("Building System");
                    _instance = go.AddComponent<BuildingSystem>();
                }

                if (EditorApplication.isPlaying)
                {
                    _instance.transform.parent = null;
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        public Transform structureParent;
        //TODO : Убрать это, слишком лобовое решение. Оно временно пока нет полноценной системы
        public List<GameObject> prefabs = new List<GameObject>();

        private WallPylon _lastPlacedWall;
        private BuildableStructure chosenStructToBuild;
        //Напишу описания вот в таком стиле. Если есть несовпадения - это повод задуматься, значит что-то не так.
        /// <summary>
        /// Выбирается и меняется через UI и другие стены
        /// </summary>
        public WallPylon CurrentWallInFocus { get => _lastPlacedWall; set => _lastPlacedWall = value; }
        /// <summary>
        /// Выбирается через UI
        /// </summary>
        public BuildableStructure ChosenStructureToBuild { get => chosenStructToBuild; set => chosenStructToBuild = value; }
    }
}