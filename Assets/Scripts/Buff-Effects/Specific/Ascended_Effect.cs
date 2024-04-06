using System;
using UnityEngine;

[Serializable]
public class Ascended_Effect : UniversalEffect
{
    [SerializeField]
    private float _power;

    IMovingAgent _changedMovement;
    bool _movementEnabled;
    float _rbDrag;


    // TODO DESIGN : ��� � ���� ������� ����������� ����������...
    public Ascended_Effect(float power, float time, Rigidbody affected) : base(affected,
        "Ascended and fixed",
        "This entity is being pulled up and fixed by constant air force, unable to move.",
        time)
    {
        _power = power;
        _rbDrag = _affected.drag;
        _changedMovement = _affected.GetComponent<IMovingAgent>();
        _movementEnabled = _changedMovement.Component.enabled;
        _changedMovement.Component.enabled = false;
    }

    public override void FixedUpdate()
    {
        float DRAG = _affected.drag;

        float height = _power * DeceasingFunction(LinearTimeK());
        //TODO : NavMeshAgent'� ���������� � ������� ����� ���������� ����� �������. ������ �����, �������� ���-�� � NMPhysicsAgent
        if (Physics.Raycast(_affected.position, Vector3.down, out _, height, raycastGroundMask))
            _affected.AddForce(Vector3.up * _power * 10, ForceMode.Acceleration);
        else
            _affected.AddForce(Vector3.down * DRAG * 2, ForceMode.Acceleration); // �������� �������� ���������� ������� drag        

        if (height < 0.5f)
            _depretiated = true;

        _affected.drag = DRAG;
    }

    public override void Update()
    {
        base.Update();
        _changedMovement.Component.enabled = false;
    }

    public override void ReverseEffect()
    {
        _affected.drag = _rbDrag;
        _affected.GetComponent<IMovingAgent>().Component.enabled = _movementEnabled;
    }

    public override UniversalEffect MergeSimilar(UniversalEffect similarTypeEffect)
    {
        if (!(similarTypeEffect is Ascended_Effect))
            return null;
        Ascended_Effect merged = (Ascended_Effect)similarTypeEffect;

        return new Ascended_Effect(Mathf.Max(merged._power, _power), (merged.EffectDuration + EffectDuration) / 2, _affected);
    }

    // ���� �� AnimationCurve ��� ������������, �� �� �����
    private float DeceasingFunction(float time)
    {
        const float MAX_X = 1;
        const float MIN_X = 0;

        time = Mathf.Clamp(time, MIN_X, MAX_X);

        time = MAX_X - time;

        return (-1 * Mathf.Pow(time, 4) + 1);
    }
}
