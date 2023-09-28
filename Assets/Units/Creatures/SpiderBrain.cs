using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[SelectionBase]
public class SpiderBrain : TargetingUtilityAI
{
    [Header("Spider")]
    public float attackSpeed = 1;
    public LegsHarmoniser legsHarmony;
    public SpiderLegControl attackingLeg;    
    public float legRaiseHeight = 2;
    public float rotationInfluence = 2;
    public float heightControlMultiplyer = 5;

    public AnimationCurve prepare;
    public AnimationCurve attack;
    public AnimationCurve returning;
    
    public LayerMask terrain;

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

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        //TODO : Если вдруг ноги изначально будут поставлены не правильно, то эта штука не сработает.
        initialBodyHeightOffset = transform.position.y - legsHarmony.legs[0].legTarget.position.y;
        initialBodyRotation = transform.rotation;
    }

    protected override void Update()
    {
        base.Update();
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, legsHarmony.legsLength, terrain))
            navMeshCalcFrom.position = hit.point;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();        

        float average = 0;
        foreach(SpiderLegControl leg in legsHarmony.legs) 
            if(leg != attackingLeg && leg != null)
                average += leg.legTarget.position.y; 

        average /= legsHarmony.legs.Count;

        desireBodyHeight = average + initialBodyHeightOffset;

        const float CLOSE_ENOUGH = 0.5f;
        if (!Utilities.ValueInArea(desireBodyHeight, transform.position.y, CLOSE_ENOUGH))
        {
            Vector3 force = (desireBodyHeight- transform.position.y) * heightControlMultiplyer * Time.fixedDeltaTime * Vector3.up;
            Utilities.DrawLineWithDistance(transform.position, transform.position + force, color : Color.black);
            Body.AddForce(force, ForceMode.VelocityChange);   
        }

        foreach (LegsHarmoniser.LegsPair pair in legsHarmony.legPairs) 
        {
            if (pair.left == null || pair.right == null)
                continue;

            float initialEulerY = initialBodyRotation.y;
            float diff = pair.left.legTarget.position.y - pair.right.legTarget.position.y;
            //IDEA : Если поставить -diff, то получится хоррор.
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y, diff * rotationInfluence + initialEulerY);
        }

        if(_currentState is not AI_Attack && _spiderState != SpiderState.nothing && attackingLeg != null) 
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
        base.AttackUpdate(target);

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
}
