using Sampo.Melee;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Blade : MeleeTool
{
    //TODO DESIGN (Когда будет вариативность Melee) : Добавить сюда понятие рукояти (И основного объекта, контроллирующего всё оружие, как следествие) и угловое движение относительно MeleeFighter.distanceFrom
    
    [Header("Init-s")]
    public Transform upperPoint;
    public Transform downerPoint;
    [SerializeField]
    private Transform handle;

    [Header("lookonly")]
    public Rigidbody body;
    public Vector3 AngularVelocityEuler;
    public Faction faction;

    [Header("Constraints")]
    public Color predictionColor = Color.red;
    public int iterations = 1;
    public float noDamageTime = 0.5f;

    [Header("Visuals")]
    public ParticleSystem sparkles;

    public Transform Handle { get => handle; private set => handle = value; }

    public event EventHandler<Collision> OnBladeCollision; //Расшариваю здешнюю коллизию в MeleeFighter'a
    public event EventHandler<Collider> OnBladeTrigger;

    public struct Border
    {
        public Vector3 posUp;
        public Vector3 posDown;
        public Vector3 direction;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        faction = GetComponent<Faction>();
    }

    private void Start()
    {
        if(host)
            Physics.IgnoreCollision(GetComponent<Collider>(), host.GetComponent<AliveBeing>().vital);

        GameObject massCenterGo = new("MassCenter");
        massCenterGo.transform.parent = transform;        
        body.centerOfMass = handle.localPosition;
        massCenterGo.transform.position = body.worldCenterOfMass;

        additionalMeleeReach = Vector3.Distance(upperPoint.position, handle.position)/2;
    }

    private void Update()
    {
        if (faction)
        {
            if (host)
                faction.ChangeFactionCompletely(host.GetComponent<Faction>().FactionType);
            else
                faction.ChangeFactionCompletely(Faction.FType.aggressive);
        }
    }

    /// <summary>
    /// Возвращает предсказание позиции меча
    /// </summary>
    /// <param name="withDistance">Расстояние, которое должно пройти лезвие</param>
    /// <returns>Все данные о позиции меча</returns>
    public Border GetPrediction(float withDistance) 
    {
        Border border = new();

        border.direction = body.velocity.normalized;

        float timeToFly = withDistance / body.velocity.magnitude;

        Quaternion rotationIteration = Quaternion.Euler(AngularVelocityEuler * timeToFly);

        Vector3 rotatedPosUp = upperPoint.position - transform.position;
        rotatedPosUp = rotationIteration * rotatedPosUp;
        border.posUp = transform.position + rotatedPosUp + (body.velocity * timeToFly);

        Vector3 rotatedPosDown = downerPoint.position - transform.position;
        rotatedPosDown = rotationIteration * rotatedPosDown;
        border.posDown = transform.position + rotatedPosDown + (body.velocity * timeToFly);        

        return border;
    }

    private void FixedUpdate()
    {
        //TODO : Добавить систему для проверки коллизии на высокой скорости
        AngularVelocityEuler = body.angularVelocity * 360 / (2 * Mathf.PI);
    }


    private void OnCollisionEnter(Collision collision)
    {
        OnBladeCollision?.Invoke(this, collision);

        if(collision.collider.transform.TryGetComponent<AliveBeing>(out var alive)) 
        {
            alive.Damage(body.velocity.magnitude * body.mass * damageMultiplier, IDamagable.DamageType.sharp);
            GetComponent<Collider>().isTrigger = true;
            Invoke(nameof(ResetCollision), noDamageTime);
        }
        else if(collision.collider.transform.TryGetComponent<Blade>(out _)) 
        {
            GetComponent<Collider>().isTrigger = true;
            Invoke(nameof(ResetCollision), noDamageTime);

            if (sparkles)
            {
                Vector3 sparklesSpread = collision.GetContact(0).point;
                transform.position = sparklesSpread;
                sparkles.Emit((int)sparkles.emission.GetBurst(0).count.constant);
            }
        }
    }

    private void ResetCollision() 
    {
        GetComponent<Collider>().isTrigger = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        OnBladeTrigger?.Invoke(this, other);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(downerPoint.position, upperPoint.position);
    }
}
