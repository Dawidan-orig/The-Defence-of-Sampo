using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackCatcher : MonoBehaviour
{

    [Header("init-s")]
    [Range(0, 30)]
    public int predictions = 10;
    public bool Debug_Draw = true;
    [Min(0.75f)]
    public float ignoredDistance = 10; // Домножается на CriticalDistance. Чем выше - тем больше предсказаний будет учтено.
    public float ignoredSpeed = 5; // Определеяет минимальную скорость, начиная с которой объект надо отбить. TODO : Заменить на импульс f=mv
    public List<Rigidbody> ignored = new List<Rigidbody>();

    [Header("Readonly")]
    public Collider checker; // Этот колайдер - шестое чувство бойца.
    public Collider vital; // Этого колайдера объекты коснуться не должны!
    public List<GameObject> controlled = new(); // Это те штуки, за которыми надо следить в течении каждого кадра.

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
                if (blade.host != null)
                {
                    SwordIncoming(blade);
                    continue;
                }
            
            StuffIncoming(rb);
        }
    }

    private void SwordIncoming(Blade blade)
    {
        // Тут вычисляем две точки: Куда прилетит меч?
        Vector3 center = vital.bounds.center;
        List<Blade.border> preditionList = blade.FixedPredict(predictions);
        //preditionList.Reverse();

        Blade.border closest = preditionList[0];

        foreach (Blade.border border in preditionList) // Проходимся по всем предсказаням
        {
            Vector3 borderCenter = Vector3.Lerp(border.posDown, border.posUp, 0.5f);
            if (Vector3.Dot(border.direction, vital.bounds.center - borderCenter) < 0)
                continue;
            // В этом предсказании вектор движения меча будет направлен в сторону vital.

            if ((center - border.posUp).magnitude > ignoredDistance)
            {
                continue;
            }
            // В этом предсказании верхний край меча находится достаточно близко

            if (Physics.Raycast(border.posDown, (border.posUp - border.posDown).normalized, (border.posUp - border.posDown).magnitude) //Снизу вверх
                || 
                Physics.Raycast(border.posUp, (border.posDown-border.posUp).normalized, (border.posUp - border.posDown).magnitude) //Сверху вниз
                )
            {
                continue;
            }

            // Игнорируем всё, что ударяется во внейшний коллайдер - оно далеко.
            // А так же всё, что ударяется во внутренний - оно уже чрезвычайно близко.

            const float TOO_CLOSE = 0.5f; // Определяем минимальную дистанцию предсказания
            if (Vector3.Distance(border.posDown, vital.ClosestPointOnBounds(border.posDown)) < TOO_CLOSE
                || Vector3.Distance(border.posUp, vital.ClosestPointOnBounds(border.posUp)) < TOO_CLOSE)
                continue;

            closest = border;
        }

        // Нет смысла делать что-то с самим мечом - в следующем кадре он уже будет в vital коллайдере.
        // У struct'ов нет "==", но сравнить достаточно и по одному значению, чтобы убедится в полном равенстве.
        if (closest.posUp == preditionList[0].posUp)
            return;

        if (Debug_Draw)
        {
            Debug.DrawLine(closest.posUp, closest.posDown, Color.yellow);
            Debug.DrawRay(closest.posUp, closest.direction * 0.1f, Color.green);
        }

        OnIncomingAttack?.Invoke(this,
            new AttackEventArgs { body = blade.body, free = false, start = closest.posUp, end = closest.posDown, direction = closest.direction, impulse = blade.body.mass * blade.body.velocity.magnitude });
    }

    private void StuffIncoming(Rigidbody rb)
    {
        if (Vector3.Dot(rb.velocity, vital.bounds.center - rb.position) < 0)
            return;
        // Штука летит в сторону этого transform

        Vector3 center = rb.GetComponent<Collider>().bounds.center;

        if ((rb.position - center).magnitude >= ignoredDistance)
            return;
        // Штука уже близко!

        if (rb.velocity.magnitude < ignoredSpeed)
            return;
        // У штуки достаточно высокая скорость

        Vector3 predictionPoint = rb.position + predictions * Time.fixedDeltaTime * rb.velocity;

        // Если точка пролетела насквозь - уже надо брать максимально близкую позицию к себе
        if (Vector3.Dot(rb.velocity, vital.bounds.center - predictionPoint) < 0)
        {
            predictionPoint = NearestPointOnLine(rb.position, rb.velocity, vital.ClosestPointOnBounds(rb.position));
            //Debug.DrawLine(vital.bounds.center, predictionPoint, Color.cyan, 2);
        }

        const float BLADE_MIN_DIST = 0.5f;

        // Отталкиваем
        if(Vector3.Distance(predictionPoint, vital.ClosestPointOnBounds(predictionPoint)) < BLADE_MIN_DIST)
            predictionPoint = predictionPoint + (rb.position - predictionPoint).normalized * BLADE_MIN_DIST;

        if (Debug_Draw)
            Debug.DrawLine(rb.position, predictionPoint, Color.yellow);

        OnIncomingAttack?.Invoke(this, new AttackEventArgs { body = rb,direction = rb.velocity.normalized, start = predictionPoint, end = predictionPoint, free = true, impulse = rb.mass * rb.velocity.magnitude });
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
        else
            Debug.Log("Blunt damage", collision.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody body = other.GetComponent<Rigidbody>();
        if (body == null)
            return;
        // Эта хрень имеет способность самостоятельно перемещаться.

        // Этого уже достаточно, чтобы постоянно фиксировать объект в поле видимости.
        controlled.Add(body.gameObject);
        controlled.RemoveAll(item => item == null);
    }

    private void OnTriggerExit(Collider other)
    {
        // Объект вышел из поля, за ним больше не нужно постоянно наблюдать.
        controlled.Remove(other.gameObject);
    }

    //linePnt - point the line passes through
    //lineDir - unit vector in direction of line, either direction works
    //pnt - the point to find nearest on line for
    private Vector3 NearestPointOnLine(Vector3 linePnt, Vector3 lineDir, Vector3 pnt)
    {
        lineDir.Normalize();//this needs to be a unit vector
        var v = pnt - linePnt;
        var d = Vector3.Dot(v, lineDir);
        return linePnt + lineDir * d;
    }
}
