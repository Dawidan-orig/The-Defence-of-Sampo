using Sampo.Melee;
using System;
using UnityEngine;

public class MeleeTool : Tool
{
    [Header("===Melee Tool===")]
    public float cooldownBetweenAttacks;
    public float damageMultiplier = 1;
    public float noDamageTime = 0.5f;

    public Transform rightHandHandle;

    [Header("Audio")]
    [SerializeField]
    AudioClip[] _collision_blade;
    [SerializeField]
    AudioClip[] _collision_alive; //TODO DESIGN : это, возможно, лучше инвертировать в зависимости. Разные живые (Особенно монстры) имеют разные звуки ранений

    [Header("lookonly")]
    public Rigidbody body;
    public AudioSource audioSource;

    public event EventHandler<Collision> OnBladeCollision; //Расшариваю здешнюю коллизию в MeleeFighter'a
    public event EventHandler<Collider> OnBladeTrigger;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        body = GetComponent<Rigidbody>();
    }

    public override float GetRange()
    {
        return base.GetRange() + host.GetComponent<MeleeFighter>().baseReachDistance;
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnBladeCollision?.Invoke(this, collision);

        if (collision.collider.transform.TryGetComponent<IDamagable>(out var damagable))
        {
            damagable.Damage(body.velocity.magnitude * body.mass * damageMultiplier, IDamagable.DamageType.sharp);
            GetComponent<Collider>().isTrigger = true;
            Invoke(nameof(DisableCollision), noDamageTime);

            audioSource.pitch = UnityEngine.Random.Range(0.5f, 0.8f);
            audioSource.clip = _collision_alive[UnityEngine.Random.Range(0, _collision_alive.Length)];
            audioSource.Play();
        }
        else if (collision.collider.transform.TryGetComponent<Blade>(out _))
        {
            GetComponent<Collider>().isTrigger = true;
            Invoke(nameof(DisableCollision), noDamageTime);

            audioSource.pitch = UnityEngine.Random.Range(0.5f, 0.8f);
            audioSource.clip = _collision_blade[UnityEngine.Random.Range(0, _collision_blade.Length)];
            audioSource.Play();

            /*
            if (sparkles)
            {
                Vector3 sparklesSpread = collision.GetContact(0).point;
                transform.position = sparklesSpread;
                sparkles.Emit((int)sparkles.emission.GetBurst(0).count.constant);
            }*/
        }
    }

    private void DisableCollision()
    {
        GetComponent<Collider>().isTrigger = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        OnBladeTrigger?.Invoke(this, other);
    }
}
