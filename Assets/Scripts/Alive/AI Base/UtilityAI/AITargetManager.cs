using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Alchemy.Serialization;
using Sampo.Core.JournalLogger;

namespace Sampo.AI
{
    /// <summary>
    /// Собирает все объекты на сцене,
    /// с которыми можно взаимодействовать,
    /// и предоставляет информацию для всех UtilityAI
    /// </summary>
    [AlchemySerialize]
    public partial class AITargetManager : MonoBehaviour
    {
        private static AITargetManager _instance;
        public static AITargetManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<AITargetManager>();

                if (_instance == null)
                {
                    GameObject go = new("AI Controlling Singleton");
                    _instance = go.AddComponent<AITargetManager>();
                }

                if (EditorApplication.isPlaying)
                {
                    _instance.transform.parent = null;
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        //TODO : Перевести в деревья насколько это возможно
        private Dictionary<AITarget, int> _targetedByUnits = new Dictionary<AITarget, int>();
        [AlchemySerializeField, NonSerialized]
        public Dictionary<FactionType, int> faction_IndexMatch = new();
        [AlchemySerializeField, NonSerialized]
        private List<Dictionary<AITarget, int>> _factionsData = new();

        public EventHandler<UAIData> NewAdded;
        public EventHandler<UAIData> NewRemoved;

        public class UAIData : EventArgs
        {
            public FactionType factionWhereChangeHappened;
            public KeyValuePair<AITarget, int> newInteractable;

            public UAIData(AITarget interactable, FactionType factionAffected)
            {
                this.newInteractable = new KeyValuePair<AITarget, int>(interactable, interactable.ai_weight);
                this.factionWhereChangeHappened = factionAffected;
            }
        }

        #region Unity
        private void Awake()
        {
            if (_instance == null)
                _instance = this;
        }
        private void OnApplicationQuit()
        {
            Destroy(_instance);
        }
        #endregion

        #region setters-Getters
        /// <summary>
        /// Возвращает все цели, которые принадлежат данной фракции. Нужно для того, чтоб целью был союзник
        /// </summary>
        /// <param name="forObject"></param>
        /// <returns></returns>
        public Dictionary<AITarget, int> GetSameFactionInteractions(AITarget forObject)
        {
            //TODO : Выдаёт данные других фракций, чего быть не должно
            Dictionary<AITarget, int> res = new();

            int index = faction_IndexMatch[forObject.FactionType];
            foreach (var kvp in _factionsData[index])
            {
                res.Add(kvp.Key, kvp.Value);
            }

            return res;
        }
        /// <summary>
        /// Возвращает вообще все доступный этой фракции цели
        /// </summary>
        /// <param name="forObject"></param>
        /// <returns></returns>
        public Dictionary<AITarget, int> GetAllInteractions(AITarget forObject)
        {
            Dictionary<AITarget, int> res = new();
            foreach (var kvp in faction_IndexMatch)
            {
                //Пропускаем все объекты, которые могут быть использованы этой же фракцией
                //Либо делаем его доступным ещё и для той же фракции, то-есть не пропускаем ничего
                if (!forObject.IsAvailableForSelfFaction)
                {
                    if (kvp.Key == forObject.FactionType)
                        continue;
                    if (kvp.Key == FactionType.neutral)
                        continue;
                }

                foreach (var kvp2 in _factionsData[kvp.Value])
                {
                    res.Add(kvp2.Key, kvp.Value);
                }
            }

            return res;
        }
        public void AddAsNewInteractable(AITarget interactable)
        {
            if (interactable.FactionType != FactionType.none)
            {
                UAIData data = new UAIData(interactable, interactable.FactionType);
                if (faction_IndexMatch.ContainsKey(interactable.FactionType)) // Если такая фракция уже есть - получаем индекс
                {
                    AddToFaction(interactable.FactionType, interactable);
                }
                else // Добавляем те фракции, которых ещё нет
                {
                    _factionsData.Add(new Dictionary<AITarget, int>());
                    int resIndex = _factionsData.Count - 1;
                    faction_IndexMatch.Add(interactable.FactionType, resIndex);
                    _factionsData[resIndex].Add(interactable, interactable.ai_weight);

                    NewAdded?.Invoke(this, data);
                }
            }
            else // Фракция у объекта отсутствует, значит это объект взаимодействия для всех.
            {
                foreach (var key in faction_IndexMatch.Keys)
                {
                    AddToFaction(key, interactable);
                }
            }
        }
        /// <summary>
        /// TODO : Используется, чтобы обновить информацию ИИ до его текущий.
        /// Это может изменить его фракцию, обновить данные о weight.
        /// </summary>
        // И, может быть, даже такие стелс-вещи как последнее известное расположение ИИ.
        // Или вовсе что-нибудь другоей
        public void UpdateAIInfo(AITarget of) 
        {
            // Обновилась ли фракция?
            // Если да - меняем

            // Стал ли доступен для своих же?
            
            // Значение weight тоже надо обновить тут
        }
        private void AddToFaction(FactionType factionIndex, AITarget interactable)
        {
            if (factionIndex == FactionType.none)
            {
                LoggerSingleton.DebugLog("Попытка добавить новый объект без фракции:", gameObject, interactable.gameObject);
                return;
            }

            var dict = _factionsData[faction_IndexMatch[factionIndex]];
            if (!dict.ContainsKey(interactable))
                dict.Add(interactable, interactable.ai_weight);
            else
                LoggerSingleton.DebugLog("Попытка повторно добавить "
                    + interactable.gameObject.name
                    + " в менеджер", gameObject);
            NewAdded?.Invoke(this, new UAIData(interactable, factionIndex));
        }        
        public void RemoveFromFaction(FactionType factionIndex, AITarget interactable)
        {
            if (factionIndex == FactionType.none)
                return;

            var dict = _factionsData[faction_IndexMatch[factionIndex]];
            dict.Remove(interactable);
            NewRemoved?.Invoke(this, new UAIData(interactable, factionIndex));
        }
        /// <summary>
        /// Полное удаление из системы обнаружения целей
        /// </summary>
        /// <param name="interactableToRemove"></param>
        public void RemoveInteractableCompletely(AITarget interactableToRemove)
        {
            var kvp = new KeyValuePair<AITarget, int>(interactableToRemove, 0);

            if (interactableToRemove.FactionType != FactionType.none)
            {
                if (faction_IndexMatch.TryGetValue(interactableToRemove.FactionType, out int resIndex))
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
        public void ChangeCongestion(AITarget to, int powerAdded)
        {
            if (!_targetedByUnits.ContainsKey(to))
            {
                _targetedByUnits.Add(to, powerAdded);
            }
            else
                _targetedByUnits[to] += powerAdded;
        }
        public int GetCongestion(AITarget from)
        {
            if (!_targetedByUnits.ContainsKey(from))
                return 0;
            else
                return _targetedByUnits[from];
        }
        #endregion
    }
}