using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.Core
{
    public interface IDamageDealer
    {
        public Transform DamageFrom { get; }
    }
}