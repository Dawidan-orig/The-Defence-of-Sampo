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

        Faction BFac;
        if (!bullet.TryGetComponent(out BFac))
            BFac = bullet.AddComponent<Faction>();
        BFac.type = host.GetComponent<Faction>().type;

        Physics.IgnoreCollision(GetComponent<Collider>(), bullet.GetComponent<Collider>());
        Physics.IgnoreCollision(host.GetComponent<Collider>(), bullet.GetComponent<Collider>());

        Bullet b = bullet.GetComponent<Bullet>();
        b.possibleDistance = range;

        readyToFire = false;
        Invoke(nameof(NextShotReady), timeBetweenBullets);
    }

    public bool AvilableToShoot(Transform to, out RaycastHit hit)
    {
        Utilities.VisualisedRaycast(transform.position,
                (to.position - transform.position).normalized,
                out hit,
                range,                
                alive + structures);

        if (hit.collider)
            if (hit.collider.isTrigger)
            {
                Utilities.VisualisedRaycast(hit.point,
                (to.position - transform.position).normalized,
                out hit,
                range - (transform.position - hit.point).magnitude,                
                alive + structures);
            }

        if (hit.transform == host)
        {
            Utilities.VisualisedRaycast(hit.point,
                (to.position - transform.position).normalized,
                out hit,
                range - (transform.position - hit.point).magnitude,                
                alive + structures);
        }

        return hit.transform == to;
    }

    private void NextShotReady()
    {
        readyToFire = true;
    }
}
