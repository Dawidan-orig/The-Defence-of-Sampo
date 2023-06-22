using System;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(AttackCatcher))]
public class SwordFighter : MonoBehaviour
// Управляет мечом, который вертится в воздухе перед объектом.
{
    [Header("constraints")]
    public GameObject enemy;
    public float actionSpeed = 10; // Скорость движения меча в руке
    public float angluarSpeed = 10; // Скорость поворота тела
    public float swingImpulse = 20; // Насколько сильно бьёт меч
    public float actionDistance = 1; // Меч может быть не дальше этой дистанции.
    public float criticalImpulse = 200; // Лучше увернуться, чем отбить объект с импульсом больше этого!

    [Header("init-s")]
    public Blade blade;
    public Transform bladeHandle;
    public Vector3 offset = Vector3.up;
    public Collider vital;
    public bool fixated = true;

    [Header("lookonly")]
    public Vector3 formalCenter;
    public GameObject bladeObject;
    public Vector3 desireDirection = Vector3.up;
    public Vector3 desirePosition;

    // Start is called before the first frame update
    void Start()
    {
        AttackCatcher catcher = gameObject.GetComponent<AttackCatcher>();
        catcher.OnIncomingAttack += Incoming;
        catcher.ignored.Add(blade.body);
        bladeObject = blade.gameObject;

        desirePosition = bladeHandle.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Bridge pattern?
        formalCenter = offset + transform.position;
    }

    private void FixedUpdate()
    {
        Contorl_MoveSword();

        Debug.DrawRay(desirePosition, desireDirection.normalized);
    }

    private void Incoming(object sender, AttackCatcher.AttackEventArgs e)
    {
        // Итак, в нас летит непонятно что.
        if (e.free)
        {
            // Это неконтроллируемый объект, который просто летит в нашу сторону. Либо отбить, либо увернуться!
            if (e.impulse < criticalImpulse)
                Swing(e.body.position);
            else
                Evade(e.body.position);
        }
        else
        {
            // С этим объектом нужно действовать уже с определённого угла.
            // Скорее всего, это чужое лезвие.
            // Можно поставить обычный блок,
            // Иногда нужно сделать жёсткий блок (Удар-отбивание)
            // А иногда лучше вообще увернуться, если атака совсем сильная.


            // Надо взять перпендикуляр от меча

            Vector3 bladeCenter = Vector3.Lerp(e.start, e.end, 0.5f); // Центр атаки, центр нашего меча должен быть в этой же точке.

            GameObject bladePrediction = new();
            bladePrediction.transform.position = bladeCenter;
            if (fixated)
            {
                Vector3 closest = vital.ClosestPoint(bladeCenter);
                bladePrediction.transform.position = closest + (bladeCenter - closest).normalized * actionDistance;
            }

            GameObject start = new();
            start.transform.position = e.start;
            start.transform.parent = bladePrediction.transform;

            GameObject end = new();
            end.transform.position = e.end;
            end.transform.parent = bladePrediction.transform;

            bladePrediction.transform.Rotate(e.direction, 90);

            Vector3 bladeStart = start.transform.position;
            Vector3 bladeEnd = end.transform.position;

            Destroy(bladePrediction);
            int ignored = blade.gameObject.layer; // Игнорируем при проверке на самовтыкание все мечи.
            ignored = ~ignored;

            // Проверяем снизу вверх
            if (Physics.Raycast(bladeStart, bladeEnd, out RaycastHit hit, (bladeStart - bladeEnd).magnitude, ignored))
            {
                // Меч втыкается куда-то. Возможно, в себя самого. Ну его.
                Debug.DrawLine(bladeStart, bladeEnd, new Color(0.9f,0.6f, 0.6f), 10);
                Debug.Log("returning, because precition in: " + hit.collider.transform.name + ", Layermask: " + ignored, hit.collider.gameObject);
                return;
            }

            // Сверху вниз.
            if (Physics.Raycast(bladeEnd, bladeStart, out hit, (bladeStart - bladeEnd).magnitude, ignored))
            {
                // Меч втыкается куда-то. Возможно, в себя самого. Ну его.
                Debug.DrawLine(bladeStart, bladeEnd, new Color(0.9f, 0.6f, 0.6f), 10);
                Debug.Log("returning, because precition in: " + hit.collider.transform.name, hit.collider.gameObject);
                return;
            }

            if ((formalCenter - bladeEnd).magnitude < (formalCenter - bladeStart).magnitude)
                Block(bladeStart, bladeEnd);
            else
                Block(bladeStart, bladeEnd);
        }
    }

    // Уворот по Rigidbody через импульс.
    private void Evade(Vector3 fromPoint) { }

    // Атака оружием из его текущей точки.
    private void Swing(Vector3 toPoint) { }

    // Установка меча с крепким удержанием на какую-то точку.
    private void Block(Vector3 point)
    {
        // Исходя из текущего положения меча, максимально быстро повернуть так, чтобы заблокировать точку.
        // Точка - центр меча, для примера
    }
    private void Block(Vector3 start, Vector3 end)
    {
        // Входные данные - это то, что надо заблокировать.

        desireDirection = (end - start).normalized;
        desirePosition = start;

        //TODO : Теперь мне надо сделать более медленное и плавное перемещение меча.

    }

    private void Contorl_MoveSword()
    {
        float progress = actionSpeed * Time.fixedDeltaTime;

        float heightFrom = (bladeHandle.position - formalCenter).y;
        float heightTo = (desirePosition - formalCenter).y;

        Vector3 from = new Vector3((bladeHandle.position - formalCenter).x, 0, (bladeHandle.position - formalCenter).z);
        Vector3 to = new Vector3((desirePosition - formalCenter).x, 0, (desirePosition - formalCenter).z);

        bladeHandle.position = formalCenter + Vector3.Slerp(from,to , progress) + new Vector3(0, Mathf.Lerp(heightFrom, heightTo, progress), 0);

        
        #region rotationControl;
        GameObject go = new();
        Transform probe = go.transform;
        probe.position = bladeHandle.position;
        probe.rotation = bladeHandle.rotation;
        probe.parent = null;

        probe.LookAt(bladeHandle.position + desireDirection, Vector3.up);
        probe.Rotate(Vector3.right, 90);

        //Есть текущее положение, и есть точка-цель с поворотом.
        //Чтобы поворот осуществлялся так, как мне нужно, надо виртуально ставить probe в ту же точку, с которой начинаем движение
        //Причём ставить надо, как бы, поворотом. То-есть брать её, и вращать на расстоянии от центра vital.
        //Так получится именно тот поворот, которой надо осуществлять.
        //Например, Если поворот старта и поворот в результате совпадают - поворота не будет совсем, и из-за position-смещения probe порежет vital.
        //


        bladeHandle.rotation = Quaternion.Lerp(bladeHandle.rotation, transform.rotation * probe.rotation, progress);

        Debug.DrawRay(bladeHandle.position, probe.rotation * Vector3.up);

        Destroy(go);
        #endregion
        
        //TODO : добавить сюда систему выталкивания меча из vital после предсказывания
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(bladeHandle.position, 0.05f);
        Gizmos.DrawSphere(desirePosition, 0.05f);
    }
}