using Sampo.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Building.Transformators
{
    /// <summary>
    /// Базовый класс для всех зданий-преобразователей пустых юнитов
    /// </summary>
    public class TransformationHouse : BuildableStructure, IInteractable
    {
        public void Interact(Transform interactor)
        {
            //TODO : Преобразование
        }

        public void PlayerInteract()
        {
            //TODO : Настройка через интерфейс
        }

        protected override void Build()
        {
            throw new System.NotImplementedException();
        }
    }
}