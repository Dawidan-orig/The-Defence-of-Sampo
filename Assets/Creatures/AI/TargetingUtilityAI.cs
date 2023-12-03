using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
[RequireComponent(typeof(MovingAgent))]
public class TargetingUtilityAI : MonoBehaviour, IAnimationProvider, IPointsDistribution
// ИИ, ставящий приоритеты выполнения действий
// Использует StateMachine в качестве исполнителя
{
    //TODO : Повесить обратную зависимость всех скриптов (Движение, Фракция и др.) от этого. Нужно для процедурного спавна и единого контроля.
    //TODO : Убрать отсюда всё, что связано с войной. Этот ИИ не занимается контролем оружия
    public bool _AIActive = true;

    [Header("Setup")]
    [Tooltip("Длина конечности, что держит оружие")]
    public float baseReachDistance = 1;
    public AnimationCurve retreatInfluence;
    [Tooltip("Влияние дистанции на выбор этого ИИ")]
    public float distanceWeightMultiplier = 1;
    [SerializeField]
    [Tooltip("То, что используется в качестве базового оружия ближнего боя и не может быть выброшено.")]
    protected MeleeTool hands; 
    [SerializeField]
    [Tooltip("Определяет начало конечности, откуда и происходит отсчёт.")]
    protected Transform distanceFrom; 

    [Header("Ground for animation and movement")]
    public Collider vital;
    public float toGroundDist = 0.3f;
    [Tooltip("Точка отсчёта для NavMesh")]
    public Transform navMeshCalcFrom;

    [Header("lookonly")]
    [SerializeField]
    protected AIAction _currentActivity;
    [SerializeField]
    protected List<AIAction> _possibleActions = new();
    [SerializeField]
    [Tooltip(@"Это очки, по котором ИИ определяет, насколько этот противник опасен.
            Может менять визуальную составляющую.
            Изначально устанавливается при процедурной инициализации, но может меняться в ходе игры.")]
    protected int visiblePowerPoints = 100;

    NavMeshAgent _nmAgent;
    MovingAgent _movingAgent;
    Rigidbody _body;
    protected AIAction _noAction;
    protected UtilityAI_Factory _factory;
    protected UtilityAI_BaseState _currentState;

    public UtilityAI_BaseState CurrentState { get => _currentState; set => _currentState = value; }
    public NavMeshAgent NMAgent { get => _nmAgent; set => _nmAgent = value; }
    public AIAction CurrentActivity { get => _currentActivity; }
    public Rigidbody Body { get => _body; set => _body = value; }
    public MovingAgent MovingAgent { get => _movingAgent; set => _movingAgent = value; }

    [Serializable]
    public class AIAction
    {
        public Transform target;
        public string name;
        private int totalWeight;
        public int baseWeight;
        public int distanceSubstraction;
        public int enemiesAmountSubstraction;
        public Tool actWith;
        public UtilityAI_BaseState whatDoWhenClose;

        public int TotalWeight { get => totalWeight; set => totalWeight = value; }

        public AIAction()
        {
            target = default;
            name = default;
            totalWeight = default;
            baseWeight = default;
            distanceSubstraction = default;
            actWith = default;
            whatDoWhenClose = default;
        }

        public AIAction(Transform target, string name, int weight, Tool actWith, UtilityAI_BaseState alignedState)
        {
            this.target = target;
            this.name = name;
            this.baseWeight = weight;
            this.actWith = actWith;
            whatDoWhenClose = alignedState;
            totalWeight = weight;
            distanceSubstraction = 0;
        }

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
            return (c1.target == c2.target) && (c1.whatDoWhenClose == c2.whatDoWhenClose) && (c1.actWith == c2.actWith);
        }
        public static bool operator !=(AIAction c1, AIAction c2)
        {
            return (c1.target != c2.target) || (c1.name != c2.name) || (c1.actWith != c2.actWith);
        }
    }

    #region Unity
    protected virtual void Awake()
    {
        _nmAgent = GetComponent<NavMeshAgent>();
        _movingAgent = GetComponent<MovingAgent>();
        _body = GetComponent<Rigidbody>();
        _factory = new UtilityAI_Factory(this);
        _currentState = _factory.Deciding();
        navMeshCalcFrom = navMeshCalcFrom == null ? transform : navMeshCalcFrom;

        _noAction = new AIAction();

        NullifyActivity();
    }

    protected virtual void OnEnable()
    {
        _AIActive = true;
        UtilityAI_Manager.Instance.changeHappened += DistributeActivityFromManager;
    }

    protected virtual void OnDisable()
    {
        UtilityAI_Manager.Instance.changeHappened -= DistributeActivityFromManager;
        UtilityAI_Manager.Instance.ChangeCongestion(_currentActivity.target.gameObject, -visiblePowerPoints);
        NullifyActivity();
        _AIActive = false;
    }

    protected virtual void Start()
    {
        
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

    protected virtual void OnDrawGizmosSelected()
    {
        if (EditorApplication.isPlaying && !EditorApplication.isPaused)

            foreach (AIAction action in _possibleActions)
            {
                Reweight();

                Utilities.CreateTextInWorld(action.baseWeight.ToString(), action.target, position: action.target.position + Vector3.up * 2);
                Utilities.CreateTextInWorld(action.distanceSubstraction.ToString(), action.target, position: action.target.position + Vector3.up * 2.5f, color: Color.blue);
                Utilities.CreateTextInWorld(action.enemiesAmountSubstraction.ToString(), action.target, position: action.target.position + Vector3.up * 3f, color: Color.yellow);
            }
    }

    #endregion

    #region actions
    private void DistributeActivityFromManager(object sender, UtilityAI_Manager.UAIData e)
    {
        if(_currentActivity != _noAction)
            UtilityAI_Manager.Instance.ChangeCongestion(_currentActivity.target.gameObject, -visiblePowerPoints);
        _currentActivity = _noAction;
        _possibleActions.Clear();

        var activities = e.interactables;
        foreach (KeyValuePair<GameObject, int> activity in activities)
        {
            GameObject target = activity.Key;
            int weight = activity.Value;

            // Прямо сейчас ИИ будут атаковать всё живое и разрушаемое
            if (!target.TryGetComponent<Interactable_UtilityAI>(out _))
                continue;

            if (!IsEnemyPassing(target.transform))
                continue;

            Tool toolUsed = ToolChosingCheck(target.transform);

            AddNewPossibleAction(target.transform, weight, target.transform.name, toolUsed, _factory.Attack());
        }
    }
    private void AddNewPossibleAction(Transform target, int weight, string name, Tool actWith, UtilityAI_BaseState treatment)
    {
        AIAction action = new AIAction(target, name, weight, actWith, treatment);

        if (_possibleActions.Contains(action))
        {
            Debug.LogWarning("Уже был добавлен " + name, transform);
            return;
        }

        _possibleActions.Add(action);
    }
    private void Reweight()
    {
        for (int i = 0; i < _possibleActions.Count; i++)
        {
            _possibleActions[i].distanceSubstraction = Mathf.RoundToInt(Vector3.Distance(transform.position, _possibleActions[i].target.position) * distanceWeightMultiplier);
            _possibleActions[i].enemiesAmountSubstraction = UtilityAI_Manager.Instance.GetCongestion(_possibleActions[i].target.gameObject);            

            _possibleActions[i].TotalWeight = _possibleActions[i].baseWeight
            - _possibleActions[i].distanceSubstraction 
            - _possibleActions[i].enemiesAmountSubstraction;
            //Utilities.DrawLineWithDistance(transform.position, action.target.position,Color.white , duration : 3);
        }
    }

    public UtilityAI_BaseState SelectBestActivity()
    {
        if (_possibleActions.Count == 0)
        {
            return null;
        }

        Reweight();

        int bestActivityIndex = 0;

        _possibleActions.Sort((i1, i2) => i2.TotalWeight.CompareTo(i1.TotalWeight));

        /*
        NavMeshPath path = new();
        if (Utilities.VisualisedRaycast(transform.position, Vector3.down, out RaycastHit hit, toGroundDist + vital.bounds.size.y / 2))
            // Проверяем достижимость NavMesh'а до цели.
            while (!NavMesh.CalculatePath(hit.point, _possibleActions[bestActivityIndex].target.position, -1, path))
            {
                bestActivityIndex++;

                if (bestActivityIndex >= _possibleActions.Count)
                    return null;
            }
        else
            return null;*/

        _currentActivity = _possibleActions[bestActivityIndex];
        UtilityAI_Manager.Instance.ChangeCongestion(_currentActivity.target.gameObject, visiblePowerPoints);

        return _currentActivity.whatDoWhenClose;
    }

    private void NullifyActivity()
    {
        _currentActivity = _noAction; // Нужно, чтобы StateMachine перебросилась в состояние Decide и не ловила nullReference
    }
    #endregion

    public bool MeleeReachable()
    {
        Vector3 closestToMe;
        Vector3 calculateFrom = distanceFrom ? distanceFrom.position : transform.position;
        if (_currentActivity.target.TryGetComponent<AliveBeing>(out var ab))
            closestToMe = ab.vital.ClosestPointOnBounds(calculateFrom);
        else if (_currentActivity.target.TryGetComponent<Collider>(out var c))
            closestToMe = c.ClosestPointOnBounds(calculateFrom);
        else
            closestToMe = _currentActivity.target.position;

        return Vector3.Distance(calculateFrom, closestToMe) < _currentActivity.actWith.additionalMeleeReach + baseReachDistance;
    }

    public bool DecidingStateRequired()
    {
        return _currentActivity == _noAction;
    }

    public virtual void GivePoints(int points)
    {
        int remaining = points;
        visiblePowerPoints = points;
        //TODO : Изменение скорости движения
    }

    #region virtual functions
    protected virtual bool IsEnemyPassing(Transform target)
    {
        bool res = true;

        Faction other = target.GetComponent<Faction>();

        if (!other.IsWillingToAttack(GetComponent<Faction>().f_type) || target == transform)
            res = false;

        if (other.TryGetComponent(out AliveBeing b))
            if (b.mainBody == transform)
                res = false;

        return res;
    }

    protected virtual Tool ToolChosingCheck(Transform target)
    {
        return hands;
    }

    /// <summary>
    /// Обычный Update, но вызываемый в состоянии, когда юнит атакует.
    /// </summary>
    /// <param name="target"></param>
    public virtual void AttackUpdate(Transform target)
    {
    }


    /// <summary>
    /// Обычный Update, но когда юнит действует
    /// </summary>
    /// <param name="target"></param>
    public virtual void ActionUpdate(Transform target) { }

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
        // ИИ Никогда не бывают в прыжке,
        //TODO : Что, вообще-то, надо бы исправить.
        return false;
    }

    public virtual Transform GetRightHandTarget()
    {
        return hands.transform;
    }
    #endregion
}