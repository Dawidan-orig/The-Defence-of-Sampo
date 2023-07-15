using System;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(AttackCatcher))]
public class SwordFighter : MonoBehaviour
// Управляет мечом, который вертится в воздухе перед объектом.
{
    [Header("constraints")]
    public float actionSpeed = 10; // Скорость движения меча в руке
    public float swing_EndDistanceMultiplier = 2; // Насколько далеко должен двинуться меч после отбивания.
    public float swing_startDistance = 2; // Насколько далеко должен двинуться меч до удара.
    public float criticalImpulse = 200; // Лучше увернуться, чем отбить объект с импульсом больше этого!
    public float bladeMaxDistance = 2; // Максимальное расстояние от vital до рукояти меча. По сути, длина руки.
    public float bladeMinDistance = 0.1f; // Минимальное расстояние от vital.
    public float close_enough = 0.1f; // Расстояние до цели, при котором можно менять состояние.
    public float toInitialAwait = 2; // Сколько времени ожидать до установки меча в обычную позицию?

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
    float currentToInitialAwait;
    [SerializeField]
    Rigidbody lastIncoming = null;

    [Header("Debug")]
    public bool isSwordFixing = true;

    void Start()
    {
        AttackCatcher catcher = gameObject.GetComponent<AttackCatcher>();
        catcher.OnIncomingAttack += Incoming;
        catcher.ignored.Add(blade.body);

        //currentToInitialAwait = toInitialAwait;

        blade.SetHost(gameObject);

        desireBlade.gameObject.SetActive(true);
        desireBlade.position = bladeHandle.position;
        desireBlade.rotation = bladeHandle.rotation;

        GameObject initialBladeGO = new GameObject("InititalBladePosition");
        initialBlade = initialBladeGO.transform;
        initialBlade.position = bladeHandle.position;
        initialBlade.rotation = bladeHandle.rotation;
        initialBlade.parent = transform;
    }

    private void FixedUpdate()
    {
        if (!isSwinging)
        {
            if (Vector3.Distance(bladeHandle.position, desireBlade.position) < close_enough)
            {   
                if (currentToInitialAwait < toInitialAwait)
                    currentToInitialAwait += Time.fixedDeltaTime;
                else
                {
                    if(initialBlade.position != desireBlade.position)
                        SetDesires(initialBlade.position, initialBlade.up, initialBlade.forward);                    
                }
            }
            
            Control_MoveSword();
        }
        else
            Control_SwingSword();

        if(isSwordFixing)
            Control_FixSword();
    }

    private void Incoming(object sender, AttackCatcher.AttackEventArgs e)
    {
        Rigidbody currentIncoming = e.body;
        currentToInitialAwait = 0;

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
                    Swing(e.start);
                }
                else // Подготовим удар
                {
                    //...Если в этом есть смысл
                    if (Vector3.Distance(e.start, bladeCenter) < swing_startDistance)
                    {
                        SetDesires(e.start + (bladeCenter - e.start).normalized * swing_startDistance,
                            (bladeCenter - vital.bounds.center).normalized,
                            (e.start - bladeHandle.position).normalized,
                            nullifyProgress: true);
                    }
                    //TODO : Поворот ВСЕГО ТЕЛА к приближающемуся объекту. Это не должен делать SwordFighter - он управляет только рукой с мечом.
                }
            }
            else
            {
                // Evade() -- Должен быть в другом скрипте, тут - только вызов
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

            // Синхронизация для параллельности vital
            //bladePrediction.transform.rotation = Quaternion.FromToRotation((end.transform.position - start.transform.position).normalized, transform.up);
          
            Vector3 toEnemyBlade_Dir = (bladePrediction.transform.position - vital.bounds.center).normalized;
            bladePrediction.transform.Rotate(toEnemyBlade_Dir, 90);

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
                return;
            }

            Block(bladeDown, bladeUp, toEnemyBlade_Dir, currentIncoming != lastIncoming);
        }

        lastIncoming = currentIncoming;
    }

    // Атака оружием по какой-то точке из текущей позиции.
    private void Swing(Vector3 toPoint)
    {
        isSwinging = true;
        
        Vector3 moveTo = toPoint + (toPoint - bladeHandle.position).normalized * swing_EndDistanceMultiplier;    

        Vector3 pointDir = (moveTo - vital.bounds.center).normalized;

        // Притягиваем ближе к vital
        float distance = (toPoint - vital.ClosestPointOnBounds(toPoint)).magnitude;
        moveTo = vital.ClosestPointOnBounds(moveTo) + (moveTo - vital.ClosestPointOnBounds(moveTo)).normalized * distance;
        
        SetDesires(moveTo, pointDir, (moveTo - toPoint).normalized);
    }

    // Установка меча по всем возможным параметрам
    private void Block(Vector3 start, Vector3 end, Vector3 SlashingDir, bool nullifyProgress = false)
    {
        SetDesires(start, (end - start).normalized, SlashingDir, nullifyProgress);
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
        GameObject go = new();
        Transform probe = go.transform;
        probe.position = bladeHandle.position;
        probe.rotation = desireBlade.rotation;
        probe.parent = null;

        bladeHandle.rotation = Quaternion.Lerp(bladeHandle.rotation, transform.rotation * probe.rotation, moveProgress);

        Destroy(go);
        #endregion

        Vector3 closestPos = vital.ClosestPointOnBounds(bladeHandle.position);
        const float TWO_DIVIDE_THREE = 2/3;
        
        if(moveProgress < TWO_DIVIDE_THREE) // Притягиваем максимально близко
        {
            desireBlade.position = closestPos + (desireBlade.position - closestPos).normalized * bladeMinDistance;

            GameObject upDirectioner = new();
            Vector3 toNearest = closestPos - desireBlade.position;
            upDirectioner.transform.up = toNearest;
            upDirectioner.transform.Rotate(0,0,90);
            desireBlade.up = upDirectioner.transform.up;
            Destroy(upDirectioner);
        }

        // На будущее:
        //TODO : Переставить desire, чтобы на пути до него не было всяких препятствий.
        //TODO : Добавить событий на начало движения меча, состояние в прогрессе и конец движения.
    }

    private void Control_SwingSword() 
    {
        //TODO : Если "Жёсткая" коллизия при swing (Удар обо что-то без Rigidbody (Стена, например)) - отменяем нафиг.

        if (Vector3.Distance(bladeHandle.position, desireBlade.position) < close_enough)
            isSwinging = false;

        if (moveProgress < 1)
            moveProgress += actionSpeed * Time.fixedDeltaTime;

        float heightFrom = bladeHandle.position.y;
        float heightTo = desireBlade.position.y;

        Vector3 from = new Vector3(bladeHandle.position.x, 0, bladeHandle.position.z);
        Vector3 to = new Vector3(desireBlade.position.x, 0, desireBlade.position.z);

        bladeHandle.position = Vector3.Slerp(from, to, moveProgress) + new Vector3(0, Mathf.Lerp(heightFrom, heightTo, moveProgress), 0);

        bladeHandle.LookAt((desireBlade.position - bladeHandle.position).normalized, (bladeHandle.position - vital.bounds.center).normalized);    
    }

    private void Control_FixSword()
    {
        // Притягиваем меч ближе
        Vector3 closestPos = vital.bounds.center;
        if (Vector3.Distance(bladeHandle.position, closestPos) > bladeMaxDistance)
            bladeHandle.position = closestPos + (bladeHandle.position - closestPos).normalized * bladeMaxDistance;

        //И Desire-позицию тоже
        if (Vector3.Distance(desireBlade.position, closestPos) > bladeMaxDistance)
            desireBlade.position = closestPos + (desireBlade.position - closestPos).normalized * bladeMaxDistance;

        //Аналогичным образом отталкиваем
        if (Vector3.Distance(bladeHandle.position, closestPos) < bladeMinDistance)
            bladeHandle.position = closestPos + (bladeHandle.position - closestPos).normalized * bladeMinDistance;

        //И Desire-позицию тоже
        if (Vector3.Distance(desireBlade.position, closestPos) < bladeMinDistance)
            desireBlade.position = closestPos + (desireBlade.position - closestPos).normalized * bladeMinDistance;
    }

    private void SetDesires(Vector3 pos, Vector3 dir, bool nullifyProgress = false) 
    {
        desireBlade.position = pos;
        desireBlade.up = dir;
        if(moveProgress > 1)
            moveProgress = 0;

        if (nullifyProgress)
            moveProgress = 0;
    }

    private void SetDesires(Vector3 pos, Vector3 up, Vector3 forward, bool nullifyProgress = false)
    {
        desireBlade.position = pos;
        desireBlade.LookAt(pos + forward, up);

        if (moveProgress > 1)
            moveProgress = 0;

        if (nullifyProgress)
            moveProgress = 0;
    }
}