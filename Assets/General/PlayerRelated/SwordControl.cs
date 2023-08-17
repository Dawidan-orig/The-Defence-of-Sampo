using System;
using UnityEngine;

public class SwordControl : MonoBehaviour
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
    public float minimalTimeBetweenAttacks = 2;

    [Header("init-s")]
    public Blade blade;
    [SerializeField]
    public Transform bladeContainer;
    [SerializeField]
    public Transform bladeHandle;
    [SerializeField]
    private Collider vital;

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
    float attackRecharge = 0;
    [SerializeField]
    private bool swinging = false;
    [SerializeField]
    private Vector3 swingEnd;

    public class ActionData : EventArgs 
    {
        public Transform moveStart;
        public Transform desire;
        public Blade blade;
    }

    public EventHandler<ActionData> OnSlashStart;
    public EventHandler<ActionData> OnSlash;
    public EventHandler<ActionData> OnSlashEnd;
    /*
    public EventHandler<ActionData> OnBlockStart;
    public EventHandler<ActionData> OnBlock;
    public EventHandler<ActionData> OnBlockEnd;
    */

    [Header("Debug")]
    [SerializeField]
    private bool isSwordFixing = true;

    private void Awake()
    {
        blade.SetHost(transform);
    }

    void Start()
    {
        attackRecharge = minimalTimeBetweenAttacks;

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

    private void FixedUpdate()
    {
        if (moveProgress < 1)
            moveProgress += actionSpeed * Time.fixedDeltaTime;

        if (attackRecharge < minimalTimeBetweenAttacks)
            attackRecharge += Time.fixedDeltaTime;

        if (!swinging)
            Control_MoveSword();
        else
            Control_SwingSword();

        if (isSwordFixing)
            Control_FixSword();
    }

    // Атака оружием по какой-то точке из текущей позиции.
    public void Swing(Vector3 toPoint)
    {
        if (swinging)
            return;

        swinging = true;        
        Vector3 moveTo = toPoint + (toPoint - bladeHandle.position).normalized * swing_EndDistanceMultiplier;

        Vector3 pointDir = (moveTo - vital.bounds.center).normalized;

        // Притягиваем ближе к vital
        float distance = (toPoint - vital.ClosestPointOnBounds(toPoint)).magnitude;
        bladeHandle.position = bladeHandle.position + (bladeHandle.position - vital.bounds.center).normalized * distance;
        moveTo = vital.ClosestPointOnBounds(moveTo) + (moveTo - vital.ClosestPointOnBounds(moveTo)).normalized * distance;
        SetDesires(moveTo, pointDir, (moveTo - toPoint).normalized);
        NullifyProgress();

        OnSlashStart?.Invoke(this, new ActionData { blade = blade, desire = desireBlade, moveStart = moveFrom});
    }

    // Установка меча по всем возможным параметрам
    public void Block(Vector3 start, Vector3 end, Vector3 SlashingDir)
    {
        if (swinging)
            return;

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
    }

    private void Control_SwingSword()
    {
        float heightFrom = moveFrom.position.y;
        float heightTo = desireBlade.position.y;

        Vector3 from = new Vector3(moveFrom.position.x, 0, moveFrom.position.z);
        Vector3 to = new Vector3(desireBlade.position.x, 0, desireBlade.position.z);

        bladeHandle.position = Vector3.Slerp(from, to, moveProgress) + new Vector3(0, Mathf.Lerp(heightFrom, heightTo, moveProgress), 0);

        bladeHandle.LookAt(bladeHandle.position + (bladeHandle.position - vital.bounds.center).normalized);
        bladeHandle.RotateAround(bladeHandle.position, bladeHandle.right, 90);

        if (moveProgress >= 1)
        {
            OnSlashEnd?.Invoke(this, new ActionData {moveStart = moveFrom, desire = desireBlade, blade = blade });
            swinging = false;
        }
        else 
        {
            OnSlash?.Invoke(this, new ActionData { moveStart = moveFrom, desire = desireBlade, blade = blade });
        }
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

    public void ReturnToInitial() 
    {
        if (swinging)
            return;

        SetDesires(initialBlade.position, initialBlade.up, initialBlade.forward);
        Control_MoveSword();
        NullifyProgress();
    }

    public void ApplyNewDesire(Vector3 pos, Vector3 up, Vector3 forward) 
    {
        if (swinging)
            return;

        SetDesires(pos, up, forward);
        Control_MoveSword();
        NullifyProgress();
    }

    private void SetDesires(Vector3 pos, Vector3 up, Vector3 forward)
    {
        desireBlade.position = pos;
        desireBlade.LookAt(pos + forward, up);

        if (isSwordFixing)
            Control_FixDesire();
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