using System;
using UnityEngine;

public class SwordControl : MonoBehaviour
{
    [Header("constraints")]
    public float actionSpeed = 10; // �������� �������� ���� � ����
    public float block_minDistance = 1; // ����������� ���������� ��� �����, ������������ ��� ���� � �����������, � �� ���������.
    public float swing_EndDistanceMultiplier = 2; // ��������� ������ ������ ��������� ��� ����� ���������.
    public float swing_startDistance = 2; // ��������� ������ ������ ��������� ��� �� �����.
    public float criticalImpulse = 200; // ����� ����������, ��� ������ ������ � ��������� ������ �����!
    public float toBladeHandle_MaxDistance = 0.6f; // ������������ ���������� �� vital �� ������� ����. �� ����, ����� ����.
    public float toBladeHandle_MinDistance = 0.1f; // ����������� ���������� �� vital.
    public float close_enough = 0.1f; // ���������� �� ����, ��� ������� ����� ������ ���������.

    [Header("timers")]
    public float minimalTimeBetweenAttacks = 2;

    [Header("init-s")]
    public Blade blade;
    [SerializeField]
    public Transform bladeContainer;
    [SerializeField]
    public Transform bladeHandle;
    [SerializeField]
    public Transform bladeHolder;
    [SerializeField]
    private Collider vital;

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
    float _attackRecharge = 0;
    [SerializeField]
    private bool _swinging = false;
    [SerializeField]
    private Vector3 _swingEnd;

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
        _attackRecharge = minimalTimeBetweenAttacks;

        if (bladeContainer == null)
            bladeContainer = transform;

        GameObject desireGO = new("DesireBlade");
        _desireBlade = desireGO.transform;
        _desireBlade.parent = bladeContainer;
        _desireBlade.gameObject.SetActive(true);
        _desireBlade.position = bladeHandle.position;
        _desireBlade.rotation = bladeHandle.rotation;

        GameObject initialBladeGO = new("InititalBladePosition");
        _initialBlade = initialBladeGO.transform;
        _initialBlade.position = bladeHandle.position;
        _initialBlade.rotation = bladeHandle.rotation;
        _initialBlade.parent = bladeContainer;

