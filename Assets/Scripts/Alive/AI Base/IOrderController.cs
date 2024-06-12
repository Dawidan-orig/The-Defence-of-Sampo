using Sampo.AI;
using System.Collections.Generic;
/// <summary>
/// ��������� ��� ���� ������� ������������ ��������,
/// ������� ��������� ������ �� ��������
/// </summary>
public interface IOrderController
{
    public List<TargetingUtilityAI> GetOrderedUnits();
    public abstract bool GetOrderStatus(Interactable_UtilityAI of);
}
