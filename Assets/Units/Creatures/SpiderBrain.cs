using System.Collections.Generic;
using UnityEngine;

public class SpiderBrain : TargetingUtilityAI
{
    public float speed = 5;
    public LegsHarmoniser legsHarmony;
    public SpiderLegControl activeLeg;
    public Vector3 wholeInitial;
    public Vector3 stateInitial;
    public Vector3 legDesire;
    public float height = 2;

    public AnimationCurve prepare;
    public AnimationCurve attack;
    public AnimationCurve returning;

    [SerializeField]
    float stateProgress = 1;
    State state = State.nothing;

    private enum State
    {
        nothing,
        prepare,
        attack,
        toReturn
    }

    public override void AttackUpdate(Transform target)
    {
        base.AttackUpdate(target);

        if (activeLeg == null)
        {
            legsHarmony.legs.RemoveAll(item => item == null);

            if (legsHarmony.legs.Count == 0)
                return;

            activeLeg = legsHarmony.legs[Random.Range(0, legsHarmony.legs.Count)];
            activeLeg.enabled = false;

            state = State.prepare;
            stateProgress = 0;
            wholeInitial = activeLeg.legTarget.position;
            stateInitial = wholeInitial;
            legDesire = wholeInitial + Vector3.up * height;
        }

        if (stateProgress <= 1)
            stateProgress += Time.deltaTime * speed;

        Debug.DrawLine(activeLeg.legTarget.position, legDesire);

        if (state == State.prepare)
            PrepareProcess(target);
        else if (state == State.attack)
            AttackProcess();
        else if (state == State.toReturn)
            ReturnProcess();
    }

    private void PrepareProcess(Transform target)
    {
        activeLeg.legTarget.position = Vector3.LerpUnclamped(stateInitial, legDesire, prepare.Evaluate(stateProgress));

        if (stateProgress > 1)
        {
            stateInitial = legDesire;
            legDesire = target.position;
            stateProgress = 0;
            state = State.attack;
            activeLeg.limb.IsDamaging = true;
        }
    }

    private void AttackProcess()
    {
        activeLeg.legTarget.position = Vector3.LerpUnclamped(stateInitial, legDesire, attack.Evaluate(stateProgress));

        if (stateProgress > 1)
        {
            stateInitial = legDesire;
            legDesire = wholeInitial;
            stateProgress = 0;
            state = State.toReturn;
            activeLeg.limb.IsDamaging = false;
        }
    }

    private void ReturnProcess()
    {
        activeLeg.legTarget.position = Vector3.LerpUnclamped(stateInitial, legDesire, returning.Evaluate(stateProgress));

        if (stateProgress > 1)
        {
            activeLeg.enabled = true;
            activeLeg = null;
        }
    }
}
