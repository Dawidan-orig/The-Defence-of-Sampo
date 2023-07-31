using System;
using System.Collections.Generic;
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
    private float actionDistance = 1;
    public int managerActionsUpdate = 5;

    [Header("lookonly")]
    [SerializeField]
    protected AIAction _currentActivity;
    [SerializeField]
    List<AIAction> _possibleActions = new();

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
        public float actDistance;
        public UtilityAI_BaseState whatDoWhenClose;
        public AIAction(ActionData data, int weight, float actDistance, UtilityAI_BaseState alignedState)
        {
            this.data = data;
            this.weight = weight;
            this.actDistance = actDistance;
            whatDoWhenClose = alignedState;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AIAction))
                return false;

            AIAction casted = (AIAction)obj;

            return (data.target == casted.data.target) && (whatDoWhenClose == casted.whatDoWhenClose);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(data, weight, whatDoWhenClose);
        }

        public static bool operator ==(AIAction c1, AIAction c2)
        {
            return (c1.data.target == c2.data.target) && (c1.whatDoWhenClose == c2.whatDoWhenClose);
        }
        public static bool operator !=(AIAction c1, AIAction c2)
        {
            return (c1.data.target != c2.data.target) || (c1.data.name != c2.data.name);
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

    protected void Awake()
    {
        _nmAgent = GetComponent<NavMeshAgent>();
        _factory = new UtilityAI_Factory(this);
        _currentState = _factory.Deciding();
    }

    protected void Start()
    {
        _noAction = new AIAction();
        _currentActivity = _noAction;

        InvokeRepeating(nameof(DistributeActivityFromManager), 0, managerActionsUpdate);
    }

    private void Update()
    {
        _currentState.UpdateState();

        HandleStates();
    }

    private void FixedUpdate()
    {
        _currentState.FixedUpdateState();
    }

    #endregion

    #region Continious

    private void HandleStates()
    {
        if (_currentActivity == _noAction)
        {
            SelectBestActivity();
        }
    }

    #endregion

    #region onetime
    private void DistributeActivityFromManager() 
    {
        _possibleActions.Clear();

        var activities = UtilityAI_Manager.instance.GetPossibleActivities();
        foreach (KeyValuePair<GameObject, int> activity in activities)
        {
            GameObject target = activity.Key;
            int weight = activity.Value;

            // Прямо сейчас ИИ будут бить атаковать всё живое и разрушаемое
            if(target.TryGetComponent<Interactable_UtilityAI>(out _)) 
            {
                AddNewPossibleAction(target.transform, weight, target.transform.name, actionDistance, _factory.Attack());
            }
        }
    }

    public void AddNewPossibleAction(Transform target, int weight, string name, float actDistance, UtilityAI_BaseState treatment)
    {
        /*
        if (weight < 1)
        {
            Debug.LogWarning("Нельзя назначать задачу весом меньше 1, отмена назначения " + name);
            return;
        }*/

        //TODO : Проверка на добавление target'а с treatment'ом таких, что уже есть в списке - выдавать ошибку в таком случае.
        // Выполняемые задачи не должны дублироваться!

        //TODO : Поменять этот treatment на enum. Иначе можно додуматься сделать новый factory, и кидать состояния оттуда - это не очень желательно.

        //IDEA: Оптимизировать. Как-нибудь. Наверное.
        RedefineActivities();

        _possibleActions.Add(new AIAction(new ActionData(target, name), weight, actDistance, treatment));
    }

    public void RedefineActivities()
    {
        _currentActivity = _noAction;

        SelectBestActivity();
    }

    public UtilityAI_BaseState SelectBestActivity()
    {
        if (_possibleActions.Count == 0)
        {
            return null;
        }

        _possibleActions.Sort((i1, i2) => i2.weight.CompareTo(i1.weight));
        _currentActivity = _possibleActions[0];
        return _currentActivity.whatDoWhenClose;
    }

    // Обычный Update, но вызываемый в состоянии, когда юнит атакует.
    public virtual void AttackUpdate(Transform target) { }

    // Обычный Update, но когда юнит действует
    public virtual void ActionUpdate(Transform target) { }

    #endregion
}
