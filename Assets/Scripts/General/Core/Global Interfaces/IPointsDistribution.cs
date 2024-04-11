using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPointsDistribution
{
    /// <summary>
    /// ������������ ������ ���� ���� �� ���������� �������, � �������� ��������� ���������.
    /// </summary>
    /// <param name="points"> ���������� ����, ������� �������������� </param>
    public abstract void AssignPoints(int points);
}
