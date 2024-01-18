using Sampo.Melee.Sword;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackCatcher : MonoBehaviour
{

    [Header("init-s")]
    public float minDistance = 0.3f;
    public bool debug_Draw = true;
    public bool blade_as_stuff = false;
    [Min(0.75f)]
    [Tooltip("Домножается на CriticalDistance. Чем выше - тем больше предсказаний будет учтено.")]
    public float ignoredDistance = 10;
    [Tooltip("Определеяет минимальную скорость, начиная с которой объект надо отбить")]
    public float ignoredImpulse = 5;

    [Header("Setup")]
    [SerializeField]
    [Tooltip("Этот колайдер - шестое чувство бойца")]
    private Collider checker;
    [SerializeField]
    [Tooltip("Этого колайдера объекты коснуться не должны")]
    private Collider vital;
    [Header("lookonly")]
    [SerializeField]
    private List<Rigidbody> ignored = new List<Rigidbody>();
    [SerializeField]
    [Tooltip("Это те штуки, за которыми надо следить в течении каждого кадра")]
    private List<GameObject> controlled = new();

    public List<GameObject> Controlled { get => new List<GameObject>(controlled);}

    public class AttackEventArgs : EventArgs
    {
        public Rigidbody body;
        public bool free; /// <summary> Определяет, что направление не имеет значение. <\summary>
        public Vector3 start;
        public Vector3 end;
        public Vector3 direction;
        public float impulse;
    }

    public event EventHandler<AttackEventArgs> OnIncomingAttack;

    private void Update()
    {
        foreach (GameObject thing in controlled)
        {
            if (!thing) // Пропускаем только что удалённые объекты
                continue;

            if(thing.TryGetComponent(out Faction f)) 
            {
                if (!f.IsWillingToAttack(GetComponent<Faction>().FactionType))
                    continue;
            }

            // У thing гарантированно есть Rigidbody. Это условие добавления в список.
            Rigidbody rb = thing.GetComponent<Rigidbody>();
            if (ignored.Contains(rb))
                continue;

            if (rb.velocity.magnitude * rb.mass < ignoredImpulse)
                continue;

            if (thing.TryGetComponent(out Blade blade))
                if (blade.host != null || !blade_as_stuff)
                {
                    BladeIncoming(blade);
                    continue;
                }
            
            StuffIncoming(rb);
        }
    }

    public void AddIgnoredObject(Rigidbody toAdd) 
    {
        ignored.Add(toAdd);
    }

    private void BladeIncoming(Blade blade)
    {
        // Тут вычисляем две точки: Куда прилетит меч?
        Vector3 center = vital.bounds.center;        
        Blade.Border predition = blade.GetPrediction(Vector3.Distance(center, blade.transform.position) - minDistance);
        Vector3 bladeCenter = Vector3.Lerp(predition.posUp, predition.posDown, 0.5f);
        Vector3 toVital = vital.bounds.ClosestPoint(predition.posUp) - bladeCenter;
        if (Vector3.Dot(predition.direction, toVital) < 0)
            return;

        if (debug_Draw)
        {
            Debug.DrawLine(predition.posUp, predition.posDown, Color.yellow);
            Debug.DrawRay(bladeCenter, predition.direction * 0.1f, Color.green);
            Debug.DrawLine(vital.bounds.center, predition.posDown, Color.yellow * 0.3f);
            Debug.DrawLine(vital.bounds.center, predition.posUp, Color.yellow * 0.3f);
        }

        OnIncomingAttack?.Invoke(this,
            new AttackEventArgs { body = blade.body, free = false, start = predition.posUp, end = predition.posDown, direction = predition.direction, impulse = blade.body.mass * blade.body.velocity.magnitude });
    }

    private void StuffIncoming(Rigidbody rb)
    {
        if (Vector3.Dot(rb.velocity, vital.bounds.center - rb.position) < 0)
            return;
        // Штука летит в сторону этого transform

        Vector3 center = vital.bounds.center;

        if ((rb.position - center).magnitude >= ignoredDistance)
            return;
        // Штука уже близко!

        if (rb.velocity.magnitude * rb.mass < ignoredImpulse)
            return;
        // У штуки достаточно высокая скорость

        Vector3 predictionPoint = rb.position + rb.velocity.normalized* (Vector3.Distance(center, rb.position) - minDistance);

        // Отталкиваем
        if(Vector3.Distance(predictionPoint, vital.ClosestPointOnBounds(predictionPoint)) < minDistance)
            predictionPoint = predictionPoint + (rb.position - predictionPoint).normalized * minDistance;

        if (debug_Draw)
        {
            Debug.DrawLine(rb.position, predictionPoint, Color.yellow);
        }

        OnIncomingAttack?.Invoke(this, new AttackEventArgs { body = rb,direction = rb.velocity.normalized, start = predictionPoint, end = predictionPoint, free = true, impulse = rb.mass * rb.velocity.magnitude });
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent<Rigidbody>(out _))
            return;

        if (!debug_Draw)
            return;

        if (collision.transform.TryGetComponent(out Blade blade))
        {
            if (TryGetComponent(out SwordFighter_StateMachine s) && blade == s?.Blade)
            {
                //Debug.Log($"Selfslash at speed {blade.body.velocity.magnitude}", collision.transform);
                Debug.DrawLine(blade.downerPoint.position, blade.upperPoint.position, new Color(0.8f, 0.2f, 0), 3);
            }
            else
            {
                //Debug.Log($"Skipped slash at speed {blade.body.velocity.magnitude}", collision.transform);
                Debug.DrawLine(blade.downerPoint.position, blade.upperPoint.position, new Color(0.5f, 0, 0), 3);
            }
        }
        else
        {
            Utilities.DrawSphere(collision.GetContact(0).point,color : Color.red, duration : 3);
            //Debug.Log($"Blunt damage at speed {collision.rigidbody.velocity.magnitude}", collision.transform);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        controlled.RemoveAll(item => item == null);

        Rigidbody body = other.GetComponent<Rigidbody>();
        if (body == null)
            return;
        // Эта хрень имеет способность самостоятельно перемещаться.

        // Этого уже достаточно, чтобы постоянно фиксировать объект в поле видимости.
        controlled.Add(body.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        // Объект вышел из поля, за ним больше не нужно постоянно наблюдать.
        controlled.Remove(other.gameObject);
    }
}
