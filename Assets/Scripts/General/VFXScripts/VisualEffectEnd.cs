using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Sampo.Core.VFX
{
    public class VisualEffectEnd : MonoBehaviour
    {
        public Light connectedLight;
        public float time = 3;
        VisualEffect connectedVFX;

        private float startTime;
        private float startIntencity;

        private void Awake()
        {
            connectedVFX = GetComponent<VisualEffect>();
        }

        private void Start()
        {
            connectedVFX.Play();
            startIntencity = connectedLight.intensity;
            startTime = Time.time;

            Invoke(nameof(Kill), time);
        }

        private void Update()
        {
            connectedLight.intensity = Mathf.Lerp(startIntencity,0,Time.time - startTime);
        }

        private void OnApplicationQuit()
        {
            Kill();
        }

        private void Kill() 
        {
            Destroy(gameObject);
        }
    }
}