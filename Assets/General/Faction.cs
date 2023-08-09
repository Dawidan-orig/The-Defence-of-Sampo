using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour
{
    public enum FType
    {
        sampo,
        enemy,
       
        neutral,
        aggressive // Нападает вообще на всех без разбору
    }

    public FType type = FType.neutral;
}
