using Sampo.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnOptionEntryController
{
    Label NameLabel;
    private SpawnOption data;

    public SpawnOption Data {
        get  { return data; }
        set  {
            data = value;
            NameLabel.text = value.name; } }

    //This function retrieves a reference to the 
    //character name label inside the UI element.

    public void SetVisualElement(VisualElement visualElement)
    {
        NameLabel = visualElement.Q<Label>("name");
    }

    //This function receives the character whose name this list 
    //element displays. Since the elements listed 
    //in a `ListView` are pooled and reused, it's necessary to 
    //have a `Set` function to change which character's data to display.
}
