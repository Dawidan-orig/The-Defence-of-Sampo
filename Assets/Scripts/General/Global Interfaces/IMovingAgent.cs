using UnityEngine;
using UnityEngine.AI;

public interface IMovingAgent
{
    protected abstract Vector3 DesireLookDir { get; }
    protected abstract Transform CountFrom { get; }
    public abstract MonoBehaviour Component { get;}
    public virtual void ExternalForceMacros() { }
    public abstract void PassPath(NavMeshPath path);
    public abstract void MoveIteration(Vector3 newPos);
    public abstract void MoveIteration(Vector3 newPos, Vector3 lookPos);
    public abstract bool IsNearObstacle(Vector3 desiredMovement, out Vector3 obstacleNormal);
}
