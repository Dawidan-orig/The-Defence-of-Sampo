using Sampo.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Building.Transformators
{
    /// <summary>
    /// ������� ����� ��� ���� ������-���������������� ������ ������
    /// </summary>
    public class TransformationHouse : BuildableStructure, IInteractable
    {
        public void Interact(Transform interactor)
        {
            //TODO : ��������������
        }

        public void PlayerInteract()
        {
            //TODO : ��������� ����� ���������
        }

        protected override void Build()
        {
            throw new System.NotImplementedException();
        }
    }
}