using Alchemy.Inspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.AI.CounterSystem
{
    /// <summary>
    /// Узел контр-атаки, в котором назначается 
    /// </summary>
    [CreateAssetMenu(fileName = "Unspecified Unit Role", menuName = "Scriptable/Units/Unit Role")]
    public class CounterNode : ScriptableObject
    {
        [Serializable]
        public struct CounterAbility 
        {
            public CounterNode counterWho;
            [Tooltip("Насколько сильно идёт контратака?\nМожно и дробные значения")]
            [Min(0)]
            public float counterRating;
        }

        public List<CounterAbility> counterWho = new();
    }
}