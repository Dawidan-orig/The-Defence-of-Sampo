using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UtilityAI_Manager : MonoBehaviour
    // Собирает все объекты на сцене, с которыми можно взаимодействовать, и предоставляет информацию для всех UtilityAI
    // Singleton
{
    public static UtilityAI_Manager instance { get; private set; }

    [Header("Setup")]
    // Оба нужны для динамического изменения уровня сложности, а так же реакции на игрока и само Сампо.
    // Например, на высоком уровне сложности юниты, способные эффективно разрушать строения будут больше целиться в Сампо, а не в другие здания.
    [SerializeField]
    private GameObject _player;
    [SerializeField]
    private GameObject _sampo;

    private Dictionary<GameObject, int> interactables = new Dictionary<GameObject, int>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        //interactables.Add(_sampo, 0);
        //interactables.Add(_player, 1);
    }

    public void AddNewInteractable(GameObject interactable, int weight) 
    {
        if(interactables.ContainsKey(interactable)) 
        {
            Debug.LogWarning($"{interactable.transform.name} уже был добавлен в список, отмена");
            return;
        }

        //TODO : Обновлять списки, когда interactable изчез. Например, когда целевое здание уничтожили.

        interactables.Add(interactable, weight);
    }

    public Dictionary<GameObject, int> GetPossibleActivities() 
    {
        // TODO : Добавить фильтры, чтобы возвращать только то, что может быть нужно:
        // - Конкретной фракции
        // - Конкретному типу ИИ
        // И так далее
        return interactables;
    }
}
