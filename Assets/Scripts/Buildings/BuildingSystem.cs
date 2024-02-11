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

        private WallPylon _lastPlacedWall;
        public WallPylon CurrentWallInFocus { get => _lastPlacedWall; set => _lastPlacedWall = value; }
    }
}