using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AttackCatcher))]
public class SwordFighter_StateMachine : MeleeFighter
{
    [Header("constraints")]
    public float actionSpeed = 1; // Скорость движения меча в руке
    public float swingSpeed = 1; // Скорость взмаха мечом, для большего контроль
    public float block_minDistance = 0.3f; // Минимальное расстояние для блока, используемое для боев с противником, а не отбивания.
    public float swing_EndDistanceMultiplier = 1.5f; // Насколько далеко должен двинуться меч после отбивания.
    public float swing_startDistance = 1.5f; // Насколько далеко должен двинуться меч до удара.
    public float criticalImpulse = 400; // Лучше увернуться, чем отбить объект с импульсом больше этого!
    public float blockCriticalVelocity = 5; // Всё, что имеет скорость выше этого значения - блокируется
    public float toBladeHandle_MaxDistance = 2; // Максимальное расстояние от vital до рукояти меча. По сути, длина руки.
    public float toBladeHandle_MinDistance = 0.1f; // Минимальное расстояние от vital.    
    public float close_enough = 0.1f; // Расстояние до цели, при котором можно менять состояние.
    public float angle_enough = 10; // Достаточный угол, чтобы считать что handle близок к desire
    public AnimationCurve attackProbability; // Указывает значения от 0 до 1 означающие вероятность выбора удара слева направо поверху.

    [Header("timers")]
    public float toInitialAwait = 2; // Сколько времени ожидать до установки меча в обычную позицию?

    [Header("init-s")]
    [SerializeField]
    private Blade _blade;
    [SerializeField]
    private Transform _bladeContainer;
    [SerializeField]
    private Transform _bladeHandle;
    [SerializeField]
    private Collider _vital;

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
    AttackCatcher _catcher;
    [SerializeField]
    Stack<ActionJoint> _currentCombo = new Stack<ActionJoint>();

    public enum ActionType
    {
        Swing,
        Reposition
    }

    public struct ActionJoint
    {
        public Vector3 relativeDesireFrom;
        public Quaternion rotationFrom;
        public Vector3 nextRelativeDesire;
        public Quaternion nextRotation;
        public ActionType currentActionType;
    }

    SwordFighter_BaseState _currentSwordState;
    SwordFighter_StateFactory _fighter_states;

    #region Getters and setters
    public SwordFighter_BaseState CurrentSwordState { get { return _currentSwordState; } set { _currentSwordState = value; } }
    public Transform BladeHandle { get { return _bladeHandle; } }
    public Transform DesireBlade { get { return _desireBlade; } }
    public Transform MoveFrom { get { return _moveFrom; } }
    public float MoveProgress { get { return _moveProgress; } }
    public Blade Blade { get { return _blade; } }
    public Transform InitialBlade { get => _initialBlade; set => _initialBlade = value; }
    public float CurrentToInitialAwait { get => _currentToInitialAwait; set => _currentToInitialAwait = value; }
    public Collider Vital { get => _vital; set => _vital = value; }
    public AttackCatcher AttackCatcher { get => _catcher; set => _catcher = value; }
    public Stack<ActionJoint> CurrentCombo { get => _currentCombo; set => _currentCombo = value; }

    #endregion

    public EventHandler<IncomingReposEventArgs> OnRepositionIncoming;
    public EventHandler<IncomingSwingEventArgs> OnSwingIncoming;

    public class IncomingReposEventArgs : EventArgs
    {
        public Vector3 bladeDown;
        public Vector3 bladeUp;
        public Vector3 bladeDir;
    }

    public class IncomingSwingEventArgs : EventArgs
    {
        public Vector3 toPoint;
    }


    [Header("Debug")]
    [SerializeField]
    private bool isSwordFixing = true;
    [SerializeField]
    private string currentState; // Нужен для вывода текущего состояния в Unity

