using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TargetingUtilityAI : MonoBehaviour, IAnimationProvider
// ИИ, ставящий приоритеты выполнения действий
// Использует StateMachine в качестве исполнителя
{
    public bool _AIActive = true;

    [Header("Setup")]
    public float baseReachDistance = 1; // Или же длина конечности, что держит оружие
    public float maxSpeed = 3.5f;
    public float retreatMultiplier = 0.2f;
    public AnimationCurve retreatInfluence;
    public float distanceWeightMultiplier = 1;
    [SerializeField]
    protected MeleeTool hands; // То, что используется в качестве базового оружия ближнего боя и не может быть выброшено.
    [SerializeField]
    protected Transform distanceFrom; // Определяет начало конечности, откуда и происходит отсчёт.

    [Header("Ground")]
    public Collider vital;
    public float toGroundDist = 0.3f;
    public Transform navMeshCalcFrom; // Указывать не обязательно. Нужно, чтобы NavMesh не бузил.

    [Header("lookonly")]
    [SerializeField]
    protected AIAction _currentActivity;
    [SerializeField]
    protected List<AIAction> _possibleActions = new();

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
        public int distanceAddition;
        public Tool actWith;
        public UtilityAI_BaseState whatDoWhenClose;

        public int TotalWeight { get => totalWeight; set => totalWeight = value; }

        public AIAction()
        {
            target = default;
            name = default;
            totalWeight = default;
            baseWeight = default;
            distanceAddition = default;
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
            distanceAddition = 0;
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
    }

    protected virtual void OnEnable()
    {
        _AIActive = true;
        UtilityAI_Manager.Instance.changeHappened += DistributeActivityFromManager;
    }

    protected virtual void OnDisable()
    {
        UtilityAI_Manager.Instance.changeHappened -= DistributeActivityFromManager;
        NullifyActivity();
        _AIActive = false;
    }

    protected virtual void Start()
    {
        if (NMAgent)
            NMAgent.speed = maxSpeed;
        _noAction = new AIAction();
        if (navMeshCalcFrom == null)
            navMeshCalcFrom = transform;
        NullifyActivity();
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
                Utilities.CreateTextInWorld(action.distanceAddition.ToString(), action.target, position: action.target.position + Vector3.up * 2.5f, color: Color.blue);
            }
    }

    #endregion

    private void Reweight()
    {
        for (int i = 0; i < _possibleActions.Count; i++)
        {
            _possibleActions[i].distanceAddition = Mathf.RoundToInt(Vector3.Distance(transform.position, _possibleActions[i].target.position) * distanceWeightMultiplier);

            _possibleActions[i].TotalWeight = _possibleActions[i].baseWeight - _possibleActions[i].distanceAddition;
            //Utilities.DrawLineWithDistance(transform.position, action.target.position,Color.white , duration : 3);
        }
    }

    public void AddNewPossibleAction(Transform target, int weight, string name, Tool actWith, UtilityAI_BaseState treatment)
    {
        AIAction action = new AIAction(target, name, weight, actWith, treatment);

        if (_possibleActions.Contains(action))
        {
            Debug.LogWarning("Уже был добавлен " + name, transform);
            return;
        }

        _possibleActions.Add(action);
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

        return _currentActivity.whatDoWhenClose;
    }

    public bool NavMeshMeleeReachable()
    {
        // Проверяем, что мы далеко:
        Vector3 countFrom = navMeshCalcFrom ? navMeshCalcFrom.position : transform.position;
        NavMeshPath path = new NavMeshPath();

        NavMesh.CalculatePath(countFrom, CurrentActivity.target.position, NavMesh.AllAreas, path);
        return Utilities.NavMeshPathLength(path) < CurrentActivity.actWith.additionalMeleeReach + baseReachDistance;
    }

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

    private void NullifyActivity()
    {
        _currentActivity = _noAction; // Нужно, чтобы StateMachine перебросилась в состояние Decide и не ловила nullReference
    }

    public bool DecidingStateRequired()
    {
        return _currentActivity == _noAction;
    }

    protected void DistributeActivityFromManager(object sender, UtilityAI_Manager.UAIData e)
    {
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

            if (!IsPassing(target.transform))
                continue;

            Tool toolUsed = ToolChosingCheck(target.transform);

            AddNewPossibleAction(target.transform, weight, target.transform.name, toolUsed, _factory.Attack());

        }
    }

    #region virtual
    protected virtual bool IsPassing(Transform target)
    {
        bool res = true;

        Faction other = target.GetComponent<Faction>();

        if (!other.IsWillingToAttack(GetComponent<Faction>().type) || target == transform)
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

    // Обычный Update, но вызываемый в состоянии, когда юнит атакует.
    public virtual void AttackUpdate(Transform target)
    {
    }


    // Обычный Update, но когда юнит действует
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