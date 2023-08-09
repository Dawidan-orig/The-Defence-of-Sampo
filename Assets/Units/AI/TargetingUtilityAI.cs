using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TargetingUtilityAI : MonoBehaviour, IAnimationProvider
// ИИ, ставящий приоритеты выполнения действий
// Использует StateMachine в качестве исполнителя
{
    [Header("Setup")]
    public float baseReachDistance = 1; // Или же длина конечности, что держит оружие
    [SerializeField]
    protected MeleeTool hands; // То, что используется в качестве базового оружия ближнего боя и не может быть выброшено.
    [SerializeField]
    protected Transform distanceFrom; // Определяет начало конечности, откуда и происходит отсчёт.

    [Header("Ground")]
    public Collider vital;
    public float toGroundDist = 0.3f;

    [Header("lookonly")]
    [SerializeField]
    protected AIAction _currentActivity;
    [SerializeField]
    protected List<AIAction> _possibleActions = new();

    AIAction _noAction;
    NavMeshAgent _nmAgent;
    protected UtilityAI_Factory _factory;
    protected UtilityAI_BaseState _currentState;

    public UtilityAI_BaseState CurrentState { get => _currentState; set => _currentState = value; }
    public NavMeshAgent NMAgent { get => _nmAgent; set => _nmAgent = value; }
    public AIAction CurrentActivity { get => _currentActivity; }

    [Serializable]
    public struct AIAction
    {
        public Transform target;
        public string name;
        public int weight;
        public Tool actWith;
        public UtilityAI_BaseState whatDoWhenClose;
        public AIAction(Transform target, string name, int weight, Tool actWith, UtilityAI_BaseState alignedState)
        {
            this.target = target;
            this.name = name;
            this.weight = weight;
            this.actWith = actWith;
            whatDoWhenClose = alignedState;
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
            return HashCode.Combine(target, name, weight, whatDoWhenClose);
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

    [Serializable]
    public struct ActionData
    {
        public Transform target;
        public string name;

        public ActionData(Transform target, string name)
        {
            this.target = target;
            this.name = name;
        }
    }
    #region Unity


    protected virtual void Awake()
    {
        _nmAgent = GetComponent<NavMeshAgent>();
        _factory = new UtilityAI_Factory(this);
        _currentState = _factory.Deciding();        
    }

    protected virtual void OnEnable()
    {
        if(UtilityAI_Manager.Instance!= null)
            DistributeActivityFromManager(this, new UtilityAI_Manager.UAIData(UtilityAI_Manager.Instance.GetInteractables()));
    }

    protected virtual void Start()
    {
        UtilityAI_Manager.Instance.changeHappened += DistributeActivityFromManager;

        _noAction = new AIAction();
        _currentActivity = _noAction;

        DistributeActivityFromManager(this, new UtilityAI_Manager.UAIData(UtilityAI_Manager.Instance.GetInteractables())); // Костыль!
        // Но без него Singleton в Awake не успевает инициализироваться,
        // А в Start события уже проходят, из-за чего не всю картину можно уловить.
        // то что я сделал сейчас - гарантирует, что всё точно было уловлено.
    }

    protected virtual void Update()
    {
        _currentState.UpdateState();
    }

    protected virtual void FixedUpdate()
    {
        _currentState.FixedUpdateState();
    }

    #endregion

    #region onetime

    public void AddNewPossibleAction(Transform target, int weight, string name, Tool actWith, UtilityAI_BaseState treatment)
    {
        //TODO : Проверка на добавление target'а с treatment'ом таких, что уже есть в списке - выдавать ошибку в таком случае.
        // Выполняемые задачи не должны дублироваться!

        AIAction action = new AIAction(target, name, weight, actWith, treatment);

        Faction.FType other = target.GetComponent<Faction>().type;

        if (other == GetComponent<Faction>().type
            || other == Faction.FType.neutral)
            return;

        if(_possibleActions.Contains(action))
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

        int bestActivityIndex = 0;

        _possibleActions.Sort((i1, i2) => i2.weight.CompareTo(i1.weight));

        NavMeshPath path = new();

        // Проверяем достижимость NavMesh'а до цели.
        while(!NavMesh.CalculatePath(transform.position, _possibleActions[bestActivityIndex].target.position, -1, path))
        {
            bestActivityIndex++;

            if (bestActivityIndex >= _possibleActions.Count)
                return null;
        }
        _currentActivity = _possibleActions[bestActivityIndex];        

        return _currentActivity.whatDoWhenClose;
    }

    public bool ActionReachable()
    {
        Vector3 closestToMe;
        if (_currentActivity.target.TryGetComponent<Collider>(out var c))
            closestToMe = c.ClosestPointOnBounds(_currentActivity.target.position);
        else
            closestToMe = _currentActivity.target.position;

        return Vector3.Distance(transform.position, closestToMe) < _currentActivity.actWith.additionalMeleeReach + baseReachDistance;
    }

    public bool ActionReachable(float actDistance) 
    {
        Vector3 closestToMe;
        if (_currentActivity.target.TryGetComponent<Collider>(out var c))
            closestToMe = c.ClosestPointOnBounds(_currentActivity.target.position);
        else
            closestToMe = _currentActivity.target.position;

        return Vector3.Distance(transform.position, closestToMe) < actDistance;
    }

    #endregion
    protected virtual void DistributeActivityFromManager(object sender, UtilityAI_Manager.UAIData e)
    {
        _possibleActions.Clear();

        var activities = e.interactables;
        foreach (KeyValuePair<GameObject, int> activity in activities)
        {
            GameObject target = activity.Key;
            int weight = activity.Value;

            // Прямо сейчас ИИ будут бить атаковать всё живое и разрушаемое
            if (target.TryGetComponent<Interactable_UtilityAI>(out _))
            {
                AddNewPossibleAction(target.transform, weight, target.transform.name, hands, _factory.Attack());
            }

            //TODO : Сделать _currentState.ForceDecideState(); Когда происходит что-то срочное, типа метеорита какого-нибудь
        }
    }

    // Обычный Update, но вызываемый в состоянии, когда юнит атакует.
    public virtual void AttackUpdate(Transform target) 
    {
        
    }

    // Обычный Update, но когда юнит действует
    public virtual void ActionUpdate(Transform target) { }

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
        return NMAgent.isOnOffMeshLink;
    }

    public virtual Vector3 GetRightHandTarget() 
    {
        return hands.transform.position;
    }
}