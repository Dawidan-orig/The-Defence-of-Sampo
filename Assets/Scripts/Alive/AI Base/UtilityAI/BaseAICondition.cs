using System;

namespace Sampo.AI.Conditions
{
    /// <summary>
    /// ƒинамическое условие дл€ »», которое обновл€етс€ независимо от него же и позвол€ет контроллировать активности
    /// </summary>
    [Serializable]
    public abstract class BaseAICondition
    {
        public bool IsConditionAlive = true;

        public abstract int WeightInfluence { get; }
        public abstract void Update();
        protected void EndCondition() 
        {
            IsConditionAlive = false;
        }
    }
}