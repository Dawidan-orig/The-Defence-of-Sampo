using UnityEngine;

public class SpiderLegControl : MonoBehaviour
{
    public Transform legTarget;
    public Vector3 desire;
    public LayerMask walkable;
    public AnimationCurve legMovement;
    public AttackingLimb limb;
    public float distanceToNew = 1;
    public float moveSpeed = 2;
    public float maxDistToNew = 1;
    public float legLength = 18;
    public float noContactRaise = 5;

    public bool readyToMove { get; private set; } = false;
    public bool stable { get; private set; } = true;
    private bool moveCommand = false;

    [SerializeField]
    private Vector3 toLegDir;
    [SerializeField]
    private float progress;
    [SerializeField]
    private bool desireSet = false;
    [SerializeField]
    private Quaternion initialRot;

    private void Start()
    {
        toLegDir = (legTarget.position - transform.position).normalized;
        initialRot = transform.rotation;
        desire = legTarget.position;
        progress = 1;
    }

    void Update()
    {
        MoveToDesire();
        readyToMove = false;

        RaycastHit hitResult;
        Vector3 legRelativePoint = (transform.rotation * Quaternion.Inverse(initialRot)) * toLegDir * legLength;
        bool hitPersist = Physics.Raycast(transform.position + legRelativePoint,
            (Vector3.down * legLength - legRelativePoint).normalized, out hitResult,
            legLength, walkable);
        hitPersist = hitPersist || Physics.Raycast(transform.position, legRelativePoint, out hitResult, legLength, walkable);

        if (hitPersist)
        {
            if (Vector3.Distance(hitResult.point, legTarget.position) > distanceToNew)
                readyToMove = true;

            if (Vector3.Distance(hitResult.point, legTarget.position) > maxDistToNew && !desireSet || moveCommand)
            {
                desire = hitResult.point + (hitResult.point - legTarget.position).normalized * moveSpeed * Time.deltaTime;
                progress = 0;
                desireSet = true;
                moveCommand = false;
            }
        }
        else 
        {
            stable = false;
            desire = transform.position + legRelativePoint + Vector3.up * noContactRaise;
            progress = 0;
            desireSet = true;
        }
    }

    public void BeginMove()
    {
        moveCommand = true;
    }
    private void MoveToDesire()
    {
        if (progress <= 1)
        {
            progress += Time.deltaTime * moveSpeed;
            stable = false;
        }
        else
        {
            legTarget.position = desire;
            desireSet = false;
            stable = true;
        }

        legTarget.position = Vector3.up * legMovement.Evaluate(progress) + Vector3.Slerp(legTarget.position, desire, progress);
    }

    private void OnDrawGizmos()
    {
        Vector3 legRelativePoint = (transform.rotation * Quaternion.Inverse(initialRot)) * toLegDir * legLength;
        Utilities.VisualisedRaycast(transform.position + legRelativePoint,
            (Vector3.down * legLength - legRelativePoint).normalized, out _,
            legLength, walkable);
        Utilities.VisualisedRaycast(transform.position, legRelativePoint, out _, legLength, walkable);
    }
}
