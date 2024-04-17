using UnityEngine;

[RequireComponent(typeof(Faction))]
public class Interactable_UtilityAI : MonoBehaviour
    // ������������� ��������� ��� GameObject'�, ����� ��� ����� �� ��������� ����� UtilityAI
{
    public int ai_weight = 1;

    protected virtual void OnEnable()
    {
        UtilityAI_Manager.Instance.AddNewInteractable(this);
    }

    protected virtual void OnDisable()
    {
        UtilityAI_Manager.Instance.RemoveInteractableCompletely(this);
    }
}
