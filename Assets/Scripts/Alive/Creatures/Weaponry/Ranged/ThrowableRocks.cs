using UnityEngine;

namespace Sampo.Weaponry.Ranged
{
    public class ThrowableRocks : BaseShooting
    {
        public int ONE_SIDE_SEPARAIONS = 0;

        protected override void Awake()
        {
            base.Awake();
            // У баллистического оружия дальность зависит от силы запуска
            float velocityAxis = Mathf.Sin(45 * Mathf.Deg2Rad) * gunPower; // 45 - угол, при котором полёт будет дальше всего
            float flyTime = (velocityAxis / 9.8f) * 2; // Вверх и потом вниз
            range = velocityAxis * flyTime;
        }

        private void OnValidate()
        {
            float velocityAxis = Mathf.Sin(45 * Mathf.Deg2Rad) * gunPower; // 45 - угол, при котором полёт будет дальше всего
            float flyTime = (velocityAxis / 9.8f) * 2; // Вверх и потом вниз
            range = velocityAxis * flyTime;
        }

        public override void Shoot(Vector3? target = null)
        {
            if (!readyToFire)
                return;

            GameObject bullet = Instantiate(bulletPrefab);
            bullet.transform.position = shootPoint.position;
            bullet.transform.rotation = shootPoint.rotation;

            //TODO : Если цель находится над transform, то происходит бесконечность.
            Vector3 flatEquvivalent = FlatEquialent(target.Value);
            float actualPower = Power(flatEquvivalent.magnitude);

            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
            bullet.GetComponent<Rigidbody>().AddForce(
                (shootPoint.forward + shootPoint.up).normalized * actualPower,
                ForceMode.VelocityChange);

            Faction BFac;
            if (!bullet.TryGetComponent(out BFac))
                BFac = bullet.AddComponent<Faction>();
            BFac.ChangeFactionCompletely(host.GetComponent<Faction>().FactionType);

            Physics.IgnoreCollision(GetComponent<Collider>(), bullet.GetComponent<Collider>());
            Physics.IgnoreCollision(host.GetComponent<Collider>(), bullet.GetComponent<Collider>());

            BulletBase b = bullet.GetComponent<BulletBase>();
            b.SetDamageDealer(transform);
            const int ADDITION_TO_NOT_EARLY_DISSOLVE = 10;
            b.possibleDistance = range + ADDITION_TO_NOT_EARLY_DISSOLVE;

            readyToFire = false;
            Invoke(nameof(NextShotReady), timeBetweenBullets);
        }

        public override bool AvilableToShoot(Transform to, out RaycastHit hit)
        {
            return AvilableToShoot(to.position, transform.position, out hit, to);
        }
        public override bool AvilableToShoot(Vector3 to, Vector3 from, out RaycastHit hit, Transform possibleTarget = null)
        {
            Vector3 flatEquvivalent = FlatEquialent(to, from);
            float actualPower = Power(flatEquvivalent.magnitude);

            if (actualPower > gunPower)
            {
                hit = new RaycastHit();
                return false;
            }

            float velocityAxis = Mathf.Sin(45 * Mathf.Deg2Rad) * actualPower;
            float flyTime = (velocityAxis / 9.8f) * 2;
            float actualRange = velocityAxis * flyTime; //TODO : Перевести на другую формулу из интернета, которая учитывает ещё и спуск.

            Vector3 relativeToUp = flatEquvivalent.normalized * actualRange / 2
                + Vector3.up * flyTime / 4 * velocityAxis;


            #region arc-check
            float toUpHorizontal = (flatEquvivalent.normalized * actualRange / 2).magnitude;
            float step = toUpHorizontal / (ONE_SIDE_SEPARAIONS + 1);
            Vector3 checkFrom = from;
            Vector3 checkTo = Vector3.zero;
            hit = new RaycastHit();
            for (int i = 1; i < ONE_SIDE_SEPARAIONS + 1; i++)
            {
                checkTo = from + flatEquvivalent.normalized * i * step + Vector3.up * Height(i * step, actualPower);
                PenetratingRaycast(checkFrom, checkTo, out hit);

                if (hit.transform != null)
                    break;

                checkFrom = checkTo;
            }

            if (hit.transform == null)
            {
                PenetratingRaycast(checkFrom, from + relativeToUp, out hit);
                checkFrom = from + relativeToUp;

                if (hit.transform == null)
                    for (int i = 1; i < ONE_SIDE_SEPARAIONS + 1; i++)
                    {
                        checkTo = from + flatEquvivalent.normalized * i * step
                            + flatEquvivalent.normalized * toUpHorizontal
                            + Vector3.up * Height(i * step + toUpHorizontal, actualPower);

                        PenetratingRaycast(checkFrom, checkTo, out hit);

                        if (hit.transform != null)
                            break;

                        checkFrom = checkTo;
                    }

                if (hit.transform == null)
                    PenetratingRaycast(checkFrom, from + flatEquvivalent, out hit);

                if (hit.transform == null)
                {
                    Vector3 direct = to - from;
                    direct.y = 0;

                    checkFrom = checkTo;
                    checkTo = from + direct + Vector3.up * Height(direct.magnitude, actualPower);

                    PenetratingRaycast(checkFrom, checkTo, out hit);
                }
            }

            #endregion



            bool res = Utilities.ValueInArea(hit.point, to, 0.1f) || (hit.transform == possibleTarget && possibleTarget != null);

            if (res)
                Debug.DrawLine(from, to, Color.green, 0);

            return res;
        }

