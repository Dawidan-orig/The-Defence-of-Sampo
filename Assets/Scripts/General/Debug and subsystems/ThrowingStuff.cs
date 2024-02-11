using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ThrowingStuff : MonoBehaviour
{
    public float startSpeed;
    public float lifetime = 5;

    [Range(0,100)]
    public float randomization = 0;

    public Transform target;
    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;
    public variant v = variant.first;


    public enum variant 
    {
        first,
        second,
        third
    }

    public float recharge = 50;
    public float charge = 100;
    public float current;

    public bool repeat = false;

    private void Start()
    {
        current = charge;
    }

    private void Update()
    {
        if (current < charge)
            current += recharge * Time.deltaTime;

        if (repeat)
            Throw();
    }

    public void Throw() 
    {
        if (current < charge)
            return;
        else
            current = 0;

        GameObject o = null;

        if(v == variant.first)
            o = Instantiate(prefab1);
        if (v == variant.second)
            o = Instantiate(prefab2);
        if (v == variant.third)
            o = Instantiate(prefab3);


        o.transform.position = transform.position;
        Vector3 delta = target.position - transform.position;
        o.GetComponent<Rigidbody>().AddForce(delta.normalized * startSpeed, ForceMode.VelocityChange);
        o.GetComponent<Rigidbody>().angularVelocity = new Vector3(UnityEngine.Random.Range(0, randomization), 0, UnityEngine.Random.Range(0, randomization));
        o.transform.rotation =  Quaternion.FromToRotation(o.transform.rotation* Vector3.forward, delta);

        Destroy(o, lifetime);
    }
}
