using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(TargetingUtilityAI))]
public class AI_Inspector : Editor
{
    public VisualTreeAsset AIWindow;

    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our inspector UI
        VisualElement myInspector = new VisualElement();

        // Load from default reference
        AIWindow.CloneTree(myInspector);

        // Get a reference to the default inspector foldout control
        VisualElement inspectorFoldout = myInspector.Q("Default_Inspector");
        // Attach a default inspector to the foldout
        InspectorElement.FillDefaultInspector(inspectorFoldout, serializedObject, this);

        // Return the finished inspector UI
        return myInspector;
    }
}
