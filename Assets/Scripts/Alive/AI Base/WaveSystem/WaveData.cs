using Sampo.AI;
using UnityEngine;

namespace Sampo
{
    [CreateAssetMenu(fileName = "New Wave Pallete", menuName = "Scriptable/Wave Data", order = 0)]
    public class WaveData : ScriptableObject
    {
        public Pallete enemies;

        /// <summary>
        /// Проверяем, что все объекты палитры - ИИ.
        /// </summary>
        public bool CheckPalleteData()
        {
            bool res = true;
            foreach (var enemy in enemies.GetPalleteObjectsRaw())
            {
                if (enemy.obj is not TargetingUtilityAI)
                {
                    res = false; break;
                }
            }

            return res;
        }
    }
}