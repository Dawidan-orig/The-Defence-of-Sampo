using UnityEditor;
using UnityEngine;

public class AliveBeing : Interactable_UtilityAI, IDamagable
{
    public float health = 100;
    public Collider vital;
    public Transform mainBody;
    public Transform root;

    private void Awake()
    {
        if (GetComponents<Collider>().Length == 1)
            vital = GetComponent<Collider>();

        if (mainBody == null)
            mainBody = transform;
        if(root == null)
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

        Utilities.CreateFlowText(Mathf.RoundToInt(harm).ToString(), 5, transform.position, Color.red);

        if (health < 0)
        {
            if (root == null)
                Destroy(gameObject);
            else
                Destroy(root.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            Utilities.CreateTextInWorld(health.ToString(), transform, position: transform.position + GetComponent<Collider>().bounds.size.y / 2 * Vector3.up, color: Color.green);
    }
}
