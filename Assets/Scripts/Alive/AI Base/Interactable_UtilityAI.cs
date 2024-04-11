using UnityEngine;

[RequireComponent(typeof(Faction))]
public class Interactable_UtilityAI : MonoBehaviour
    // Предоставляет менеджеру вес GameObject'а, делая его одной из возможных целей UtilityAI
{
    public int ai_weight = 1;

    protected virtual void OnEnable()
    {
        UtilityAI_Manager.Instance.AddNewInteractable(this, ai_weight);
    }

    protected virtual void OnDisable()
    {
        UtilityAI_Manager.Instance.RemoveInteractable(this);
    }
}
