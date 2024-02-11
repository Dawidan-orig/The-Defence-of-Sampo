using System;

namespace Sampo.AI.Conditions
{
    public class RespondToAttackCondition : BaseAICondition
    {
        DateTime start;
        float timeToEnd;
        public override int WeightInfluence => 500;

        public RespondToAttackCondition(float time) 
        {
            start = DateTime.Now;
            timeToEnd = time;
        }

        public override void Update()
        {
            if(DateTime.Now > start.AddSeconds(timeToEnd)) 
            {
                EndCondition();
            }
        }
    }
}