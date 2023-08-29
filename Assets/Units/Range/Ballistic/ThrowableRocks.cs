using UnityEngine;

public class ThrowableRocks : SimplestShooting
{
    protected void Awake()
    {
        // ” баллистического оружи€ дальность зависит от силы запуска
        float velocityAxis = Mathf.Sin(45 * Mathf.Deg2Rad) * gunPower; // 45 - угол, при котором полЄт будет дальше всего
        float flyTime = (velocityAxis / 9.8f) * 2; // ¬верх и потом вниз
        range = velocityAxis * flyTime;
    }

    public override void Shoot(Vector3? target = null)
    {
        if (!readyToFire)
            return;

        GameObject bullet = Instantiate(bulletPrefab);
        bullet.transform.position = shootPoint.position;
        bullet.transform.rotation = shootPoint.rotation;

        Vector3 flatEquvivalent = FlatEquialent(target.Value);

        // –ешение задачи из Awake в обратную сторону. (ќт range до gunPower)
        float actualPower = Mathf.Sqrt(9.8f * range * Mathf.InverseLerp(0, range, flatEquvivalent.magnitude) / 2) / Mathf.Sin(45 * Mathf.Deg2Rad);

        bullet.GetComponent<Rigidbody>().AddForce(
            (shootPoint.forward + shootPoint.up).normalized * actualPower,
            forceMode);

        Faction BFac;
        if (!bullet.TryGetComponent(out BFac))
            BFac = bullet.AddComponent<Faction>();
        BFac.type = host.GetComponent<Faction>().type;

        Physics.IgnoreCollision(GetComponent<Collider>(), bullet.GetComponent<Collider>());
        Physics.IgnoreCollision(host.GetComponent<Collider>(), bullet.GetComponent<Collider>());

        Bullet b = bullet.GetComponent<Bullet>();
        const int ADDITION_TO_NOT_EARLY_DISSOLVE = 10;
        b.possibleDistance = range + ADDITION_TO_NOT_EARLY_DISSOLVE;

        readyToFire = false;
        Invoke(nameof(NextShotReady), timeBetweenBullets);
    }

    public override bool AvilableToShoot(Transform to, out RaycastHit hit)
    {
        Vector3 flatEquvivalent = FlatEquialent(to.position);
        float actualPower = Mathf.Sqrt(9.8f * range * Mathf.InverseLerp(0, range, flatEquvivalent.magnitude) / 2) / Mathf.Sin(45 * Mathf.Deg2Rad);
        float velocityAxis = Mathf.Sin(45 * Mathf.Deg2Rad) * actualPower;
        float flyTime = (velocityAxis / 9.8f) * 2;        
        float actualRange = velocityAxis * flyTime;

        Vector3 relativeToUp = flatEquvivalent.normalized * actualRange / 2
            + Vector3.up * flyTime / 4 * velocityAxis;
        Vector3 upperPoint = transform.position + relativeToUp;
        Vector3 checkFrom = transform.position;

        RaycastHit first;

        PenetratingRaycast(checkFrom, upperPoint, out first);

        checkFrom = upperPoint;
        Vector3 endPoint = checkFrom + new Vector3(relativeToUp.x, -relativeToUp.y, relativeToUp.z);

        RaycastHit second;

        PenetratingRaycast(checkFrom, endPoint, out second);

        if (first.transform)
            hit = first;
        else
            hit = second;

        return hit.transform == to;
    }

    public override bool AvilableToShoot(Vector3 to, Vector3 from, out RaycastHit hit, Transform possibleTarget = null)
    {
        Vector3 flatEquvivalent = FlatEquialent(to, from);
        float actualPower = Mathf.Sqrt(9.8f * range * Mathf.InverseLerp(0, range, flatEquvivalent.magnitude) / 2) / Mathf.Sin(45 * Mathf.Deg2Rad);
        float velocityAxis = Mathf.Sin(45 * Mathf.Deg2Rad) * actualPower;
        float flyTime = (velocityAxis / 9.8f) * 2;
        float actualRange = velocityAxis * flyTime;

        Vector3 relativeToUp = flatEquvivalent.normalized * actualRange / 2
            + Vector3.up * flyTime / 4 * velocityAxis;
        Vector3 upperPoint = from + relativeToUp;
        Vector3 checkFrom = from;

        RaycastHit first;

        PenetratingRaycast(checkFrom, upperPoint, out first);

        checkFrom = upperPoint;
        Vector3 endPoint = checkFrom + new Vector3(relativeToUp.x, -relativeToUp.y, relativeToUp.z);

        RaycastHit second;

        PenetratingRaycast(checkFrom, endPoint, out second);

        if (first.transform)
            hit = first;
        else
            hit = second;

        bool res = Utilities.ValueInArea(hit.point, to, 0.1f) || (hit.transform == possibleTarget && possibleTarget != null);

        if (res)
            Debug.DrawLine(from, to, Color.green, 0);

        return res;
    }

    public override Vector3 GetPointToShoot(Rigidbody target)
    {
        Vector3 flatEquvivalent = FlatEquialent(target.position, transform.position);
        float actualPower = Mathf.Sqrt(9.8f * range * Mathf.InverseLerp(0, range, flatEquvivalent.magnitude) / 2) / Mathf.Sin(45 * Mathf.Deg2Rad);
        float velocityAxis = Mathf.Sin(45 * Mathf.Deg2Rad) * actualPower;
        float flyTime = (velocityAxis / 9.8f) * 2;

        Vector3 res = target.position + flyTime * target.velocity;
        Utilities.DrawSphere(res, duration: 3);
        return res;
    }

    private Vector3 FlatEquialent(Vector3 target)
    {
        return FlatEquialent(target, transform.position);
    }
    private Vector3 FlatEquialent(Vector3 target, Vector3 from)
    {
        // ѕока что будет работать только дл€ целей на той же высоте.
        return Vector3.ProjectOnPlane(target - from, Vector3.up);
    }
}
