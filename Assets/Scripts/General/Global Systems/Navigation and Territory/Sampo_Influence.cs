using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sampo_Influence : MonoBehaviour
{
    // Скрипт, контроллирующий влияние Сампо на следующие вещи:
    // - Зона высадки (Спавна) противника
    // - Восстановление разрушенной почвы

    public float LouhaBlock_Radius = 75;
    public float TerrainRehabilitation_Radius = 100;

    private void OnDrawGizmosSelected()
    {
        Utilities.DrawEllipse(transform.position, transform.up, transform.forward * -1, LouhaBlock_Radius, LouhaBlock_Radius, 40, new Color(1, 0.3f,0.5f));
        Utilities.DrawEllipse(transform.position + Vector3.up * LouhaBlock_Radius, transform.up, transform.forward * -1, LouhaBlock_Radius, LouhaBlock_Radius, 40, new Color(1, 0.3f, 0.5f));
    }
}
