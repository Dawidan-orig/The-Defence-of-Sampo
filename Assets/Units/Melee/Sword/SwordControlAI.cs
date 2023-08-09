using UnityEngine;

[RequireComponent(typeof(AttackCatcher))]
public class SwordControlAI : MonoBehaviour
{
    [Header("constraints")]
    public float actionSpeed = 10; // Скорость движения меча в руке
    public float block_minDistance = 1; // Минимальное расстояние для блока, используемое для боев с противником, а не отбивания.
    public float swing_EndDistanceMultiplier = 2; // Насколько далеко должен двинуться меч после отбивания.
    public float swing_startDistance = 2; // Насколько далеко должен двинуться меч до удара.
    public float criticalImpulse = 200; // Лучше увернуться, чем отбить объект с импульсом больше этого!
    public float toBladeHandle_MaxDistance = 2; // Максимальное расстояние от vital до рукояти меча. По сути, длина руки.
    public float toBladeHandle_MinDistance = 0.1f; // Минимальное расстояние от vital.
    public float close_enough = 0.1f; // Расстояние до цели, при котором можно менять состояние.

    [Header("timers")]
    public float toInitialAwait = 2; // Сколько времени ожидать до установки меча в обычную позицию?
    public float minimalTimeBetweenAttacks = 2;

    [Header("init-s")]
    public Blade blade;
    [SerializeField]
    private Transform bladeContainer;
    [SerializeField]
    private Transform bladeHandle;
    [SerializeField]
    private Collider vital;
    [SerializeField]
    private SwordControlAI enemy; //TODO : Заменить на MeleeFighter
    //TODO : Автоматизировать выбор этого самого enemy

    [Header("lookonly")]
    [SerializeField]
    Transform initialBlade;
    [SerializeField]
    Transform moveFrom;
    [SerializeField]
    Transform desireBlade;
    [SerializeField]
    float moveProgress;
    [SerializeField]
    float currentToInitialAwait;
    [SerializeField]
    Rigidbody lastIncoming = null;
    [SerializeField]
    float attackRecharge = 0;
    [SerializeField]
    ControlState stateOfBlade = ControlState.interruptable;

    [Header("Debug")]
    [SerializeField]
    private bool isSwordFixing = true;

    private enum ControlState
    {
        interruptable,
        swinging,
        interruptable_repositioning,
        hard_repositioning
    }

    void Start()
    {
        AttackCatcher catcher = gameObject.GetComponent<AttackCatcher>();
        catcher.OnIncomingAttack += Incoming;
        catcher.ignored.Add(blade.body);

        currentToInitialAwait = toInitialAwait;
        attackRecharge = minimalTimeBetweenAttacks;

        blade.GetComponent<Tool>().SetHost(transform);
        blade.OnBladeCollision += BladeCollisionEnter;

        if (bladeContainer == null)
            bladeContainer = transform;

        GameObject desireGO = new("DesireBlade");
        desireBlade = desireGO.transform;
        desireBlade.parent = bladeContainer;
        desireBlade.gameObject.SetActive(true);
        desireBlade.position = bladeHandle.position;
        desireBlade.rotation = bladeHandle.rotation;

        GameObject initialBladeGO = new("InititalBladePosition");
        initialBlade = initialBladeGO.transform;
        initialBlade.position = bladeHandle.position;
        initialBlade.rotation = bladeHandle.rotation;
        initialBlade.parent = bladeContainer;

        SetDesires(initialBlade.position, initialBlade.up, initialBlade.forward);
        NullifyProgress();
        moveProgress = 1;
    }

    private void BladeCollisionEnter(object sender, Collision collision)
    {
        if ((!collision.gameObject.TryGetComponent<Rigidbody>(out _)) ||
            collision.gameObject.TryGetComponent<Blade>(out _))
        {
            BeginTransitionToInitial();
            return;
        }

        //TODO : Raycast для хорошей коллизии; Пули не отбиваются - слишком маленькие. Но с Continious Detection сбиваются нормально.

        //TODO : При отбивании меча блоком ->
        // Это отбивание происходит не мгновенно, первые несколько кадров после отбивания летящий меч колбасит.
        // Вариант: Сделать время "Контузии" после столкновения, при котором не происходит никакой реакции (Incoming игнорируется)
    }

