using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.Core
{
    public class OnDestroyNotifier : MonoBehaviour
    {
        public EventHandler onDestroy;

        private void OnDestroy()
        {
            onDestroy?.Invoke(gameObject, null);
        }
    }
}