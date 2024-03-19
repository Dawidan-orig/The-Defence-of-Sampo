using Sampo.AI.Conditions;
using Sampo.Core;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sampo.AI
{
    //TODO : Добавить Logger или другую систему логирования.
    // Надо понимать, что происходит с каждым юнитом в переделах Editor'а.

    [SelectionBase]
    [RequireComponent(typeof(IMovingAgent))]
    [RequireComponent(typeof(Faction))]
    public abstract class TargetingUtilityAI : MonoBehaviour, IAnimationProvider, IPointsDistribution
    // ИИ, ставящий приоритеты выполнения действий
    // Использует StateMachine в качестве исполнителя
    {
        public bool _AIActive = true;

        [Header("Setup")]
        [Tooltip("Кривая от 0 до 1, определяющая то," +
            "с какой интенсивностью в зависимости от дальности оружия ИИ будет отдоходить от цели")]
        public AnimationCurve retreatInfluence;
        [Tooltip("Влияние дистанции на выбор этого ИИ")]
        public float distanceWeightMultiplier = 1;

        [Header("Ground for animation and movement")]
        public Collider vital;
        public float toGroundDist = 0.3f;
        [Tooltip("Точка отсчёта для NavMeshAgent")]
        public Transform navMeshCalcFrom;

        [Header("lookonly")]
        [SerializeField]
        protected AIAction _currentActivity;
        [SerializeField]
        protected List<AIAction> _possibleActions = new();
        [SerializeField]
        [Tooltip(@"Это очки внешней опасности, по котором ИИ определяет, насколько этот противник опасен.
            Может менять визуальную составляющую.
            Изначально устанавливается при процедурной инициализации, но может меняться в ходе игры.")]
        protected int visiblePowerPoints = 100;

        IMovingAgent _movingAgent;
        Rigidbody _body;
        private AIAction _noAction;
        protected UtilityAI_Factory _factory;
        protected UtilityAI_BaseState _currentState;

        public UtilityAI_BaseState CurrentState { get => _currentState; set => _currentState = value; }
        public AIAction CurrentActivity { get => _currentActivity; }
        public Rigidbody Body { get => _body; set => _body = value; }
        public IMovingAgent MovingAgent { get => _movingAgent; set => _movingAgent = value; }
        public int VisiblePowerPoints { get => visiblePowerPoints; set => visiblePowerPoints = value; }

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
                        Mathf.RoundToInt(Vector3.Distance(actionOf.transform.position, target.position) * actionOf.distanceWeightMultiplier);
                enemiesAmountSubstraction =
                    UtilityAI_Manager.Instance.GetCongestion(target.GetComponent<Interactable_UtilityAI>());

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
            _movingAgent = GetComponent<IMovingAgent>();
            _body = GetComponent<Rigidbody>();
            _factory = new UtilityAI_Factory(this);
            _currentState = _factory.Deciding();
            navMeshCalcFrom = navMeshCalcFrom == null ? transform : navMeshCalcFrom;
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
                ModifyActionOf(dDealer.DamageFrom, new RespondToAttackCondition(TIME_OF_BEING_ANGRY));
        }

        protected virtual void OnDisable()
        {
            UtilityAI_Manager.Instance.NewAdded -= FetchNewActivityFromManager;
            UtilityAI_Manager.Instance.NewRemoved -= RemoveActivityFromManager;
            if (_currentActivity.target)
                UtilityAI_Manager.Instance.ChangeCongestion(_currentActivity.target.GetComponent<Interactable_UtilityAI>(), -visiblePowerPoints);
            NullifyActivity();
            _AIActive = false;
        }
        #endregion

        #region actions
        private void FetchAndAddAllActivities()
        {
            var dict = UtilityAI_Manager.Instance.GetAllInteractions(GetComponent<Faction>().FactionType);
            foreach (var kvp in dict)
            {
                Interactable_UtilityAI target = kvp.Key;
                int weight = kvp.Value;

                if (!IsEnemyPassing(target.transform))
                    return;

                Tool toolUsed = ToolChosingCheck(target.transform);

                AddNewPossibleAction(target.transform, weight, target.transform.name, toolUsed, _factory.Attack());
            }
        }
        private void FetchNewActivityFromManager(object sender, UtilityAI_Manager.UAIData e)
        {
            Faction faction = GetComponent<Faction>();
            if (!faction.IsWillingToAttack(e.factionWhereChangeHappened))
                return;

            Interactable_UtilityAI target = e.newInteractable.Key;
            int weight = e.newInteractable.Value;

            if (!IsEnemyPassing(target.transform))
                return;

            Tool toolUsed = ToolChosingCheck(target.transform);

            //TODO DESIGN : _factory.Attack() - не обязательно он, надо выбирать из фабрики нужное
            AIAction action = new AIAction(this, target.transform, name, weight, toolUsed, _factory.Attack());

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
            if (!IsNoActionCurrently() && _currentActivity.target) //Убираем влияние текущей цели
                UtilityAI_Manager.Instance.ChangeCongestion(_currentActivity.target.GetComponent<Interactable_UtilityAI>(), -visiblePowerPoints);
            _currentActivity = to;
            UtilityAI_Manager.Instance.ChangeCongestion(_currentActivity.target.GetComponent<Interactable_UtilityAI>(), visiblePowerPoints);
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
            foreach(var action in _possibleActions) 
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

        /// <summary>
        /// Независимая команда вычисления, находящая ближайшую точку для больших объектов
        /// </summary>
        /// <param name="ofTarget">Огромная цель, для которой нужно найти ближайшую точку</param>
        /// <param name="CalculateFrom">От этой позиции находится ближайшая позиция</param>
        /// <returns>Ближайшая точка</returns>
        public Vector3 GetClosestPoint(Transform ofTarget, Vector3 CalculateFrom)
        {
            Vector3 closestPointToTarget;
            if (ofTarget.TryGetComponent<IDamagable>(out var ab))
                closestPointToTarget = ab.Vital.ClosestPointOnBounds(CalculateFrom);
            else if (ofTarget.TryGetComponent<Collider>(out var c))
                closestPointToTarget = c.ClosestPointOnBounds(CalculateFrom);
            else
                closestPointToTarget = ofTarget.position;

            //closestPointToTarget = ofTarget.position;

            return closestPointToTarget;
        }
        #region virtual functions

        /// <summary>
        /// Присваивание очков и изменение параметров
        /// </summary>
        /// <param name="points">Присваиваемые очки</param>
        public virtual void AssignPoints(int points)
        {
            int remaining = points;
            visiblePowerPoints = points;

            //TODO DESIGN : Гармоничное изменение скорости движения
        }
        /// <summary>
        /// Проверка фракции на себя
        /// </summary>
        /// <param name="target">Относительно этой цели</param>
        /// <returns>true, если цель подходит</returns>
        protected virtual bool IsEnemyPassing(Transform target)
        {
            bool res = true;

            Faction other = target.GetComponent<Faction>();

            if (!other.IsWillingToAttack(GetComponent<Faction>().FactionType) || target == transform)
                res = false;

            if (other.TryGetComponent(out AliveBeing b))
                if (b.mainBody == transform)
                    res = false;

            return res;
        }
        /// <summary>
        /// Выбор оружия исходя из внутренних условий
        /// </summary>
        /// <param name="target">Относительно этой цели</param>
        /// <returns>Выбранное оружие</returns>
        protected abstract Tool ToolChosingCheck(Transform target);
        /// <summary>
        /// Определяет точку, куда следует отступать
        /// </summary>
        /// <returns>Точка относительно ИИ, длина вектора указывает силу отсупления</returns>
        public abstract Vector3 RelativeRetreatMovement();

        /*TODO dep AI_Factory : Сделать так, чтобы управляющая StateMachine была интегрирована сюда, либо вообще редуцирована...
        * Эти Функции не должны быть публичны, они - только для управляющей StateMachine!
        * Сделать через Event'ы после переделки factory.
        */
        /// <summary>
        /// Обычный Update, но вызываемый в состоянии, когда юнит атакует.
        /// </summary>
        /// <param name="target"></param>
        public abstract void AttackUpdate(Transform target);

        /// <summary>
        /// Обычный Update, но когда юнит действует
        /// </summary>
        /// <param name="target"></param>
        public abstract void ActionUpdate(Transform target);
        #endregion

        #region animation
        public Vector3 GetLookTarget()
        {
            return (_currentActivity.target ? _currentActivity.target.position : Vector3.zero);
        }

        public bool IsGrounded()
        {
            return Physics.BoxCast(vital.bounds.center, new Vector3(vital.bounds.size.x / 2, 0.1f, vital.bounds.size.z / 2),
                transform.up * -1, out _, transform.rotation, vital.bounds.size.y / 2 + toGroundDist);
        }

        public bool IsInJump()
        {
            //TODO DESIGN : ИИ Никогда не бывают в прыжке. Что, вообще-то, надо бы исправить.
            return false;
        }
        /// <summary>
        /// Нужно для анимации тела
        /// </summary>
        /// <returns>Точка, куда будет направлена рука</returns>
        public virtual Transform GetRightHandTarget() { return null; }
        #endregion

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