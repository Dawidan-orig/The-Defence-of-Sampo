using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Sampo.Core.VFX
{
    public class VisualEffectEnd : MonoBehaviour
    {
        public Light connectedLight;
        VisualEffect connectedVFX;

        private float startIntencity;

        private void Awake()
        {
            connectedVFX = GetComponent<VisualEffect>();
            connectedVFX.outputEventReceived += OnVFXEnd;
            //TODO : Лучше сделать перевод на Event какой-нибудь
        }

        private void Start()
        {
            connectedVFX.Play();
            startIntencity = connectedLight.intensity;
        }

        private void Update()
        {
            connectedLight.intensity = connectedVFX.aliveParticleCount / startIntencity;

            if(connectedVFX.aliveParticleCount <= 0) 
            {
                Destroy(gameObject);
            }
        }

        private void OnVFXEnd(VFXOutputEventArgs args) 
        {
            Destroy(gameObject);
        }
    }
}