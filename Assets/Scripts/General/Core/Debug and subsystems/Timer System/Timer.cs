using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.Core.Timers
{
    public abstract class Timer : IDisposable
    {
        public float CurrentTime { get; protected set; }
        public bool IsRunning { get; private set; }

        protected float initialTime;

        public float Progess => Mathf.Clamp(CurrentTime / initialTime, 0, 1);

        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };

        
        protected Timer(float value) 
        {
            initialTime = value;
        }

        public void Start() 
        {
            CurrentTime = initialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                TimerManager.RegisterTimer(this);
                OnTimerStart.Invoke();
            }
        }

        public void Stop() 
        {
            if (!IsRunning)
            {
                IsRunning = false;
                TimerManager.DeregisterTimer(this);
                OnTimerStop.Invoke();
            }
        }

        public abstract void Tick();
        public abstract bool IsFinished { get; }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        public virtual void Reset() => CurrentTime = initialTime;
        public virtual void Reset (float newTime) 
        {
            initialTime = newTime;
            Reset();
        }

        #region memory works
        bool disposed;

        ~Timer()
        {
            Dispose(false);
        }
        //Ќужен дл€ вывода всей информации из TimerManager.
        //ѕоскольку мы влезли в глубокий код, автоматические системы Unity тут уже не работают.
        //Ќадо контроллировать пам€ть и глобальные данные самосто€тельно.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if(disposing) 
            {
                TimerManager.DeregisterTimer (this);    
            }

            disposed = true;
        }
        #endregion
    }
}