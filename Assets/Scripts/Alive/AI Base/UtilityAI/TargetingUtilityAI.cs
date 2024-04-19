using Sampo.AI.Conditions;
using Sampo.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI
{
    //TODO : Refactor, этот класс подвергся очень большому количеству изменений и тут осталось много мусора

    //TODO!!! : Добавить Logger или другую систему логирования.
    // Надо понимать, что происходит с каждым юнитом в пределах Editor'а.

    [SelectionBase]
    [RequireComponent(typeof(IMovingAgent))]
    [RequireComponent(typeof(Faction))]
    [RequireComponent(typeof(AIBehaviourBase))]
    public class TargetingUtilityAI : MonoBehaviour
    // ИИ, ставящий приоритеты выполнения действий
    // Использует StateMachine в качестве исполнителя
    {
        public bool _AIActive = true;

        [Header("Setup")]
        [Tooltip("Влияние дистанции на выбор этого ИИ")]
        public float distanceWeightMultiplier = 1;
        [Tooltip("Влияние других ИИ на цель выбора этого ИИ")]
        public float congestionMultiplier = 1;

        [Header("lookonly")]
        [SerializeField]
        protected AIAction _currentActivity;
        [SerializeField]
        protected List<AIAction> _possibleActions = new();

        IMovingAgent _movingAgent;
        private AIAction _noAction;
        private AIBehaviourBase _behaviourAI;
        private bool _hasCongestion = true;
        protected UtilityAI_Factory _factory;
        protected UtilityAI_BaseState _currentState;

        public UtilityAI_BaseState CurrentState { get => _currentState; set => _currentState = value; }
        public AIAction CurrentActivity { get => _currentActivity; }
        public IMovingAgent MovingAgent { get => _movingAgent; set => _movingAgent = value; }
        public AIBehaviourBase BehaviourAI { get => _behaviourAI ??= GetComponent<AIBehaviourBase>(); }
        public bool HasCongestion { get => _hasCongestion;
            set 
            {
                bool prev = value;
                _hasCongestion = prev;
                if(prev == true && value == false)
                    UtilityAI_Manager.Instance.ChangeCongestion(
                    _currentActivity.target.GetComponent<Interactable_UtilityAI>(),
                    -BehaviourAI.VisiblePowerPoints);

                if (prev == false && value == true)
                    UtilityAI_Manager.Instance.ChangeCongestion(
                    _currentActivity.target.GetComponent<Interactable_UtilityAI>(),
                    BehaviourAI.VisiblePowerPoints);
            }
        }

        [Serializable]
        public struct AIAction
        {
            private TargetingUtilityAI actionOf;

            public Transform target;
            public string name;
            private int baseWeight;
            private int distanceSubstraction;
            private int enemiesAmountSubstraction;
            private List<BaseAICondition> _conditions;
            public Tool actWith;
            public UtilityAI_BaseState whatDoWhenClose;

            [SerializeField]
            private int _totalWeight;

            public int TotalWeight
            {
                get => _totalWeight;
            }
            public void Update()
            {
                _conditions.RemoveAll(item => item == null || !item.IsConditionAlive);

                distanceSubstraction =
                        Mathf.RoundToInt(
                            Vector3.Distance(
                                actionOf.transform.position, target.position)
                            * actionOf.distanceWeightMultiplier);

                if (actionOf.HasCongestion)
                    enemiesAmountSubstraction =
                        Mathf.RoundToInt(
                            UtilityAI_Manager.Instance.GetCongestion(
                                target.GetComponent<Interactable_UtilityAI>())
                            * actionOf.congestionMultiplier);

                _totalWeight = baseWeight - distanceSubstraction - enemiesAmountSubstraction;

                foreach (var c in _conditions)
                {
                    c.Update();
                    _totalWeight += c.WeightInfluence;
                }
            }
            public void Modify(BaseAICondition condition)
            {
                _conditions.Add(condition);
                Update();
            }

            #region constructors
            public AIAction(TargetingUtilityAI actionOf)
            {
                this.actionOf = actionOf;

                target = default;
                name = default;
                baseWeight = default;
                distanceSubstraction = default;
                actWith = default;
                whatDoWhenClose = default;

                enemiesAmountSubstraction = 0;
                _totalWeight = 0;

                _conditions = new();
            }
            //TODO : UtilityAI_BaseState - лучше спрятать в отдельный namespace
            public AIAction(TargetingUtilityAI actionOf, Transform target, string name, int weight, Tool actWith, UtilityAI_BaseState alignedState)
            {
                this.actionOf = actionOf;

                this.target = target;
                this.name = name;
                this.baseWeight = weight;
                this.actWith = actWith;
                whatDoWhenClose = alignedState;
                distanceSubstraction = 0;

                enemiesAmountSubstraction = 0;
                _totalWeight = 0;

                _conditions = new();

                Update();
            }
            #endregion

            #region overrides
            public override bool Equals(object obj)
            {
                if (!(obj is AIAction))
                    return false;

                AIAction casted = (AIAction)obj;

                return (target == casted.target) && (whatDoWhenClose == casted.whatDoWhenClose) && (actWith == casted.actWith);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(target, name, baseWeight, whatDoWhenClose);
            }

            public static bool operator ==(AIAction c1, AIAction c2)
            {
                /*
                if (c1 is null && c2 is null)
                    return true;
                else if (c1 is null && c2 is not null || c1 is not null && c2 is null)
                    return false;
                */

                return (c1.target == c2.target) && (c1.whatDoWhenClose == c2.whatDoWhenClose) && (c1.actWith == c2.actWith);
            }
            public static bool operator !=(AIAction c1, AIAction c2)
            {
                /*
                if (c1 is null && c2 is null)
                    return true;
                else if (c1 is null && c2 is not null || c1 is not null && c2 is null)
                    return false;
                */

                return (c1.target != c2.target) || (c1.name != c2.name) || (c1.actWith != c2.actWith);
            }
            #endregion
        }

        #region Unity

        protected virtual void Awake()
        {
            //TODO : Установка всех ключевых переменных прямо в OnValidate, один раз при создании
            // Это упростит создание новых юнитов в дальнейшем
            _movingAgent = GetComponent<IMovingAgent>();
            _factory = new UtilityAI_Factory(this);
            _currentState = _factory.Deciding();
        }

        protected virtual void OnEnable()
        {
            _AIActive = true;
        }

        protected virtual void Start()
        {
            UtilityAI_Manager.Instance.NewAdded += FetchNewActivityFromManager;
            UtilityAI_Manager.Instance.NewRemoved += RemoveActivityFromManager;

            _noAction = new AIAction(this);

            NullifyActivity();

            FetchAndAddAllActivities();
        }

        protected virtual void Update()
        {
            if (!_AIActive)
            {
                return;
            }

            _currentState.UpdateState();
        }

        protected virtual void FixedUpdate()
        {
            if (!_AIActive)
            {
                return;
            }

            _currentState.FixedUpdateState();
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            const float TIME_OF_BEING_ANGRY = 20;

            if (collision.gameObject.TryGetComponent<IDamageDealer>(out var dDealer))
                //TODO : Перейти на универсальый Condition, работающий на времени
                ModifyActionOf(dDealer.DamageFrom, new RespondToAttackCondition(TIME_OF_BEING_ANGRY));
        }

        protected virtual void OnDisable()
        {
            UtilityAI_Manager.Instance.NewAdded -= FetchNewActivityFromManager;
            UtilityAI_Manager.Instance.NewRemoved -= RemoveActivityFromManager;
            if (_currentActivity.target && HasCongestion)
                UtilityAI_Manager.Instance.ChangeCongestion(
                    _currentActivity.target.GetComponent<Interactable_UtilityAI>(),
                    -BehaviourAI.VisiblePowerPoints);
            NullifyActivity();
            _AIActive = false;
        }

        private void OnValidate()
        {

        }
        #endregion

        #region actions
        private void FetchAndAddAllActivities()
        {
            var dict = BehaviourAI.GetActionsDictionary();
            foreach (var kvp in dict)
            {
                Interactable_UtilityAI target = kvp.Key;
                int weight = kvp.Value;

                if (!BehaviourAI.IsTargetPassing(target.transform))
                    return;

                Tool toolUsed = BehaviourAI.ToolChosingCheck(target.transform);

                AddNewPossibleAction(target.transform, weight, target.transform.name, toolUsed, BehaviourAI.TargetReaction(target.transform));
            }
        }
        private void FetchNewActivityFromManager(object sender, UtilityAI_Manager.UAIData e)
        {
            Faction faction = GetComponent<Faction>();
            if (!faction.IsWillingToAttack(e.factionWhereChangeHappened))
                return;

            Interactable_UtilityAI target = e.newInteractable.Key;
            int weight = e.newInteractable.Value;

            if (!BehaviourAI.IsTargetPassing(target.transform))
                return;

            Tool toolUsed = BehaviourAI.ToolChosingCheck(target.transform);

            AIAction action = new AIAction(
                this, target.transform, name, weight, toolUsed, BehaviourAI.TargetReaction(target.transform));

            AddNewPossibleAction(action);
        }
        private void RemoveActivityFromManager(object sender, UtilityAI_Manager.UAIData e)
        {
            AIAction similar = _possibleActions.Find(item => item.target == e.newInteractable.Key.transform);

            if (_possibleActions.Contains(similar))
            {
                _possibleActions.Remove(similar);

                if (CurrentActivity == similar)
                    NullifyActivity();
            }
        }
        private void AddNewPossibleAction(AIAction action)
        {
            if (_possibleActions.Contains(action))
            {
                Debug.LogWarning("Уже был добавлен " + name, transform);
                return;
            }

            NormilizeAction(action);

            if (action.TotalWeight > CurrentActivity.TotalWeight)
                ChangeAction(action);

            _possibleActions.Add(action);
        }
        private void AddNewPossibleAction(Transform target, int weight, string name, Tool actWith, UtilityAI_BaseState treatment)
        {
            AIAction action = new AIAction(this, target, name, weight, actWith, treatment);

            AddNewPossibleAction(action);
        }
        private void ChangeAction(AIAction to)
        {
            if (!IsNoActionCurrently() && _currentActivity.target && HasCongestion) //Убираем влияние текущей цели
                UtilityAI_Manager.Instance.ChangeCongestion(
                    _currentActivity.target.GetComponent<Interactable_UtilityAI>(),
                    -BehaviourAI.VisiblePowerPoints);
            _currentActivity = to;
            if (HasCongestion)
                UtilityAI_Manager.Instance.ChangeCongestion(
                    _currentActivity.target.GetComponent<Interactable_UtilityAI>(),
                    BehaviourAI.VisiblePowerPoints);
        }
        public UtilityAI_BaseState SelectBestActivity()
        {
            if (_possibleActions.Count == 0)
                return null;

            NormilizeActions(); //TODO : Проверить и убрать, если в этом нет смысла. Но его наличие создаёт надёжность.

            if (_possibleActions.Count == 0)
                return null;

            int bestActivityIndex = 0;

            _possibleActions.Sort((i1, i2) => i2.TotalWeight.CompareTo(i1.TotalWeight));

            ChangeAction(_possibleActions[bestActivityIndex]);

            return _currentActivity.whatDoWhenClose;
        }
        private void NormilizeActions()
        {
            //TODO? : LINQ для красоты
            for (int i = 0; i < _possibleActions.Count; i++)
            {
                AIAction action = _possibleActions[i];

                if (action.target != null)
                {
                    NormilizeAction(action);
                }
                else
                {
                    _possibleActions.RemoveAt(i);
                    i--;
                }
            }
        }
        private void NormilizeAction(AIAction action)
        {
            action.Update();
        }
        private void NullifyActivity()
        {
            _currentActivity = _noAction; // Нужно, чтобы StateMachine перебросилась в состояние Decide и не ловила nullReference
        }
        public bool IsNoActionCurrently() => _currentActivity == _noAction;
        #endregion

        //TODO!!! : ПЕРЕПИСАТЬ ЭТО НАХРЕН! Пусть состояние выбираются процендурно, и с возможностью их выбора
        //Может быть даже сделать Factory состояний как отдельный Singleton, безо всех этих глупых переходов
        public UtilityAI_BaseState GetAttackState() => _factory.Attack();
        public UtilityAI_BaseState GetRepositionState() => _factory.Reposition();
        public UtilityAI_BaseState GetActionState() => _factory.Action();

        /// <summary>
        /// Обновление всех связанных с целью задач
        /// </summary>
        /// <param name="withCondition">Динамическое условие, что применяется на все задачи</param>
        public void ModifyActionOf(Transform target, BaseAICondition withCondition)
        {
            AIAction? best = null;
            foreach (var action in _possibleActions)
            {
                if (best == null)
                    best = action;

                if (action.target == target)
                {
                    action.Modify(withCondition);
                    if (action.TotalWeight > best.Value.TotalWeight)
                    {
                        best = action;
                    }
                }
            }
            if (best?.TotalWeight > CurrentActivity.TotalWeight)
                ChangeAction(best.Value);
        }

        /// <summary>
        /// Проверка, что в данный момент требуется выбрать новое состояние
        /// </summary>
        /// <returns></returns>
        public bool IsDecidingStateRequired()
        {
            return _currentActivity == _noAction;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            NavMeshCalculations.Cell cell = NavMeshCalculations.Instance.GetCell(transform.position);
            if (cell == null)
                return;

            cell.DrawGizmo();
            Gizmos.DrawLine(cell.Center(), transform.position);
        }
    }
}