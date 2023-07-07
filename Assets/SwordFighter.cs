using System;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(AttackCatcher))]
public class SwordFighter : MonoBehaviour
// Управляет мечом, который вертится в воздухе перед объектом.
{
    [Header("constraints")]
    public float actionSpeed = 10; // Скорость движения меча в руке
    public float angluarSpeed = 10; // Скорость поворота тела
    public float swingDistanceMultiplier = 2; // Насколько далеко должен двинуться меч после отбивания.
    public float startSwingDistance = 2; // Насколько далеко должен двинуться меч до удара.
    public float criticalImpulse = 200; // Лучше увернуться, чем отбить объект с импульсом больше этого!
    public float bladeMaxDistance = 2; // Максимальное расстояние от vital до рукояти меча. По сути, длина руки.
    public float close_enough = 0.1f; // Расстояние до цели, при котором можно менять состояние.    

    [Header("init-s")]
    public Blade blade;
    public Transform bladeHandle;
    public Collider vital;
    public Transform desireBlade;

    [Header("lookonly")]
    [SerializeField]
    Transform initialBlade;
    [SerializeField]
    float moveProgress = 0;
    [SerializeField]
    bool isSwinging = false;
    [SerializeField]
    bool isRepositioning = false;

    //TODO : Состояния. Всего два: Idle и Busy. Busy означает, что меч сейчас используется.

    void Start()
    {
        AttackCatcher catcher = gameObject.GetComponent<AttackCatcher>();
        catcher.OnIncomingAttack += Incoming;
        catcher.ignored.Add(blade.body);

        blade.SetHost(gameObject);

        desireBlade.gameObject.SetActive(true);
        desireBlade.position = bladeHandle.position;
        desireBlade.up = Vector3.up;
        desireBlade.forward = Vector3.forward;

        GameObject initialBladeGO = new GameObject();
        initialBlade = initialBladeGO.transform;
        initialBlade.position = desireBlade.position;
        initialBlade.rotation = desireBlade.rotation;
    }

    void Update()
    {
        // Чтобы при изменении в Editor'е работало, добавил это:
        if (desireBlade.hasChanged)
            SetDesires(desireBlade.position, desireBlade.up);
    }

    private void FixedUpdate()
    {
        //TODO : Чтобы не городить огороды со всякими isSwinging, есть смысл перейти на корутины для работы с мечами
        if (!isSwinging)
        {            
            if (Vector3.Distance(bladeHandle.position, desireBlade.position) < close_enough)
                SetDesires(initialBlade.position, initialBlade.up);
            
            Control_MoveSword();
        }
        else
            Control_SwingSword();
    }

    private void Incoming(object sender, AttackCatcher.AttackEventArgs e)
    {
        // Итак, в нас летит непонятно что.
        if (e.free)
        {
            // Это неконтроллируемый объект, который просто летит в нашу сторону. Либо отбить, либо увернуться!
            if (e.impulse < criticalImpulse && !isSwinging)
            {
                Vector3 bladeCenter = Vector3.Lerp(blade.upperPoint.position, blade.downerPoint.position, 0.5f);

                if (Vector3.Distance(vital.bounds.center, e.body.position) <
                    Vector3.Distance(vital.bounds.center, bladeCenter) + bladeMaxDistance) // Достаточно близко, чтобы бить.
                {
                    if (Vector3.Distance(e.start, bladeCenter) > startSwingDistance)
                        Swing(e.start);
                    else
                        Swing(e.start + (bladeCenter - e.start).normalized * startSwingDistance,to : e.start);
                }
            }
            else
            {// Evade() -- Должен быть в другом скрипте, тут - только вызов
            }    
        }
        else
        {
            Vector3 enemyBladeCenter = Vector3.Lerp(e.start, e.end, 0.5f); // Центр атаки, центр нашего меча должен быть в этой же точке.

            GameObject bladePrediction = new();
            bladePrediction.transform.position = enemyBladeCenter;

            GameObject start = new();
            start.transform.position = e.start;
            start.transform.parent = bladePrediction.transform;

            GameObject end = new();
            end.transform.position = e.end;
            end.transform.parent = bladePrediction.transform;

            //TODO : Это логика рапиры, из-за чего отбиваемое оружие "отражается".
            //bladePrediction.transform.Rotate(e.direction, 90);

            bladePrediction.transform.rotation = Quaternion.FromToRotation((end.transform.position - start.transform.position).normalized, transform.up);

            Vector3 closest = vital.ClosestPointOnBounds(enemyBladeCenter);            
            Vector3 toBlade_Dir = (bladePrediction.transform.position - closest).normalized;
            toBlade_Dir.y = 0;
            bladePrediction.transform.Rotate(toBlade_Dir, 90);

            Vector3 bladeDown = start.transform.position;
            Vector3 bladeUp = end.transform.position;

            Destroy(bladePrediction);
            int ignored = blade.gameObject.layer; // Для игнора лезвий при проверке.
            ignored = ~ignored;

            if (Physics.Raycast(bladeDown, bladeUp - bladeDown, (bladeDown - bladeUp).magnitude, ignored) // Снизу вверх
                ||
                Physics.Raycast(bladeUp, bladeDown - bladeUp, (bladeDown - bladeUp).magnitude, ignored) // Сверху вниз
                )
            {
                // Меч втыкается куда-то, игнорируем.
                //Debug.Log("returning, because precition in: " + hit.collider.transform.name + ", Layermask: " + ignored, hit.collider.gameObject);
                return;
            }            
            
            Block(bladeDown, bladeUp);
        }
    }

    // Атака оружием по какой-то точке из текущей позиции.
    private void Swing(Vector3 toPoint)
    {
        isSwinging = true;
        
        Vector3 moveTo = toPoint + (toPoint - bladeHandle.position).normalized * swingDistanceMultiplier;    

        Vector3 pointDir = (moveTo - vital.bounds.center).normalized;

        // Притягиваем ближе к vital
        float distance = (toPoint - vital.ClosestPointOnBounds(toPoint)).magnitude;
        moveTo = vital.ClosestPointOnBounds(moveTo) + (moveTo - vital.ClosestPointOnBounds(moveTo)).normalized * distance;
        
        SetDesires(moveTo, pointDir);
    }

    // Взмах мечом из точки в точку
    private void Swing(Vector3 from, Vector3 to) 
    {
        throw new NotImplementedException();

        // Выполняем взмах мечом до тех пор, пока не достигнем цели.
        isRepositioning = true;
        isSwinging = true;

        SetDesires(from, (from - vital.bounds.center).normalized); // На точку from, в направлении от vital.

        //TODO : Это более красивый вид взмаха, который будет применяться в обычном бою.
        // Полная копирка Nintendo Wii Sport Resort: Сначала меч максимально эффективно двигается на подходящую точку,
        // И уже оттуда взмахивается.
    }

    // Чёткая установка меча по двум точкам. Start - позиция, end - определяет направление меча.
    private void Block(Vector3 start, Vector3 end)
    {
        SetDesires(start, (end - start).normalized);
    }

    private void Control_MoveSword()
    {        
        if(moveProgress < 1)
            moveProgress += actionSpeed * Time.fixedDeltaTime;

        float heightFrom = bladeHandle.position.y;
        float heightTo = desireBlade.position.y;

        Vector3 from = new Vector3(bladeHandle.position.x, 0, bladeHandle.position.z);
        Vector3 to = new Vector3(desireBlade.position.x, 0, desireBlade.position.z);

        bladeHandle.position = Vector3.Slerp(from, to, moveProgress) + new Vector3(0, Mathf.Lerp(heightFrom, heightTo, moveProgress), 0);
        
        #region rotationControl;
        //TODO : Адаптировать поворот под moveProgress!
        GameObject go = new();
        Transform probe = go.transform;
        probe.position = bladeHandle.position;
        probe.rotation = bladeHandle.rotation;
        probe.parent = null;

        probe.LookAt(bladeHandle.position + desireBlade.up, Vector3.up);
        probe.Rotate(Vector3.right, 90);        

        // Если меч смотрит в сторону vital
        if (Vector3.Dot(probe.up, vital.bounds.center - probe.position) < 0)
        {            
            //probe.Rotate(probe.up,180);
        }

        bladeHandle.rotation = Quaternion.Lerp(bladeHandle.rotation, transform.rotation * probe.rotation, actionSpeed * Time.fixedDeltaTime);
        bladeHandle.LookAt(probe);

        Destroy(go);
        #endregion
        
        // Притягиваем меч ближе
        Vector3 closestPos = vital.bounds.center;
        if (Vector3.Distance(desireBlade.position, closestPos) > bladeMaxDistance)        
            desireBlade.position = closestPos + (desireBlade.position - closestPos).normalized * bladeMaxDistance;

        //TODO (На 01.07.2023) :
        // - Сделать перемещение меча в idle к initialPosition
        // - Пусть меч всё время "смотрит" верхом перпендикулярно ближайшей позиции к vital. Должно выглядеть красивенько.
        // - Это:
        //TODO : Пусть меч "Подтягивается" ближе во время перемещений. Это и выглядит естественнее, и быстрее!
        //TODO : Попробовать сделать не как в Lerp, а относительно Distance(current, desire) < CLOSE_ENOUGH. То-есть как в Control_SwingSword()
        // Итого три итерации: "Притягивание"; Потом движение; Потом "Отталкивание" с нужным поворотом.
        // И придётся отойти от логики Lerp'а. Мне нужен полный контроль.

        // На будущее:
        //TODO : Добавить событий на начало движения меча, состояние в прогрессе и конец движения.
    }

    private void Control_SwingSword() 
    {
        if (Vector3.Distance(bladeHandle.position, desireBlade.position) < close_enough)
            isSwinging = false;

        if (isRepositioning)
        {
            Control_MoveSword();
            if (Vector3.Distance(bladeHandle.position, desireBlade.position) < close_enough)
                isRepositioning = false;
            return;
        }

        if (moveProgress < 1)
            moveProgress += actionSpeed * Time.fixedDeltaTime;

        float heightFrom = bladeHandle.position.y;
        float heightTo = desireBlade.position.y;

        Vector3 from = new Vector3(bladeHandle.position.x, 0, bladeHandle.position.z);
        Vector3 to = new Vector3(desireBlade.position.x, 0, desireBlade.position.z);

        bladeHandle.position = Vector3.Slerp(from, to, moveProgress) + new Vector3(0, Mathf.Lerp(heightFrom, heightTo, moveProgress), 0);

        bladeHandle.up = (bladeHandle.position-vital.bounds.center).normalized * swingDistanceMultiplier;
        /*
        Vector3 closestPos = vital.bounds.center;
        if (Vector3.Distance(desireBlade.position, closestPos) > bladeMaxDistance)
            desireBlade.position = closestPos + (desireBlade.position - closestPos).normalized * bladeMaxDistance;
        */
    }

    private void SetDesires(Vector3 pos, Vector3 dir) 
    {
        desireBlade.position = pos;
        desireBlade.up = dir;
        moveProgress = 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(bladeHandle.position, 0.05f);
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f);
        Gizmos.DrawSphere(desireBlade.position, 0.05f);
        Gizmos.DrawRay(desireBlade.position, desireBlade.up.normalized * (blade.upperPoint.position - blade.downerPoint.position).magnitude);
    }
}