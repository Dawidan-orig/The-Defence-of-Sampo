using UnityEngine;
using UnityEngine.AI;

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
    private int idleHash;
    private int moveHash;
    private bool firstPassInIdle = true;
    private bool lastFrameWasJump = false;

    private Vector3 nullLook;
    private Vector3 nullRightHand;
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
        idleHash = Animator.StringToHash("BasicMotions@Idle01");
        moveHash = Animator.StringToHash("Moving");

        initialShoulders = shouldersTarget.localRotation;
        initialRightShoulder = rightShoulder.localRotation;
        nullLook = lookTarget.position - transform.position;
        nullRightHand = rightHandTarget.position - transform.position;
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
            lookTarget.position = transform.position + transform.rotation * nullLook;//TODO : Поменять на смену weight на ноль.
    }

    private void RightHandControl() 
    {
        Vector3 rHandPos = provider.GetRightHandTarget();

        if (rHandPos != Vector3.zero)
            rightHandTarget.position = rHandPos;
        else
            rightHandTarget.position = transform.position + transform.rotation * nullRightHand; // TODO : Поменять на смену weight на ноль.
    }

    private void ShouldersControl() 
    {
        Quaternion delta = rightShoulder.localRotation * Quaternion.Inverse(initialRightShoulder);

        shouldersTarget.rotation = initialShoulders * delta;
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
        // IDEA : Добавить сюда Fall анимацию, с которой произошёл ряд трудностей, связанных с использованием мной BlendTree

        controlled.SetFloat("jump", 0);

        if (provider.IsInJump() && !lastFrameWasJump)
        {
            // Jump start
            controlled.SetFloat("jump", 1); // Вместо Bool для выбора анимации в BlendTree в аниматоре. TODO : Придумать лучше.
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
            controlled.Play(idleHash);
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
            controlled.Play(moveHash);
        }

        lastFrameWasJump = provider.IsInJump();
    }
}
