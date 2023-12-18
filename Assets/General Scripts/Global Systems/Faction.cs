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

    private void Start()
    {
        var visuals = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in visuals)
        switch (f_type)
        {
            case FType.sampo: renderer.material = Variable_Provider.Instance.sampo; break;
            case FType.enemy: renderer.material = Variable_Provider.Instance.enemy; break;
            case FType.aggressive: renderer.material = Variable_Provider.Instance.agro; break;
        }
    }

    public FType f_type = FType.neutral;

    public bool IsWillingToAttack(FType type)
    {
        bool comparedFactions = this.f_type != type; // На будущее, если вдруг захочу какие-нибудь альянсы.

        return (comparedFactions || this.f_type == FType.aggressive) && this.f_type != FType.neutral;
    }
}
