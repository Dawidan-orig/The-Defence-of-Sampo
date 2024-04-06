using Sampo.Core.Shaderworks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrassTile))]
public class GrassTile_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        if(EditorGUI.LinkButton(new Rect(0,0,100, 22.5f), new GUIContent("CreateMesh"))) 
        {
            GrassTile casted = (GrassTile)target;
            casted.SaveMeshFromComputeShader();
        }

        DrawDefaultInspector();
    }
}
