using UnityEngine;

namespace WingedCore.AI
{
    public class AITarget : MonoBehaviour
    // Предоставляет менеджеру вес GameObject'а, делая его одной из возможных целей UtilityAI.
    // Определяет также и группу-фракцию
    {
        public int ai_weight = 1;

        public System.Action factionChanged;

        //[Alchemy.Inspector.HideInPlayMode]
        [SerializeField] private FactionType _currentFaction = FactionType.none;
        public FactionType FactionType { get => _currentFaction; }
        public bool IsAvailableForSelfFaction
        {
            get => isAvailableForSelfFaction;

            set
            {
                bool prev = isAvailableForSelfFaction;
                isAvailableForSelfFaction = value;

                if (value == false && prev == true)
                    MonoBehaviourSingleton<AITargetManager>.Instance.RemoveFromFaction(_currentFaction, this);
                else if (value == true && prev == false)
                    MonoBehaviourSingleton<AITargetManager>.Instance.AddAsNewInteractable(this);

                //TODO : Простое использоваие AITargetManager.Instance.UpdateAIInfo(this);
            }
        }
        [SerializeField]
        private bool isAvailableForSelfFaction = false;

#if UNITY_EDITOR
        [Header("Debug")]
        public bool useDebugColors = false;

#endif

        #region Unity
        private void OnValidate()
        {
            DebugChangeColors();
        }

        private void Awake()
        {
            DebugChangeColors();
        }

        protected virtual void OnEnable()
        {
            if (_currentFaction != FactionType.none)
                MonoBehaviourSingleton<AITargetManager>.Instance.AddAsNewInteractable(this);
        }

        protected virtual void OnDisable()
        {
            //TODO : Найти способ не обращаться к Instance, если происходит завершение игры
            if (_currentFaction != FactionType.none)
                MonoBehaviourSingleton<AITargetManager>.Instance.RemoveInteractableCompletely(this);
        }
        #endregion

        /// <summary>
        /// Полноценная смена фракции, из-за которой боевая сторона меняется полностью и без возможности восстановления.
        /// </summary>
        public void ChangeFactionCompletely(FactionType newFactionType)
        {
            MonoBehaviourSingleton<AITargetManager>.Instance.RemoveFromFaction(_currentFaction, this);
            _currentFaction = newFactionType;
            MonoBehaviourSingleton<AITargetManager>.Instance.AddAsNewInteractable(this);

            factionChanged?.Invoke();

            /*
             * _ftype = newFactionType;
             * AITargetManager.Instance.UpdateAIInfo(this);
             */

            DebugChangeColors();
        }

        public bool IsWillingToAttack(FactionType type)
        {
            bool comparedFactions = _currentFaction != type; // На будущее, если вдруг захочу какие-нибудь альянсы.

            return (comparedFactions || _currentFaction == FactionType.aggressive) && _currentFaction != FactionType.neutral;
        }

        protected virtual void DebugChangeColors() 
        {
#if UNITY_EDITOR
            if (useDebugColors)
            {
                WingedCore.DebugSystems.VariableProvider provider = MonoBehaviourSingleton<WingedCore.DebugSystems.VariableProvider>.Instance;

                Material material = null;

                switch (_currentFaction)
                {
                    case FactionType.sampo: material = provider.friend; break;
                    case FactionType.enemy: material = provider.enemy; break;
                    case FactionType.aggressive: material = provider.agro; break;
                    case FactionType.neutral: material = provider.neutral; break;
                }

                if (material != null)
                    foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
                    {
                        renderer.sharedMaterial = material;
                    }
            }
#endif
        }
    }
}