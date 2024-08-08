using System;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.AI.CounterSystem
{
    /// <summary>
    /// Узел контр-атаки, в котором назначается 
    /// </summary>
    [Alchemy.Serialization.AlchemySerialize]
    [CreateAssetMenu(fileName = "Unspecified Unit Role", menuName = "Scriptable/Units/Unit Role")]
    public partial class CounterNode : ScriptableObject
    {
        [Alchemy.Serialization.AlchemySerializeField, NonSerialized]
        public Dictionary<CounterNode, float> counterWho = new();
    }
}