using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using WingedCore.Core;

public class MonoBehaviourSingleton<T> : MonoBehaviour
    where T : Component
{
    //TODO : Перевести всё на этот синглтон.
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var objs = FindObjectsOfType(typeof(T)) as T[];
                if (objs.Length > 0)
                    _instance = objs[0];
                if (objs.Length > 1)
                {
                    Debug.LogError("Среди всех активных сцен есть больше одного " + typeof(T).Name);
                }
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.transform.parent = MonoBehaviourSingleton<Variable_Provider>.Instance.singletonsContainer;
                    obj.hideFlags = HideFlags.DontSave;
                    _instance = obj.AddComponent<T>();
                }
            }

#if UNITY_EDITOR
            //Позволяет подписаться только один раз
            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;
#endif
            static void OnPlayModeState(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingEditMode)
                {
                    _instance = null;
                }
            }

            return _instance;
        }
    }
}