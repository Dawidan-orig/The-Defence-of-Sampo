using UnityEngine;

public class Faction : MonoBehaviour
{
    public enum FType
    {
        neutral,

        sampo,
        enemy,
        
        aggressive // Нападает вообще на всех без разбору
    }

    //IDEA : Изменение фракции создаёт изменение в менеджере.
    // Лучший вариант: локальное изменение меняет поведение только текущего AI. Так сначала бывшие союзники не поймут изменения.
    // Сделать это простым get-set; Поле сделать private
    // Идея на будущее, когда появится возможность временно менять фракцию через всякие ярости берсеркера и тому подобное

    private void Start()
    {
        var visuals = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in visuals)
            switch (_ftype)
            {
                case FType.sampo: renderer.material = Variable_Provider.Instance.sampo; break;
                case FType.enemy: renderer.material = Variable_Provider.Instance.enemy; break;
                case FType.aggressive: renderer.material = Variable_Provider.Instance.agro; break;
            }
    }

    [SerializeField]
    private FType _ftype = FType.neutral;

    public FType FactionType { get => _ftype;}

    /// <summary>
    /// Полноценная смена фракции, из-за которой боевая сторона меняется полностью и без возможности восстановления.
    /// </summary>
    public void ChangeFactionCompletely(FType newFactionType) 
    {        
        _ftype = newFactionType;
    }

    public bool IsWillingToAttack(FType type)
    {
        bool comparedFactions = _ftype != type; // На будущее, если вдруг захочу какие-нибудь альянсы.

        return (comparedFactions || _ftype == FType.aggressive) && _ftype != FType.neutral;
    }
}
