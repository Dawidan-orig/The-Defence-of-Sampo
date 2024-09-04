using WingedCore.AI;
using WingedCore.Core;
using UnityEngine;
using Sampo.Factions;

namespace WingedCore.Weaponry
{
    public class Tool : MonoBehaviour
    {
        [SerializeField]
        protected Transform _host;
        public float additionalMeleeReach;
        public LayerMask alive;
        public LayerMask structures;

        public Transform Host
        {
            get => _host;
            set
            {
                _host = value;
                if (_host != null)
                {
                    Physics.IgnoreCollision(GetComponent<Collider>(), _host.GetComponent<IDamagable>().Vital);
                }
            }
        }
        public FactionType GetHostFaction() { return _host.GetComponent<AITarget>().FactionType; }
        public virtual float GetRange() { return additionalMeleeReach; }
    }
}