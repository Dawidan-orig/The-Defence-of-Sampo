using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 startPoint;
    public float possibleDistance = 1000;
    public float remainingTime = 300;

    private void Start()
    {
        startPoint = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.transform.TryGetComponent<IDamagable>(out var c))
        {
            Rigidbody r = GetComponent<Rigidbody>();
            c.Damage(r.mass * r.velocity.magnitude, IDamagable.DamageType.blunt);
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        Utilities.DrawSphere(transform.position, duration: 5);

        if (remainingTime > 0)
            remainingTime -= Time.deltaTime;
        else
            Destroy(gameObject);

        if (Vector3.Distance(startPoint, transform.position) > possibleDistance)
            Destroy(gameObject);
    }
}
