using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PalleteObject))]
public class PalleteComponent_PropertyDrawer : PropertyDrawer
{
    private SerializedProperty _leftP;
    private SerializedProperty _rightP;

    /*
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new();

        var leftPointer = new PropertyField(property.FindPropertyRelative("left"));
        var rightPointer = new PropertyField(property.FindPropertyRelative("right"));
        var objectUsed = new PropertyField(property.FindPropertyRelative("obj"));

        container.Add(leftPointer);
        container.Add(rightPointer);
        container.Add(objectUsed);

        return container;
    }*/

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        #region Planning
        //fieldInfo.GetValue() позволяет вернуть значение поданного в него поля.
        // Обычно используется property, подаваемый в функцию. Он содержит в себе это поле как раз.
        // SerializedProperty - Это сборка всего, что он в себе содержит.
        //property.serializedObject.targetObject позволяет найти по названию для конкретного сериализованного объекта, это мне не подходит.
        // Итого: Надо извлечь из property Pallete, который точно в нём хранится. Иначе бы не попали вот в эту текущую функцию.

        //property.propertyPath содержит в себе путь к текущему объекту, который и надо извлечь.
        //property.serializedObject.FindProperty() позволяет искать по пути... Пробую.
        //Debug.Log(property.propertyPath);
        //Debug.Log(property.serializedObject.FindProperty(property.propertyPath).displayName); - Тут получается Element 0. То-есть это кусок списка. Почти
        //  SerializedProperty listSerializedElement = property.serializedObject.FindProperty(property.propertyPath);
        // Его надо изучить.
        //Debug.Log(listSerializedElement.type); -> PalleteObject, то, что надо.

        //Дальше пришлось покопаться в интернете, и я нашёл ряд функций. Вот одна из них:
        //  PalleteObject used = Utilities.Editor.SerializedPropertyToObject<PalleteObject>(listSerializedElement);
        // По сути, эти функции делают то, что я описал выше, но в декомпозированном виде. На моё счатье, что я их нашёл.
        // Вот только они не работают, GetFieldOrPropertyValue стукается об NullRef в первой же строчке.

        // Оказалось, что те штуки не умеют работать со множествами (Списки и массивы, все IList'ы). Я добавил какой-то костыль. Проверяю.
        //  used.OnValidate();
        // Работает!
        #endregion

        EditorGUI.BeginProperty(position, label, property);

        var identLevel = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var leftPointerProp = property.FindPropertyRelative("left");
        var rightPointerProp = property.FindPropertyRelative("right");
        var content = property.FindPropertyRelative("obj");

        const float POINTER_WIDTH = 18 * 4;

        Rect leftPos = new Rect(position.x, position.y, POINTER_WIDTH, position.height);
        Rect contentPos = new Rect(position.x + position.width / 2 - 50, position.y, 100, position.height);
        Rect rightPos = new Rect(position.x + position.width - POINTER_WIDTH, position.y, POINTER_WIDTH, position.height);

        SerializedProperty listSerializedElement = property.serializedObject.FindProperty(property.propertyPath);
        PalleteObject used = Utilities.Editor.SerializedPropertyToObject<PalleteObject>(listSerializedElement);

        leftPointerProp.floatValue = FloatVal(leftPos, leftPointerProp, used.left);

        if (content == null)
            EditorGUI.LabelField(contentPos, "Empty");
        else
            EditorGUI.PropertyField(contentPos, content, GUIContent.none);

        rightPointerProp.floatValue = FloatVal(rightPos, rightPointerProp, used.right);

        EditorGUI.indentLevel = identLevel;

        EditorGUI.EndProperty();

        property.serializedObject.ApplyModifiedProperties();
    }

    private float FloatVal(Rect pos, SerializedProperty pointerProp, float passedValue)
    { 
        //TODO : После использование Add New кнопки в Pallete_PropertyDrawer 
        // Значения тут нередактируемы. Я уже всю голову сломал! (Затрачено чистого времени > 6 часов на решение. Хватит.)
        float val = EditorGUI.FloatField(pos, passedValue);
        Debug.Log("redacted value : " + val + " real: " + passedValue + " when serProp is " + pointerProp.floatValue);
        pointerProp.floatValue = val;

        //used.OnValidate();

        return val;
    }
}