using System.Collections.Generic;
using UnityEngine;

public class SwordFighter_StateMachine : MeleeFighter
{
    [Header("constraints")]
    public float actionSpeed = 1; // Скорость движения меча в руке
    public float block_minDistance = 0.3f; // Минимальное расстояние для блока, используемое для боев с противником, а не отбивания.
    public float swing_EndDistanceMultiplier = 1.5f; // Насколько далеко должен двинуться меч после отбивания.
    public float swing_startDistance = 1.5f; // Насколько далеко должен двинуться меч до удара.
    public float criticalImpulse = 400; // Лучше увернуться, чем отбить объект с импульсом больше этого!
    public float toBladeHandle_MaxDistance = 2; // Максимальное расстояние от vital до рукояти меча. По сути, длина руки.
    public float toBladeHandle_MinDistance = 0.1f; // Минимальное расстояние от vital.
    public float close_enough = 0.1f; // Расстояние до цели, при котором можно менять состояние.
    public float angle_enough = 10; // Достаточный угол, чтобы считать что handle близок к desire

    [Header("timers")]
    public float toInitialAwait = 2; // Сколько времени ожидать до установки меча в обычную позицию?
    public float minimalTimeBetweenAttacks = 2;

    [Header("init-s")]
    [SerializeField]
    private Blade _blade;
    [SerializeField]
    private Transform _bladeContainer;
    [SerializeField]
    private Transform _bladeHandle;
    [SerializeField]
    private Collider _vital;
    [SerializeField]
    private Transform enemy; //TODO : Заменить на MeleeFighter
    //TODO : Автоматизировать выбор этого самого enemy

    [Header("lookonly")]
    [SerializeField]
    Transform _initialBlade;
    [SerializeField]
    Transform _moveFrom;
    [SerializeField]
    Transform _desireBlade;
    [SerializeField]
    float _moveProgress;
    [SerializeField]
    float _currentToInitialAwait;
    [SerializeField]
    float _attackRecharge = 0;
    [SerializeField]
    AttackCatcher _catcher;
    [SerializeField]
    bool _attackReposition = false; //TODO : Удалить, заменить на систему комбо.
    [SerializeField]
    Stack<ActionJoint> _combo = new Stack<ActionJoint>(); //TODO : Добавить в этот класс функцию, возвращающую новое состояние, а так же убирающее уже сделанное действие.
    // Эта штука позволит создавать комбо разной длины.

    enum ActionType 
    {
        Swing,
        Reposition
    }

    public struct ActionJoint 
    {
        public Transform currentDesire;
        public Transform nextDesire;
        ActionType nextActionType;
    }

    SwordFighter_BaseState _currentState;
    SwordFighter_StateFactory _states;

    //Getters and setters
    public SwordFighter_BaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    public Transform BladeHandle { get { return _bladeHandle; } }
    public Transform DesireBlade { get { return _desireBlade; } }
    public Transform MoveFrom { get { return _moveFrom; } }
    public float MoveProgress { get { return _moveProgress; } }
    public Blade Blade { get { return _blade; } }
    public Transform InitialBlade { get => _initialBlade; set => _initialBlade = value; }
    public float CurrentToInitialAwait { get => _currentToInitialAwait; set => _currentToInitialAwait = value; }
    public Collider Vital { get => _vital; set => _vital = value; }
    public AttackCatcher AttackCatcher { get => _catcher; set => _catcher = value; }
    public float AttackRecharge { get => _attackRecharge; set => _attackRecharge = value; }
    public Transform Enemy { get => enemy; set => enemy = value; }
    public bool AttackReposition { get => _attackReposition; set => _attackReposition = value; } //TODO : Заменить на обработку комбинаций ударов!
    

    [Header("Debug")]
    [SerializeField]
    private bool isSwordFixing = true;
    [SerializeField]
    private string currentState;

