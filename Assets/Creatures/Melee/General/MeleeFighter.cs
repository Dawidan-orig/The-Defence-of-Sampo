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
        //TODO DESIGN : Сделать обратную зависимость оружия к управлению в состояниях, чтобы всё было Generic полностью.
        [Header("===MeleeFighter===\nSetup")]
        [Tooltip("Основное оружие этого ИИ")]
        public MeleeTool weapon;
        [Tooltip("Оружие, которое всегда есть у этого ИИ, например кулаки.")]
        public MeleeTool defaultWeapon;
        [Tooltip("Длина конечности, что держит оружие")]
        public float baseReachDistance = 1; //TODO? (С учётом двуручки вроде посохов) : Считать от distanceFrom до самого оружия.
        [SerializeField]
        [Tooltip("Определяет начало конечности, откуда и происходит отсчёт.")]
        protected Transform distanceFrom;
        [SerializeField]
        [Tooltip("Определяет движение взмаха оружием")]
        protected AnimationCurve swingMotion;
        [SerializeField]
        [Tooltip("Определяет перемещения оружия")]
        protected AnimationCurve repositionMotion;

        [Header("parameters")]
        [Tooltip("Базовая скорость движения оружия в руке")]
        public float actionSpeed = 1;
        [Tooltip("Базовая скорость ударного взмаха оружием")]
        public float swingSpeed = 1;
        [Tooltip("Насколько далеко должен двинуться меч для отбивания.")]
        public float swing_EndDistanceMultiplyer = 1.5f;
        [Tooltip("Насколько далеко должен отодвинуться меч назад при ударе")]
        public float swing_startDistance = 1.5f;
        [Tooltip("Лучше увернуться, чем отбить объект с импульсом больше этого")]
        public float criticalImpulse = 400;
        [Tooltip("Всё, что имеет скорость выше этого значения - должно блокироваться")]
        public float blockCriticalVelocity = 5;
        [Tooltip("Максимальное расстояние от vital до рукояти меча. По сути, длина руки.")]
        public float toBladeHandle_MaxDistance = 1.2f;
        [Tooltip("Минимальное допустимое расстояние от vital")]
        public float toBladeHandle_MinDistance = 0.1f;
        [Tooltip("Расстояние до цели, при котором можно менять состояние")]
        public float close_enough = 0.1f;
        [Tooltip("Достаточный угол, чтобы считать что handle близок к desire")]
        public float angle_enough = 10;
        [Tooltip("Указывает значения от 0 до 1 означающие вероятность выбора удара слева(0)-сверху(0.5)-справа(1)")]
        public AnimationCurve attackProbability;

        [Header("timers")]
        [Tooltip("Сколько времени ожидать до установки меча в обычную позицию?")]
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

        protected override void Awake()
        {
            base.Awake();
            distanceFrom = distanceFrom ? distanceFrom : transform;
            _catcher = gameObject.GetComponent<AttackCatcher>();
        }

        protected override void Start()
        {
            base.Start();

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

        public bool MeleeReachable()
        {
            Vector3 closestTo;
            Vector3 calculateFrom = distanceFrom.position;
            if (_currentActivity.target.TryGetComponent<AliveBeing>(out var ab))
                closestTo = ab.vital.ClosestPointOnBounds(calculateFrom);
            else if (_currentActivity.target.TryGetComponent<Collider>(out var c))
                closestTo = c.ClosestPointOnBounds(calculateFrom);
            else
                closestTo = _currentActivity.target.position;

            return Vector3.Distance(calculateFrom, closestTo) < _currentActivity.actWith.additionalMeleeReach + baseReachDistance;
        }

        public void BecomeReadyToSwing()
        {
            _swingReady = true;
        }

        public abstract void Block(Vector3 start, Vector3 end, Vector3 SlashingDir);
    }
}