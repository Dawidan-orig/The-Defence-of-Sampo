using Sampo.Core.JournalLogger.Behaviours;
using UnityEngine;

namespace Sampo.Core.JournalLogger
{
    /// <summary>
    /// ������� ������������, ������� ��������� ����������� ���� � ���������� ��������.
    /// </summary>
    public static class LoggerSingleton
    {
        public static void Journal(string data, GameObject context) 
        {
            //TODO : ������������� ������ ��������, ��������� ��� ������
        }
#if UNITY_EDITOR
        public static void DebugLog(string data, GameObject caller, GameObject context = null) 
        {
            if(!caller.TryGetComponent(out JournalComponent logger)) 
                logger = caller.AddComponent<JournalComponent>();            

            logger.Append(data, context);
        }
#endif
    }
}