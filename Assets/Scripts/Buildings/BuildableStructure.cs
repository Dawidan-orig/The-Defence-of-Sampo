using WingedCore.AI;
using WingedCore.Core.JournalLogger;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using WingedCore.Core.Utility;
using Alchemy.Inspector;
using WingedCore.DebugSystems;
using WingedCore.Core.Balance;


namespace WingedCore.Building
{
    /// <summary>
    /// Класс для всех построек.
    /// Поддерживет процесс строительства и автоматически выравнивается по земле
    /// </summary>
    [SelectionBase]
    public abstract class BuildableStructure : MonoBehaviour
    {
        //TODO : Дальше надо привязывать структуры к Terrain'у, чтобы делать rebuild сегмента для NavMesh и NMCalcs.
        //Для этого надо будет перейти на новую систему, а это перепись NMCalcs.

        [Header("General Building Settings")]
        [Tooltip("Определяет насколько объект можно загнать в землю")]
        [Min(0.1f)]
        public float possibleHeightToBuild = 0.1f;
        [Tooltip("Количество работы для завершения строительства")]
        public int progressToBuild = -1;
        public Transform pivot = null;

        protected int _currentProgressToBuild = 0;
        [SerializeField, ReadOnly]
        protected LayerMask ground = 8 + 1;
        [SerializeField]
        private bool isBuilt = false;

        private BalanceInfluencer balanceInfluencer;
        
        protected bool IsBuilt { get => isBuilt; }
        public BalanceInfluencer BalanceInfluencer { get => balanceInfluencer;}

        private void Awake()
        {
            ground = MonoBehaviourSingleton<VariableProvider>.Instance.ground;
            balanceInfluencer = GetComponent<BalanceInfluencer>();

            if (pivot == null)
                pivot = transform;

            if (TryGetComponent(out AITarget interact))
                interact.enabled = false;
        }

        protected virtual void Start()
        {
            pivot.parent = MonoBehaviourSingleton<BuildingSystem>.Instance.structureParent;

            const float MAX_DISTANCE = 100;
            if (Physics.Raycast(pivot.position + Vector3.up * possibleHeightToBuild, Vector3.down, out var hit, MAX_DISTANCE, ground))
            {
                if (Vector3.Distance(pivot.position, hit.point) > possibleHeightToBuild)
                {
                    pivot.position = hit.point + Vector3.down * possibleHeightToBuild/2;
                    LoggerSystem.DebugLog("Опускаю строение вниз на землю", gameObject);
                }
            }
            else
            {
                Debug.LogWarning("Земля для строения не найдена! Отключаю", pivot);
                gameObject.SetActive(false);
            }
        }

        protected virtual void Update()
        {
            if (_currentProgressToBuild >= progressToBuild && !isBuilt)
            {
                Build();

                if(TryGetComponent(out AITarget interact))                
                    interact.enabled = true;

                isBuilt = true;
            }
        }

        protected virtual void OnDrawGizmos()
        {
            var array = GetType().ToString().Split('.');
            string text = array[array.Length - 1];
            Vector3 resPos = transform.position
            + Vector3.up * GetComponent<Collider>().bounds.max.y;

            EditorHelper.DrawEditorText(resPos, text);
        }

        /// <summary>
        /// Завершение строительства, когда здание обретает полный функционал.
        /// Можно использовать вместо конструктора
        /// </summary>
        protected abstract void Build();
    }
}
