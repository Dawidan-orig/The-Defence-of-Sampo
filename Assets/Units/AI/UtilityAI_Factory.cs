using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityAI_Factory
{
    TargetingUtilityAI _context;

    AI_LongReposition _reposition;
    AI_Action _doSmthToAlly;
    AI_Attack _attackEnemy;
    AI_LocalReposition _navMeshShortMove;
    AI_Decide _deciding;

    public UtilityAI_Factory(TargetingUtilityAI currentContext)
    {
        _reposition = new AI_LongReposition(currentContext, this);
        _doSmthToAlly = new AI_Action(currentContext, this);
        _attackEnemy = new AI_Attack(currentContext, this);
        _navMeshShortMove = new AI_LocalReposition(currentContext, this);
        _deciding = new AI_Decide(currentContext, this);
        _context = currentContext;
    }
    public UtilityAI_BaseState Reposition()
    {
        return _reposition;
    }
    public UtilityAI_BaseState Action()
    {
        return _doSmthToAlly;
    }
    public UtilityAI_BaseState Attack()
    {
        return _attackEnemy;
    }
    public UtilityAI_BaseState LocalNavMeshReposition()
    {
        return _navMeshShortMove;
    }
    public UtilityAI_BaseState Deciding()
    {
        return _deciding;
    }
}
