using System;
using UnityEngine;

namespace Sampo.Melee.Sword
{
    [RequireComponent(typeof(AttackCatcher))]
    public class SwordFighter_StateMachine : MeleeFighter
    {
        public SwordFighter_BaseState CurrentSwordState { get { return _currentSwordState; } set { _currentSwordState = value; } }

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
        [Tooltip("Нужен для вывода текущего состояния в Unity inspector")]
        private string currentState;

        #region Unity
        protected override void Awake()
        {
            base.Awake();

            _catcher.AddIgnoredObject(_blade.body);

            _currentToInitialAwait = toInitialAwait;

            _fighter_states = new SwordFighter_StateFactory(this);
            _currentSwordState = _fighter_states.Idle();
            _currentSwordState.EnterState();

            _blade.GetComponent<Tool>().SetHost(transform);
        }

        protected override void Start()
        {
            base.Start();

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

            SetDesires(_initialBlade.position, _initialBlade.up, _initialBlade.forward);
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
            //TODO DESIGN : Добавить коррекцию движения оружия уже во время Swing, чтобы нивелировать движение как врага, так и самого юнита.
            //TODO DESIGN : Придумать, как можно вытащить StateMachine и изменять в ней только принципы движения, хваты рук, и добавить возможность делать траекторию через BezierCurve
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

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

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

            // Скорость низкая и объект можно отразить как угодно - взмах
            if (e.free && e.body.velocity.magnitude < blockCriticalVelocity)
            {
                //IDEA: Вариант сделать замах: С помощью BezierCurve.

                Vector3 toPoint = e.start;

                Vector3 bladeCenter = Vector3.Lerp(Blade.upperPoint.position, Blade.downerPoint.position, 0.5f);
                float bladeCenterLen = Vector3.Distance(bladeCenter, Blade.downerPoint.position);
                float swingDistance = bladeCenterLen + toBladeHandle_MaxDistance;

                if (Vector3.Distance(distanceFrom.position, toPoint) < swingDistance)
                {
                    OnSwingIncoming?.Invoke(this, new IncomingSwingEventArgs { toPoint = toPoint });
                }
            }
            // То же самое, но тут блокируем - взмах долгий слижком
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
            // Тут надо отразить точным блоком
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

                //IDEA : Это логика рапиры, из-за чего отбиваемое оружие "отражается".
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

                Vector3 centerOffset = (Blade.downerPoint.position - Blade.downerPoint.position).normalized *
                    (-Vector3.Distance(BladeHandle.position, Blade.downerPoint.position)); // Смещение для ровной установки рукояти

                OnRepositionIncoming?.Invoke(this, new IncomingReposEventArgs { bladeDown = centerOffset + bladeDown, bladeUp = centerOffset + bladeUp, bladeDir = toEnemyBlade_Dir });
            }
        }

        // Установка меча по всем возможным параметрам
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

            if (MeleeReachable())
            {
                //TODO : Тут ещё можем выбирать конкретную комбинацию из библиотеки комбо.

                ActionJoint afterPreparation = new ActionJoint();
                ActionJoint preparation = new ActionJoint();

                // Выбираем какую-то точку для удара
                const int LIMIT = 50;
                float posX = 0;
                bool res = false;
                int iteration = 0;
                while (!res) //TODO : Оптимизировать для решения одной формулой, а не циклом.
                {
                    posX = UnityEngine.Random.Range(0, 1);
                    float prob = UnityEngine.Random.Range(0, 1);
                    res = attackProbability.Evaluate(posX) > prob;

                    if (iteration++ > LIMIT)
                        break;
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

                //Добавляем начиная с последнего
                _currentCombo.Push(afterPreparation);
                _currentCombo.Push(preparation);

                Destroy(gameObj);
            }
        }

        // Атака оружием по какой-то точке из текущей позиции.
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
            throw new NotImplementedException();

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
                //TODO:
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

            if (isSwordFixing)
                FixDesire();

            if (MoveProgress >= 1)
            {
                InitiateNewBladeMove();
            }
        }
        /// <summary>
        /// Достаточно ли близко к цели по расстоянию?
        /// </summary>
        public bool CloseToDesire()
        {
            return Vector3.Distance(_bladeHandle.position, _desireBlade.position) < close_enough;
        }
        /// <summary>
        /// Соответствует ли цели полностью?
        /// </summary>
        public bool AlmostDesire()
        {
            return CloseToDesire()
                && Quaternion.Angle(_bladeHandle.rotation, _desireBlade.rotation) < angle_enough;
        }
        #endregion
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

            //TODO
        }

        public override void ActionUpdate(Transform target)
        {

        }
        #endregion
    }
}