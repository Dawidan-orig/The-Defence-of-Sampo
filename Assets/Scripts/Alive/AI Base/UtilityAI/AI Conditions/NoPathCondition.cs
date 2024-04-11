using System;
using UnityEngine.AI;

namespace Sampo.AI.Conditions
{
    public class NoPathCondition : BaseAICondition
    {
        //TODO? : Может быть, лучше было бы делать здесь проверку на NavMeshPath и достижимость пути, это было бы красивее и надёжнее. Но дорого.
        DateTime start;
        float timeToEnd;
        public override int WeightInfluence => -1000;

        public NoPathCondition(float time)
        {
            start = DateTime.Now;
            timeToEnd = time;
        }

        public override void Update()
        {
            if (DateTime.Now > start.AddSeconds(timeToEnd))
            {
                EndCondition();
            }
        }
    }
}