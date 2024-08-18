using UnityEngine;
using WingedCore.AI;

namespace WingedCore.DebugSystems
{
    public class VariableProvider : MonoBehaviour
    {
        public Transform unitsContainer;
        public Transform buildingsContainer;

        //TODO : Сделать из этого соответствующие GameObject'ы для всех фракций юнитов. Убрать это в UtilityAIManager
        //TODO : Перевести в Blackboard от Git-Amend
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

        public bool useDebugColors;

        public void DebugChangeColors(GameObject target, int faction)
        {
#if UNITY_EDITOR
            if (useDebugColors)
            {
                Material material = null;

                switch (faction)
                {
                    case 1: material = neutral; break;
                    case 2: material = friend; break;
                    case 3: material = enemy; break;
                    case 4: material = agro; break;
                }

                if (material != null)
                    foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
                    {
                        if (renderer.gameObject.TryGetComponent(out TMPro.TextMeshPro _))
                            continue;

                        renderer.sharedMaterial = material;
                    }
            }
#endif
        }
    }
}