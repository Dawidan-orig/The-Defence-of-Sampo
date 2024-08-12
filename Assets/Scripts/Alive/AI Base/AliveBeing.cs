using UnityEngine;
using WingedCore.Core;
using WingedCore.Core.Utility;

namespace WingedCore.AI
{
    public class AliveBeing : AITarget, IDamagable
    {
        public float health = 100;
        [Tooltip("Коллайдер, которые регистрирует получение урона")]
        public Collider vital;
        [Tooltip("Этот объект определяет ту часть тела, в которой расположен TargetingUtilityAI (Мозг)")]
        //TODO : Refactor, заменить transfrom на TargetingAI
        public Transform brainBody;
        [Tooltip("Этот объект будет удалён, когда здоровье опустится ниже 100")]
        public Transform parentToDestroy;

        public Collider Vital => vital;



        private void Awake()
        {
            if (GetComponents<Collider>().Length == 1)
                vital = GetComponent<Collider>();

            if (brainBody == null)
                brainBody = transform;
            if (parentToDestroy == null)
                parentToDestroy = transform;
        }

        public void Damage(float harm, IDamagable.DamageType type)
        {
            if (type == IDamagable.DamageType.sharp)
                health -= harm * 0.5f;
            else if (type == IDamagable.DamageType.blunt)
                health -= harm * 0.2f;
            else if (type == IDamagable.DamageType.thermal)
                health -= harm;

            DebugVisualsHelper.CreateFlowText(Mathf.RoundToInt(harm).ToString(), 5, transform.position, new Color(0.3f, 0, 0, 0.3f));

            if (health < 0)
            {
                if (parentToDestroy == null)
                    Destroy(gameObject);
                else
                    Destroy(parentToDestroy.gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            DebugVisualsHelper.CreateTextInWorld(health.ToString(), transform, position: transform.position + GetComponent<Collider>().bounds.size.y / 2 * Vector3.up, color: Color.green);
        }

        protected override void DebugChangeColors()
        {
#if UNITY_EDITOR
            if (useDebugColors)
            {
                WingedCore.DebugSystems.VariableProvider provider = MonoBehaviourSingleton<WingedCore.DebugSystems.VariableProvider>.Instance;

                Material material = null;

                switch (this.FactionType)
                {
                    case FactionType.sampo: material = provider.friend; break;
                    case FactionType.enemy: material = provider.enemy; break;
                    case FactionType.aggressive: material = provider.agro; break;
                    case FactionType.neutral: material = provider.neutral; break;
                }

                Transform highest = parentToDestroy;
                if (highest == null)
                    highest = transform;

                if (material != null)
                    foreach (Renderer renderer in highest.GetComponentsInChildren<Renderer>())
                    {
                        renderer.sharedMaterial = material;
                    }
            }
#endif
        }
    }
}