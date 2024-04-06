using Sampo.AI;
using Sampo.Melee.Sword;
using System.Collections.Generic;
using UnityEngine;
using static Sampo.Melee.Sword.SwordFighter_StateMachine;

namespace Sampo.Melee
{
    public abstract class MeleeFighter : TargetingUtilityAI
    {
        #region parameters
        //TODO DESIGN : ������� �������� ����������� ������ � ���������� � ����������, ����� �� ���� Generic ���������.
        [Header("===MeleeFighter===\nSetup")]
        [Tooltip("�������� ������ ����� ��")]
        public MeleeTool weapon;
        [Tooltip("������, ������� ������ ���� � ����� ��, �������� ������.")]
        public MeleeTool defaultWeapon;
        [Tooltip("����� ����������, ��� ������ ������")]
        public float baseReachDistance = 1; //TODO DESIGN (� ������ �������� ����� �������) : ������� �� distanceFrom �� ������ ������.
        [SerializeField]
        [Tooltip("���������� ������ ����������, ������ � ���������� ������.")]
        protected Transform distanceFrom;
        [SerializeField]
        [Tooltip("���������� �������� ������ �������")]
        protected AnimationCurve swingMotion;
        [SerializeField]
        [Tooltip("���������� ����������� ������")]
        protected AnimationCurve repositionMotion;

        [Header("parameters")]
        [Tooltip("������� �������� �������� ������ � ����")]
        public float actionSpeed = 1;
        [Tooltip("������� �������� �������� ������ �������")]
        public float swingSpeed = 1;
        [Tooltip("��������� ������ ������ ��������� ��� ��� ���������.")]
        public float swing_EndDistanceMultiplyer = 1.5f;
        [Tooltip("��������� ������ ������ ������������ ��� ����� ��� �����")]
        public float swing_startDistance = 1.5f;
        [Tooltip("����� ����������, ��� ������ ������ � ��������� ������ �����")]
        public float criticalImpulse = 400;
        [Tooltip("��, ��� ����� �������� ���� ����� �������� - ������ �������������")]
        public float blockCriticalVelocity = 5;
        [Tooltip("����������� ���������� ���������� �� vital")]
        public float toBladeHandle_MinDistance = 0.1f;
        [Tooltip("���������� �� ����, ��� ������� ����� ������ ���������")]
        public float close_enough = 0.1f;
        [Tooltip("����������� ����, ����� ������� ��� handle ������ � desire")]
        public float angle_enough = 10;
        [Tooltip("��������� �������� �� 0 �� 1 ���������� ����������� ������ ����� �����(0)-������(0.5)-������(1)")]
        public AnimationCurve attackProbability;

        [Header("timers")]
        [Tooltip("������� ������� ������� �� ��������� ���� � ������� �������?")]
        public float toInitialAwait = 2;

        [Header("setup")]
        [SerializeField]
        protected Blade _blade;
        [SerializeField]
        protected Transform _bladeContainer;
        [SerializeField]
        protected Transform _bladeHandle;
        [SerializeField]
        protected Collider _vital;

        [Header("lookonly")]
        [SerializeField]
        protected AttackCatcher _catcher;
        [SerializeField]
        protected Transform _initialBlade;
        [SerializeField]
        protected Transform _moveFrom;
        [SerializeField]
        protected Transform _desireBlade;
        [SerializeField]
        protected float _moveProgress;
        [SerializeField]
        protected float _AnimatedMoveProgress;
        [SerializeField]
        protected float _currentToInitialAwait;
        [SerializeField]
        protected Stack<ActionJoint> _currentCombo = new Stack<ActionJoint>();
        [SerializeField]
        protected bool _swingReady = true;
        #endregion

        public AttackCatcher AttackCatcher { get => _catcher; set => _catcher = value; }
        public bool SwingReady { get => _swingReady; set => _swingReady = value; }

        #region properties for state machine
        public Transform BladeHandle { get { return _bladeHandle; } }
        public Transform DesireBlade { get { return _desireBlade; } }
        public Transform MoveFrom { get { return _moveFrom; } }
        public float MoveProgress { get { return _AnimatedMoveProgress; } }
        public Blade Blade { get { return _blade; } }
        public Transform InitialBlade { get => _initialBlade; set => _initialBlade = value; }
        public float CurrentToInitialAwait { get => _currentToInitialAwait; set => _currentToInitialAwait = value; }
        public Collider Vital { get => _vital; set => _vital = value; }
        public Stack<ActionJoint> CurrentCombo { get => _currentCombo; set => _currentCombo = value; }

        #endregion

        public enum ActionType
        {
            Swing,
            Reposition
        }

        [System.Serializable]
        public struct ActionJoint
        {
            public Vector3 relativeDesireFrom;
            public Quaternion rotationFrom;
            public Vector3 nextRelativeDesire;
            public Quaternion nextRotation;
            public ActionType currentActionType;
        }

        protected override void Start()
        {
            base.Start();

            distanceFrom = distanceFrom ? distanceFrom : transform;
            _catcher = gameObject.GetComponent<AttackCatcher>();

            if (weapon == null)
            {
                weapon = defaultWeapon;
            }
        }
        public override void AttackUpdate(Transform target)
        {

        }

        protected override Tool ToolChosingCheck(Transform target)
        {
            return weapon;
        }

        public override Transform GetRightHandTarget()
        {
            return weapon.rightHandHandle;
        }

        public virtual void Swing(Vector3 toPoint)
        {
            _swingReady = false;

            Invoke(nameof(BecomeReadyToSwing), weapon.cooldownBetweenAttacks);
        }

        public bool MeleeReachable(out Vector3 closestPointToTarget)
        {
            Vector3 calculateFrom = distanceFrom.position;

            closestPointToTarget = GetClosestPoint(_currentActivity.target, calculateFrom);

            return Vector3.Distance(calculateFrom, closestPointToTarget) < _currentActivity.actWith.additionalMeleeReach + baseReachDistance;
        }

        public void BecomeReadyToSwing()
        {
            _swingReady = true;
        }

        public abstract void Block(Vector3 start, Vector3 end, Vector3 SlashingDir);
    }
}