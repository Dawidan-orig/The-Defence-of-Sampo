using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ThrowingStuff : MonoBehaviour
{
    public float startSpeed;

    [Range(0,100)]
    public float randomization = 0;

    public Transform target;
    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;


    public float recharge = 50;
    public float charge = 100;
    public float current;

    private void Start()
    {
        current = charge;
    }

    private void Update()
    {
        if (current < charge)
            current += recharge * Time.deltaTime;
    }

    public void Throw() 
    {
        var o = Instantiate(prefab1);
        o.transform.position = transform.position;
        Vector3 delta = target.position - transform.position;
        o.GetComponent<Rigidbody>().AddForce(delta.normalized * startSpeed, ForceMode.VelocityChange);
        o.GetComponent<Rigidbody>().angularVelocity = new Vector3(UnityEngine.Random.Range(0, randomization), UnityEngine.Random.Range(0, randomization), UnityEngine.Random.Range(0, randomization));
        o.transform.rotation =  Quaternion.FromToRotation(o.transform.rotation* Vector3.forward, delta);

        Destroy(o, 5);
    }
}
