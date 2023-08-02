using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TargetingUtilityAI : MonoBehaviour
// ИИ, ставящий приоритеты выполнения действий
// Использует StateMachine для чёткого
{
    //TODO : Сами объекты предоставляют вес действия, а так же то, что с ними надо делать!
    // Сампо: Враги подходят и ломают
    // 

    [Header("Setup")]
    [SerializeField]
    public float baseReachDistance = 1; // Или же длина конечности, что держит оружие
    [SerializeField]
    protected MeleeTool hands; // То, что используется в качестве базового оружия ближнего боя и не может быть выброшено.


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
        public ActionData data;
        public int weight;
        public Tool actWith;
        public UtilityAI_BaseState whatDoWhenClose;
        public AIAction(ActionData data, int weight, Tool actWith, UtilityAI_BaseState alignedState)
        {
            this.data = data;
            this.weight = weight;
            this.actWith = actWith;
            whatDoWhenClose = alignedState;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AIAction))
                return false;

            AIAction casted = (AIAction)obj;

            return (data.target == casted.data.target) && (whatDoWhenClose == casted.whatDoWhenClose) && (actWith == casted.actWith);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(data, weight, whatDoWhenClose);
        }

        public static bool operator ==(AIAction c1, AIAction c2)
        {
            return (c1.data.target == c2.data.target) && (c1.whatDoWhenClose == c2.whatDoWhenClose) && (c1.actWith == c2.actWith);
        }
        public static bool operator !=(AIAction c1, AIAction c2)
        {
            return (c1.data.target != c2.data.target) || (c1.data.name != c2.data.name) || (c1.actWith != c2.actWith);
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
        if(UtilityAI_Manager.instance!= null)
        DistributeActivityFromManager(this, new UtilityAI_Manager.UAIData(UtilityAI_Manager.instance.GetInteractables()));
    }

    protected virtual void Start()
    {
        UtilityAI_Manager.instance.changeHappened += DistributeActivityFromManager;

        _noAction = new AIAction();
        _currentActivity = _noAction;

        DistributeActivityFromManager(this, new UtilityAI_Manager.UAIData(UtilityAI_Manager.instance.GetInteractables())); // Костыль!
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

        AIAction action = new AIAction(new ActionData(target, name), weight, actWith, treatment);

        Faction.Type other = target.GetComponent<Faction>().type;

        if (other == GetComponent<Faction>().type
            || other == Faction.Type.neutral)
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
        while(!NavMesh.CalculatePath(transform.position, _possibleActions[bestActivityIndex].data.target.position, -1, path))
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
        if (_currentActivity.data.target.TryGetComponent<Collider>(out var c))
            closestToMe = c.ClosestPointOnBounds(_currentActivity.data.target.position);
        else
            closestToMe = _currentActivity.data.target.position;

        return Vector3.Distance(transform.position, closestToMe) < _currentActivity.actWith.additionalMeleeReach + baseReachDistance;
    }

    public bool ActionReachable(float actDistance) 
    {
        Vector3 closestToMe;
        if (_currentActivity.data.target.TryGetComponent<Collider>(out var c))
            closestToMe = c.ClosestPointOnBounds(_currentActivity.data.target.position);
        else
            closestToMe = _currentActivity.data.target.position;

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
}
