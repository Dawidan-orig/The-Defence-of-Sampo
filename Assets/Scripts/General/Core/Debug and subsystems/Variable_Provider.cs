using UnityEngine;

public class Variable_Provider : MonoBehaviour
{
    private static Variable_Provider _instance;
    public static Variable_Provider Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<Variable_Provider>();

            if (_instance == null)
            {
                GameObject go = new("Variable Provider");
                _instance = go.AddComponent<Variable_Provider>();
            }
            
            if (UnityEditor.EditorApplication.isPlaying)
            {
                _instance.transform.parent = null;
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    public Material sampo;
    public Material enemy;
    public Material agro;

    public Transform unitsContainer;

    public LayerMask ground;
}
