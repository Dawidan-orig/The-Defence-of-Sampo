using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class HumanBodyControl : MonoBehaviour
{
    public Transform mainBody;
    public float fallAnimation_TransitionSpeed = 10;
    public float jumpAnimation_TransitionSpeed = 10;
    public Transform lookTarget;
    public Transform rightHandTarget;
    public Transform shouldersTarget;
    public Transform rightShoulder;
    public Animator controlled;

    [Header("offsets")]
    public Vector3 rightHandOffset = new Vector3(-0.05f,0, 0.12f);

    [Header("Constraints")]
    public MultiAimConstraint HeadLookAt;
    public TwoBoneIKConstraint firstIK;
    public TwoBoneIKConstraint recalculationIK;
    public TwistCorrection shoulders;

    [Header("lookonly")]
    [SerializeField]
    private Vector3 _speedToPass;
    [SerializeField]
    private float _airAnimationProgress = 0;
    [SerializeField]
    private float _jumpAnimationProgress = 0;

    private IAnimationProvider provider;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private Vector3 lastPos;
    private int idleAnimationHash;
    private int moveAnimationHash;
    private bool firstPassInIdle = true;
    private bool lastFrameWasJump = false;

    private Quaternion initialShoulders;
    private Quaternion initialRightShoulder;

    private void Awake()
    {
        agent = mainBody.GetComponent<NavMeshAgent>();
        rb = mainBody.GetComponent<Rigidbody>();

        provider = mainBody.GetComponent<IAnimationProvider>();
    }

    private void Start()
    {
        lastPos = transform.position;
        idleAnimationHash = Animator.StringToHash("BasicMotions@Idle01");
        moveAnimationHash = Animator.StringToHash("Moving");

        initialShoulders = shouldersTarget.localRotation;
        initialRightShoulder = rightShoulder.localRotation;
    }

    private void Update()
    {
        HeadControl();
        RightHandControl();
        ShouldersControl();

        if (agent)
            SetAnimatorFromNavMesh();
        else if (rb)
            SetAnimatorFromRigidbody();
        else
            SetAnimatorByDifference();
    }

    private void HeadControl() 
    {
        Vector3 lookTo = provider.GetLookTarget();
        if (lookTo != Vector3.zero)
            lookTarget.position = lookTo;
        else
            HeadLookAt.weight = 0;
    }

    private void RightHandControl() 
    {
        if(provider.GetRightHandTarget() == null)
        {
            firstIK.weight = 0;
            recalculationIK.weight = 0;
            return;
        }

        firstIK.weight = 1;
        recalculationIK.weight = 1;        

        Vector3 rHandPos = provider.GetRightHandTarget().position - transform.rotation* rightHandOffset;
        Quaternion rHandRot = provider.GetRightHandTarget().rotation;
        rHandRot *= Quaternion.Euler(90f, 90f, 0);
        rHandRot *= Quaternion.Euler(180, 0, 180);

        if (rHandPos != Vector3.zero)
        {
            rightHandTarget.position = rHandPos;
            rightHandTarget.rotation = rHandRot;
        }
        else
        {
            firstIK.weight = 0;
            recalculationIK.weight = 0;
        }
    }

    private void ShouldersControl() 
    {
        Quaternion delta = rightShoulder.localRotation * Quaternion.Inverse(initialRightShoulder);

        shouldersTarget.rotation = transform.rotation * initialShoulders * delta;
    }

    private void SetAnimatorFromRigidbody()
    {
        _speedToPass = rb.velocity;
        _speedToPass.y = 0;
        _speedToPass = Quaternion.FromToRotation(transform.forward, _speedToPass.normalized) * Vector3.forward * _speedToPass.magnitude;

        SetAllVarsAnimator();
    }

    private void SetAnimatorFromNavMesh()
    {
        _speedToPass = agent.velocity;
        _speedToPass.y = 0;

        _speedToPass = Quaternion.FromToRotation(transform.forward, _speedToPass.normalized) * Vector3.forward * _speedToPass.magnitude;

        SetAllVarsAnimator();
    }

    private void SetAnimatorByDifference()
    {
        _speedToPass = (transform.position - lastPos) / Time.deltaTime;
        _speedToPass.y = 0;
        _speedToPass = Quaternion.FromToRotation(transform.forward, _speedToPass.normalized) * Vector3.forward * _speedToPass.magnitude;

        SetAllVarsAnimator();

        lastPos = transform.position;
    }

    private void SetAllVarsAnimator()
    {
        // IDEA : �������� ���� Fall ��������, � ������� ��������� ��� ����������, ��������� � �������������� ���� BlendTree

        controlled.SetFloat("jump", 0);

        if (provider.IsInJump() && !lastFrameWasJump)
        {
            // Jump start
            controlled.SetFloat("jump", 1); // ������ Bool ��� ������ �������� � BlendTree � ���������. TODO : ��������� �����.
            _jumpAnimationProgress = 0;
        }
        else if (provider.IsInJump() && lastFrameWasJump)
        {
            controlled.SetFloat("jump", 1);
            // Jump process
            if (_jumpAnimationProgress < 0.5f)
                _jumpAnimationProgress += Time.deltaTime * jumpAnimation_TransitionSpeed;
            else _jumpAnimationProgress = 0.5f;
        }

        if (!provider.IsGrounded() && _airAnimationProgress < 1)
        {
            _airAnimationProgress += Time.deltaTime * fallAnimation_TransitionSpeed;
        }
        else if (provider.IsGrounded() && _airAnimationProgress > 0)
        {
            _airAnimationProgress -= Time.deltaTime * fallAnimation_TransitionSpeed;
        }


        if (Utilities.ValueInArea(_speedToPass, Vector3.zero, 0.01f) && !firstPassInIdle && _airAnimationProgress <= 0)
        {
            controlled.Play(idleAnimationHash);
            firstPassInIdle = true;
        }
        else if (!Utilities.ValueInArea(_speedToPass, Vector3.zero, 0.01f) || _airAnimationProgress > 0)
        {

            firstPassInIdle = false;
            controlled.SetFloat("speedX", _speedToPass.x);
            controlled.SetFloat("speedZ", _speedToPass.z);
            controlled.SetFloat("speedMag", _speedToPass.magnitude);
            controlled.SetFloat("airBlend", _airAnimationProgress);
            controlled.SetFloat("jumpBlend", _jumpAnimationProgress);
            controlled.Play(moveAnimationHash);
        }

        lastFrameWasJump = provider.IsInJump();
    }
}
