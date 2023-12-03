using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class BaseShooting : Tool
{
    public Transform shootPoint;
    public GameObject bulletPrefab;

    public float range;
    public float timeBetweenBullets;
    public float gunPower;

    public ForceMode forceMode;

    protected bool readyToFire = true;

    public virtual void Shoot(Vector3? target = null)
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
        BFac.f_type = host.GetComponent<Faction>().f_type;

        Physics.IgnoreCollision(GetComponent<Collider>(), bullet.GetComponent<Collider>());
        Physics.IgnoreCollision(host.GetComponent<Collider>(), bullet.GetComponent<Collider>());

        Bullet b = bullet.GetComponent<Bullet>();
        b.possibleDistance = range;

        readyToFire = false;
        Invoke(nameof(NextShotReady), timeBetweenBullets);
    }

    public virtual bool AvilableToShoot(Transform to, out RaycastHit hit)
    {
        PenetratingRaycast(transform.position, transform.position + (to.position - transform.position).normalized * range, out hit);

        return hit.transform == to;
    }

    public virtual bool AvilableToShoot(Vector3 to, Vector3 from, out RaycastHit hit, Transform possibleTarget = null)
    {
        const float DEBUG_DURATION = 0;
        Color DEBUG_COLOR_RIGHTHIT = Color.green;
        Color DEBUG_COLOR_WRONGHIT = Color.gray;

        PenetratingRaycast(from, from + (to-from).normalized * range, out hit, DEBUG_DURATION, DEBUG_COLOR_WRONGHIT);

        bool res = Utilities.ValueInArea(hit.point, to, 0.1f) || (hit.transform == possibleTarget && possibleTarget != null);

        if (res)
            Debug.DrawLine(from, to, DEBUG_COLOR_RIGHTHIT, DEBUG_DURATION);

        return res;
    }
    
    public Vector3 NavMeshClosestAviableToShoot(Transform target)
    {
        Vector3 res = Vector3.zero;
        Utilities.VisualisedRaycast(transform.position, Vector3.down, out var heightCheck, range);
        // Высота оружия до земли.
        // TODO : Поменять на вычисление: Относительно низа коллайдера до y-положения оружия
        float height = heightCheck.collider ? (heightCheck.point - transform.position).magnitude : 0;
        Vector3 delta = transform.position - host.position;
        delta.x = 0; delta.z = 0;
        height = delta.magnitude + host.GetComponent<AliveBeing>().vital.bounds.size.y/2;   

        NavMeshCalculations.Cell start = NavMeshCalculations.Instance.GetCell(target.position);

        List<NavMeshCalculations.Cell> toCheck = new() { start };
        List<NavMeshCalculations.Cell> alreadyChecked = new();

        float bestDistanceToTarget = 0;
        float bestDistanceFromGun = 100000;

        while (toCheck.Count > 0)
        {
            NavMeshCalculations.Cell current = toCheck[0];
            alreadyChecked.Add(current);
            toCheck.RemoveAt(0);

            Vector3 shootFrom = current.NavMeshCenter() + Vector3.up * height;

            if (Vector3.Distance(shootFrom, target.position) < range &&
                AvilableToShoot(target.position, shootFrom, out RaycastHit hit, target))
            {
                float distanceFromGun = Vector3.Distance(shootFrom, transform.position);
                float distanceToTarget = Vector3.Distance(shootFrom, target.position);

                if (distanceFromGun < bestDistanceFromGun &&
                    distanceToTarget > bestDistanceToTarget)
                {
                    bestDistanceFromGun = distanceFromGun;
                    bestDistanceToTarget = distanceToTarget;
                    res = shootFrom;

                    List<NavMeshCalculations.Cell> toAdd = current.Neighbors;
                    toCheck.AddRange(toAdd);
                    toCheck.RemoveAll(item => alreadyChecked.Contains(item));
                }                

                if(NavMeshCalculations.CellCount() < toCheck.Count) 
                {
                    throw new StackOverflowException("Количество объектов для проверки больше, чем их общее количество");
                }
            }
        }

        return res;
    }

    public virtual Vector3 PredictMovement(Rigidbody target)
    {
        Vector3 speedToTarget = Vector3.ProjectOnPlane(shootPoint.forward * gunPower, target.velocity);
        float timeToTarget = Vector3.Distance(transform.position, target.position) / speedToTarget.magnitude;

        Vector3 res = target.position + timeToTarget * target.velocity;
        return res;
    }

    protected void PenetratingRaycast(Vector3 from, Vector3 to, out RaycastHit hit, float duration = 0, Color? color = null) 
    {
        if(color == null)
            color = Color.white;

        //TODO : Перевести в рекурсию

        Utilities.VisualisedRaycast(from,
                (to - from).normalized,
                out hit,
                (to - from).magnitude,
                alive + structures, duration: duration, color: color);

        if (hit.collider)
            if (hit.collider.isTrigger)
            {
                Utilities.VisualisedRaycast(hit.point,
                (to - from).normalized,
                out hit,
                (to - from).magnitude - (from - hit.point).magnitude,
                alive + structures, duration: duration, color: color);
            }

        if (hit.transform == host)
        {
            Utilities.VisualisedRaycast(hit.point,
                (to - from).normalized,
            out hit,
                (to - from).magnitude - (from - hit.point).magnitude,
                alive + structures, duration: duration, color: color);
        }
    }

    protected void NextShotReady()
    {
        readyToFire = true;
    }
}
