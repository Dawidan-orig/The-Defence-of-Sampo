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

    public FType f_type = FType.neutral;

    public bool IsWillingToAttack(FType type) 
    {
        bool comparedFactions = this.f_type != type; // На будущее, если вдруг захочу какие-нибудь альянсы.

        return (comparedFactions || this.f_type == FType.aggressive) && this.f_type != FType.neutral;
    }
}
