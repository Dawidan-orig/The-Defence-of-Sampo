using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class UtilityAI_Manager : MonoBehaviour
// Собирает все объекты на сцене, с которыми можно взаимодействовать, и предоставляет информацию для всех UtilityAI
// Singleton
{
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

            if (EditorApplication.isPlaying)
            {
                _instance.transform.parent = null;
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private Dictionary<Interactable_UtilityAI, int> _targetedByUnits = new Dictionary<Interactable_UtilityAI, int>();
    public Dictionary<Faction.FType, int> faction_IndexMatch = new();
    private List<Dictionary<Interactable_UtilityAI, int>> _factionsData = new();

    public EventHandler<UAIData> NewAdded;
    public EventHandler<UAIData> NewRemoved;

    public class UAIData : EventArgs
    {
        public Faction.FType factionWhereChangeHappened;
        public KeyValuePair<Interactable_UtilityAI, int> newInteractable;

        public UAIData(Interactable_UtilityAI interactable, Faction.FType factionAffected)
        {
            this.newInteractable = new KeyValuePair<Interactable_UtilityAI, int>(interactable, interactable.ai_weight);
            this.factionWhereChangeHappened = factionAffected;
        }
    }

    private void OnApplicationQuit()
    {
        Destroy(_instance);
    }

    #region setters-Getters
    public Dictionary<Interactable_UtilityAI, int> GetAllInteractions(Faction.FType forFaction) 
    {
        Dictionary<Interactable_UtilityAI, int> res = new();
        foreach (var kvp in faction_IndexMatch) 
        {
            if (kvp.Key == forFaction)
                continue;
            if (kvp.Key == Faction.FType.neutral)
                continue;

            foreach(var kvp2 in _factionsData[kvp.Value]) 
            {
                res.Add(kvp2.Key, kvp.Value);
            }
        }

        return res;
    }
    public void AddNewInteractable(Interactable_UtilityAI interactable, int weight)
    {
        var res = new KeyValuePair<Interactable_UtilityAI, int>(interactable, weight);

        if (interactable.TryGetComponent(out Faction f))
        {
            UAIData data = new UAIData(interactable, f.FactionType);
            if (faction_IndexMatch.ContainsKey(f.FactionType)) // Если такая фракция уже есть - получаем индекс
            {
                int resIndex = faction_IndexMatch[f.FactionType];
                _factionsData[resIndex].Add(interactable, weight);
                NewAdded?.Invoke(this, data);
            }
            else // Добавляем те фракции, которых ещё нет
            {
                _factionsData.Add(new Dictionary<Interactable_UtilityAI, int>());
                int resIndex = _factionsData.Count - 1;
                faction_IndexMatch.Add(f.FactionType, resIndex);
                _factionsData[resIndex].Add(interactable, weight);

                NewAdded?.Invoke(this, data);
            }

        }
        else // Фракция у объекта отсутствует, значит это объект взаимодействия для всех.
        {
            foreach (var key in faction_IndexMatch.Keys)
            {
                var dict = _factionsData[faction_IndexMatch[key]];
                dict.Add(interactable, weight);
                NewAdded?.Invoke(this, new UAIData(interactable, key));
            }
        }
    }

    public void RemoveInteractable(Interactable_UtilityAI interactableToRemove) 
    {
        var kvp = new KeyValuePair<Interactable_UtilityAI, int>(interactableToRemove, 0);        

        if (interactableToRemove.TryGetComponent(out Faction f))
        {
            if (faction_IndexMatch.TryGetValue(f.FactionType, out int resIndex))
            {
                _factionsData[resIndex].Remove(interactableToRemove);

                foreach (var key in faction_IndexMatch.Keys)
                {
                    NewRemoved?.Invoke(this, new UAIData(interactableToRemove, key));
                }
            }
        }
        else // Фракция у объекта отсутствует, значит это объект взаимодействия для всех.
        {
            foreach (var key in faction_IndexMatch.Keys)
            {
                var dict = _factionsData[faction_IndexMatch[key]];
                dict.Remove(interactableToRemove);
                NewRemoved?.Invoke(this, new UAIData(interactableToRemove, key));
            }
        }
        _targetedByUnits.Remove(interactableToRemove);
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