using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Core
{
    public interface IInteractable
    {
        public abstract void Interact(Transform interactor);
        public abstract void PlayerInteract();
    }
}