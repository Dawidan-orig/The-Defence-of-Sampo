using System;
using System.Collections.Generic;
using UnityEngine;

public class UtilityAI_Manager : MonoBehaviour
// Собирает все объекты на сцене, с которыми можно взаимодействовать, и предоставляет информацию для всех UtilityAI
// Singleton
{
    private static UtilityAI_Manager _instance;
    public static UtilityAI_Manager Instance
    {
        get //TODO : Ей богу, этот паттерн надо уже в делегат выводить...
            // Надо сделать это универсальным
        {
            _instance = FindObjectOfType<UtilityAI_Manager>();

            if (_instance == null)
            {
                GameObject go = new("AI Controlling Singleton");
                _instance = go.AddComponent<UtilityAI_Manager>();
            }

            /*
            if (EditorApplication.isPlaying)
            {
                _instance.transform.parent = null;
                DontDestroyOnLoad(_instance.gameObject);
            }*/

            return _instance;
        }
    }

    private Dictionary<GameObject, int> _interactables = new Dictionary<GameObject, int>();
    private Dictionary<GameObject, int> _targetedByEnemies = new Dictionary<GameObject, int>();

    public EventHandler<UAIData> changeHappened;

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

    public Dictionary<GameObject, int> GetInteractables()
    {
        return new Dictionary<GameObject, int>(_interactables);
    }

    public void AddNewInteractable(GameObject interactable, int weight)
    {
        if (_interactables.ContainsKey(interactable))
        {
            Debug.LogWarning($"{interactable.transform.name} уже был добавлен в список, отмена");
            return;
        }

        _interactables.Add(interactable, weight);

        changeHappened?.Invoke(this, new UAIData(_interactables));
    }

    public void RemoveInteractable(GameObject interactable)
    {
        _interactables.Remove(interactable);
        _targetedByEnemies.Remove(interactable);

        changeHappened?.Invoke(this, new UAIData(_interactables));
    }

    public void ChangeCongestion(GameObject to, int powerAdded)
    {
        if (!_targetedByEnemies.ContainsKey(to))
        {
            Debug.Log($"Добавлен {to.name} с занятостью {powerAdded}");
            _targetedByEnemies.Add(to, powerAdded);
        }
        else
            _targetedByEnemies[to] += powerAdded;
    }

    public int GetCongestion(GameObject from)
    {
        if (!_targetedByEnemies.ContainsKey(from))
            return 0;
        else
            return _targetedByEnemies[from];
    }
}
