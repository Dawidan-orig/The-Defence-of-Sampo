using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Это поведение связанного с Клеткой transform'а. Нужен для работы систем поиска
/// </summary>
public class TransfromCellBehavior : MonoBehaviour
{
    [SerializeField]
    private NavMeshCalculations.Cell aligned;

    public NavMeshCalculations.Cell Aligned { get => aligned; set => aligned = value; }
}
