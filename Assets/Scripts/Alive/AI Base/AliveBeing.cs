using UnityEditor;
using UnityEngine;

namespace Sampo.AI
{
    public class AliveBeing : Interactable_UtilityAI, IDamagable
    {
        public float health = 100;
        [Tooltip("���������, ������� ������������ ��������� �����")]
        public Collider vital;
        [Tooltip("���� ������ ���������� �� ����� ����, � ������� ���������� TargetingUtilityAI (����)")]
        public Transform mainBody;
        [Tooltip("���� ������ ����� �����, ����� �������� ��������� ���� 100")]
        public Transform root;

        public Collider Vital => vital;

        private void Awake()
        {
            if (GetComponents<Collider>().Length == 1)
                vital = GetComponent<Collider>();

            if (mainBody == null)
                mainBody = transform;
            if (root == null)
                root = transform;
        }

        public void Damage(float harm, IDamagable.DamageType type)
        {
            if (type == IDamagable.DamageType.sharp)
                health -= harm * 0.5f;
            else if (type == IDamagable.DamageType.blunt)
                health -= harm * 0.2f;
            else if (type == IDamagable.DamageType.thermal)
                health -= harm;

            Utilities.CreateFlowText(Mathf.RoundToInt(harm).ToString(), 5, transform.position, new Color(0.3f, 0, 0, 0.3f));

            if (health < 0)
            {
                if (root == null)
                    Destroy(gameObject);
                else
                    Destroy(root.gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
                Utilities.CreateTextInWorld(health.ToString(), transform, position: transform.position + GetComponent<Collider>().bounds.size.y / 2 * Vector3.up, color: Color.green);
        }
    }
}