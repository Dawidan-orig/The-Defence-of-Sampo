using UnityEngine;

public class MeleeFighter : TargetingUtilityAI
{
    //ƒобавить сюда использование кулаков
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

    protected override Tool ToolChosingCheck(Transform target)
    {
        return weapon;
    }

    public override Transform GetRightHandTarget()
    {
        return weapon.rightHandHandle;
    }

    public virtual void Swing(Vector3 toPoint) 
    {
        _swingReady = false;

        Invoke(nameof(BecomeReadyToSwing), weapon.cooldownBetweenAttacks);
    }

    public void BecomeReadyToSwing()
    {
        _swingReady = true;
    }

    public virtual void Block(Vector3 start, Vector3 end, Vector3 SlashingDir) { }
}
