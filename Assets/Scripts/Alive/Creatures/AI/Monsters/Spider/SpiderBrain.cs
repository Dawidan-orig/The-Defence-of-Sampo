using Sampo.AI.Monsters.Spider;
using UnityEngine;

namespace Sampo.AI.Monsters
{
    [SelectionBase]
    public class SpiderBrain : AIBehaviourBase
    {
        //TODO : Преобразовать в StateMachine от Git-Amend.
        [Header("Spider")]
        [Tooltip("Как часто происходят удары ногами")]
        public float attackSpeed = 1;
        public LegsHarmoniser legsHarmony;
        [Tooltip("Как высоко поднимаются ноги при шаге")]
        public float legRaiseHeight = 2;
        [Tooltip("Модификатор, определяющий тангаж тела паука")]
        public float rotationInfluence = 2;
        [Tooltip("Модификатор, определяющий силу притяжения или отталкивания тела паука от земли")]
        public float heightControlMultiplyer = 5;

        public AnimationCurve prepare;
        public AnimationCurve attack;
        public AnimationCurve returning;

        public LayerMask terrain;

        private SpiderLegControl attackingLeg;
        private Vector3 _wholeInitial;
        private Vector3 _stateInitial;
        private Vector3 _legDesire;
        private float _stateProgress = 1;
        private SpiderState _spiderState = SpiderState.nothing;
        private float desireBodyHeight;
        private Quaternion initialBodyRotation;
        private float initialBodyHeightOffset;

        private enum SpiderState
        {
            nothing,
            prepare,
            attack,
            toReturn
        }

        protected void Start()
        {
            initialBodyHeightOffset = transform.position.y - legsHarmony.legs[0].legTarget.position.y;
            initialBodyRotation = transform.rotation;
        }

        protected void Update()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out var hit, legsHarmony.legsLength, terrain))
                NavMeshCalcFrom.position = hit.point + Vector3.up* 1.5f;
        }

        protected void FixedUpdate()
        {
            short onGroundAmount = 0;
            float average = 0;
            foreach (SpiderLegControl leg in legsHarmony.legs)
                if (leg != attackingLeg && leg != null)
                {
                    average += leg.legTarget.position.y;
                    if (leg.stable)
                        onGroundAmount++;
                }

            average /= legsHarmony.legs.Count;

            desireBodyHeight = average + initialBodyHeightOffset;

            const float CLOSE_ENOUGH = 0.5f;
            if (!Utilities.ValueInArea(desireBodyHeight, transform.position.y, CLOSE_ENOUGH))
            {
                Vector3 force = (desireBodyHeight - transform.position.y) * heightControlMultiplyer * Time.fixedDeltaTime * Vector3.up;
                Body.AddForce(force, ForceMode.VelocityChange);
            }

            if (onGroundAmount > legsHarmony.legs.Count / 2) // Без этого условия тело паука страшнейним образом вращается в воздухе.
            {
                float diff = 0;
                foreach (LegsHarmoniser.LegsPair pair in legsHarmony.legPairs)
                {
                    if (pair.left == null || pair.right == null)
                        continue;

                    diff += pair.left.legTarget.position.y - pair.right.legTarget.position.y;
                }


                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y,
                    diff * rotationInfluence / legsHarmony.legPairs.Count + initialBodyRotation.y);
            }

            if (_spiderState != SpiderState.nothing && attackingLeg != null)
            {
                if (_spiderState != SpiderState.toReturn)
                {
                    _spiderState = SpiderState.toReturn;
                    _stateInitial = attackingLeg.limb.transform.position;
                    _legDesire = _wholeInitial;
                    _stateProgress = 0;
                    attackingLeg.limb.IsDamaging = false;
                }

                if (_stateProgress <= 1)
                    _stateProgress += Time.fixedDeltaTime * attackSpeed;

                ReturnProcess();
            }
        }

        public override void AttackUpdate(Transform target)
        {
            if (attackingLeg == null)
            {
                legsHarmony.legs.RemoveAll(item => item == null);

                if (legsHarmony.legs.Count == 0)
                    return;

                attackingLeg = legsHarmony.legs[Random.Range(0, legsHarmony.legs.Count)];
                attackingLeg.enabled = false;

                _spiderState = SpiderState.prepare;
                _stateProgress = 0;
                _wholeInitial = attackingLeg.legTarget.position;
                _stateInitial = _wholeInitial;
                _legDesire = _wholeInitial + Vector3.up * legRaiseHeight;
            }

            if (_stateProgress <= 1)
                _stateProgress += Time.deltaTime * attackSpeed;

            if (_spiderState == SpiderState.prepare)
                PrepareProcess(target);
            else if (_spiderState == SpiderState.attack)
                AttackProcess();
            else if (_spiderState == SpiderState.toReturn)
                ReturnProcess();
        }

        private void PrepareProcess(Transform target)
        {
            attackingLeg.legTarget.position = Vector3.LerpUnclamped(_stateInitial, _legDesire, prepare.Evaluate(_stateProgress));

            if (_stateProgress > 1)
            {
                _stateInitial = _legDesire;
                _legDesire = target.position;
                _stateProgress = 0;
                _spiderState = SpiderState.attack;
                attackingLeg.limb.IsDamaging = true;
            }
        }

        private void AttackProcess()
        {
            attackingLeg.legTarget.position = Vector3.LerpUnclamped(_stateInitial, _legDesire, attack.Evaluate(_stateProgress));

            if (_stateProgress > 1)
            {
                _stateInitial = _legDesire;
                _legDesire = _wholeInitial;
                _stateProgress = 0;
                _spiderState = SpiderState.toReturn;
                attackingLeg.limb.IsDamaging = false;
            }
        }

        private void ReturnProcess()
        {
            attackingLeg.legTarget.position = Vector3.LerpUnclamped(_stateInitial, _legDesire, returning.Evaluate(_stateProgress));

            if (_stateProgress > 1)
            {
                attackingLeg.enabled = true;
                attackingLeg = null;
            }
        }

        public override void AssignPoints(int points)
        {
            base.AssignPoints(points);

            int remaining = points;

            //TODO DESIGN
        }

        public override Tool ToolChosingCheck(Transform target)
        {
            return legsHarmony.legs[0].limb; //Не важно, какую ногу, важны лишь параметры. У всех ног параметры (Пока что) одинаковы.
        }

        public override void ActionUpdate(Transform target)
        {
            throw new System.NotImplementedException();
        }

        public override Vector3 RelativeRetreatMovement()
        {
            throw new System.NotImplementedException();
        }
    }
}