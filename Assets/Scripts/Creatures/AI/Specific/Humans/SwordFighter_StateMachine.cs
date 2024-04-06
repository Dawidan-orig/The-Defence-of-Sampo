using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sampo.Melee.Sword
{
    [RequireComponent(typeof(AttackCatcher))]
    public class SwordFighter_StateMachine : MeleeFighter
    {
        public SwordFighter_BaseState CurrentSwordState { get { return _currentSwordState; } set { _currentSwordState = value; } }

        SwordFighter_BaseState _currentSwordState;
        SwordFighter_StateFactory _fighter_states;

        public System.EventHandler<IncomingReposEventArgs> OnRepositionIncoming;
        public System.EventHandler<IncomingSwingEventArgs> OnSwingIncoming;

        public class IncomingReposEventArgs : System.EventArgs
        {
            public Vector3 bladeDown;
            public Vector3 bladeUp;
            public Vector3 bladeDir;
        }
        public class IncomingSwingEventArgs : System.EventArgs
        {
            public Vector3 toPoint;
        }

        [Header("Debug")]
        [SerializeField]
        private bool isSwordFixing = true;
        [SerializeField]
        [Tooltip("����� ��� ������ �������� ��������� � Unity inspector")]
        private string currentState;

        #region Unity

        protected override void Start()
        {
            base.Start();

            _catcher.AddIgnoredObject(_blade.body);

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

            if (_moveFrom == null)
            {
                GameObject moveFromGo = new("BladeMoveStart");
                _moveFrom = moveFromGo.transform;
            }

            SetInitialDesires();
            InitiateNewBladeMove();
            _moveProgress = 1;
            _AnimatedMoveProgress = 1;

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
            //TODO DESIGN : �������� ��������� �������� ������ ��� �� ����� Swing, ����� ������������ �������� ��� �����, ��� � ������ �����.
            //TODO DESIGN : ���������, ��� ����� �������� StateMachine � �������� � ��� ������ �������� ��������, ����� ���, � �������� ����������� ������ ���������� ����� BezierCurve
            base.FixedUpdate();
            _currentSwordState.FixedUpdateState();

            if (_moveProgress < 1)
            {
                if (_currentSwordState is SwordFighter_RepositioningState)
                {
                    _moveProgress += actionSpeed * Time.fixedDeltaTime / Vector3.Distance(_moveFrom.position, _desireBlade.position);
                    _moveProgress = Mathf.Clamp01(_moveProgress);
                    _AnimatedMoveProgress = repositionMotion.Evaluate(_moveProgress);
                }
                else if (_currentSwordState is SwordFighter_SwingingState)
                {
                    _moveProgress += swingSpeed * Time.fixedDeltaTime / Vector3.Distance(_moveFrom.position, _desireBlade.position);
                    _moveProgress = Mathf.Clamp01(_moveProgress);
                    _AnimatedMoveProgress = swingMotion.Evaluate(_moveProgress);
                }
            }
        }

        protected void OnDrawGizmosSelected()
        {
            if (_desireBlade != null)
            {
                Gizmos.color = _currentSwordState is SwordFighter_SwingingState ? Color.red : Color.black;
                Gizmos.DrawLine(_desireBlade.position, _moveFrom.position);
                Gizmos.color = Color.gray;
                Gizmos.DrawRay(_desireBlade.position, _desireBlade.up);
                Gizmos.DrawRay(_moveFrom.position, _moveFrom.up);
            }
        }
        #endregion

        #region event control and distribution
        private void Incoming(object sender, AttackCatcher.AttackEventArgs e)
        {
            Rigidbody currentIncoming = e.body;
            CurrentToInitialAwait = 0;

            // �������� ������ � ������ ����� �������� ��� ������ - �����
            if (e.free && e.body.velocity.magnitude < blockCriticalVelocity)
            {
                //IDEA: ������� ������� �����: � ������� BezierCurve.

                Vector3 toPoint = e.start;

                Vector3 bladeCenter = Vector3.Lerp(Blade.upperPoint.position, Blade.downerPoint.position, 0.5f);
                float bladeCenterLen = Vector3.Distance(bladeCenter, Blade.downerPoint.position);
                float swingDistance = bladeCenterLen + baseReachDistance;

                if (Vector3.Distance(distanceFrom.position, toPoint) < swingDistance)
                {
                    OnSwingIncoming?.Invoke(this, new IncomingSwingEventArgs { toPoint = toPoint });
                }
            }
            // �� �� �����, �� ��� ��������� - ����� ������ �������
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

                int ignored = Blade.gameObject.layer; // ��� ������ ������ ��� ��������.
                ignored = ~ignored;

                BoxCollider bladeCollider = Blade.GetComponent<BoxCollider>();
                Vector3 bladeHalfWidthLength = new Vector3((bladeCollider.size.x * bladeCollider.transform.lossyScale.x) / 2, 0.1f, (bladeCollider.size.z * bladeCollider.transform.lossyScale.z) / 2);

                //IDEA : ����������, ������� ������� �����.
                // ������ ����� ����� ������������ ������������ ��-�� ��������. ���� �������������� �������: ��������� ��� �������� ��������� ����� �� ���� �� ���������� �����,
                // ��� ��� ��������� ��� ��� - � ��� ����� ������������.

                OnRepositionIncoming?.Invoke(this, new IncomingReposEventArgs { bladeDown = bladeDown, bladeUp = bladeUp, bladeDir = toEnemyBlade_Dir });
                //OnSwingIncoming?.Invoke(this, new IncomingSwingEventArgs {toPoint = bladeUp });
            }
            // ��� ���� �������� ������ ������
            else
            {
                Vector3 blockPoint = Vector3.Lerp(e.start, e.end, 0.5f);

                GameObject bladePrediction = new("NotDeletedPrediction");
                bladePrediction.transform.position = blockPoint;

                GameObject start = new();
                start.transform.position = e.start;
                start.transform.parent = bladePrediction.transform;

                GameObject end = new();
                end.transform.position = e.end;
                end.transform.parent = bladePrediction.transform;

                //IDEA : ��� ������ ������, ��-�� ���� ���������� ������ "����������".
                //bladePrediction.transform.Rotate(e.direction, 90);

                // ������������� ��� �������������� vital
                //bladePrediction.transform.rotation = Quaternion.FromToRotation((end.transform.position - start.transform.position).normalized, transform.up);

                Vector3 toEnemyBlade_Dir = (bladePrediction.transform.position - Vital.bounds.center).normalized;
                bladePrediction.transform.Rotate(toEnemyBlade_Dir, 90); // ������ ���������������


                // ����������� ��� ����������� ������ � ����.
                /*
                if (e.body.GetComponent<Tool>().host != null)
                {
                    bladePrediction.transform.position = distanceFrom.position
                        + (bladePrediction.transform.position - distanceFrom.position).normalized * block_minDistance;
                }*/

                Vector3 bladeDown = start.transform.position;
                Vector3 bladeUp = end.transform.position;
                Destroy(bladePrediction);

                int ignored = Blade.gameObject.layer; // ��� ������ ������ ��� ��������.
                ignored = ~ignored;

                BoxCollider bladeCollider = Blade.GetComponent<BoxCollider>();
                Vector3 bladeHalfWidthLength = new Vector3((bladeCollider.size.x * bladeCollider.transform.lossyScale.x) / 2, 0.1f, (bladeCollider.size.z * bladeCollider.transform.lossyScale.z) / 2);

                Vector3 centerOffset = (Blade.downerPoint.position - Blade.downerPoint.position).normalized *
                    (-Vector3.Distance(BladeHandle.position, Blade.downerPoint.position)); // �������� ��� ������ ��������� �������

                OnRepositionIncoming?.Invoke(this, new IncomingReposEventArgs { bladeDown = centerOffset + bladeDown, bladeUp = centerOffset + bladeUp, bladeDir = toEnemyBlade_Dir });
            }
        }

        // ��������� ���� �� ���� ��������� ����������
        public override void Block(Vector3 start, Vector3 end, Vector3 SlashingDir)
        {
            if (Vector3.Distance(distanceFrom.position, start) > Vector3.Distance(distanceFrom.position, end))
                (end, start) = (start, end);

            SetDesires(start, (end - start).normalized, SlashingDir);
        }

        public override void AttackUpdate(Transform target)
        {
            base.AttackUpdate(target);

            if (!_swingReady || CurrentCombo.Count > 0)
                return;

            if (MeleeReachable(out _))
            {
                //TODO DESIGN : ��� ��� ����� �������� ���������� ���������� �� ���������� �����.

                ActionJoint afterPreparation = new ActionJoint();
                ActionJoint preparation = new ActionJoint();

                // �������� �����-�� ����� ��� �����
                float posX = 0;
                List<Keyframe> sorted = attackProbability.keys.ToList();
                sorted.Sort((item1, item2) => item1.value.CompareTo(item2.value));
                for (int i = 0; i < sorted.Count; i++) 
                {
                    Keyframe key = sorted[i];
                    posX = key.time;
                    float prob = UnityEngine.Random.Range(0, 1);
                    if (prob > key.value)
                    {
                        float offset = UnityEngine.Random.Range(
                            attackProbability.Evaluate(i == 0 ? 0 : sorted[i - 1].time),
                            attackProbability.Evaluate(i == sorted.Count -1 ? sorted.Count - 1 : sorted[i + 1].time));
                        attackProbability.Evaluate(posX + offset);
                        break;
                    }
                }

                Vector3 newPos = distanceFrom.position + new Vector3(posX - 0.5f, Mathf.Abs(posX - 0.5f)).normalized * swing_startDistance;

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
                afterPreparation.nextRelativeDesire = CurrentActivity.target.position - transform.position;
                afterPreparation.currentActionType = ActionType.Swing;

                //��������� ������� � ����������
                _currentCombo.Push(afterPreparation);
                _currentCombo.Push(preparation);

                Destroy(gameObj);
            }
        }

        // ����� ������� �� �����-�� ����� �� ������� �������.
        public override void Swing(Vector3 toPoint)
        {
            if (!_swingReady)
                return;

            base.Swing(toPoint);

            Vector3 moveTo = toPoint + (toPoint - BladeHandle.position).normalized * swing_EndDistanceMultiplyer;

            Vector3 pointDir = (moveTo - _vital.bounds.center).normalized;

            Vector3 closest = _vital.ClosestPointOnBounds(moveTo);
            float distance = (toPoint - closest).magnitude;
            moveTo = closest + (moveTo - closest).normalized * distance;
            SetDesires(moveTo, pointDir, (moveTo - toPoint).normalized);
        }
        #endregion

        #region check desire
        private void FixDesire()
        {
            Vector3 countFrom = distanceFrom.position;
            if (Vector3.Distance(_desireBlade.position, countFrom) > baseReachDistance)
            {
                Vector3 dir = (_desireBlade.position - countFrom).normalized;
                _desireBlade.position = countFrom + dir * baseReachDistance;
            }
        }

        public void SetDesires(Vector3 pos, Vector3 up, Vector3 forward)
        {
            //TODO : ��������� ��� ������ �� localPosition'�, ��� ����� ������� ����� ��������������� ��
            _desireBlade.position = pos;
            _desireBlade.LookAt(pos + up, pos + forward);
            _desireBlade.RotateAround(_desireBlade.position, _desireBlade.right, 90);

            if (isSwordFixing)
                FixDesire();

            if (MoveProgress >= 1)
            {
                InitiateNewBladeMove();
            }
        }
        /// <summary>
        /// ���������� �� ������ � ���� �� ����������?
        /// </summary>
        public bool CloseToDesire()
        {
            return Vector3.Distance(_bladeHandle.position, _desireBlade.position) < close_enough;
        }
        /// <summary>
        /// ������������� �� ���� ���������?
        /// </summary>
        public bool AlmostDesire()
        {
            return CloseToDesire()
                && Quaternion.Angle(_bladeHandle.rotation, _desireBlade.rotation) < angle_enough;
        }
        #endregion

        public void SetInitialDesires() 
        {
            SetDesires(_initialBlade.position,_initialBlade.up, _initialBlade.forward);
        }

        public void InitiateNewBladeMove()
        {
            _moveFrom.position = BladeHandle.position;
            _moveFrom.rotation = BladeHandle.rotation;
            _moveFrom.parent = _bladeContainer;
            _AnimatedMoveProgress = 0;
            _moveProgress = 0;
        }

        #region Specifications overrided

        protected override Tool ToolChosingCheck(Transform target)
        {
            return _blade;
        }

        public override Transform GetRightHandTarget()
        {
            return _blade.rightHandHandle;
        }

        public override void AssignPoints(int points)
        {
            base.AssignPoints(points);

            int remaining = points;

            //TODO DESIGN
        }

        public override void ActionUpdate(Transform target)
        {

        }

        public override Vector3 RelativeRetreatMovement()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}