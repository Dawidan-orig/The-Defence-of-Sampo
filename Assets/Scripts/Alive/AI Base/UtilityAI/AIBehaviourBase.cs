using Sampo.AI.Conditions;
using Sampo.Core;
using Sampo.Weaponry;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Sampo.AI.TargetingUtilityAI;

namespace Sampo.AI
{
    public abstract class AIBehaviourBase : MonoBehaviour, IPointsDistribution, IAnimationProvider
    {
        #region variables
        [SerializeField]
        [Tooltip(@"��� ���� ������� ���������, �� ������� �� ����������, ��������� ���� ��������� ������.
            ����� ������ ���������� ������������.
            ���������� ��������������� ��� ����������� �������������, �� ����� �������� � ���� ����.")]
        protected int visiblePowerPoints = 100;

        [Header("Ground for animation and movement")]
        [Tooltip("������ �� 0 �� 1, ������������ ��," +
            "� ����� �������������� � ����������� �� ��������� ������ �� ����� ���������� �� ����")]
        public AnimationCurve retreatInfluence;
        public float toGroundDist = 0.3f;
        [Header("Targeting AI")]
        public float distanceInfluence = 1;
        public float congestionInfluence = 1;
        [SerializeField]
        private bool _hasCongestion = true;

        private Transform _navMeshCalcFrom;
        private Collider _vital;
        private Rigidbody _body;
        protected TargetingUtilityAI _AITargeting;

        #region path and movement
        protected NavMeshPath path;
        protected Vector3 moveTargetPos { get; private set; }
        private Vector3 repathLastTargetPos;
        private const float RECALC_DIFF = 3;
        #endregion

        #endregion

        #region properties
        public int VisiblePowerPoints { get => visiblePowerPoints; set => visiblePowerPoints = value; }
        public Rigidbody Body { get => _body; set => _body = value; }
        public AIAction CurrentActivity
        {
            get { return _AITargeting.CurrentActivity; }
        }
        public Transform CalcFrom
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
                if (!_vital)
                {
                    var colliders = GetMainTransform().GetComponents<Collider>();
                    if (colliders.Length != 1)
                        Debug.LogWarning("����� ����������� �� ��� �� ����� �������, ���� � Vital ����� ������", transform);
                    _vital = colliders[0];
                }
                return _vital;
            }
        }
        public bool HasCongestion
        {
            get => _hasCongestion;
            set
            {
                bool prev = value;
                _hasCongestion = prev;
                if (prev == true && value == false)
                    UtilityAI_Manager.Instance.ChangeCongestion(
                    CurrentActivity.target.GetComponent<Interactable_UtilityAI>(),
                    -VisiblePowerPoints);

                if (prev == false && value == true)
                    UtilityAI_Manager.Instance.ChangeCongestion(
                    CurrentActivity.target.GetComponent<Interactable_UtilityAI>(),
                    VisiblePowerPoints);
            }
        }
        public abstract Tool BehaviourWeapon { get; }
        #endregion

