using UnityEngine;

public class AI_Attack : UtilityAI_BaseState
// ИИ двигается и атакует в этом состоянии.
{
    public AI_Attack(TargetingUtilityAI currentContext, UtilityAI_Factory factory) : base(currentContext, factory)
    {
    }

    public override bool CheckSwitchStates()
    {
        if (_ctx.DecidingStateRequired())
        {
            SwitchStates(_factory.Deciding());
            return true;
        }

        Tool weapon = _ctx.CurrentActivity.actWith;

        //TODO!!!!! : REFACTOR IS NEEEEEEDEEEEED!!!!!!!!!!!!!!!!!!!!
        if (_ctx.MovingAgent)
            _ctx.MovingAgent.MoveIteration(_ctx.transform.position);

        //TODO : Чтобы не плодить все эти разделения на стреляющего, ближнего боя и прочих - лучше сделать реакцию прямо в _ctx.
        // Это нужно в основном для существ, у которых оружие не классифицируемое.
        if (weapon is BaseShooting)
        {
            if (!((BaseShooting)weapon).AvilableToShoot(_ctx.CurrentActivity.target, out RaycastHit hit))
            {
                SwitchStates(_factory.Deciding());
                return true;
            }

            // Отходим назад
            if (_ctx.MovingAgent)
            {
                //TODO : Переместить это в LocalReposition
                float progress = 1 - (Vector3.Distance(_ctx.CurrentActivity.target.position, _ctx.transform.position) / (((BaseShooting)weapon).range));

                Vector3 newPos = _ctx.transform.position + _ctx.retreatInfluence.Evaluate(progress)
                    * (_ctx.CurrentActivity.target.position - _ctx.transform.position);

                if (_ctx.MovingAgent.IsNearObstacle(newPos - _ctx.transform.position, out Vector3 normal))
                {
                    Vector3 dir = Vector3.ProjectOnPlane((_ctx.CurrentActivity.target.position - _ctx.transform.position).normalized, normal);
                    dir.Normalize();

                    newPos = _ctx.transform.position + _ctx.retreatInfluence.Evaluate(progress) *
                        (_ctx.CurrentActivity.target.position - _ctx.transform.position).magnitude * dir;
                }

                _ctx.MovingAgent.MoveIteration(newPos,
                    _ctx.CurrentActivity.target.position);
            }

            return false;
        }

        if (!_ctx.MeleeReachable())
        {
            SwitchStates(_factory.Deciding());
            return true;
        }

        // Отходим назад
        //TODO : Переместить это в LocalReposition
        if (_ctx.MovingAgent)
        {
            float progress = 1 - (Vector3.Distance(_ctx.CurrentActivity.target.position, _ctx.transform.position)
                / (_ctx.CurrentActivity.actWith.additionalMeleeReach + _ctx.baseReachDistance));

            RetreatReposition(progress);
        }

        return false;
    }

    public override void EnterState()
    {

    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {

    }

    public override void UpdateState()
    {
        Debug.DrawRay(_ctx.transform.position, Vector3.up * 2, Color.red);

        if (CheckSwitchStates())
            return;

        _ctx.AttackUpdate(_ctx.CurrentActivity.target);
    }
    public override void FixedUpdateState()
    {

    }

    public override string ToString()
    {
        return "Attacking";
    }

    private void RetreatReposition(float retreatCurveTime)
    {
        Vector3 newPos = _ctx.transform.position + _ctx.retreatInfluence.Evaluate(retreatCurveTime)
                * (_ctx.CurrentActivity.target.position - _ctx.transform.position);

        if (_ctx.MovingAgent.IsNearObstacle(newPos - _ctx.transform.position, out Vector3 normal))
        {
            Vector3 dir = Vector3.ProjectOnPlane((_ctx.CurrentActivity.target.position - _ctx.transform.position).normalized, normal);
            dir.Normalize();

            Debug.DrawRay(_ctx.transform.position, dir, Color.black);

            newPos = _ctx.transform.position + dir
                * (_ctx.CurrentActivity.target.position - _ctx.transform.position).magnitude;
        }

        _ctx.MovingAgent.MoveIteration(newPos, _ctx.CurrentActivity.target.position);
    }
}
