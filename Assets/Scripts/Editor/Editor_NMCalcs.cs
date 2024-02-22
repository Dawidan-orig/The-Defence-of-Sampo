using Sampo.Core;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavMeshCalculations))]
public class Editor_NMCalcs : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Initialize")) 
        {
            NavMeshCalculations.Instance.Initialize();
        }
    }
}