    protected override void Awake()
    {
        base.Awake();

        _catcher = gameObject.GetComponent<AttackCatcher>();
        _catcher.ignored.Add(_blade.body);

        _currentToInitialAwait = toInitialAwait;

        _fighter_states = new SwordFighter_StateFactory(this);
        _currentSwordState = _fighter_states.Idle();
        _currentSwordState.EnterState();

        _blade.GetComponent<Tool>().SetHost(transform);

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

    protected override void Start()
    {
        base.Start();

        AttackCatcher.OnIncomingAttack += Incoming;
    }

    protected override void Update()
    {
        if (!_AIActive)
            return;

        base.Update();
        _currentSwordState.UpdateState();

        currentState = _currentSwordState.ToString();
    }

    protected override void FixedUpdate()
    {
        if (!_AIActive)
            return;

        base.FixedUpdate();
        _currentSwordState.FixedUpdateState();

        if (_moveProgress < 1) {
            if (_currentSwordState is SwordFighter_RepositioningState)            
                _moveProgress += actionSpeed * Time.fixedDeltaTime / Vector3.Distance(_moveFrom.position, _desireBlade.position);            
            else if(_currentSwordState is SwordFighter_SwingingState)
                _moveProgress += swingSpeed * Time.fixedDeltaTime / Vector3.Distance(_moveFrom.position, _desireBlade.position);
        }
    }

    private void Incoming(object sender, AttackCatcher.AttackEventArgs e)
    {
        Rigidbody currentIncoming = e.body;
        CurrentToInitialAwait = 0;

        if (e.free && e.body.velocity.magnitude < blockCriticalVelocity)
        {
            if (e.impulse > criticalImpulse)
            {
                //IDEA: Вариант сделать замах: С помощью Curve.

                Vector3 toPoint = e.start;

                Vector3 bladeCenter = Vector3.Lerp(Blade.upperPoint.position, Blade.downerPoint.position, 0.5f);
                float bladeCenterLen = Vector3.Distance(bladeCenter, Blade.downerPoint.position);
                float swingDistance = bladeCenterLen + toBladeHandle_MaxDistance;

                if (Vector3.Distance(distanceFrom.position, toPoint) < swingDistance)
                {
                    OnSwingIncoming?.Invoke(this, new IncomingSwingEventArgs { toPoint = toPoint });
                }
            }
        }
        else if (e.free && e.body.velocity.magnitude >= blockCriticalVelocity)
        {
            Vector3 blockPoint = Vector3.Lerp(e.start, e.end, 0.5f);

            GameObject bladePrediction = new("NotDeletedPrediction");
            bladePrediction.transform.position = _blade.transform.position;

            GameObject start = new();
            start.transform.position = _blade.downerPoint.position;
            start.transform.parent = bladePrediction.transform;

            GameObject end = new();
            end.transform.position = _blade.upperPoint.position;
            end.transform.parent = bladePrediction.transform;

            bladePrediction.transform.position = blockPoint;

            Vector3 toEnemyBlade_Dir = (bladePrediction.transform.position - Vital.bounds.center).normalized;

            bladePrediction.transform.LookAt(start.transform.position +
                Vector3.ProjectOnPlane((end.transform.position - start.transform.position).normalized, e.body.velocity), start.transform.position + Vector3.up);

            //Vector3 closest = _vital.ClosestPointOnBounds(bladePrediction.transform.position);
            //bladePrediction.transform.position = closest
            //    + (bladePrediction.transform.position - closest).normalized * block_minDistance;

            Vector3 bladeDown = start.transform.position;
            Vector3 bladeUp = end.transform.position;

            Destroy(bladePrediction);

            int ignored = Blade.gameObject.layer; // Для игнора лезвий при проверке.
            ignored = ~ignored;

            BoxCollider bladeCollider = Blade.GetComponent<BoxCollider>();
            Vector3 bladeHalfWidthLength = new Vector3((bladeCollider.size.x * bladeCollider.transform.lossyScale.x) / 2, 0.1f, (bladeCollider.size.z * bladeCollider.transform.lossyScale.z) / 2);

            //IDEA : Усложнение, которое сделает лучше.
            // Сейчас очень много предсказаний аннулируются из-за коллизий. Есть альтернативное решение: Подбирать при коллизии ближайшие точки от меча до коллайдера такие,
            // Что вот буквально ещё шаг - и уже будет столкновение.

            OnRepositionIncoming?.Invoke(this, new IncomingReposEventArgs { bladeDown = bladeDown, bladeUp = bladeUp, bladeDir = toEnemyBlade_Dir });
            //OnSwingIncoming?.Invoke(this, new IncomingSwingEventArgs {toPoint = bladeUp });
        }
        else
        {
            Vector3 blockPoint = Vector3.Lerp(e.start, e.end, 0.5f);
            Debug.DrawLine(e.start, e.end, Color.white);

            GameObject bladePrediction = new("NotDeletedPrediction");
            bladePrediction.transform.position = blockPoint;

            GameObject start = new();
            start.transform.position = e.start;
            start.transform.parent = bladePrediction.transform;

            GameObject end = new();
            end.transform.position = e.end;
            end.transform.parent = bladePrediction.transform;

            //TODO : Это логика рапиры, из-за чего отбиваемое оружие "отражается".
            //bladePrediction.transform.Rotate(e.direction, 90);

            // Синхронизация для параллельности vital
            //bladePrediction.transform.rotation = Quaternion.FromToRotation((end.transform.position - start.transform.position).normalized, transform.up);

            Vector3 toEnemyBlade_Dir = (bladePrediction.transform.position - Vital.bounds.center).normalized;
            bladePrediction.transform.Rotate(toEnemyBlade_Dir, 90); // Ставим перпендикулярно


            // Притягиваем меч максимально близко к себе.
            /*
            if (e.body.GetComponent<Tool>().host != null)
            {
                bladePrediction.transform.position = distanceFrom.position
                    + (bladePrediction.transform.position - distanceFrom.position).normalized * block_minDistance;
            }*/

            Vector3 bladeDown = start.transform.position;
            Vector3 bladeUp = end.transform.position;
            Destroy(bladePrediction);

            int ignored = Blade.gameObject.layer; // Для игнора лезвий при проверке.
            ignored = ~ignored;

            BoxCollider bladeCollider = Blade.GetComponent<BoxCollider>();
            Vector3 bladeHalfWidthLength = new Vector3((bladeCollider.size.x * bladeCollider.transform.lossyScale.x) / 2, 0.1f, (bladeCollider.size.z * bladeCollider.transform.lossyScale.z) / 2);

            /*
            if (Utilities.VisualisedBoxCast(bladeDown,
                bladeHalfWidthLength,
                (bladeUp - bladeDown).normalized,
                out _,
                Quaternion.FromToRotation(Vector3.up, (bladeUp - bladeDown).normalized),
                (bladeDown - bladeUp).magnitude,
                ignored,
                true,
                new Color(0.5f, 0.5f, 1f, 0.6f))
                ||
                Utilities.VisualisedBoxCast(bladeUp,
                bladeHalfWidthLength,
                (bladeDown - bladeUp).normalized,
                out _,
                Quaternion.FromToRotation(Vector3.up, (bladeDown - bladeUp).normalized),
                (bladeDown - bladeUp).magnitude,
                ignored,
                true,
                new Color(0.5f, 0.5f, 1f, 0.6f)))
            {
                return;
            }*/

            //IDEA : Усложнение, которое сделает лучше.
            // Сейчас очень много предсказаний аннулируются из-за коллизий. Есть альтернативное решение: Подбирать при коллизии ближайшие точки от меча до коллайдера такие,
            // Что вот буквально ещё шаг - и уже будет столкновение.
            Vector3 centerOffset = (Blade.downerPoint.position - Blade.downerPoint.position).normalized *
                (-Vector3.Distance(BladeHandle.position, Blade.downerPoint.position)); // Смещение для ровной установки рукояти

            OnRepositionIncoming?.Invoke(this, new IncomingReposEventArgs { bladeDown = centerOffset+ bladeDown, bladeUp = centerOffset+ bladeUp, bladeDir = toEnemyBlade_Dir });
        }   
    }

    // Установка меча по всем возможным параметрам
    public override void Block(Vector3 start, Vector3 end, Vector3 SlashingDir)
    {
        base.Block(start, end, SlashingDir);

        if (Vector3.Distance(distanceFrom.position, start) > Vector3.Distance(distanceFrom.position, end))
            (end, start) = (start, end);

        SetDesires(start, (end - start).normalized, SlashingDir);
    }

    public override void AttackUpdate(Transform target)
    {
        base.AttackUpdate(target);

        if (!_swingReady || CurrentCombo.Count > 0)
            return;

        //Тут ещё можем выбирать конкретную комбинацию из библиотеки комбо.

        ActionJoint afterPreparation = new ActionJoint();
        ActionJoint preparation = new ActionJoint();

        /*
        Plane transformXY = new(transform.forward, transform.position);
        Vector3 toNewPosDir = (transformXY.ClosestPointOnPlane(BladeHandle.position) - transform.position).normalized;
        Vector3 newPos = distanceFrom.position + toNewPosDir * swing_startDistance;
        */

        // Выбираем какую-то точку для удар
        const int LIMIT = 50;
        float posX=0;
        bool res = false;
        int iteration = 0;
        while(!res) //TODO : Оптимизировать одной формулой
        {
            posX = UnityEngine.Random.Range(0, 1);
            float prob = UnityEngine.Random.Range(0, 1);
            res = attackProbability.Evaluate(posX) > prob;

            if (iteration++ > LIMIT)
                break;
        }

        Vector3 newPos = distanceFrom.position + new Vector3(posX-0.5f,Mathf.Abs(posX-0.5f)).normalized * swing_startDistance;

        GameObject gameObj = new GameObject("NotDestroyedInAttackUpdate");
        Transform preaparePoint = gameObj.transform;
        preaparePoint.parent = transform;
        preaparePoint.position = newPos;
        preaparePoint.LookAt(preaparePoint.position + (preaparePoint.position - Vital.bounds.center).normalized,
            (CurrentActivity.target.position - preaparePoint.position).normalized);
        preaparePoint.RotateAround(preaparePoint.position, preaparePoint.right, 90);

        preparation.rotationFrom = _bladeHandle.rotation;
        preparation.relativeDesireFrom = _bladeHandle.position - transform.position;
        preparation.nextRelativeDesire = preaparePoint.position - transform.position;
        preparation.nextRotation = preaparePoint.rotation;
        preparation.currentActionType = ActionType.Reposition;

        afterPreparation.relativeDesireFrom = preaparePoint.position - transform.position;
        afterPreparation.rotationFrom = preaparePoint.rotation;
        afterPreparation.nextRelativeDesire = CurrentActivity.target.position - transform.position; //TODO : Поменять на Transform
        //Поворот игнорируем, поскольку swing
        afterPreparation.currentActionType = ActionType.Swing;

        //Добавляем начиная с последнего
        _currentCombo.Push(afterPreparation);
        _currentCombo.Push(preparation);

        Destroy(gameObj);
    }

    // Атака оружием по какой-то точке из текущей позиции.
    public override void Swing(Vector3 toPoint)
    {
        if (!_swingReady)
            return;

        base.Swing(toPoint);

        Vector3 moveTo = toPoint + (toPoint - BladeHandle.position).normalized * swing_EndDistanceMultiplier;

        Vector3 pointDir = (moveTo - _vital.bounds.center).normalized;

        Vector3 closest = _vital.ClosestPointOnBounds(moveTo);
        float distance = (toPoint - closest).magnitude;
        moveTo = closest + (moveTo - closest).normalized * distance;
        SetDesires(moveTo, pointDir, (moveTo - toPoint).normalized);
    }

    private void FixDesire()
    {
        if (!isSwordFixing)
            return;

        Vector3 countFrom = distanceFrom.position;
        Vector3 closest = _vital.ClosestPointOnBounds(_desireBlade.position);
        if (Vector3.Distance(_desireBlade.position, countFrom) > toBladeHandle_MaxDistance)
        {
            Vector3 toCloseDir = (closest - _desireBlade.position).normalized;
            Vector3 exceededHand = _desireBlade.position - countFrom;
            float toCloseLen = -1;

            // Теорема косинусов + Решение квадратного уравнения
            float angle = Vector3.Angle(toCloseDir, -exceededHand);

            Debug.DrawRay(_desireBlade.position, toCloseDir);
            Debug.DrawRay(_desireBlade.position, -exceededHand);

            float b = exceededHand.magnitude * Mathf.Cos(angle);
            float diskr = 4 *
                (Mathf.Pow(toBladeHandle_MaxDistance, 2) -
                Mathf.Pow(exceededHand.magnitude, 2) *
                Mathf.Pow(Mathf.Sin(angle * Mathf.Deg2Rad), 2));
            float s1 = b + Mathf.Sqrt(diskr);
            float s2 = b - Mathf.Sqrt(diskr);
            toCloseLen = (s1 > s2 ? s1 : s2);

            Debug.Log(diskr);
            if (diskr > 0)
            {
                Debug.DrawLine(countFrom, _desireBlade.position + toCloseDir * toCloseLen, Color.black);

                _desireBlade.position += toCloseDir * toCloseLen;
            }
            else
            {
                // Означает, что решения нет. А нет его по той причине, что новая точка будет уже в пределах досягаемости руки,
                // А значит нет смысла двигать ещё ближе.
            }
        }

        if (Vector3.Distance(_desireBlade.position, countFrom) < toBladeHandle_MinDistance)
        {
            /*
            Vector3 fromCloseDir = (_desireBlade.position - closest).normalized;
            Vector3 exceededHand = _desireBlade.position - countFrom;
            // Теорема косинусов
            float a = 1;
            float b = -2 * exceededHand.magnitude * Mathf.Cos(Vector3.Angle(fromCloseDir, -exceededHand));
            float c = Mathf.Pow(exceededHand.magnitude, 2) - Mathf.Pow(toBladeHandle_MaxDistance, 2);
            float diskr = Mathf.Pow(b, 2) - 4 * a * c;
            float s1 = (-b - Mathf.Sqrt(diskr)) / (2 * a);
            float s2 = (-b + Mathf.Sqrt(diskr)) / (2 * a);
            float fromCloseLen = (s1 > s2 ? s1 : s2);

            _desireBlade.position += fromCloseDir * fromCloseLen;
            */
        }
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

    protected override Tool ToolChosingCheck(Transform target)
    {
        return _blade;
    }

    public override Transform GetRightHandTarget()
    {
        return _bladeHandle;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

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
