using System;
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

    private Dictionary<GameObject, int> _interactables = new Dictionary<GameObject, int>();

    public EventHandler<UAIData> changeHappened;

    public class UAIData : EventArgs
    {
        public Dictionary<GameObject, int> interactables;

        public UAIData(Dictionary<GameObject, int> interactables)
        {
            this.interactables = interactables;
        }
    }    

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
    }

    public Dictionary<GameObject, int> GetInteractables ()
    {
        return _interactables;
    }

    public void AddNewInteractable(GameObject interactable, int weight) 
    {
        if(_interactables.ContainsKey(interactable)) 
        {
            Debug.LogWarning($"{interactable.transform.name} уже был добавлен в список, отмена");
            return;
        }

        //TODO : Обновлять списки, когда interactable изчез. Например, когда целевое здание уничтожили.

        _interactables.Add(interactable, weight);

        changeHappened?.Invoke(this, new UAIData(_interactables));
    }
}
