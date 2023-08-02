using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour
{
    public enum Type
    {
        sampo,
        enemy,
       
        neutral,
        aggressive // Нападает вообще на всех без разбору
    }

    public Type type = Type.neutral;
}
