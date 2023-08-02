using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplestShooting : Tool
{
    public Transform shootPoint;
    public float range;
    public float timeBetweenBullets;
    public GameObject bulletPrefab;
    public float gunPower;
    public ForceMode forceMode;

    private bool readyToFire = true;
    public virtual void Shoot() 
    {
        if (!readyToFire)
            return;

        GameObject bullet = Instantiate(bulletPrefab);
        bullet.transform.position = shootPoint.position;
        bullet.transform.rotation = shootPoint.rotation;
        bullet.GetComponent<Rigidbody>().AddForce(shootPoint.forward * gunPower, forceMode);

        Physics.IgnoreCollision(GetComponent<Collider>(), bullet.GetComponent <Collider>());

        Bullet b = bullet.GetComponent<Bullet>();
        b.possibleDistance = range;

        readyToFire = false;
        Invoke(nameof(NextShotReady), timeBetweenBullets);
    }

    public bool AvilableToShoot(Transform to) 
    {
        Utilities.VisualisedRaycast(transform.position,
                (to.position - transform.position).normalized,
                range,
                out RaycastHit hit,
                ~2);

        return hit.transform == to;
    }

    private void NextShotReady() 
    {
        readyToFire = true;
    }
}