    private void FixedUpdate()
    {
        if (moveProgress < 1)
            moveProgress += actionSpeed * Time.fixedDeltaTime;

        if (attackRecharge < minimalTimeBetweenAttacks)
            attackRecharge += Time.fixedDeltaTime;

        if (stateOfBlade == ControlState.interruptable)
            HardPreparedSwing(enemy.transform.position);

        if (stateOfBlade != ControlState.swinging)
        {
            if (Vector3.Distance(bladeHandle.position, desireBlade.position) < close_enough)
            {
                stateOfBlade = ControlState.interruptable;

                if (currentToInitialAwait < toInitialAwait)
                    currentToInitialAwait += Time.fixedDeltaTime;
                else
                {
                    if (initialBlade.position != desireBlade.position)
                        BeginTransitionToInitial();
                }
            }

            Control_MoveSword();
        }
        else
            Control_SwingSword();

        if (isSwordFixing)
            Control_FixSword();
    }

    private void Incoming(object sender, AttackCatcher.AttackEventArgs e)
    {
        Rigidbody currentIncoming = e.body;
        currentToInitialAwait = 0;

        //TODO : Если у меча есть host, то пусть берётся меньшее растояние от этого самого host'а до меча! Тогда не будет расколбаса.
        // Ещё вариант: Сделать BlockingDistance.

        if (e.free)
        {
            if (e.impulse < criticalImpulse)
            {
                PreparedSwing(e.start);
            }
            else
            {
                //TODO : Evade() -- Должен быть в другом скрипте, тут - только вызов
            }
        }
        else
        {
            Vector3 enemyBladeCenter = Vector3.Lerp(e.start, e.end, 0.5f);

            GameObject bladePrediction = new();
            bladePrediction.transform.position = enemyBladeCenter;

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

            Vector3 toEnemyBlade_Dir = (bladePrediction.transform.position - vital.bounds.center).normalized;
            bladePrediction.transform.Rotate(toEnemyBlade_Dir, 90); // Ставим перпендикулярно

            if (e.body.GetComponent<Tool>().host != null) // Притягиваем меч максимально близко к себе.
            {
                //TODO : Заменить на handle
                //TODO : Заменить на SDF; Во время блока меч влезает внутрь тела.  
                Vector3 boundsClosest = vital.ClosestPointOnBounds(bladePrediction.transform.position);
                bladePrediction.transform.position = boundsClosest
                    + (bladePrediction.transform.position - boundsClosest).normalized * block_minDistance;
            }

            Vector3 bladeDown = start.transform.position;
            Vector3 bladeUp = end.transform.position;

            Destroy(bladePrediction);
            int ignored = blade.gameObject.layer; // Для игнора лезвий при проверке.
            ignored = ~ignored;

            //TODO : Поменять на BoxCast'ы
            if (Physics.Raycast(bladeDown, bladeUp - bladeDown, (bladeDown - bladeUp).magnitude, ignored) // Снизу вверх
                ||
                Physics.Raycast(bladeUp, bladeDown - bladeUp, (bladeDown - bladeUp).magnitude, ignored) // Сверху вниз
                )
            {
                //Debug.DrawLine(bladeUp,bladeDown,Color.gray, 2);
                return;
            }

            //IDEA : Усложнение, которое сделает лучше.
            // Сейчас очень много предсказаний аннулируются из-за коллизий. Есть альтернативное решение: Подбирать при коллизии ближайшие точки от меча до коллайдера такие,
            // Что вот буквально ещё шаг - и уже будет столкновение.

            Block(bladeDown, bladeUp, toEnemyBlade_Dir);
            if ((currentIncoming != lastIncoming)) // Если новый объект летит - обновляем движение
                NullifyProgress();
        }

        lastIncoming = currentIncoming;
    }

