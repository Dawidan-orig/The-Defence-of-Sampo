using Sampo.Core;
using UnityEngine;

public class Faction : MonoBehaviour
{
    public enum FType
    {
        neutral,

        sampo,
        enemy,
        
        aggressive // �������� ������ �� ���� ��� �������
    }

    //IDEA : ��������� ������� ������ ��������� � ���������.
    // ������ �������: ��������� ��������� ������ ��������� ������ �������� AI. ��� ������� ������ �������� �� ������ ���������.
    // ������� ��� ������� get-set; ���� ������� private
    // ���� �� �������, ����� �������� ����������� �������� ������ ������� ����� ������ ������ ���������� � ���� ��������

    private void Start()
    {
        /*
        var visuals = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in visuals)
            switch (_ftype)
            {
                case FType.sampo: renderer.material = Variable_Provider.Instance.sampo; break;
                case FType.enemy: renderer.material = Variable_Provider.Instance.enemy; break;
                case FType.aggressive: renderer.material = Variable_Provider.Instance.agro; break;
            }
        */
    }

    [SerializeField]
    private FType _ftype = FType.neutral;

    public FType FactionType { get => _ftype;}
    public bool IsAvailableForSelfFaction {
        get => isAvailableForSelfFaction;

        set
        {
            bool prev = isAvailableForSelfFaction;
            isAvailableForSelfFaction = value;

            if (TryGetComponent(out Interactable_UtilityAI interact))
            {
                if (value == false && prev == true)
                    UtilityAI_Manager.Instance.RemoveFromFaction(_ftype, GetComponent<Interactable_UtilityAI>());
                else if (value == true && prev == false)
                    UtilityAI_Manager.Instance.AddToFaction(_ftype, interact);
            }
        }
    }
    [SerializeField]
    private bool isAvailableForSelfFaction = false;

    /// <summary>
    /// ����������� ����� �������, ��-�� ������� ������ ������� �������� ��������� � ��� ����������� ��������������.
    /// </summary>
    public void ChangeFactionCompletely(FType newFactionType) 
    {
        if (TryGetComponent(out Interactable_UtilityAI interactable))
        {
            var kvp = new System.Collections.Generic.KeyValuePair<Interactable_UtilityAI, int>(interactable, interactable.ai_weight);

            UtilityAI_Manager.UAIData data = new(interactable, _ftype);
            UtilityAI_Manager.Instance.NewRemoved?.Invoke(this, data);
            _ftype = newFactionType;
            data = new(interactable, _ftype);
            UtilityAI_Manager.Instance.NewAdded?.Invoke(this, data);
        }
        else
            _ftype = newFactionType;
    }

    public bool IsWillingToAttack(FType type)
    {
        bool comparedFactions = _ftype != type; // �� �������, ���� ����� ������ �����-������ �������.

        return (comparedFactions || _ftype == FType.aggressive) && _ftype != FType.neutral;
    }
}
