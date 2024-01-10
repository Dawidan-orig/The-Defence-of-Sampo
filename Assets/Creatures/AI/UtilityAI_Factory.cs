using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI
{
    public class UtilityAI_Factory
    {
        //TODO DESIGN: Эта штука управляет имеющимися состояниями.
        //Надо придумать такой функционал, при котром можно будет добавлять любые состояния.
        // для этого надо будет обозначить некие главные состояния, а так же их выбор среди расширенных.

        AI_LongReposition _reposition;
        AI_Action _act;
        AI_Attack _attackEnemy;
        AI_Decide _deciding;

        public UtilityAI_Factory(TargetingUtilityAI currentContext)
        {
            _reposition = new AI_LongReposition(currentContext, this);
            _act = new AI_Action(currentContext, this);
            _attackEnemy = new AI_Attack(currentContext, this);
            _deciding = new AI_Decide(currentContext, this);
        }
        public UtilityAI_BaseState Reposition()
        {
            return _reposition;
        }
        public UtilityAI_BaseState Action()
        {
            return _act;
        }
        public UtilityAI_BaseState Attack()
        {
            return _attackEnemy;
        }
        public UtilityAI_BaseState Deciding()
        {
            return _deciding;
        }
    }
}