        public override Vector3 PredictMovement(Rigidbody target)
        {
            Vector3 flatEquvivalent = FlatEquialent(target.position, transform.position);
            float actualPower = Power(flatEquvivalent.magnitude);
            float velocityAxis = Mathf.Sin(45 * Mathf.Deg2Rad) * actualPower;
            float flyTime = (velocityAxis / 9.8f) * 2;

            Vector3 res = target.position + flyTime * target.velocity;
            return res;
        }

        private Vector3 FlatEquialent(Vector3 target)
        {
            return FlatEquialent(target, transform.position);
        }

        private Vector3 FlatEquialent(Vector3 target, Vector3 from)
        {
            Vector3 directVector = Vector3.ProjectOnPlane(target - from, Vector3.up);
            float height = target.y - from.y;
            float length = directVector.magnitude;
            float cosinus = Mathf.Cos(45 * Mathf.Deg2Rad);
            float tangent = Mathf.Tan(45 * Mathf.Deg2Rad);

            float velocityUsed = Mathf.Sqrt(
                (-9.8f * length * length) /
                (2 * cosinus * cosinus * (height - length * tangent))
                );

            // float resTime = 2 * velocityUsed / 9.8f;

            float a = -9.8f / 2;
            float b = velocityUsed;
            float c = -height;
            float diskr = b * b - 4 * a * c;
            float time1 = (-b + Mathf.Sqrt(diskr)) / (2 * a);
            float time2 = (-b - Mathf.Sqrt(diskr)) / (2 * a);

            Vector3 res1 = directVector.normalized * time1 * velocityUsed;
            Vector3 res2 = directVector.normalized * time2 * velocityUsed;

            //Debug.DrawLine(from + directVector, from + res1, Color.black);
            //Debug.DrawLine(from + directVector, from + res2, Color.cyan);

            return directVector + res1;
        }

        private float Power(float dist)
        {
            //return Mathf.Sqrt(9.8f * range * Mathf.InverseLerp(0, range, dist) / 2) / Mathf.Sin(45 * Mathf.Deg2Rad);
            return dist / (Mathf.Sqrt(2 * dist / 9.8f) * Mathf.Sin(45 * Mathf.Deg2Rad));
        }
        private float Height(float dist, float power)
        {
            float velocityAxis = Mathf.Sin(45 * Mathf.Deg2Rad) * power;
            float flyTime = dist / velocityAxis;

            return (velocityAxis * flyTime - 9.8f * flyTime * flyTime / 2);
        }

        private float HorizontalVelocity()
        {
            return Mathf.Sin(45 * Mathf.Deg2Rad) * gunPower;
        }
    }
}