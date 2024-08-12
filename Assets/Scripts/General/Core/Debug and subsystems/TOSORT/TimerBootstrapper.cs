using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using WingedCore.Core.Utility.Deep;

namespace WingedCore.Core.Timers
{
    internal static class TimerBootstrapper
    {
        static PlayerLoopSystem timerSystem;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize() 
        {
            PlayerLoopSystem current = PlayerLoop.GetCurrentPlayerLoop();

            if(!InsertTimerManager<Update>(ref current, 0)) 
            {
                Debug.LogWarning("Таймеры не были инициализированы, не вышло зарегестрировать TimerManager в Update Loop");
                return;
            }
            PlayerLoop.SetPlayerLoop(current);
            PlayerLoopUtils.PrintPlayerLoop(current);

#if UNITY_EDITOR
            //Позволяет подписаться только один раз
            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;
#endif

            // О да, объявление функции прямо в функции. Так можно, но глазам немного больно.
            static void OnPlayModeState(PlayModeStateChange state) 
            {
                if(state == PlayModeStateChange.ExitingEditMode) 
                {
                    PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
                }
            }
        }

        static bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index) 
        {
            timerSystem = new PlayerLoopSystem()
            {
                type = typeof(TimerManager),
                updateDelegate = TimerManager.UpdateTimers,
                subSystemList = null
            };

            return PlayerLoopUtils.InsertSystem<T>(ref loop, in timerSystem, index);
        }
    }
}