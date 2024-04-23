using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI
{
    public abstract class AIBehaviourBase : MonoBehaviour, IPointsDistribution, IAnimationProvider
    {
        [SerializeField]
        [Tooltip(@"Это очки внешней опасности, по котором ИИ определяет, насколько этот противник опасен.
            Может менять визуальную составляющую.
            Изначально устанавливается при процедурной инициализации, но может меняться в ходе игры.")]
        protected int visiblePowerPoints = 100;

        [Header("Ground for animation and movement")]
        [Tooltip("Кривая от 0 до 1, определяющая то," +
            "с какой интенсивностью в зависимости от дальности оружия ИИ будет отдоходить от цели")]
        public AnimationCurve retreatInfluence;
        public float toGroundDist = 0.3f;
        [Header("Targeting AI")]
        public float distanceInfluence = 1;
        public float congestionInfluence = 1;

        private Transform _navMeshCalcFrom;
        private Collider _vital;
        private Rigidbody _body;
        protected TargetingUtilityAI _AITargeting;

        #region properties
        public int VisiblePowerPoints { get => visiblePowerPoints; set => visiblePowerPoints = value; }
        public Rigidbody Body { get => _body; set => _body = value; }
        public TargetingUtilityAI.AIAction CurrentActivity
        {
            get { return _AITargeting.CurrentActivity; }
        }
        public Transform NavMeshCalcFrom
        {
            get
            {
                _navMeshCalcFrom ??= transform;
                return _navMeshCalcFrom;
            }
        }
        public Collider Vital 
        {
            get 
            {
                if (!_vital) {
                    var colliders = GetMainTransform().GetComponents<Collider>();
                    if (colliders.Length != 1)
                        Debug.LogWarning("Много коллайдеров за раз на одном объекте, беру в Vital самый первый",transform);
                    _vital = colliders[0];
                }
                return _vital;
            }
        }
        public abstract Tool BehaviourWeapon { get; }
        #endregion

        protected virtual void Awake()
        {            
            Transform absoluteParent = GetMainTransform();

            _body = absoluteParent.gameObject.GetComponent<Rigidbody>();
            _navMeshCalcFrom ??= transform;
        }

        /// <summary>
        /// Независимая команда вычисления, находящая ближайшую точку для больших объектов
        /// </summary>
        /// <param name="ofTarget">Огромная цель, для которой нужно найти ближайшую точку</param>
        /// <param name="CalculateFrom">От этой позиции находится ближайшая позиция</param>
        /// <returns>Ближайшая точка</returns>
        public Vector3 GetClosestPoint(Transform ofTarget, Vector3 CalculateFrom)
        {
            Vector3 closestPointToTarget;
            if (ofTarget.TryGetComponent<IDamagable>(out var ab))
                closestPointToTarget = ab.Vital.ClosestPointOnBounds(CalculateFrom);
            else if (ofTarget.TryGetComponent<Collider>(out var c))
                closestPointToTarget = c.ClosestPointOnBounds(CalculateFrom);
            else
                closestPointToTarget = ofTarget.position;

            //closestPointToTarget = ofTarget.position;

            return closestPointToTarget;
        }

        #region virtual functions
        protected Transform GetMainTransform() 
        {
            if(!_AITargeting)
                _AITargeting = GetComponent<TargetingUtilityAI>();
            if (!_AITargeting)
                _AITargeting = GetComponentInParent<TargetingUtilityAI>();

            return _AITargeting.transform;
        }
        /// <summary>
        /// Присваивание очков и изменение параметров
        /// </summary>
        /// <param name="points">Присваиваемые очки</param>
        public virtual void AssignPoints(int points)
        {
            int remaining = points;
            visiblePowerPoints = points;

            //TODO DESIGN : Гармоничное изменение скорости движения
        }
        /// <summary>
        /// Проверка что цель следует превращать в действие
        /// </summary>
        /// <param name="target">Проверка относительно этой цели</param>
        /// <returns>true, если цель подходит</returns>
        public virtual bool IsTargetPassing(Transform target)
        {
            bool res = true;

            Faction other = target.GetComponent<Faction>();

            if (!other.IsWillingToAttack(GetMainTransform().GetComponent<Faction>().FactionType) || target == transform)
                res = false;

            if (other.TryGetComponent(out AliveBeing b))
                if (b.mainBody == transform)
                    res = false;

            return res;
        }
        /// <summary>
        /// Предоставляет словарь всех доступных объектов взаимодействия из менеджера
        /// </summary>
        public virtual Dictionary<Interactable_UtilityAI, int> GetActionsDictionary()
        {
            return UtilityAI_Manager.Instance.GetAllInteractions(GetMainTransform().GetComponent<Faction>());
        }
        /// <summary>
        /// Определяет точку, куда следует отступать
        /// </summary>
        /// <returns>Точка относительно ИИ, длина вектора указывает силу отсупления</returns>
        public abstract Vector3 RelativeRetreatMovement();
        /// <summary>
        /// Обычный Update, но когда юнит действует
        /// </summary>
        /// <param name="target"></param>
        //TODO : Вместе с редуцированием системы состояний эта функция должна уйти.
        public abstract void ActionUpdate(Transform target);
        /// <summary>
        /// Определяет количество очков на данный момент времени для текущей формы поведения.
        /// </summary>
        /// <returns>Количество очков для определения приоритетности поведения</returns>
        public abstract int GetCurrentWeaponPoints();
        #endregion

        #region animation
        public Vector3 GetLookTarget()
        {
            return (CurrentActivity.target ? CurrentActivity.target.position : Vector3.zero);
        }

        public bool IsGrounded()
        {
            const float PLANE_CAST_MINIMUM = 0.1f;

            return Physics.BoxCast(Vital.bounds.center,
                new Vector3(Vital.bounds.size.x / 2,
                PLANE_CAST_MINIMUM,
                Vital.bounds.size.z / 2),
                transform.up * -1,
                out _,
                transform.rotation,
                Vital.bounds.size.y / 2 + toGroundDist);
        }

        public bool IsInJump()
        {
            //TODO DESIGN : ИИ Никогда не бывают в прыжке. Что, вообще-то, надо бы исправить.
            return false;
        }
        /// <summary>
        /// Нужно для анимации тела,
        /// </summary>
        /// <returns>Точка, куда будет направлена рука</returns>
        public virtual Transform GetRightHandTarget() { return null; }
        #endregion
    }
}