        SetDesires(_initialBlade.position, _initialBlade.up, _initialBlade.forward);
        NullifyProgress();
        _moveProgress = 1;
    }

    private void FixedUpdate()
    {
        if (_moveProgress < 1)
            _moveProgress += actionSpeed * Time.fixedDeltaTime;

        if (_attackRecharge < minimalTimeBetweenAttacks)
            _attackRecharge += Time.fixedDeltaTime;

        if (!_swinging)
            Control_MoveSword();
        else
            Control_SwingSword();

        if (isSwordFixing)
            Control_FixSword();
    }

    // ����� ������� �� �����-�� ����� �� ������� �������.
    public void Swing(Vector3 toPoint)
    {
        if (_swinging)
            return;

        _swinging = true;        
        Vector3 moveTo = toPoint + (toPoint - bladeHandle.position).normalized * swing_EndDistanceMultiplier;

        Vector3 pointDir = (moveTo - bladeHolder.position).normalized;

        // ����������� ����� � vital
        float distance = (toPoint - vital.ClosestPointOnBounds(toPoint)).magnitude;
        bladeHandle.position = bladeHandle.position + (bladeHandle.position - bladeHolder.position).normalized * distance;
        moveTo = vital.ClosestPointOnBounds(moveTo) + (moveTo - vital.ClosestPointOnBounds(moveTo)).normalized * distance;

        SetDesires(moveTo, pointDir, (moveTo - toPoint).normalized);
        //NullifyProgress();

        OnSlashStart?.Invoke(this, new ActionData { blade = blade, desire = _desireBlade, moveStart = _moveFrom});
    }

    // ��������� ���� �� ���� ��������� ����������
    public void Block(Vector3 start, Vector3 end, Vector3 SlashingDir)
    {
        if (_swinging)
            return;

        ApplyNewDesire(start, (end - start).normalized, SlashingDir);
    }

    private void Control_MoveSword()
    {
        float heightFrom = _moveFrom.position.y;
        float heightTo = _desireBlade.position.y;

        Vector3 from = new Vector3(_moveFrom.position.x, 0, _moveFrom.position.z);
        Vector3 to = new Vector3(_desireBlade.position.x, 0, _desireBlade.position.z);

        bladeHandle.position = Vector3.Slerp(from, to, _moveProgress) + new Vector3(0, Mathf.Lerp(heightFrom, heightTo, _moveProgress), 0);

        #region rotationControl;
        GameObject go = new();
        Transform probe = go.transform;
        probe.position = _moveFrom.position;
        probe.rotation = _desireBlade.rotation;
        probe.parent = null;

        bladeHandle.rotation = Quaternion.Lerp(_moveFrom.rotation, probe.rotation, _moveProgress);

        Destroy(go);
        #endregion
    }

    private void Control_SwingSword()
    {
        float relativeHeightFrom = _moveFrom.position.y - transform.position.y;
        float relativeHeightTo = _desireBlade.position.y - transform.position.y;

        Vector3 relativeFrom = _moveFrom.position - transform.position;
        relativeFrom.y = 0;
        Vector3 relativeTo = _desireBlade.position - transform.position;
        relativeTo.y = 0;

        bladeHandle.LookAt(bladeHandle.position + (bladeHandle.position - bladeHolder.position).normalized);
        bladeHandle.RotateAround(bladeHandle.position, bladeHandle.right, 90);

        if (_moveProgress <= 0.5f)
        {
            bladeHandle.position = transform.position
                + Vector3.Slerp(relativeFrom, transform.forward * toBladeHandle_MaxDistance, _moveProgress * 2)
                + new Vector3(0, Mathf.Lerp(relativeHeightFrom, relativeHeightTo, _moveProgress), 0);
        }
        else 
        {
            bladeHandle.position = transform.position
             + Vector3.Slerp(transform.forward * toBladeHandle_MaxDistance, relativeTo, (_moveProgress - 0.5f)*2)
             + new Vector3(0, Mathf.Lerp(relativeHeightFrom, relativeHeightTo, _moveProgress), 0);
        }

        Utilities.DrawSphere(bladeHandle.position, duration: 0.5f);

        if (_moveProgress >= 1)
        {
            OnSlashEnd?.Invoke(this, new ActionData {moveStart = _moveFrom, desire = _desireBlade, blade = blade });
            NullifyProgress();
            _swinging = false;
        }
        else 
        {
            OnSlash?.Invoke(this, new ActionData { moveStart = _moveFrom, desire = _desireBlade, blade = blade });
        }
    }

    private void Control_FixSword()
    {
        // ����������� ��� �����
        Vector3 closestPos = vital.ClosestPointOnBounds(bladeHandle.position);
        if (Vector3.Distance(bladeHandle.position, closestPos) > toBladeHandle_MaxDistance)
            bladeHandle.position = closestPos + (bladeHandle.position - closestPos).normalized * toBladeHandle_MaxDistance;

        //����������� ������� �����������
        if (Vector3.Distance(bladeHandle.position, closestPos) < toBladeHandle_MinDistance)
            bladeHandle.position = closestPos + (bladeHandle.position - closestPos).normalized * toBladeHandle_MinDistance;
    }

    private void Control_FixDesire()
    {
        Vector3 closestPos = vital.ClosestPointOnBounds(_desireBlade.position);
        if (Vector3.Distance(_desireBlade.position, closestPos) > toBladeHandle_MaxDistance)
            _desireBlade.position = closestPos + (_desireBlade.position - closestPos).normalized * toBladeHandle_MaxDistance;
        if (Vector3.Distance(_desireBlade.position, closestPos) < toBladeHandle_MinDistance)
            _desireBlade.position = closestPos + (_desireBlade.position - closestPos).normalized * toBladeHandle_MinDistance;
    }

    public void ReturnToInitial() 
    {
        if (_swinging)
            return;

        SetDesires(_initialBlade.position, _initialBlade.up, _initialBlade.forward);
        Control_MoveSword();
        NullifyProgress();
    }

    public void ApplyNewDesire(Vector3 pos, Vector3 up, Vector3 forward) 
    {
        if (_swinging)
            return;

        SetDesires(pos, up, forward);
        Control_MoveSword();
        NullifyProgress();
    }

    private void SetDesires(Vector3 pos, Vector3 up, Vector3 forward)
    {
        _desireBlade.position = pos;
        _desireBlade.LookAt(pos + forward, up);

        if (isSwordFixing)
            Control_FixDesire();
    }

    private void NullifyProgress()
    {
        if (_moveFrom != null)
            Destroy(_moveFrom.gameObject);
        GameObject moveFromGO = new("BladeIsMovingFromThatTransform");
        _moveFrom = moveFromGO.transform;
        _moveFrom.position = bladeHandle.position;
        _moveFrom.rotation = bladeHandle.rotation;
        _moveFrom.parent = bladeContainer;
        _moveProgress = 0;
    }

    private void OnDrawGizmosSelected()
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