    //Сначала обязательно замах, потом - удар.
    void HardPreparedSwing(Vector3 toPoint)
    {
        Vector3 bladeCenter = Vector3.Lerp(blade.upperPoint.position, blade.downerPoint.position, 0.5f);
        float bladeCenterLen = Vector3.Distance(bladeCenter, blade.downerPoint.position);
        float swingDistance = bladeCenterLen + toBladeHandle_MaxDistance;

        if (Vector3.Distance(vital.bounds.center, toPoint) >= swingDistance
            && attackRecharge < minimalTimeBetweenAttacks)
            return;

        if (stateOfBlade == ControlState.interruptable)
        {
            Vector3 toPoint_dir = (bladeCenter - toPoint).normalized;
            Vector3 bladeStart = toPoint + toPoint_dir * swing_startDistance;
            Vector3 closest = vital.ClosestPointOnBounds(bladeStart);

            SetDesires(closest + (bladeStart - closest).normalized * toBladeHandle_MinDistance,
                    (bladeCenter - vital.bounds.center).normalized,
                    (toPoint - bladeHandle.position).normalized);
            BeginTransitionToDestire();
            stateOfBlade = ControlState.hard_repositioning;
            
            if(CloseToDesire())
            {
                Swing(toPoint);
                NullifyProgress();
            }
        }
    }

    // Бьём по цели, если возможно. Иначе - готовим удар.
    void PreparedSwing(Vector3 toPoint)
    {
        Vector3 bladeCenter = Vector3.Lerp(blade.upperPoint.position, blade.downerPoint.position, 0.5f);
        float bladeCenterLen = Vector3.Distance(bladeCenter, blade.downerPoint.position);
        float swingDistance = bladeCenterLen + toBladeHandle_MaxDistance;

        if (Vector3.Distance(vital.bounds.center, toPoint) < swingDistance
            && IsInterruptableState())
        {
            Swing(toPoint);
            NullifyProgress();
        }
        else if (stateOfBlade == ControlState.interruptable)
        {
            SetDesires(toPoint + (bladeCenter - toPoint).normalized * swing_startDistance,
                    (bladeCenter - vital.bounds.center).normalized,
                    (toPoint - bladeHandle.position).normalized);
            BeginTransitionToDestire();
            stateOfBlade = ControlState.interruptable_repositioning;
        }
    }

    // Атака оружием по какой-то точке из текущей позиции.
    public void Swing(Vector3 toPoint)
    {
        attackRecharge = 0;
        stateOfBlade = ControlState.swinging;

        Vector3 moveTo = toPoint + (toPoint - bladeHandle.position).normalized * swing_EndDistanceMultiplier;

        Vector3 pointDir = (moveTo - vital.bounds.center).normalized;

        // Притягиваем ближе к vital
        float distance = (toPoint - vital.ClosestPointOnBounds(toPoint)).magnitude;
        moveTo = vital.ClosestPointOnBounds(moveTo) + (moveTo - vital.ClosestPointOnBounds(moveTo)).normalized * distance;
        SetDesires(moveTo, pointDir, (moveTo - toPoint).normalized);
    }

    // Установка меча по всем возможным параметрам
    public void Block(Vector3 start, Vector3 end, Vector3 SlashingDir)
    {
        SetDesires(start, (end - start).normalized, SlashingDir);
    }

    private void Control_MoveSword()
    {
        float heightFrom = moveFrom.position.y;
        float heightTo = desireBlade.position.y;

        Vector3 from = new Vector3(moveFrom.position.x, 0, moveFrom.position.z);
        Vector3 to = new Vector3(desireBlade.position.x, 0, desireBlade.position.z);

        bladeHandle.position = Vector3.Slerp(from, to, moveProgress) + new Vector3(0, Mathf.Lerp(heightFrom, heightTo, moveProgress), 0);

        #region rotationControl;
        GameObject go = new();
        Transform probe = go.transform;
        probe.position = moveFrom.position;
        probe.rotation = desireBlade.rotation;
        probe.parent = null;

        bladeHandle.rotation = Quaternion.Lerp(moveFrom.rotation, probe.rotation, moveProgress);

        Destroy(go);
        #endregion

        /*
        Vector3 closestPos = vital.ClosestPointOnBounds(bladeHandle.position);
        const float TWO_DIVIDE_THREE = 2/3;
        
        if(moveProgress < TWO_DIVIDE_THREE) // Притягиваем максимально близко
        {
            desireBlade.position = closestPos + (desireBlade.position - closestPos).normalized * bladeMinDistance;

            GameObject upDirectioner = new();
            Vector3 toNearest = closestPos - desireBlade.position;
            upDirectioner.transform.up = toNearest;
            upDirectioner.transform.Rotate(0,0,90);
            desireBlade.up = upDirectioner.transform.up;
            Destroy(upDirectioner);
        }
        */

        // На будущее:
        //TODO : Переставлять desire, когда на пути до него есть препятствия
        //TODO : Добавить событий на начало движения меча, состояние в прогрессе и конец движения.
    }

