using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_UtilityAI : MonoBehaviour
    // ѕредоставл€ет менеджеру вес GameObject'а, дела€ его одной из возможных целей UtilityAI
    // ¬ыполн€ет Debug-отрисовку: 
    // - Ѕазового веса
    // - ƒобавлени€ веса относительно дистанции до конкретного UAI.
    // - »змененного веса под воздействием вли€ни€ относительно конкретного UAI
    // - ”меньшени€ веса, вз€того от UtilityAI Manager, если на этот GameObject уже есть те, кто взаимодействует с ним. Ќапример когда одно здание атакует трое - нет смысла добавл€ть четвЄртого.
{
    public int weight = 1;

    protected virtual void OnEnable()
    {
        UtilityAI_Manager.Instance.AddNewInteractable(gameObject, weight);
    }

    protected virtual void OnDisable()
    {
        UtilityAI_Manager.Instance.RemoveInteractable(gameObject);
    }
}
