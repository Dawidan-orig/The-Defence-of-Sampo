using System;
using UnityEngine;

namespace WingedCore.Core.Balance
{
    /// <summary>
    /// Нужно подписать присущий игре Observer на OnInfluence для контроля влияния объектов.
    /// </summary>
    public class BalanceInfluencer : MonoBehaviour
    {
        public static event Action<int, GameObject> OnInfluence;

        public void DoInfluence(int power) 
        {
            OnInfluence?.Invoke(power, gameObject);
        }
    }
}