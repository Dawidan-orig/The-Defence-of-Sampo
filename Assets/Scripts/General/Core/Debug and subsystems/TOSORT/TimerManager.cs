using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.Core.Timers
{
    public static class TimerManager
    {
        static readonly List<Timer> timers = new();

        public static void RegisterTimer(Timer timer) => timers.Add(timer);
        public static void DeregisterTimer(Timer timer) => timers.Remove(timer);

        public static void UpdateTimers()
        {
            foreach (Timer timer in timers)
            {
                timer.Tick();
            }
        }

        public static void Clear() => timers.Clear();
    }
}