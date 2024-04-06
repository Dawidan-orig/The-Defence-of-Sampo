using System;
using UnityEngine;

[Serializable] // ���� ������� ����������� �������, ��� ��� ��� ����� �������� � �����.
public class UniversalEffect
{
    public LayerMask raycastGroundMask;

    [SerializeField]
    protected Rigidbody _affected;
    [SerializeField]
    private float _currentTimeLeft = -1;
    [SerializeField]
    private float _effectDuration;
    [SerializeField]
    private string _name;
    [SerializeField]
    private string _description;
    [SerializeField]
    private Sprite _icon;

    protected bool _depretiated = false;

    public bool Depretiated { get => _depretiated; set => _depretiated = value; }
    public float EffectDuration { get => _effectDuration; set => _effectDuration = value; }

    //TODO DESIGN : � ��� ����������-������-������ ��� �������-�������������.
    //- �����-������ ������� ��������� ��� ������ �������;
    //- Ƹ���� ����� ��� ��������� ������������� ������...

    public UniversalEffect(Rigidbody affected, string name, string description, float effectDuration)
    {
        _affected = affected;
        _effectDuration = effectDuration;
        _name = name;
        _description = description;
        _currentTimeLeft = effectDuration;

        raycastGroundMask = LayerMask.NameToLayer("default");
    }

    public virtual void Update()
    {
        if (_currentTimeLeft > 0)
            _currentTimeLeft -= Time.deltaTime;
        else
            _depretiated = true;
    }

    public virtual void ReverseEffect() { }

    public virtual void FixedUpdate() { }

    public virtual UniversalEffect MergeSimilar(UniversalEffect similarTypeEffect) { return null; }

    protected float LinearTimeK()
    {
        return _currentTimeLeft / _effectDuration;
    }
}
