using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Sampo.Core.VFX
{
    public class VisualEffectEnd : MonoBehaviour
    {
        VisualEffect connectedVFX;

        private void Awake()
        {
            connectedVFX = GetComponent<VisualEffect>();
            connectedVFX.outputEventReceived += OnVFXEnd;
            //TODO : ����� ������� ������� �� Event �����-������
        }

        private void Start()
        {
            connectedVFX.Play();
        }

        private void OnVFXEnd(VFXOutputEventArgs args) 
        {
            Destroy(gameObject);
        }
    }
}