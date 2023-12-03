using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Pallete))]
public class Pallete_ProperyDrawer : PropertyDrawer
{  
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        const float DEFAULT_HEIGHT = 18f; // Посмотрел в PropertyDrawer (через Показать определение)

        SerializedProperty list = property.FindPropertyRelative("objectsSummary");
        EditorGUI.BeginProperty(position, label,property);

        //var identLevel = EditorGUI.indentLevel;
        //EditorGUI.indentLevel = 0;
        Rect listOnlyPos = new Rect(position.x, position.y, position.width, position.height);
        //TODO : Убрать это. Сделать отрисовку потомков напрочь по своему. Относительн уже существующих параметров надо синхронизировать значения.
        // Pallete должен делать override PropertyDrawer'а у PalleteObject.
        EditorGUI.PropertyField(listOnlyPos, list, GUIContent.none, true);

        for(int i = 0; i < list.arraySize; i++) 
        {
            PalleteObject arrayElement = Utilities.Editor.SerializedPropertyToObject < PalleteObject >(list.GetArrayElementAtIndex(i));
            if (arrayElement.WasModified)
            {
                Debug.Log($"{arrayElement} should be modified");               
                arrayElement.OnValidate();
                arrayElement.WasModified = false;
            }
        }


        Rect buttonOffset = new Rect(position);
        buttonOffset.height = DEFAULT_HEIGHT;
        buttonOffset.y += DEFAULT_HEIGHT * list.CountInProperty() + DEFAULT_HEIGHT;

        //var genericType = fieldInfo.GetType().GetGenericArguments()[0];
        //var used = Pallete.GetActualObjectForSerializedProperty(fieldInfo, property);
        var used = Utilities.Editor.SerializedPropertyToObject<Pallete>(property);

        if (EditorGUI.LinkButton(buttonOffset, "Add new"))
        {
            used.AddNew(null, 0.5f);
        }

        buttonOffset.y += DEFAULT_HEIGHT;

        if (EditorGUI.LinkButton(buttonOffset, "Clear"))
        {
            used.Clear();
        }

        buttonOffset.y += DEFAULT_HEIGHT;

        if (EditorGUI.LinkButton(buttonOffset, "Get Real Values"))
        {
            string res = "Objects:\n";
            foreach(PalleteObject obj in used.GetPalleteObjectsRaw()) 
            {
                res += obj.ToString() + "\n";
            }

            Debug.Log(res);
        }

        //EditorGUI.indentLevel = identLevel;

        EditorGUI.EndProperty();

        property.serializedObject.ApplyModifiedProperties();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty list = property.FindPropertyRelative("objectsSummary");
        float defaultHeight = base.GetPropertyHeight(property, label);
        return defaultHeight * list.CountInProperty() + defaultHeight * 4;
    }
}