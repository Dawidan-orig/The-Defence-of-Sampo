using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackCatcher : MonoBehaviour
{

    [Header("init-s")]
    [Range(0, 15)]
    public int predictions = 10;
    public bool draw = true;
    [Min(0.75f)]
    public float ignoredDistance = 10; // Домножается на CriticalDistance. Чем выше - тем больше предсказаний будет учтено.
    public float ignoredSpeed = 5; // Эта штука слишком медленная, чтобы о ней переживать.
    public List<Rigidbody> ignored = new List<Rigidbody>();

    [Header("Readonly")]
    public Collider checker; // Этот колайдер - шестое чувство бойца.
    public Collider vital; // Этого колайдера объекты коснуться не должны!
    public List<GameObject> controlled = new(); // Это те штуки, за которыми надо следить в течении каждого кадра.

    public class AttackEventArgs : EventArgs
    {
        public Rigidbody body;
        public bool free; // Определяет, что направление не имеет значение.
        public Vector3 start;
        public Vector3 end;
        public Vector3 direction;
        public float impulse;
    }

    public event EventHandler<AttackEventArgs> OnIncomingAttack;


    private void FixedUpdate()
    {
        foreach (GameObject thing in controlled)
        {
            if (!thing) // Позволяет пропустить удалённые в этом кадре объекты
                continue;

            // У thing гарантированно есть Rigidbody. Это условие добавление в список.
            Rigidbody rb = thing.GetComponent<Rigidbody>();
            if (ignored.Contains(rb))
                continue;

            if (rb.velocity.magnitude < ignoredSpeed)
                continue;

            if (thing.TryGetComponent(out Blade blade))
                SwordIncoming(blade);
            else
                StuffIncoming(rb);
        }
    }

    private void SwordIncoming(Blade blade)
    {
        // Тут вычисляем две точки: Куда прилетит меч?
        Vector3 center = vital.bounds.center;
        List<Blade.border> preditionList = blade.FixedPredict(predictions);
        Blade.border closest = preditionList[0];
        foreach (Blade.border border in preditionList)
        {
            Vector3 borderCenter = Vector3.Lerp(border.posDown, border.posUp, 0.5f);
            if (Vector3.Dot(border.direction, vital.bounds.center - borderCenter) < 0)
                continue;
            // В этом предсказании меч будет направлен в сторону vital.

            if ((center - border.posUp).magnitude > ignoredDistance)
            {
                continue;
            }
            // В это предсказании верхний край меча находится достаточно близко

            RaycastHit hit;
            if (Physics.Raycast(border.posDown, (border.posUp - border.posDown).normalized, out hit, (border.posUp - border.posDown).magnitude, 64))
            {
                continue;
            }
            // Игнорируем всё, что ударяется во внейшний коллайдер - оно далеко.
            // А так же всё, что ударяется во внутренний - оно уже чрезвычайно близко.

            if (MathF.Abs((center - borderCenter).magnitude) <
                MathF.Abs((center - Vector3.Lerp(closest.posDown, closest.posUp, 0.5f)).magnitude))
            {
                closest = border;
            }
        }

        // Нет смысла делать что-то с самим мечом - в следующем кадре он уже будет в vital коллайдере.
        // У struc'ов нет "==", но сравнить достаточно и по одному значению, чтобы убедится в полном равенстве.
        if (closest.posUp == preditionList[0].posUp)
            return;

        if (draw)
        {
            Debug.DrawLine(closest.posUp, closest.posDown, Color.yellow);
            Debug.DrawRay(closest.posUp, closest.direction * 0.1f, Color.green);
        }
        OnIncomingAttack?.Invoke(this, new AttackEventArgs { body = blade.body, free = false, start = closest.posUp, end = closest.posDown, direction = closest.direction, impulse = blade.body.mass * blade.body.velocity.magnitude });
    }

    private void StuffIncoming(Rigidbody rb)
    {
        if (!Physics.Raycast(rb.position, rb.velocity.normalized))
            return;
        // Штука летит в сторону этого transform

        if ((transform.position - rb.position).magnitude >= ignoredDistance)
            return;
        // Штука уже близко!

        //TODO: Тут вычисляем две точки, вдоль которых надо ударить, чтобы отбить случайный объект.


        if(draw)
            Debug.DrawLine(transform.position, transform.position + Vector3.up * 5, Color.red);
        OnIncomingAttack?.Invoke(this, new AttackEventArgs { body = rb, free = true, impulse = rb.mass * rb.velocity.magnitude });
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out Blade blade))
        {
            if (blade == GetComponent<SwordFighter>().blade)
                Debug.Log("Selfslash", collision.transform);
            else
                Debug.Log("Skipped slash", collision.transform);

            Debug.DrawLine(blade.downerPoint.position, blade.upperPoint.position, new Color(0.5f, 0, 0), 3); 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
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
