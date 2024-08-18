using Alchemy.Inspector;
using Alchemy.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.Core.Balance
{
    /// <summary>
    /// Эта система отвечает за глобальный баланс вклада игрока(-ов) в мир,
    /// И предполагаемую силу воздействия мира обратно.
    /// Singleton.
    /// </summary>
    public class CentralBalance : MonoBehaviour
    {
        /*
         * Как предполагается его работа:
         * - Все мира препятствия можно обернуть в число очков, выражаемое в Факторе Хаоса.
         * - Фактор хаоса - это абсолютное число, отражающее силу и возможности мира по влиянию на игрока.
         * - Аболютность означает текущее равновесие баланса.
         * - То-есть чем выше это значение - тем, можно сказать, сложнее для игрока.
         * - Предполагается, что это число будет изменяться не только по своим закономерностям
         * (Увеличиваться со временем, менятся относительно закономерностей)
         * Но и относительно действий игрока.
         * - Вклад игрока считается отдельно и вносится сюда.
         * - Дальше нужно оценить, насколько вклад игрока велик.
         * - Оценка идёт относительно силы мира.
         * - Мир подтягивается, в зависимости от вклада игрока,
         * - И соответственно спадает, если игрок не вытягивает уже сам.
         * 
         * Есть проблема - Игрок может выстроить среднюю оборону, игра посчитает её слишком сильной,
         * И мир среагирует кардинально, сметя всё на своём пути.
         * Решение - уменшить количество очков этой самый обороны. Всо.
         */

        public float chaosIntesivity = 0.7f;

        [Tooltip("Текущее количество очков мира, либо же ещё фактор хаоса." +
            "\r\nПредполагается, что чем это число выше, тем больше всего может произойти.")]
        [SerializeField, ReadOnly]
        int chaosFactor = 500;
        [Tooltip("Влияение игрока на мир за текущую итерацию")]
        [SerializeField, ReadOnly]
        int playerInfluence = 0;

        public void SetPlayerInfluence(int precountedPlayerInfl) 
        {
            playerInfluence = precountedPlayerInfl;
        }

        /// <summary>
        /// Балансирует влияение игрока и мира между собой.
        /// </summary>
        /// <returns>Баланса мира и игрока. Если значение уход в минус - игрок сильнее. В плюс - мир сильнее.</returns>
        public int CalculateBalance() 
        {
            int res = chaosFactor - playerInfluence;

            chaosFactor = (int) Mathf.Lerp(chaosFactor, playerInfluence, chaosIntesivity);

            return res;
        }
    }
}