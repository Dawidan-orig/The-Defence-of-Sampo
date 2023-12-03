using UnityEngine;

[RequireComponent(typeof(Movement))]
public class PlayerController : MonoBehaviour, IAnimationProvider
{
    //TODO : Перенести в State Machine для более удобного контроля
    Movement movement;

    public Transform usedMainHand;
    public Transform handTarget;
    [Header("Weaponry")]
    [Header("MeleeWeapon")]
    public SwordControl swordControl;
    public float mouseDeltaForSwing = 80;
    public float reachLength = 1;
    public float castToWeaponSpaceK = 100;

    [Header("lookonly")]
    [SerializeField]
    Vector3 prevMouse;

    private void Awake()
    {
        movement = GetComponent<Movement>();
    }

    private void Update()
    {
        UpdateInput();

        if(TryGetComponent<SwordControl>(out var c)) 
        {
            swordControl = c;
            handTarget = c.bladeHandle;
        }
    }

    private void UpdateInput()
    {
        Vector2 input;
        input.x = Input.GetAxisRaw("Vertical");
        input.y = Input.GetAxisRaw("Horizontal");

        Movement.SpeedType type = Movement.SpeedType.walk;
        if (movement.IsGrounded && Input.GetKey(KeyCode.LeftShift))
            type = Movement.SpeedType.sprint;
        else if (movement.IsGrounded)
            type = Movement.SpeedType.run;

        movement.PassInputDirect(input, type, Input.GetKeyDown(KeyCode.Space) && movement.IsGrounded && movement.JumpReady);

        if (swordControl)
        {
            // Простое перемещение оружия
            if (Input.GetMouseButton(0) && Vector3.Distance(Input.mousePosition, prevMouse) < mouseDeltaForSwing)
            {
                swordControl.ApplyNewDesire(CastMouseToSwordSpace(),transform.up, transform.forward);

                prevMouse = Input.mousePosition;
            }
            // Взмах оружием
            else if (Input.GetMouseButton(0) && Vector3.Distance(Input.mousePosition, prevMouse) > mouseDeltaForSwing) 
            {
                Vector3 screenDeltaDir = (Input.mousePosition - prevMouse).normalized;

                Vector3 swingPos = usedMainHand.position + transform.rotation * (reachLength * screenDeltaDir);

                swordControl.Swing(swingPos);

                prevMouse = Input.mousePosition;
            }
            else if (Input.GetMouseButton(1)) 
            {
                swordControl.Block(CastMouseToSwordSpace(), transform.position + transform.forward * reachLength, transform.forward);

                prevMouse = Input.mousePosition;
            }
            else 
            {
                swordControl.ReturnToInitial();
                prevMouse = new Vector3(Screen.width/2,Screen.height/2);
            }
        }
    }

    private Vector3 CastMouseToSwordSpace() 
    {
        return CastMouseToSwordSpace(Input.mousePosition);
    }

    private Vector3 CastMouseToSwordSpace(Vector3 screenPos)
    {
        Vector3 centralize = (screenPos - new Vector3(Screen.width / 2, Screen.height / 2)) / castToWeaponSpaceK;
        Vector3 flatCast = transform.position + Camera.main.transform.forward * reachLength + transform.rotation * centralize;

        Vector3 res = usedMainHand.position + reachLength * (flatCast - usedMainHand.position).normalized;

        return res;
    }

    public Vector3 GetLookTarget()  
    {
        Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, out var res, 100, LayerMask.NameToLayer("Default"));
        return res.point;
    }

    public bool IsGrounded()
    {
        return movement.IsGrounded;
    }

    public bool IsInJump()
    { 
        return movement.InJump;
    }

    public Transform GetRightHandTarget()
    {
        return handTarget;
    }
}
