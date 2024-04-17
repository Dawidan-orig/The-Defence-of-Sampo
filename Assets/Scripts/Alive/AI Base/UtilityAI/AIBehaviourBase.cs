using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI
{
    [RequireComponent(typeof(TargetingUtilityAI))]
    public abstract class AIBehaviourBase : MonoBehaviour, IPointsDistribution, IAnimationProvider
    {
        [SerializeField]
        [Tooltip(@"��� ���� ������� ���������, �� ������� �� ����������, ��������� ���� ��������� ������.
            ����� ������ ���������� ������������.
            ���������� ��������������� ��� ����������� �������������, �� ����� �������� � ���� ����.")]
        protected int visiblePowerPoints = 100;

        [Header("Ground for animation and movement")]
        public Collider vital;
        [Tooltip("������ �� 0 �� 1, ������������ ��," +
            "� ����� �������������� � ����������� �� ��������� ������ �� ����� ���������� �� ����")]
        public AnimationCurve retreatInfluence;
        public float toGroundDist = 0.3f;
        [Tooltip("����� ������� ��� NavMeshAgent")]
        private Transform navMeshCalcFrom;

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
                navMeshCalcFrom ??= transform;
                return navMeshCalcFrom;
            }
        }
        #endregion

        protected virtual void Awake()
        {
            _AITargeting = GetComponent<TargetingUtilityAI>();
            _body = GetComponent<Rigidbody>();
            navMeshCalcFrom ??= transform;
        }

        /// <summary>
        /// ����������� ������� ����������, ��������� ��������� ����� ��� ������� ��������
        /// </summary>
        /// <param name="ofTarget">�������� ����, ��� ������� ����� ����� ��������� �����</param>
        /// <param name="CalculateFrom">�� ���� ������� ��������� ��������� �������</param>
        /// <returns>��������� �����</returns>
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
        /// <summary>
        /// ������������ ����� � ��������� ����������
        /// </summary>
        /// <param name="points">������������� ����</param>
        public virtual void AssignPoints(int points)
        {
            int remaining = points;
            visiblePowerPoints = points;

            //TODO DESIGN : ����������� ��������� �������� ��������
        }
        /// <summary>
        /// �������� ������� �� ����
        /// </summary>
        /// <param name="target">������������ ���� ����</param>
        /// <returns>true, ���� ���� ��������</returns>
        public virtual bool IsTargetPassing(Transform target)
        {
            bool res = true;

            Faction other = target.GetComponent<Faction>();

            if (!other.IsWillingToAttack(GetComponent<Faction>().FactionType) || target == transform)
                res = false;

            if (other.TryGetComponent(out AliveBeing b))
                if (b.mainBody == transform)
                    res = false;

            return res;
        }
        /// <summary>
        /// ������������� ������� ���� ��������� �������� �������������� �� ���������
        /// </summary>
        public virtual Dictionary<Interactable_UtilityAI, int> GetActionsDictionary()
        {
            return UtilityAI_Manager.Instance.GetAllInteractions(GetComponent<Faction>());
        }
        /// <summary>
        /// ����� ������ ������ �� ���������� �������
        /// </summary>
        /// <param name="target">������������ ���� ����</param>
        /// <returns>��������� ������</returns>
        public abstract Tool ToolChosingCheck(Transform target);
        /// <summary>
        /// ���������� ��������� �� ���� ��� �� ��� ����� ��������
        /// </summary>
        /// <param name="target">����</param>
        /// <returns>���������, ������� ����� ��������� � ����</returns>
        public virtual UtilityAI_BaseState TargetReaction(Transform target)
        {
            return _AITargeting.GetAttackState();
        }
        /// <summary>
        /// ���������� �����, ���� ������� ���������
        /// </summary>
        /// <returns>����� ������������ ��, ����� ������� ��������� ���� ����������</returns>
        public abstract Vector3 RelativeRetreatMovement();
        /*TODO dep AI_Factory : ������� ���, ����� ����������� StateMachine ���� ������������� ����, ���� ������ ������������...
        * ��� ������� �� ������ ���� ��������, ��� - ������ ��� ����������� StateMachine!
        * ������� ����� Event'� ����� ��������� factory.
        */
        /// <summary>
        /// ������� Update, �� ���������� � ���������, ����� ���� �������.
        /// </summary>
        /// <param name="target"></param>
        public abstract void AttackUpdate(Transform target);

        /// <summary>
        /// ������� Update, �� ����� ���� ���������
        /// </summary>
        /// <param name="target"></param>
        public abstract void ActionUpdate(Transform target);
        #endregion

        #region animation
        public Vector3 GetLookTarget()
        {
            return (CurrentActivity.target ? CurrentActivity.target.position : Vector3.zero);
        }

        public bool IsGrounded()
        {
            const float PLANE_CAST_MINIMUM = 0.1f;

            return Physics.BoxCast(vital.bounds.center,
                new Vector3(vital.bounds.size.x / 2,
                PLANE_CAST_MINIMUM,
                vital.bounds.size.z / 2),
                transform.up * -1,
                out _,
                transform.rotation,
                vital.bounds.size.y / 2 + toGroundDist);
        }

        public bool IsInJump()
        {
            //TODO DESIGN : �� ������� �� ������ � ������. ���, ������-��, ���� �� ���������.
            return false;
        }
        /// <summary>
        /// ����� ��� �������� ����
        /// </summary>
        /// <returns>�����, ���� ����� ���������� ����</returns>
        public virtual Transform GetRightHandTarget() { return null; }
        #endregion
    }
}