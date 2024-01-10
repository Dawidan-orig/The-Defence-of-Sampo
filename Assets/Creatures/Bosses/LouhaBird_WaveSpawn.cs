using System.Collections.Generic;
using UnityEngine;

public class LouhaBird_WaveSpawn : MonoBehaviour
// Этот компонент отвечает за управление Птицы Лоухи, когда она ещё не является боссом и непосредственно не сражается.
// Она просто раскидывает волны.
{
    public Transform leftCornerSpawnLocation;
    public Transform rightCornerSpawnLocation;
    public float timeToNewWave = 100;
    public float offsetFromBorder = 25;

    [Header("Lookonly")]
    [SerializeField]
    private bool _newWaveTimerExpired = true;
    [SerializeField]
    private float _currentAwaitedToNewWave = 0;


    [Header("Movement")]
    [SerializeField]
    private GenericObjectPair<Vector3>? currentMovement;
    [SerializeField]
    private Stack<GenericObjectPair<Vector3>> moves = new();
    [SerializeField]
    private Vector3 toStartMovementSequence;
    [SerializeField]
    MovingAgent agent;
    [SerializeField]
    float lastSpawnProgress = 0;

    private void Awake()
    {
        agent = GetComponent<MovingAgent>();
    }

    private void Update()
    {
        if (_newWaveTimerExpired && moves.Count == 0 && currentMovement == null) // Состояние: Стоим idle
        {
            _currentAwaitedToNewWave = 0;
            _newWaveTimerExpired = false;
            InitiateWave();
        }
        else if (!_newWaveTimerExpired)
        {
            _currentAwaitedToNewWave += Time.deltaTime;
            if (_currentAwaitedToNewWave > timeToNewWave)
            {
                _newWaveTimerExpired = true;
            }
        }
    }

    private void FixedUpdate()
    {
        PerformMove();
    }

    private void PerformMove()
    {
        const float CLOSE_ENOUGH = 2f;
        Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);

        // Выбираем движение к первой точке, это операция на один кадр.
        if (toStartMovementSequence == Vector3.zero && currentMovement == null && moves.Count != 0)
        {
            currentMovement = moves.Pop();
            if (!Utilities.ValueInArea(currentMovement.Value.From, flatPos, CLOSE_ENOUGH))
                toStartMovementSequence = currentMovement.Value.From;
        }
        else if (toStartMovementSequence == Vector3.zero && currentMovement != null)
        {
            Moving_Chain(flatPos, CLOSE_ENOUGH);
        }

        if (toStartMovementSequence != Vector3.zero)
        {
            Moving_First(flatPos, CLOSE_ENOUGH);
        }
        else if (currentMovement == null && moves.Count == 0)
        {
            Moving_Idle();
        }
    }

    private void Moving_First(Vector3 flatPos, float closeEnoughtDistance)
    {
        toStartMovementSequence.y = 0;

        if (!Utilities.ValueInArea(flatPos, toStartMovementSequence, closeEnoughtDistance))
            agent.MoveIteration(toStartMovementSequence);
        else
            toStartMovementSequence = Vector3.zero;
    }
    private void Moving_Chain(Vector3 flatPos, float closeEnoughtDistance)
    {
        if (!Utilities.ValueInArea(flatPos, currentMovement.Value.To, closeEnoughtDistance))
        {
            agent.MoveIteration(currentMovement.Value.To);

            Vector3 relativePos = currentMovement.Value.From - transform.position;
            // Vector lerp
            float flightProgress = (currentMovement.Value.To - currentMovement.Value.From).magnitude / relativePos.magnitude;
            if (flightProgress > 1) // Если так вышло, что сбились с курса и текущая точка чёрт знает где
                flightProgress = 1 / flightProgress;

            float progressFraction = 1 / (float)WaveHandler.Instance.GetAmountOfUnitsToSpawn();
            if (flightProgress > lastSpawnProgress + progressFraction)
            {
                GameObject unit = WaveHandler.Instance.GetSpawnedUnit(transform.position, GetComponent<Faction>().FactionType, transform.rotation);
                unit.GetComponent<Rigidbody>().AddForce(Vector3.up * 10);
                lastSpawnProgress = flightProgress;
            }
        }
        else
        {
            lastSpawnProgress = 0;
            currentMovement = null;
        }
    }
    private void Moving_Idle()
    {
        
    }

    private void InitiateWave()
    {
        Vector3 center = Vector3.Lerp(leftCornerSpawnLocation.position,rightCornerSpawnLocation.position,0.5f);
        center.y = 0;

        Vector3 rectPointFrom = leftCornerSpawnLocation.position;
        rectPointFrom = rectPointFrom +(center - rectPointFrom).normalized * offsetFromBorder;
        Vector3 rectPointTo =rightCornerSpawnLocation.position;
        rectPointTo = rectPointTo +(center - rectPointTo).normalized * offsetFromBorder;

        Vector3 diagonalRelative = rectPointTo - rectPointFrom;

        GenericObjectPair<Vector3>[] edges = new GenericObjectPair<Vector3>[]
        {
            new GenericObjectPair<Vector3> (rectPointFrom + diagonalRelative.x * Vector3.right, rectPointFrom + diagonalRelative),
            new GenericObjectPair<Vector3> (rectPointFrom + diagonalRelative, rectPointFrom + diagonalRelative.z * Vector3.forward),
            new GenericObjectPair<Vector3> (rectPointFrom + diagonalRelative.z * Vector3.forward, rectPointFrom),
            new GenericObjectPair<Vector3> (rectPointFrom, rectPointFrom + diagonalRelative.x * Vector3.right)
        };

        GenericObjectPair<Vector3> actualMovement = new();
        int chosenEdge = UnityEngine.Random.Range(0, 4);
        actualMovement.From = Vector3.Lerp(edges[chosenEdge].From, edges[chosenEdge].To, UnityEngine.Random.Range(0f, 1f));
        int anotherEdge = UnityEngine.Random.Range(0, 4);
        anotherEdge = anotherEdge == chosenEdge ? (anotherEdge + 2) % 4 : anotherEdge;
        actualMovement.To = Vector3.Lerp(edges[anotherEdge].From, edges[anotherEdge].To, UnityEngine.Random.Range(0f, 1f));

        moves.Push(actualMovement);

        WaveHandler.Instance.UsePrefabPallete();
    }
}