    void Awake()
    {
        _catcher = gameObject.GetComponent<AttackCatcher>();
        _catcher.ignored.Add(_blade.body);

        _currentToInitialAwait = toInitialAwait;
        _attackRecharge = minimalTimeBetweenAttacks;

        _states = new SwordFighter_StateFactory(this);
        _currentState = _states.Idle();
        _currentState.EnterState();

        _blade.SetHost(gameObject);

        if (_bladeContainer == null)
            _bladeContainer = transform;

        GameObject desireGO = new("DesireBlade");
        _desireBlade = desireGO.transform;
        _desireBlade.parent = _bladeContainer;
        _desireBlade.gameObject.SetActive(true);
        _desireBlade.position = BladeHandle.position;
        _desireBlade.rotation = BladeHandle.rotation;

        GameObject initialBladeGO = new("InititalBladePosition");
        _initialBlade = initialBladeGO.transform;
        _initialBlade.position = BladeHandle.position;
        _initialBlade.rotation = BladeHandle.rotation;
        _initialBlade.parent = _bladeContainer;

        SetDesires(_initialBlade.position, _initialBlade.up, _initialBlade.forward);
        NullifyProgress();
        _moveProgress = 1;
    }

    private void Update()
    {
        _currentState.UpdateState();

        currentState = _currentState.ToString();
    }

    private void FixedUpdate()
    {
        _currentState.FixedUpdateState();

        if (_moveProgress < 1)
            _moveProgress += actionSpeed * Time.fixedDeltaTime / Vector3.Distance(_moveFrom.position, _desireBlade.position);

        if (_attackRecharge < minimalTimeBetweenAttacks)
            _attackRecharge += Time.fixedDeltaTime;
    }

    // Установка меча по всем возможным параметрам
    public override void Block(Vector3 start, Vector3 end, Vector3 SlashingDir)
    {
        SetDesires(start, (end - start).normalized, SlashingDir);
    }

    // Атака оружием по какой-то точке из текущей позиции.
    public override void Swing(Vector3 toPoint)
    {
        _attackRecharge = 0;

        Vector3 moveTo = toPoint + (toPoint - BladeHandle.position).normalized * swing_EndDistanceMultiplier;

        Vector3 pointDir = (moveTo - _vital.bounds.center).normalized;

        // Притягиваем ближе к vital
        float distance = (toPoint - _vital.ClosestPointOnBounds(toPoint)).magnitude;
        moveTo = _vital.ClosestPointOnBounds(moveTo) + (moveTo - _vital.ClosestPointOnBounds(moveTo)).normalized * distance;
        SetDesires(moveTo, pointDir, (moveTo - toPoint).normalized);
    }

    private void FixDesire()
    {
        if (!isSwordFixing)
            return;

        Vector3 closestPos = _vital.ClosestPointOnBounds(DesireBlade.position);
        if (Vector3.Distance(_desireBlade.position, closestPos) > toBladeHandle_MaxDistance)
            _desireBlade.position = closestPos + (_desireBlade.position - closestPos).normalized * toBladeHandle_MaxDistance;

        if (Vector3.Distance(_desireBlade.position, closestPos) < toBladeHandle_MinDistance)
            _desireBlade.position = closestPos + (_desireBlade.position - closestPos).normalized * toBladeHandle_MinDistance;
    }

    public void SetDesires(Vector3 pos, Vector3 up, Vector3 forward)
    {
        _desireBlade.position = pos;
        _desireBlade.LookAt(pos + up, pos + forward);
        _desireBlade.RotateAround(_desireBlade.position, _desireBlade.right, 90);

        FixDesire();

        if (MoveProgress >= 1)
        {
            NullifyProgress();
        }
    }
    public bool CloseToDesire()
    {
        return Vector3.Distance(_bladeHandle.position, _desireBlade.position) < close_enough;
    }

    public bool AlmostDesire()
    {
        return Vector3.Distance(_bladeHandle.position, _desireBlade.position) < close_enough
            && Quaternion.Angle(_bladeHandle.rotation, _desireBlade.rotation) < angle_enough;
    }

    public void NullifyProgress()
    {
        if (_moveFrom != null)
            Destroy(_moveFrom.gameObject);
        GameObject moveFromGO = new("BladeMoveStart");
        _moveFrom = moveFromGO.transform;
        _moveFrom.position = BladeHandle.position;
        _moveFrom.rotation = BladeHandle.rotation;
        _moveFrom.parent = _bladeContainer;
        _moveProgress = 0;
    }

    private void OnDrawGizmos()
    {
        if (_desireBlade != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(_desireBlade.position, _moveFrom.position);
            Gizmos.color = Color.gray;
            Gizmos.DrawRay(_desireBlade.position, _desireBlade.up);
            Gizmos.DrawRay(_moveFrom.position, _moveFrom.up);
        }
    }
}
