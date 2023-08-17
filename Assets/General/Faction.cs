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

    //TODO : Изменение фракции создаёт изменение в менеджере.
    // Лучший вариант: локальное изменение меняет поведение только текущего AI. Так сначала бывшие союзники не поймут изменения.
    // Сделать это простым get-set; Поле седлать private

    public FType type = FType.neutral;
}
