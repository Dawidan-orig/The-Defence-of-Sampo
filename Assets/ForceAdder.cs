using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ForceAdder : MonoBehaviour
{
    public Vector3 forcePower = Vector3.forward * 10;
    public ForceMode mode = ForceMode.Impulse;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit info, 100)) 
            {
                info.rigidbody.AddForce(forcePower, mode);
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit info, 100))
            {
                info.rigidbody.AddTorque(forcePower, mode);
            }
        }
    }
}