        #region unity
        protected virtual void Awake()
        {
            Transform absoluteParent = GetMainTransform();

            _body = absoluteParent.gameObject.GetComponent<Rigidbody>();
            _navMeshCalcFrom ??= transform;
        }
        protected virtual void Start()
        {
            _AITargeting.AddNewActionsFromBehaviour(this);
        }
        protected virtual void Update()
        {
            if (CurrentActivity.target == null
                || path?.status == NavMeshPathStatus.PathInvalid)
            {
                //TODO? : ����� ��� ��� ������� �������-����� ��� ������ ������ �� 5,
                // ���� �� ������ ������� ����������.
                if (!_AITargeting.SelectBestActivityIfAny())
                    return;
            }

            CheckRepath();

            if (CurrentActivity.behaviour.BehaviourWeapon == null
                && CurrentActivity.target.TryGetComponent(out IInteractable interact))
            {
                if (Vector3.Distance(GetMainTransform().position, CurrentActivity.target.position)
                    < interact.GetInteractionRange())
                {
                    interact.Interact(GetMainTransform());
                    _AITargeting.CompleteCurrentAction();
                    return;
                }
            }

            if (AvailableToAct())
                RetreatReposition();
            else
                MoveAlongPath(10);

            //TODO DESIGN : �����������. ��� ����� ��������� ���������, �� �� AIBehaviour.
            // ��� ��� ����������, ����� � ��� ����������� � ��� �� �����.
            // ��� ��������� Action'� � TargetingUtilityAI.

            // ��� ������ Action �������� � ���� AIBehaviour.
            // ������ ��� ����������� ������ AIBehaviour ��� ������������.
            // ������ ��� ����� ������ ������������ ������ �������������� ������ ���� Kit � AIBehaviour.
            // � �� ����� ������ ���� ����� ������������.
            // MultiweaponUnit ��� ������� �� ������ �� ���������� �����,
            // ������� ��������� ����������� ��� ������ �� ������������, � GetCurrentWeaponPoints() ������������ ��������
            // ��� ������ ����� ����������� ��� ActionUpdate - ����� ������������ ����������� �� ����� ���������� �� ����.
            // ����������� ����� ���� ���������� �������, �� ������� �� ����������,
            // � ������ ���� ����� ��� ������ ����� �������� �������� ����������.
        }
        #endregion

        /// <summary>
        /// ������� ��������� ����� ��� ������� ��������
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

        #region movement control
        /// <summary>
        /// ���������, ���� �� ����� ������������� ���� � ����
        /// </summary>
        private void CheckRepath()
        {
            // ��� �� ������ - ��������� ���� ����, ����� ����������� ���� ������
            if (Vector3.Distance(repathLastTargetPos, _AITargeting.CurrentActivity.target.position) < GetTotalRange() * 2
                && !Utilities.ValueInArea(repathLastTargetPos, _AITargeting.CurrentActivity.target.position, RECALC_DIFF / 7))
                Repath();

            if (!Utilities.ValueInArea(repathLastTargetPos, _AITargeting.CurrentActivity.target.position, RECALC_DIFF))
                Repath();
        }
        /// <summary>
        /// ������� ��������� �������� ������
        /// </summary>
        /// <returns>��������� ������</returns>
        private float GetTotalRange()
        {
            if (_AITargeting.CurrentActivity.behaviour.BehaviourWeapon == null
                && _AITargeting.CurrentActivity.target.TryGetComponent(out IInteractable interact))
            {
                return interact.GetInteractionRange();
            }
            else if (_AITargeting.CurrentActivity.behaviour.BehaviourWeapon != null)
            {
                return _AITargeting.CurrentActivity.behaviour.BehaviourWeapon.GetRange();
            }
            return 1;
        }
        /// <summary>
        /// ������������� ���� � ����
        /// </summary>
        private void Repath()
        {
            Vector3 closestPos = GetClosestPoint(CurrentActivity.target, CalcFrom.position);
            moveTargetPos = closestPos;

            if (CurrentActivity.behaviour.BehaviourWeapon
                is Sampo.Weaponry.Ranged.BaseShooting shooting) // ���� ������ ������� ��� ��������
            {
                moveTargetPos = shooting.NavMeshClosestAviableToShoot(_AITargeting.CurrentActivity.target);
            }

            const float SEARCH_RADIUS = 100;

            if (NavMesh.SamplePosition(moveTargetPos, out var hit, SEARCH_RADIUS, NavMesh.AllAreas))
                moveTargetPos = hit.position;

            repathLastTargetPos = moveTargetPos;

            path = new NavMeshPath();
            NavMesh.CalculatePath(CalcFrom.position, moveTargetPos, NavMesh.AllAreas, path);
            _AITargeting.MovingAgent.PassPath(path);

            /*
            Vector3 forwardLook = (_ctx.CurrentActivity.target.position - _ctx.transform.position).normalized;
            forwardLook.y = 0;
            Quaternion resRot = Quaternion.LookRotation(forwardLook, Vector3.up);
            _ctx.transform.rotation = Quaternion.RotateTowards(_ctx.transform.rotation, resRot, 20);
            */

            MoveAlongPath(10);
        }
        /// <summary>
        /// ��� ������� ������� � ��������
        /// </summary>
        /// <param name="lookDist">���������, ��� ������� ��� ����� �������� �� ����</param>
        private void MoveAlongPath(float lookDist)
        {
            if (path == null)
                return;

            if (path.status != NavMeshPathStatus.PathInvalid && path.corners.Length > 1)
            {
                if (Vector3.Distance(GetMainTransform().position, _AITargeting.CurrentActivity.target.position) > lookDist)
                    _AITargeting.MovingAgent.MoveIteration(path.corners[1]);
                else
                    _AITargeting.MovingAgent.MoveIteration(path.corners[1], _AITargeting.CurrentActivity.target.position);
            }
            else
            {
                _AITargeting.ModifyAllActionsOf(_AITargeting.CurrentActivity.target, new NoPathCondition(10));

                var closest = NavMeshCalculations.Instance.GetCell(CalcFrom.position);
                moveTargetPos = closest.Center();
                _AITargeting.MovingAgent.MoveIteration(moveTargetPos);
            }
        }
        private void RetreatReposition()
        {
            //Vector3 newPos = _ctx.transform.position - _ctx.BehaviourAI.retreatInfluence.Evaluate(retreatCurveTime)
            //        * (_ctx.CurrentActivity.target.position - _ctx.transform.position);

            Vector3 newPos = RelativeRetreatMovement();

            if (_AITargeting.MovingAgent.IsNearObstacle(newPos - GetMainTransform().position, out Vector3 normal))
            {
                Vector3 dir = Vector3.ProjectOnPlane(
                    (CurrentActivity.target.position - GetMainTransform().position).normalized,
                    normal);
                dir.Normalize();

                newPos = GetMainTransform().position - dir
                    * (CurrentActivity.target.position - GetMainTransform().position).magnitude;
            }

            _AITargeting.MovingAgent.MoveIteration(newPos, CurrentActivity.target.position);
        }
        #endregion

