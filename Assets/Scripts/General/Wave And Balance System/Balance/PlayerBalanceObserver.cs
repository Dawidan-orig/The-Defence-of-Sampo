using Alchemy.Inspector;
using Sampo.Factions;
using Sampo.Waves;
using UnityEngine;
using WingedCore.Core.Balance;
using WingedCore.Core.JournalLogger;

namespace Sampo.Balance
{
    /// <summary>
    /// Считает текущее влияние игрока на мир.
    /// Специфично для The Defence of Sampo.
    /// Singleton.
    /// </summary>
    //TODO : Сделать наследником общего класса от WingedMind
    public class PlayerBalanceObserver : MonoBehaviour
    {
        //TODO : Перевести на Generic Observer от Git-Amend.

        //Триада баланса в игре
        [SerializeField, ReadOnly]
        int buildingsInfl = 0;
        [SerializeField, ReadOnly]
        int playerCharacterInfl = 0;
        [SerializeField, ReadOnly]
        int sampoUnitsInfl = 0;

        [SerializeField, ReadOnly]
        float buildingsPart = 0;
        [SerializeField, ReadOnly]
        float playerCharacterPart = 0;
        [SerializeField, ReadOnly]
        float sampoUnitsPart = 0;

        // Подсчёт влияния строений:
        /*
         * - Относительно прочности
         * - Наносимый урон
         * - Блокирование пути (Установка на "Красные" тайлы, см. #3 HnP)
         */
        // Подсчёт влияния юнитов:
        /*
         * - Использование способностей
         * - Наносимый урон
         * - Текущая видимая стоимость (Возможно, часть лучше скрывать, имитируя нехватку информации у противника)
         */
        // Подсчёт влияния игрока:
        /*
         * По сути, каждое действие
         * - Наносимый урон
         * - Использование способностей
         * - Командование (Одновременно и к юнитам)
         * - Строительство (Одновременно и к строениям)
         */

        private void OnEnable()
        {
            MonoBehaviourSingleton<WaveHandler>.Instance.OnPreWave += SetBalanceParameters;
            BalanceInfluencer.OnInfluence += AddToInfluence;
        }
        private void OnDisable()
        {
            MonoBehaviourSingleton<WaveHandler>.Instance.OnPreWave -= SetBalanceParameters;
            BalanceInfluencer.OnInfluence -= AddToInfluence;
        }
        public void AddToInfluence(int toAdd, GameObject fromWho)
        {
            if (fromWho.GetComponent<WingedCore.AI.AITarget>().FactionType != FactionType.sampo)
                return;

            BalanceTriad? type = null;
            if (fromWho.TryGetComponent<WingedCore.Player.PlayerController>(out _))
                type = BalanceTriad.player;
            if (fromWho.TryGetComponent<WingedCore.AI.AIBehaviourBase> (out _))
                type = BalanceTriad.units;
            if (fromWho.TryGetComponent<WingedCore.Building.BuildableStructure>(out _))
                type = BalanceTriad.buildings;

            switch (type)
            {
                case BalanceTriad.player: playerCharacterInfl += toAdd; break;
                case BalanceTriad.units: sampoUnitsInfl += toAdd; break;
                case BalanceTriad.buildings: buildingsInfl += toAdd; break;
                default: Debug.LogWarning("Тип не совпал, так как не нашёлся соответствующий компонент!", fromWho); break;
            }

            RecalcParts();
        }
        public void AddToInfluence(int toAdd, GameObject toWho, string reason)
        {
            AddToInfluence(toAdd, toWho);
            LoggerSystem.Journal(reason, gameObject);
        }

        private void RecalcParts() 
        {
            float sum = buildingsInfl + playerCharacterInfl + sampoUnitsInfl;
            buildingsPart = buildingsInfl / sum;
            playerCharacterPart = playerCharacterInfl / sum;
            sampoUnitsPart = sampoUnitsInfl / sum;
        }

        private void SetBalanceParameters()
        {
            int powerSum = buildingsInfl + playerCharacterInfl + sampoUnitsInfl;
            CentralBalance overallBalance = MonoBehaviourSingleton<CentralBalance>.Instance;
            overallBalance.SetPlayerInfluence(powerSum);
            buildingsInfl = 0;
            playerCharacterInfl = 0;
            sampoUnitsInfl = 0;

            MonoBehaviourSingleton<WaveHandler>.Instance.wave_power =
                overallBalance.CalculateBalance() + powerSum;
        }
    }
}