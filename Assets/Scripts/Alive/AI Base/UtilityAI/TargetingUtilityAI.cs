using Sampo.AI.Conditions;
using Sampo.AI.Humans;
using Sampo.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI
{
    // TODO (Сложное!) : Поиск ближайших укрытий при массированном обстреле

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
            public AIBehaviourBase behaviour;

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
                            * behaviour.distanceInfluence);

                if (actionOf.HasCongestion)
                    enemiesAmountSubstraction =
                        Mathf.RoundToInt(
                            UtilityAI_Manager.Instance.GetCongestion(
                                target.GetComponent<Interactable_UtilityAI>())
                            * behaviour.congestionInfluence);

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
                behaviour = default;
                
                enemiesAmountSubstraction = 0;
                _totalWeight = 0;

                _conditions = new();
            }
            public AIAction(TargetingUtilityAI actionOf, Transform target, string name, int weight, AIBehaviourBase behaviour)
            {
                this.actionOf = actionOf;

                this.target = target;
                this.name = name;
                this.baseWeight = weight;
                this.behaviour = behaviour;
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

                return (target == casted.target) && (behaviour == casted.behaviour);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(target, name, baseWeight, behaviour);
            }

            public static bool operator ==(AIAction c1, AIAction c2)
            {
                /*
                if (c1 is null && c2 is null)
                    return true;
                else if (c1 is null && c2 is not null || c1 is not null && c2 is null)
                    return false;
                */

                return (c1.target == c2.target) && (c1.behaviour == c2.behaviour);
            }
            public static bool operator !=(AIAction c1, AIAction c2)
            {
                /*
                if (c1 is null && c2 is null)
                    return true;
                else if (c1 is null && c2 is not null || c1 is not null && c2 is null)
                    return false;
                */

                return (c1.target != c2.target) || (c1.name != c2.name) || (c1.behaviour != c2.behaviour);
            }
            #endregion
        }

        #region Unity

        protected virtual void Awake()
        {
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
            UtilityAI_Manager.Instance.NewRemoved += RemoveActivityGotFromManager;

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
            UtilityAI_Manager.Instance.NewRemoved -= RemoveActivityGotFromManager;
            if (_currentActivity.target && HasCongestion)
                UtilityAI_Manager.Instance.ChangeCongestion(
                    _currentActivity.target.GetComponent<Interactable_UtilityAI>(),
                    -BehaviourAI.VisiblePowerPoints);
            NullifyActivity();
            _AIActive = false;
        }
        #endregion

        #region actions
        /// <summary>
        /// Добавляем новые действия в runtime, связанные с поведением
        /// </summary>
        /// <param name="beh">Используемое поведение</param>
        public void AddNewActionsFromBehaviour(AIBehaviourBase beh) 
        {
            var dict = beh.GetActionsDictionary();
            foreach (var kvp in dict)
            {
                Interactable_UtilityAI target = kvp.Key;
                int weight = kvp.Value;

                if (!beh.IsTargetPassing(target.transform))
                    return;

                AddNewPossibleAction(target.transform, weight, target.transform.name, beh);
            }
        }
        /// <summary>
        /// Получаем все доступные действия из менеджера в соответствии с текущим поведением
        /// </summary>
        private void FetchAndAddAllActivities()
        {
            var dict = BehaviourAI.GetActionsDictionary();
            foreach (var kvp in dict)
            {
                Interactable_UtilityAI target = kvp.Key;
                int weight = kvp.Value;

                if (!BehaviourAI.IsTargetPassing(target.transform))
                    return;

                AddNewPossibleAction(target.transform, weight, target.transform.name, BehaviourAI);
            }
        }
        /// <summary>
        /// Добавляем новое действия из менеджера
        /// Это может быть новый созданный объект взаимодействия, например
        /// </summary>
        private void FetchNewActivityFromManager(object sender, UtilityAI_Manager.UAIData e)
        {
            Interactable_UtilityAI target = e.newInteractable.Key;
            int weight = e.newInteractable.Value;

            if (!BehaviourAI.IsTargetPassing(target.transform))
                return;

            AIAction action = new AIAction(
                this, target.transform, name, weight, BehaviourAI);

            AddNewPossibleAction(action);
        }
        /// <summary>
        /// Удаляем активность по событию из менеджера
        /// Например, какой-то объект перестал существовать
        /// </summary>
        private void RemoveActivityGotFromManager(object sender, UtilityAI_Manager.UAIData e)
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
        private void AddNewPossibleAction(Transform target, int weight, string name, AIBehaviourBase behaviour)
        {
            AIAction action = new AIAction(this, target, name, weight, behaviour);

            AddNewPossibleAction(action);
        }
        public void MarkCurrentActionAsDone() 
        {
            _possibleActions.Remove(CurrentActivity);
            NullifyActivity();
        }
        public void RemoveAction(Transform target, AIBehaviourBase behaviour) 
        {
            AIAction similar = _possibleActions.Find(item => item.target == target && item.behaviour == behaviour);

            if (_possibleActions.Contains(similar))
            {
                _possibleActions.Remove(similar);

                if (CurrentActivity == similar)
                    NullifyActivity();
            }
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
        public bool SelectBestActivityIfAny()
        {
            if (_possibleActions.Count == 0)
                return false;

            NormilizeActions(); //TODO : Проверить и убрать, если в этом нет смысла. Но его наличие создаёт надёжность.

            if (_possibleActions.Count == 0)
                return false;

            int bestActivityIndex = 0;

            _possibleActions.Sort((i1, i2) => i2.TotalWeight.CompareTo(i1.TotalWeight));

            ChangeAction(_possibleActions[bestActivityIndex]);

            return true;
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