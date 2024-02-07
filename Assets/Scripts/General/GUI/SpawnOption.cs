using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.GUI
{
    [CreateAssetMenu(fileName = "Spawnable Buyable", menuName = "Scriptable/Player/Spawnable", order = 0)]
    public class SpawnOption : ScriptableObject
    {
        [SerializeField]
        string path = "";
        [SerializeField]
        GameObject prefab;

        public GameObject Prefab { get => prefab; }
        public string Path { get => path;}
    }
}