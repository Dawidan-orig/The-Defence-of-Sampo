using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPointsDistribution
{
    /// <summary>
    /// Распределяет данные очки силы по параметрам объекта, к которому прикреплён интерфейс.
    /// </summary>
    /// <param name="points"> Количество силы, которое распределяется </param>
    public abstract void GivePoints(int points);
}
