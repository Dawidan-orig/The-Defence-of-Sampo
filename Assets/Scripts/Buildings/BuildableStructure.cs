using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Sampo.Building
{
    public abstract class BuildableStructure : MonoBehaviour
    {
        //TODO : Дальше надо привязывать структуры к Terrain'у, чтобы делать rebuild сегмента для NavMesh и NMCalcs.
        //Для этого надо будет перейти на новую систему, а это перепись NMCalcs.

        [Header("General Building Settings")]
        [Tooltip("Определяет насколько объект можно загнать в землю")]
        [Min(0.1f)]
        public float possibleHeightToBuild = 0.1f;
        [Tooltip("Количество работы для завершения строительства")]
        public int progressToBuild = 0;

        protected int _currentProgressToBuild = 0;
        protected readonly LayerMask ground = 8;

        protected virtual void Awake()
        {

        }

        protected virtual void Start()
        {
            const float MAX_DISTANCE = 100;
            if (Physics.Raycast(transform.position + Vector3.up * possibleHeightToBuild, Vector3.down, out var hit, MAX_DISTANCE, ground))
            {
                if (Vector3.Distance(transform.position, hit.point) > possibleHeightToBuild)
                {
                    transform.position = hit.point + Vector3.down * possibleHeightToBuild/2;
                    Debug.Log("Опускаю строение вниз на землю");
                }
            }
            else
            {
                Debug.LogWarning("Земля для строения не найдена! Отключаю", transform);
                gameObject.SetActive(false);
            }
        }

        protected virtual void Update()
        {
            if (_currentProgressToBuild < progressToBuild)
            {
                Build();
            }
        }

        protected abstract void Build();
    }
}
