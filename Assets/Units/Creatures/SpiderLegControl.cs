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

        if (Physics.Raycast(transform.position, (transform.rotation * Quaternion.Inverse(initialRot)) * toLegDir, out RaycastHit hit, legLength, walkable))
        {
            if (Vector3.Distance(hit.point, legTarget.position) > distanceToNew)
                readyToMove = true;

            if (Vector3.Distance(hit.point, legTarget.position) > maxDistToNew && !desireSet || moveCommand)
            {
                desire = hit.point;
                progress = 0;
                desireSet = true;
                moveCommand = false;
            }
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

        legTarget.position = Vector3.up * legMovement.Evaluate(progress) + Vector3.Lerp(legTarget.position, desire, progress);
    }
}
