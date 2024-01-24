using Sampo.AI;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PhysicalNMAgent : MonoBehaviour, IMovingAgent
    // Синхронизирует NavMeshAgent и Rigidbody
{
    [Tooltip("Высота стены, когда её уже следует избегать, чтобы не застрять")]
    public float wallHeight = 1;
    [Tooltip("Длина от края, который лучше избегать")]
    public float edgeDistance = 2;
    [Tooltip("Глубина вниз от края, когда его следует избегать")]
    public float edgeDepth = 1;
    public LayerMask terrainMask;

    Vector3 IMovingAgent.DesireLookDir => desireLookDir;
    Transform IMovingAgent.CountFrom => countFrom;

    public MonoBehaviour Component => this;

    private Vector3 desireLookDir;
    private Transform countFrom;
    private NavMeshAgent agent;
    private Rigidbody rb;

    private NavMeshPath savedPath;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        desireLookDir = transform.forward;
        desireLookDir.y = 0;

        countFrom = transform;
        if (TryGetComponent(out TargetingUtilityAI ai) && ai.navMeshCalcFrom)
            countFrom = ai.navMeshCalcFrom;

        agent.autoRepath = false;
    }

    private void Update()
    {
        if (agent.isOnNavMesh && savedPath != null)
        {
            agent.SetPath(savedPath);
            savedPath = null;
        }
    }

    private void FixedUpdate()
    {
        if(!agent.enabled) 
        {
            if(Physics.Raycast(transform.position, Vector3.down, 1, terrainMask)) 
            {
                ResetAgent();
            }
        }
    }

    private void OnDisable()
    {
        DisableAgent();
    }

    private void OnEnable()
    {
        ResetAgent();
    }

    void IMovingAgent.ExternalForceMacros()
    {
        DisableAgent();
    }

    void DisableAgent() 
    {
        rb.isKinematic = false;
        agent.enabled = false;
    }

    void ResetAgent() 
    {
        rb.isKinematic = true;
        agent.enabled = true;
    }

    public void MoveIteration(Vector3 newPos)
    {
        Vector3 dir = (newPos - countFrom.position).normalized;
        dir.y = 0;
        MoveIteration(newPos, newPos + dir);
    }

    public void MoveIteration(Vector3 newPos, Vector3 lookPos)
    {
        Vector3 dir = (newPos - countFrom.position).normalized;
        dir.y = 0;

        if (lookPos != null)
        {
            Vector3 lookDir = (lookPos - countFrom.position).normalized;
            lookDir.y = 0;
            desireLookDir = lookDir;
            Quaternion rotation = Quaternion.LookRotation(lookDir);
            //Да, знаю, что так использовать Lerp не хорошо.
            // Но в данном случае это очень простой способ, а большего и не нужно от агента.
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, agent.angularSpeed / 360);
        }

        Utilities.DrawArrow(transform.position, newPos,0, Color.blue);

        if(agent.isOnNavMesh)
            agent.destination = newPos;
    }

    public void PassPath(NavMeshPath path)
    {
        if (agent.isOnNavMesh)
            agent.SetPath(path);
        else        
            savedPath = path;
        
    }

    public bool IsNearObstacle(Vector3 desiredMovement, out Vector3 obstacleNormal)
    {
        Vector3 bottom = countFrom.position + Vector3.down * transform.GetComponent<AliveBeing>().vital.bounds.size.y / 2;

        Vector3 edgeFlatPoint = bottom + desiredMovement.normalized * edgeDistance;
        Vector3 wallHeightPoint = edgeFlatPoint + Vector3.up * wallHeight;
        Vector3 edgeDepthPoint = edgeFlatPoint + Vector3.down * edgeDepth;

        bool wallHit = Physics.Raycast(bottom,
            (wallHeightPoint - bottom).normalized,
            out RaycastHit wall,
            (wallHeightPoint - bottom).magnitude,
            terrainMask);

        bool stepFloorHit = Physics.Raycast(wallHeightPoint,
            (edgeDepthPoint - wallHeightPoint).normalized,
            out _,
            (edgeDepthPoint - wallHeightPoint).magnitude,
            terrainMask);


        Physics.Raycast(edgeDepthPoint,
            (bottom - edgeDepthPoint + Vector3.down * edgeDepth).normalized,
            out RaycastHit edge,
            (bottom - edgeDepthPoint + Vector3.down * edgeDepth).magnitude,
            terrainMask);

        obstacleNormal = (wall.normal == null ? edge.normal : wall.normal);


        return wallHit || !stepFloorHit;
    }
}
