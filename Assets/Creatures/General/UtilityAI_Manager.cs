using System;
using System.Collections.Generic;
using UnityEngine;

public class UtilityAI_Manager : MonoBehaviour
// Собирает все объекты на сцене, с которыми можно взаимодействовать, и предоставляет информацию для всех UtilityAI
// Singleton
{
    //TODO : Сделать сюда отсортированные списки (По постоянным значениям),
    // Там же надо ещё придумать способы по оптимизации загрузки списков во всех юнитов. Сейчас очень тяжело уже.

    private static UtilityAI_Manager _instance;
    public static UtilityAI_Manager Instance
    {
        get
        {
            if (_instance == null)
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

    private Dictionary<Interactable_UtilityAI, int> _targetedByUnits = new Dictionary<Interactable_UtilityAI, int>();

    private Dictionary<Faction.FType, int> factionIndex_match = new();
    //Этот список содержит в себе объекты взаимодействия от и для конкретной фракции.
    //TODO : Использовать структуру данных типа бинарного дерева. У меня тут сортировка может происходить автоматически при добавлении
    private List<Dictionary<Interactable_UtilityAI, int>> _factionsData = new();

    public EventHandler<UAIData> changeHappened; //TODO : Мне не обязательно отправлять вообще весь Dictionary,
                                                 //Достаточно бросить всем новый добавленный объект.
                                                 // Уже там - разберуться, спасибо бинарным деревьям.
    public class UAIData : EventArgs
    {
        public Faction.FType factionWhereChangeHappened;
        public Dictionary<Interactable_UtilityAI, int> interactables;

        public UAIData(Dictionary<Interactable_UtilityAI, int> interactables, Faction.FType factionAffected)
        {
            this.interactables = interactables;
            this.factionWhereChangeHappened = factionAffected;
        }
    }

    private void OnApplicationQuit()
    {
        Destroy(_instance);
    }
    
    #region setters-Getters
    public void AddNewInteractable(Interactable_UtilityAI interactable, int weight)
    {
        if (interactable.TryGetComponent(out Faction f))
        {
            if (factionIndex_match.ContainsKey(f.f_type))
            {
                int resIndex = factionIndex_match[f.f_type];
                _factionsData[resIndex].Add(interactable, weight);
                changeHappened?.Invoke(this, new UAIData(_factionsData[resIndex], f.f_type));
            }
            else // Добавляем те фракции, которых ещё нет
            {
                _factionsData.Add(new Dictionary<Interactable_UtilityAI, int>());
                int resIndex = _factionsData.Count - 1;
                factionIndex_match.Add(f.f_type, resIndex);
                _factionsData[resIndex].Add(interactable, weight);
                changeHappened?.Invoke(this, new UAIData(_factionsData[resIndex], f.f_type));
            }
            
        }
        else // Фракция у объекта отсутствует, значит это объект взаимодействия для всех.
        {
            foreach(var key in factionIndex_match.Keys) 
            {
                var dict = _factionsData[factionIndex_match[key]];
                dict.Add(interactable, weight);
                changeHappened?.Invoke(this, new UAIData(dict, key));
            }
        }        
    }

    public void RemoveInteractable(Interactable_UtilityAI interactable)
    {
        int resIndex = 0;
        if (interactable.TryGetComponent(out Faction f))
        {
            if (factionIndex_match.TryGetValue(f.f_type, out resIndex))
            {
                _factionsData[resIndex].Remove(interactable);
            }
            changeHappened?.Invoke(this, new UAIData(_factionsData[resIndex], f.f_type));
        }
        else // Фракция у объекта отсутствует, значит это объект взаимодействия для всех.
        {
            foreach (var key in factionIndex_match.Keys)
            {
                var dict = _factionsData[factionIndex_match[key]];
                dict.Remove(interactable);
                changeHappened?.Invoke(this, new UAIData(dict, key)); // Три раза для каждой из фракций.
            }
        }
        _targetedByUnits.Remove(interactable);        
    }

    public void ChangeCongestion(Interactable_UtilityAI to, int powerAdded)
    {
        if (!_targetedByUnits.ContainsKey(to))
        {
            _targetedByUnits.Add(to, powerAdded);
        }
        else
            _targetedByUnits[to] += powerAdded;
    }

    public int GetCongestion(Interactable_UtilityAI from)
    {
        if (!_targetedByUnits.ContainsKey(from))
            return 0;
        else
            return _targetedByUnits[from];
    }
    #endregion
}