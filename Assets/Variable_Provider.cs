using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Variable_Provider : MonoBehaviour
{
    private static Variable_Provider _instance;
    public static Variable_Provider Instance
    {
        get //TODO : Ей богу, этот паттерн надо уже в делегат выводить...
            // Надо сделать это универсальным
        {
            _instance = FindObjectOfType<Variable_Provider>();

            if (_instance == null)
            {
                GameObject go = new("Variable Provider");
                _instance = go.AddComponent<Variable_Provider>();
            }

            /*
            if (EditorApplication.isPlaying)
            {
                _instance.transform.parent = null;
                DontDestroyOnLoad(_instance.gameObject);
            }*/

            return _instance;
        }
    }

    public Material sampo;
    public Material enemy;
    public Material agro;
}
