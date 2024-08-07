using WingedCore.AI;
using System.Collections.Generic;
/// <summary>
/// Интерфейс для всех внешних контроллеров приказов,
/// Который позволяет менять им значения
/// </summary>
public interface IOrderController
{
    public List<TargetingUtilityAI> GetOrderedUnits();
    public abstract bool GetOrderStatus(AITarget of);
}
