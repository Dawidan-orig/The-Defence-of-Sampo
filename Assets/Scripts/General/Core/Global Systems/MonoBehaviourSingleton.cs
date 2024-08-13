using UnityEditor;
using UnityEngine;
using WingedCore.DebugSystems;

public class MonoBehaviourSingleton<T> : MonoBehaviour
    where T : Component
{
    static bool stopped = false;

    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null && !stopped)
            {
                var objs = FindObjectsOfType(typeof(T)) as T[];
                if (objs.Length > 0)
                {
                    _instance = objs[0];

#if UNITY_EDITOR
                    //Позволяет подписаться только один раз
                    EditorApplication.playModeStateChanged -= OnPlayModeState;
                    EditorApplication.playModeStateChanged += OnPlayModeState;
#endif
                }
                if (objs.Length > 1)
                {
                    Debug.LogError("Среди всех активных сцен есть больше одного " + typeof(T).Name);
                }
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    //obj.transform.parent = MonoBehaviourSingleton<VariableProvider>.Instance.singletonsContainer;
                    //obj.hideFlags = HideFlags.DontSave;
                    _instance = obj.AddComponent<T>();

#if UNITY_EDITOR
                    //Позволяет подписаться только один раз
                    EditorApplication.playModeStateChanged -= OnPlayModeState;
                    EditorApplication.playModeStateChanged += OnPlayModeState;
#endif
                    
                }

                void OnPlayModeState(PlayModeStateChange state)
                {
                    if (state == PlayModeStateChange.ExitingPlayMode)
                    {
                        stopped = true;
                    }
                }
            }

            return _instance;
        }
    }
}