using UnityEngine;

public class MeleeFighter : TargetingUtilityAI
{
    public MeleeTool weapon;

    [SerializeField]
    protected bool _swingReady = true;

    public bool SwingReady { get => _swingReady; set => _swingReady = value; }

    protected override void Start()
    {
        base.Start();

        if (weapon == null)
        {
            weapon = hands;
        }
    }
    public override void AttackUpdate(Transform target)
    {

    }

    public void ReadyToSwing() 
    {
        _swingReady = true;
    }

    protected override void DistributeActivityFromManager(object sender, UtilityAI_Manager.UAIData e)
    {
        base.DistributeActivityFromManager(sender, e);
    }

    public override Transform GetRightHandTarget()
    {
        return weapon.rightHandHandle;
    }

    public virtual void Swing(Vector3 toPoint) 
    {
        if (!_swingReady)
            return;

        _swingReady = false;

        Invoke(nameof(ReadyToSwing), weapon.cooldownBetweenAttacks);
    }

    public virtual void Block(Vector3 start, Vector3 end, Vector3 SlashingDir) { }
}
