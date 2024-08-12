using UnityEngine;

namespace WingedCore.DebugSystems
{
    public class VariableProvider : MonoBehaviour
    {
        //TODO : Сделать из этого соответствующие GameObject'ы для всех фракций юнитов. Убрать это в UtilityAIManager
        public Transform unitsContainer;
        public Transform singletonsContainer;

        public Material friend;
        public Material enemy;
        public Material agro;
        public Material neutral;

        /// <summary>
        /// Это слой модификации очков для симуляции эмоций юнита.
        /// От юнита TODO зависит влияние этого слоя и его собственная эмоциональность
        /// </summary>
        public const int emotionalPointsLayer = 10000;
        /// <summary>
        /// Слой модифицкации очков для приказов этого юнита
        /// Приказы - это приоритетные Interactable,
        /// динамически изменяющие поведение юнита
        /// </summary>
        public const int orderPointsLayer = 1000;

        public LayerMask ground;
    }
}