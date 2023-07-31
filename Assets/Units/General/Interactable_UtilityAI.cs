using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_UtilityAI : MonoBehaviour
    // Предоставляет менеджеру вес GameObject'а, делая его одной из возможных целей UtilityAI
    // Выполняет Debug-отрисовку: 
    // - Базового веса
    // - Добавления веса относительно дистанции до конкретного UAI.
    // - Измененного веса под воздействием влияния относительно конкретного UAI
    // - Уменьшения веса, взятого от UtilityAI Manager, если на этот GameObject уже есть те, кто взаимодействует с ним. Например когда одно здание атакует трое - нет смысла добавлять четвёртого.
{
    public int weight = 1;
    public bool sampoSide = true; //TODO : Убрать, заменив на enum-Фракции.

    private void Start()
    {
        UtilityAI_Manager.instance.AddNewInteractable(gameObject, weight);
    }
}
