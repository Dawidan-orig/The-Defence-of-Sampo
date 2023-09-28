using UnityEngine;

[RequireComponent(typeof(Movement))]
public class MovingAgent : MonoBehaviour
{
    public float walkToTargetDist = 5; // Дистанция, меньше которой агент будет двигаться со скоростью ходьбы.
    public float runToTargetDist = 30; // Дистанция, меньше которой агент будет бежать.

    public float angularRotatingSpeed = 360;

    public float wallHeight = 1;
    public float edgeDistance = 2;
    public float edgeDepth = 1;

    public LayerMask terrainMask;

    private Vector3 desireLookDir;
    private Transform countFrom;

    Movement movement;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        countFrom = transform;
        if (TryGetComponent(out TargetingUtilityAI ai))
            countFrom = ai.navMeshCalcFrom;
    }

    private void Start()
    {
        if (runToTargetDist < walkToTargetDist)
            runToTargetDist = walkToTargetDist;

        desireLookDir = transform.forward;
        desireLookDir.y = 0;
    }

    private void FixedUpdate()
    {
        Vector3 newDir = Vector3.RotateTowards(transform.forward,desireLookDir, Time.fixedDeltaTime * angularRotatingSpeed * Mathf.Deg2Rad, 1);
        newDir = newDir.normalized;

        transform.LookAt(transform.position + newDir, Vector3.up);
    }

    public void MoveIteration(Vector3 newPos) 
    {       
        Vector3 dir = (newPos - countFrom.position).normalized;
        dir.y = 0;
        MoveIteration(newPos, newPos+ dir);       
    }

    public void MoveIteration(Vector3 newPos, Vector3? lookPos = null)
    {
        Vector3 dir = (newPos - countFrom.position).normalized;
        dir.y = 0;

        if (lookPos != null)
        {
            Vector3 lookDir = (lookPos.Value - countFrom.position).normalized;
            lookDir.y = 0;
            desireLookDir = lookDir;
        }
        dir = Quaternion.Inverse(countFrom.rotation) * dir;
        Vector2 input = new Vector2(dir.z, dir.x);

        Debug.DrawLine(countFrom.position, newPos);

        if (Vector3.Distance(newPos, countFrom.position) < walkToTargetDist)
            movement.PassInput(input, Movement.SpeedType.walk, false);
        else if (Vector3.Distance(newPos, countFrom.position) < runToTargetDist)
            movement.PassInput(input, Movement.SpeedType.run, false);
        else
            movement.PassInput(input, Movement.SpeedType.sprint, false);
    }

    public bool IsNearObstacle(Vector3 desiredMovement,out Vector3 obstacleNormal) 
    {
        Vector3 bottom = countFrom.position + Vector3.down* transform.GetComponent<AliveBeing>().vital.bounds.size.y / 2;

        Vector3 edgeFlatPoint = bottom + desiredMovement.normalized * edgeDistance;
        Vector3 wallHeightPoint = edgeFlatPoint + Vector3.up * wallHeight;
        Vector3 edgeDepthPoint = edgeFlatPoint + Vector3.down * edgeDepth;

        bool wallHit = Utilities.VisualisedRaycast(bottom,
            (wallHeightPoint - bottom).normalized,
            out RaycastHit wall,
            (wallHeightPoint - bottom).magnitude,
            terrainMask);

        bool stepFloorHit = Utilities.VisualisedRaycast(wallHeightPoint,
            (edgeDepthPoint - wallHeightPoint).normalized,
            out _,
            (edgeDepthPoint - wallHeightPoint).magnitude,
            terrainMask);
        

        Utilities.VisualisedRaycast(edgeDepthPoint,
            (bottom - edgeDepthPoint + Vector3.down * edgeDepth).normalized,
            out RaycastHit edge,
            (bottom - edgeDepthPoint + Vector3.down * edgeDepth).magnitude,
            terrainMask);

            obstacleNormal = (wall.normal == null ? edge.normal : wall.normal);
        

        return wallHit || !stepFloorHit;
    }
}
