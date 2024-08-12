using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.Core.Timers
{
    public class CountdownTimer : Timer
    {
        public override bool IsFinished => CurrentTime <= 0;

        public CountdownTimer(float value) : base(value) { }

        public override void Tick()
        {
            if(IsRunning && CurrentTime > 0) 
            {
                CurrentTime -= Time.deltaTime;
            }

            if(IsRunning && CurrentTime <= 0)
            {
                Stop();
            }
        }
    }
}