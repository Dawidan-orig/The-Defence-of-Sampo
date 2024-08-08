using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace WingedCore.AI.CounterSystem
{
    /// <summary>
    /// Основа системы контр-атаки, куда надо обращаться чтобы понять свои контр-возможности
    /// </summary>
    [ExecuteInEditMode]
    [Alchemy.Serialization.AlchemySerialize]
    [CreateAssetMenu(fileName = "New Counter Map", menuName = "Scriptable/Units/Counter Map")]
    public partial class CounterSystem : ScriptableObject
    {
        private static CounterSystem _instance;
        public static CounterSystem Instance { get => _instance; }

        public List<CounterNode> allNodesInput;

        [Alchemy.Inspector.ReadOnly]
        [Alchemy.Serialization.AlchemySerializeField, System.NonSerialized]
        Dictionary<CounterNode, Dictionary<CounterNode, float>> loadedCounters;

        public bool IsCounter(CounterNode one, CounterNode another, out float modifier) 
        {
            //Вроде бы, это самое оптимальное.
            try
            {
                modifier = loadedCounters[one][another];
                return true;
            }
            catch (System.ArgumentException) { modifier = 1; return false; }
        }

        public void TryGiveTagTo(Transform target, CounterNode newRole) 
        {
            if(target.TryGetComponent(out CounterTagLocal localCounter))
            {
                localCounter.AddNewRole(newRole);
            }
            else 
            {
                Debug.LogWarning("Попытка дать роль туда, где нет компонента системы ролей");
            }
        }

        private void Awake()
        {
#if UNITY_EDITOR
            if (_instance == null)
            {
                _instance = this;
            }
            else 
            {
                Debug.LogWarning("Только один словарь контр-атак может существовать за раз.\n" +
                    "Удаляю новый созданный.\n" +
                    "Существующий находится тут: " + AssetDatabase.GetAssetPath(_instance));
                DestroyImmediate(this);
            }
#endif
        }

#if UNITY_EDITOR
        EditorCoroutine timer;

        private void OnValidate()
        {
            if (timer != null)
                EditorCoroutineUtility.StopCoroutine(timer);
            timer = EditorCoroutineUtility.StartCoroutine(rebuildTimer(), this);
        }

        private IEnumerator rebuildTimer() 
        {
            yield return new WaitForSeconds(5);

            LoadInput();
        }

        private void LoadInput() 
        {
            loadedCounters = new();
            foreach (var node in allNodesInput) 
            {
                loadedCounters.Add(node, new Dictionary<CounterNode, float>(node.counterWho));
            }
        }
#endif
    }
}
