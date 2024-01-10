using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WindSlide : Ability
{
    public const float RECHARGE = 3;
    public const float DISTANCE = 30;
    public const float SPEED = 40;

    [SerializeField]
    private Rigidbody _body;
    [SerializeField]
    private float _currentRecharge = 0;
    [SerializeField]
    private float _surpassedDistance = 0;
    [SerializeField]
    private Vector3 _prevPos = Vector3.zero;
    [SerializeField]
    private Vector3 _direction = Vector3.zero;


    public WindSlide(Transform user) : base(user)
    {
        _currentRecharge = RECHARGE;
        _prevPos = user.position;
        _body = user.GetComponent<Rigidbody>();
    }

    public override void Activate()
    {
        if (_currentRecharge >= RECHARGE)
        {
            base.Activate();
            _direction= Camera.main.transform.forward;
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
        _currentRecharge = 0;
        _surpassedDistance = 0;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (_activated)
        {
            _body.AddForce(_direction * SPEED * Time.fixedDeltaTime, ForceMode.VelocityChange);

            _surpassedDistance += Vector3.Distance(_prevPos, user.position);

            if (_surpassedDistance > DISTANCE)
                Deactivate();
        }

        _prevPos = user.position;
    }

    public override void Update()
    {
        if (_activated)
            return;

        if (_currentRecharge < RECHARGE)
            _currentRecharge += Time.deltaTime;
    }

    public override void Enable()
    {
        if (user.TryGetComponent<SwordControl>(out _))        
            base.Enable();        
    }

    public override void Disable()
    {
        base.Disable();
    }
}
