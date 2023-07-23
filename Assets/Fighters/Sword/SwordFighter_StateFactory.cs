public class SwordFighter_StateFactory
{
    SwordFighter_StateMachine _context;

    public SwordFighter_StateFactory(SwordFighter_StateMachine currentContext)
    {
        _context = currentContext;
    }
    public SwordFighter_BaseState Initial() 
    {
        return new SwordFighter_InitialState(_context, this);
    }
    public SwordFighter_BaseState Repositioning()
    {
        return new SwordFighter_InterruptableRepositioningState(_context, this);
    }
    public SwordFighter_BaseState Swinging()
    {
        return new SwordFighter_SwingingState(_context, this);
    }
    public SwordFighter_BaseState Idle()
    {
        return new SwordFighter_IdleState(_context, this);
    }
}
