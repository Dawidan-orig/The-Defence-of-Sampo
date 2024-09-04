using Sampo.Economy.Adders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WingedCore.AI;

namespace Sampo.Economy.Adders
{
    public class EconomyOnDeath : MonoBehaviour
    {
        public int economyToAdd = 5;

        private void OnDestroy()
        {
            if (TryGetComponent(out AIBehaviourBase ai))
                economyToAdd = ai.VisiblePowerPoints / 80;
            MonoBehaviourSingleton<EconomySystem>.Instance.AddToAll(economyToAdd);
        }
    }
}