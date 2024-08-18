using WingedCore.AI;
using UnityEngine;

namespace Sampo.Waves
{
    [CreateAssetMenu(fileName = "New Wave Pallete", menuName = "Scriptable/Wave Data", order = 0)]
    public class WaveData : ScriptableObject
    {
        public Pallete enemies;
    }
}