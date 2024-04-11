using Sampo.GUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnMenuController
{
    VisualTreeAsset ListEntryTemplate;

    List<SpawnOption> allSpawnables;
    ListView spawnablesList;

    public GameObject ConnectUIToObject(VisualElement listElement) 
    {
        if (listElement == null)
            return null;

        return ((SpawnOptionEntryController)listElement.userData).Data.Prefab;
    }

    public void InitializeCharacterList(VisualElement root, VisualTreeAsset listElementTemplate, List<SpawnOption> options)
    {
        allSpawnables = new();
        allSpawnables.AddRange(options);

        // Store a reference to the template for the list entries
        ListEntryTemplate = listElementTemplate;

        // Store a reference to the character list element
        spawnablesList = root.Q<ListView>("spawnables");

        FillCharacterList();
    }

    public void ClearVisuals() 
    {
        spawnablesList.itemsSource = null;
    }

    void FillCharacterList()
    {
        // Set up a make item function for a list entry
        spawnablesList.makeItem = () =>
        {
            // Instantiate the UXML template for the entry
            var newListEntry = ListEntryTemplate.Instantiate();

            // Instantiate a controller for the data
            var newListEntryLogic = new SpawnOptionEntryController();
            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;
            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry);

            // Return the root of the instantiated visual tree
            return newListEntry;
        };

        // Set up bind function for a specific list entry
        spawnablesList.bindItem = (item, index) =>
        {
            (item.userData as SpawnOptionEntryController).Data = allSpawnables[index];
        };

        // Set a fixed item height
        spawnablesList.fixedItemHeight = 45;

        // Set the actual item's source list/array
        spawnablesList.itemsSource = allSpawnables;
    }
}
