using WingedCore.AI;
using UnityEngine;
using WingedCore.Core;
using WingedCore.Core.Balance;

namespace WingedCore.Weaponry.Ranged
{
    public class ProjectileBase : MonoBehaviour, IDamageDealer
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
            gameObject.hideFlags = HideFlags.HideInHierarchy;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.transform.TryGetComponent<IDamagable>(out var c))
            {
                Rigidbody r = GetComponent<Rigidbody>();
                float dmg = r.mass * r.velocity.magnitude * damageMultyplier;

                if(_damageSource != null)
                if (_damageSource.TryGetComponent(out AIBehaviourBase unit)) 
                {
                    unit.BalanceInfluence.DoInfluence((int)dmg);
                }

                _damageSource?.GetComponent<BalanceInfluencer>().DoInfluence((int)dmg);
                c.Damage(dmg, IDamagable.DamageType.blunt);
            }

            HandledSelfDestroy();
        }

        private void Update()
        {
            if (remainingTime > 0)
                remainingTime -= Time.deltaTime;
            else
                HandledSelfDestroy();

            if (Vector3.Distance(startPoint, transform.position) > possibleDistance)
                HandledSelfDestroy();
        }

        public void SetDamageDealer(Transform dealer)
        {
            _damageSource = dealer;
        }

        private void HandledSelfDestroy() 
        {
            if (instantiatedOnDestroy)
                Instantiate(instantiatedOnDestroy, transform.position, transform.rotation, null);

            Destroy(gameObject);
        }
    }
}