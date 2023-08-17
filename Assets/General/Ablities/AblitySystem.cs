using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AblitySystem : MonoBehaviour
{
    public GameObject slashPrefab; // TODO : Переместить в ScriptableObject, добавив его в ProceedingSlash. это уберёт это поле отсюда, оно лишнее и не связанно с системой - только с Proceeding Slash
    public Ability windSlashAbility;

    private void Awake()
    {
        windSlashAbility = new ProceedingSlash(transform, GetComponent<SwordControl>(), slashPrefab);

        windSlashAbility.Activated = true;
    }
}