    private void Control_SwingSword()
    {
        if (CloseToDesire())
            BeginTransitionToDestire();

        float heightFrom = moveFrom.position.y;
        float heightTo = desireBlade.position.y;

        Vector3 from = new Vector3(moveFrom.position.x, 0, moveFrom.position.z);
        Vector3 to = new Vector3(desireBlade.position.x, 0, desireBlade.position.z);

        bladeHandle.position = Vector3.Slerp(from, to, moveProgress) + new Vector3(0, Mathf.Lerp(heightFrom, heightTo, moveProgress), 0);

        bladeHandle.LookAt(bladeHandle.position + (bladeHandle.position - vital.bounds.center).normalized);
        bladeHandle.RotateAround(bladeHandle.position, bladeHandle.right, 90);
    }

    private void Control_FixSword()
    {
        // Притягиваем меч ближе
        Vector3 closestPos = vital.ClosestPointOnBounds(bladeHandle.position);
        if (Vector3.Distance(bladeHandle.position, closestPos) > toBladeHandle_MaxDistance)
            bladeHandle.position = closestPos + (bladeHandle.position - closestPos).normalized * toBladeHandle_MaxDistance;

        //Аналогичным образом отталкиваем
        if (Vector3.Distance(bladeHandle.position, closestPos) < toBladeHandle_MinDistance)
            bladeHandle.position = closestPos + (bladeHandle.position - closestPos).normalized * toBladeHandle_MinDistance;
    }

    private void Control_FixDesire()
    {
        Vector3 closestPos = vital.ClosestPointOnBounds(desireBlade.position);
        if (Vector3.Distance(desireBlade.position, closestPos) > toBladeHandle_MaxDistance)
            desireBlade.position = closestPos + (desireBlade.position - closestPos).normalized * toBladeHandle_MaxDistance;
        if (Vector3.Distance(desireBlade.position, closestPos) < toBladeHandle_MinDistance)
            desireBlade.position = closestPos + (desireBlade.position - closestPos).normalized * toBladeHandle_MinDistance;
    }

    private void BeginTransitionToDestire()
    {
        currentToInitialAwait = 0;
        stateOfBlade = ControlState.hard_repositioning;
        SetDesires(desireBlade.position, desireBlade.up, desireBlade.forward);
        NullifyProgress();
    }

    private void BeginTransitionToInitial()
    {
        stateOfBlade = ControlState.hard_repositioning;
        SetDesires(initialBlade.position, initialBlade.up, initialBlade.forward);
        NullifyProgress();
        currentToInitialAwait = toInitialAwait;
    }
    private void SetDesires(Vector3 pos, Vector3 up, Vector3 forward)
    {
        desireBlade.position = pos;
        desireBlade.LookAt(pos + forward, up);

        if (isSwordFixing)
            Control_FixDesire();

        if (moveProgress >= 1)
        {
            NullifyProgress();
        }
    }

    private bool CloseToDesire()
    {
        Debug.DrawLine(bladeHandle.position, desireBlade.position);
        return Vector3.Distance(bladeHandle.position, desireBlade.position) < close_enough;
    }

    private bool IsInterruptableState()
    {
        return stateOfBlade == ControlState.interruptable || stateOfBlade == ControlState.interruptable_repositioning;
    }

    private void NullifyProgress()
    {
        if (moveFrom != null)
            Destroy(moveFrom.gameObject);
        GameObject moveFromGO = new("BladeIsMovingFromThatTransform");
        moveFrom = moveFromGO.transform;
        moveFrom.position = bladeHandle.position;
        moveFrom.rotation = bladeHandle.rotation;
        moveFrom.parent = bladeContainer;
        moveProgress = 0;
    }
}