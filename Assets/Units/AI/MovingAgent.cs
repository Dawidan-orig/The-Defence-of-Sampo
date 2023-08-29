using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class MovingAgent : MonoBehaviour
{
    public float walkToTargetDist = 5; // Дистанция, меньше которой агент будет двигаться со скоростью ходьбы.
    public float runToTargetDist = 30; // Дистанция, меньше которой агент будет бежать.

    public float angularRotatingSpeed = 360;

    private Vector3 desireLookDir;

    Movement movement;

    private void Awake()
    {
        movement = GetComponent<Movement>();
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
        Vector3 dir = (newPos - transform.position).normalized;
        dir.y = 0;
        desireLookDir = dir;
        dir = Quaternion.Inverse(transform.rotation) * dir;
        Vector2 input = new Vector2(dir.z, dir.x);

        Debug.DrawLine(transform.position, newPos);

        if(Vector3.Distance(newPos, transform.position) < walkToTargetDist)         
            movement.PassInput(input, Movement.SpeedType.walk, false);        
        else if(Vector3.Distance(newPos, transform.position) < runToTargetDist)        
            movement.PassInput(input, Movement.SpeedType.run, false);                          
        else
            movement.PassInput(input, Movement.SpeedType.sprint, false);        
    }

    public void MoveIteration(Vector3 newPos, Vector3? lookPos = null)
    {
        Vector3 dir = (newPos - transform.position).normalized;
        dir.y = 0;

        if (lookPos != null)
        {
            Vector3 lookDir = (lookPos.Value - transform.position).normalized;
            lookDir.y = 0;
            desireLookDir = lookDir;
        }
        dir = Quaternion.Inverse(transform.rotation) * dir;
        Vector2 input = new Vector2(dir.z, dir.x);

        Debug.DrawLine(transform.position, newPos);

        if (Vector3.Distance(newPos, transform.position) < walkToTargetDist)
            movement.PassInput(input, Movement.SpeedType.walk, false);
        else if (Vector3.Distance(newPos, transform.position) < runToTargetDist)
            movement.PassInput(input, Movement.SpeedType.run, false);
        else
            movement.PassInput(input, Movement.SpeedType.sprint, false);
    }
}