        #region virtual functions
        public Transform GetMainTransform()
        {
            if (!_AITargeting)
                _AITargeting = GetComponent<TargetingUtilityAI>();
            if (!_AITargeting)
                _AITargeting = GetComponentInParent<TargetingUtilityAI>();

            return _AITargeting.transform;
        }
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
        /// �������� ��� ���� ������� ���������� � ��������
        /// </summary>
        /// <param name="target">�������� ������������ ���� ����</param>
        /// <returns>true, ���� ���� ��������</returns>
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
        /// ������������� ������� ���� ��������� �������� �������������� �� ���������
        /// </summary>
        public virtual Dictionary<Interactable_UtilityAI, int> GetActionsDictionary()
        {
            return UtilityAI_Manager.Instance.GetAllInteractions(GetMainTransform().GetComponent<Faction>());
        }
        /// <summary>
        /// ���������� �����, ���� ������� ���������
        /// </summary>
        /// <returns>����� ������������ ��, ����� ������� ��������� ���� ����������</returns>
        public abstract Vector3 RelativeRetreatMovement();
        /// <summary>
        /// �������� �� ������� ���� ��� �����/��������������?
        /// </summary>
        public virtual bool AvailableToAct() 
        {
            return Vector3.Distance(GetMainTransform().position,
                CurrentActivity.target.position)
                < GetTotalRange();
        }
        /// <summary>
        /// ���������� ���������� ����� �� ������ ������ ������� ��� ������� ����� ���������.
        /// </summary>
        /// <returns>���������� ����� ��� ����������� �������������� ���������</returns>
        public abstract int GetCurrentWeaponPoints();
        #endregion

        #region animation
        public Vector3 GetLookTarget()
        {
            if(CurrentActivity.target == null)
                return Vector3.zero;

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
            //TODO DESIGN : �� ������� �� ������ � ������. ���, ������-��, ���� �� ���������.
            return false;
        }
        /// <summary>
        /// ����� ��� �������� ����,
        /// </summary>
        /// <returns>�����, ���� ����� ���������� ����</returns>
        public virtual Transform GetRightHandTarget() { return null; }
        #endregion
    }
}