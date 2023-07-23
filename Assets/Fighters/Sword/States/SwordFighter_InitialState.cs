public class SwordFighter_InitialState : SwordFighter_BaseState
{
    public SwordFighter_InitialState(SwordFighter_StateMachine currentContext, SwordFighter_StateFactory factory)
        : base(currentContext, factory) { }

    public override void EnterState()
    {
        throw new System.NotImplementedException();
    }

    public override void ExitState()
    {
        throw new System.NotImplementedException();
    }

    public override void InitializeSubState()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void CheckSwitchStates()
    {

    }

    public override void FixedUpdateState()
    {
        throw new System.NotImplementedException();
    }
}
