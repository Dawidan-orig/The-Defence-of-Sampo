using Sampo.Melee;
using System;
using UnityEngine;

namespace Sampo.Weaponry.Melee
{
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
        AudioClip[] _collision_alive; //TODO DESIGN : ���, ��������, ����� ������������� � �����������. ������ ����� (�������� �������) ����� ������ ����� �������

        [Header("lookonly")]
        public Rigidbody body;
        public AudioSource audioSource;

        public event EventHandler<Collision> OnBladeCollision; //���������� ������� �������� � MeleeFighter'a
        public event EventHandler<Collider> OnBladeTrigger;

        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            body = GetComponent<Rigidbody>();
        }

        public override float GetRange()
        {
            float addition = 0;

            if (_host.TryGetComponent<MeleeFighter>(out var m))
                addition += m.baseReachDistance;


            return base.GetRange() + addition;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!gameObject)
                return;

            OnBladeCollision?.Invoke(this, collision);

            if (collision.collider.transform.TryGetComponent<IDamagable>(out var damagable))
            {
                damagable.Damage(body.velocity.magnitude * body.mass * damageMultiplier, IDamagable.DamageType.sharp);
                GetComponent<Collider>().isTrigger = true;
                Invoke(nameof(DisableCollision), noDamageTime);

                if (audioSource)
                {
                    audioSource.pitch = UnityEngine.Random.Range(0.5f, 0.8f);
                    audioSource.clip = _collision_alive[UnityEngine.Random.Range(0, _collision_alive.Length)];
                    audioSource.Play();
                }
            }
            else if (collision.collider.transform.TryGetComponent<Blade>(out _))
            {
                GetComponent<Collider>().isTrigger = true;
                Invoke(nameof(DisableCollision), noDamageTime);

                if (audioSource)
                {
                    audioSource.pitch = UnityEngine.Random.Range(0.5f, 0.8f);
                    audioSource.clip = _collision_blade[UnityEngine.Random.Range(0, _collision_blade.Length)];
                    audioSource.Play();
                }

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
}