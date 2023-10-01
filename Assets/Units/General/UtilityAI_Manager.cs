using System;
using System.Collections.Generic;
using UnityEngine;

public class UtilityAI_Manager : MonoBehaviour
    // Собирает все объекты на сцене, с которыми можно взаимодействовать, и предоставляет информацию для всех UtilityAI
    // Singleton
{
    private static UtilityAI_Manager _instance;

    private Dictionary<GameObject, int> _interactables = new Dictionary<GameObject, int>();

    public EventHandler<UAIData> changeHappened;

    public static UtilityAI_Manager Instance 
    { 
        get
            {
            if (!_instance)
            {
                _instance = new GameObject().AddComponent<UtilityAI_Manager>();
                // name it for easy recognition
                _instance.name = _instance.GetType().ToString();
                // mark root as DontDestroyOnLoad();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        } 
    }

    private void OnApplicationQuit()
    {
        Destroy(_instance);
    }

    public class UAIData : EventArgs
    {
        public Dictionary<GameObject, int> interactables;

        public UAIData(Dictionary<GameObject, int> interactables)
        {
            this.interactables = interactables;
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

    public void RemoveInteractable(GameObject interactable) 
    {
        _interactables.Remove(interactable);

        changeHappened?.Invoke(this, new UAIData(_interactables));
    }
}
