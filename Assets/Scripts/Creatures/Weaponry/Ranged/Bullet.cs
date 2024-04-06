using UnityEngine;

public class Bullet : MonoBehaviour, IDamageDealer
{
    public GameObject instantiatedOnDestroy;
    public Vector3 startPoint;
    public float possibleDistance = 1000;
    public float remainingTime = 300;
    public float damageMultyplier = 1;

    private Transform _damageSource; 
    public Transform DamageFrom { get => _damageSource; }

    private void Start()
    {
        startPoint = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.transform.TryGetComponent<IDamagable>(out var c))
        {
            Rigidbody r = GetComponent<Rigidbody>();
            c.Damage(r.mass * r.velocity.magnitude * damageMultyplier, IDamagable.DamageType.blunt);
        }

        Destroy(gameObject);
    }

    private void Update()
    {   
        if (remainingTime > 0)
            remainingTime -= Time.deltaTime;
        else
            Destroy(gameObject);

        if (Vector3.Distance(startPoint, transform.position) > possibleDistance)
            Destroy(gameObject);
    }

    public void SetDamageDealer(Transform dealer) 
    {
        _damageSource = dealer;
    }

    private void OnDestroy()
    {
        if (instantiatedOnDestroy)
            Instantiate(instantiatedOnDestroy,transform.position,transform.rotation, null);
    }
}
