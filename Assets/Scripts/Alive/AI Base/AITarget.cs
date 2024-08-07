using UnityEditor;
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
        [SerializeField]  private FactionType _currentFaction = FactionType.none;
        public FactionType FactionType { get => _currentFaction; }
        public bool IsAvailableForSelfFaction
        {
            get => isAvailableForSelfFaction;

            set
            {                
                bool prev = isAvailableForSelfFaction;
                isAvailableForSelfFaction = value;

                if (value == false && prev == true)
                    AITargetManager.Instance.RemoveFromFaction(_currentFaction, this);
                else if (value == true && prev == false)
                    AITargetManager.Instance.AddAsNewInteractable(this);

                //Просто: AITargetManager.Instance.UpdateAIInfo(this);
            }
        }
        [SerializeField]
        private bool isAvailableForSelfFaction = false;

        #region Unity
        protected virtual void OnEnable()
        {
            if (_currentFaction != FactionType.none)
                AITargetManager.Instance.AddAsNewInteractable(this);
        }

        private void Start()
        {
            /*
var visuals = GetComponentsInChildren<Renderer>();
foreach (Renderer renderer in visuals)
    switch (_ftype)
    {
        case FType.sampo: renderer.material = Variable_Provider.Instance.sampo; break;
        case FType.enemy: renderer.material = Variable_Provider.Instance.enemy; break;
        case FType.aggressive: renderer.material = Variable_Provider.Instance.agro; break;
    }
*/
        }

        protected virtual void OnDisable()
        {
            //TODO : Найти способ не обращаться к Instance, если происходит завершение игры
            if (_currentFaction != FactionType.none)
                AITargetManager.Instance.RemoveInteractableCompletely(this);
        }
        #endregion

        /// <summary>
        /// Полноценная смена фракции, из-за которой боевая сторона меняется полностью и без возможности восстановления.
        /// </summary>
        public void ChangeFactionCompletely(FactionType newFactionType)
        {
            AITargetManager.Instance.RemoveFromFaction(_currentFaction, this);
            _currentFaction = newFactionType;
            AITargetManager.Instance.AddAsNewInteractable(this);

            factionChanged?.Invoke();

            /*
             * _ftype = newFactionType;
             * AITargetManager.Instance.UpdateAIInfo(this);
             */
        }

        public bool IsWillingToAttack(FactionType type)
        {
            bool comparedFactions = _currentFaction != type; // На будущее, если вдруг захочу какие-нибудь альянсы.

            return (comparedFactions || _currentFaction == FactionType.aggressive) && _currentFaction != FactionType.neutral;
        }
    }
}