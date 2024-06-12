using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Core.JournalLogger.Behaviours
{
    /// <summary>
    /// Компонент для индивидуального логирования
    /// </summary>
    public class JournalComponent : MonoBehaviour
    {
        //TODO : Специальный Editor для этой системы для удобства и красоты
        [SerializeField]
        private List<LoggerData> _logged;

        [Serializable]
        public struct LoggerData
        {
            //DateTime не сериализуется
            public string timeWhenHappened;
            public string message;
            public GameObject context;
        }

        private void Awake()
        {
            _logged = new List<LoggerData>();
        }

        public void Append(string message, GameObject context) 
        {
            _logged.Add(new LoggerData 
            {
                context = context,
                message = message,
                timeWhenHappened = DateTime.Now.ToString("HH:mm:ss")
            });
        }
    }
}