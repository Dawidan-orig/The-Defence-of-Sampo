using System;

namespace Sampo.AI.Conditions
{
    /// <summary>
    /// ������������ ������� ��� ��, ������� ����������� ���������� �� ���� �� � ��������� ��������������� ����������
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