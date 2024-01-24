using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(ThrowingStuff))]
public class ThrowerInspector : Editor
{
    public VisualTreeAsset m_InspectorXML;

    //delegate void deleg_Throw();

    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our inspector UI
        VisualElement myInspector = new VisualElement();

        // Load from default reference
        m_InspectorXML.CloneTree(myInspector);

        // Get a reference to the default inspector foldout control
        VisualElement inspectorFoldout = myInspector.Q("Default_Inspector");
        // Attach a default inspector to the foldout
        InspectorElement.FillDefaultInspector(inspectorFoldout, serializedObject, this);

        var myButton = new UnityEngine.UIElements.Button() { text = "Spawn" };
        Action d = ((ThrowingStuff)target).Throw;
        myButton.clicked += d;
        myInspector.Add(myButton);
        if (!EditorApplication.isPlaying)
            myButton.SetEnabled(false);

        // Return the finished inspector UI
        return myInspector;
    }
}
