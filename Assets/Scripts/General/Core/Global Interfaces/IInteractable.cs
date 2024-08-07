using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.Core
{
    public interface IInteractable
    {
        public abstract void Interact(Transform interactor);
        public abstract void PlayerInteract();
        public abstract float GetInteractionRange();
    }
}