using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace WingedCore.Core.Utility
{
    public static class EditorHelper
    {
        //https://discussions.unity.com/t/convert-serializedproperty-to-custom-class/94163/4

        private static int serializationDepth = 0; //ѕеременна€-регистр, что посто€нно используетс€ в сериализации
        public static T SerializedPropertyToObject<T>(SerializedProperty property)
        {
            return GetNestedObject<T>(property.propertyPath, GetSerializedPropertyRoot(property), true); //The "true" means we will also check all base classes
        }
        public static UnityEngine.Object GetSerializedPropertyRoot(SerializedProperty property)
        {
            var checking = property.serializedObject.targetObject;
            if (checking is Component)
                return (Component)checking;
            else if (checking is ScriptableObject)
                return (ScriptableObject)checking;
            else
                throw new InvalidCastException($"{checking.GetType()} - не компонент и не ScriptableObject");
        }
        public static T GetNestedObject<T>(string path, object obj, bool includeAllBases = false)
        {
            serializationDepth = 0;
            var splitted = path.Split('.');
            for (; serializationDepth < splitted.Length; serializationDepth++)
            {
                obj = GetFieldOrPropertyValue<object>(splitted[serializationDepth], obj, path, includeAllBases);
            }
            return (T)obj;
        }
        public static T GetFieldOrPropertyValue<T>(string fieldName, object obj, string fullPath, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (typeof(IList).IsAssignableFrom(obj.GetType()))
            {
                string[] splitted = fullPath.Split('.');
                char[] indexChar = splitted[serializationDepth + 1].ToString().Where(c => char.IsDigit(c)).ToArray();

                int index = Convert.ToInt32(new string(indexChar));
                var handledList = (IList)obj;
                serializationDepth++; // Skipping one step, because array elements are formed by Array.data[{index}], two things.
                return (T)handledList[index];
            }

            //TODO : ѕри решении закинуть его на форум (ссылка в начале Utilities.Editor)

            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field != null) return (T)field.GetValue(obj);

            PropertyInfo property = obj.GetType().GetProperty(fieldName, bindings);
            if (property != null) return (T)property.GetValue(obj, null);

            if (includeAllBases)
            {
                foreach (Type type in GetBaseClassesAndInterfaces(obj.GetType()))
                {
                    field = type.GetField(fieldName, bindings);
                    if (field != null) return (T)field.GetValue(obj);

                    property = type.GetProperty(fieldName, bindings);
                    if (property != null) return (T)property.GetValue(obj, null);
                }
            }

            return default;
        }
        public static IEnumerable<Type> GetBaseClassesAndInterfaces(this Type type, bool includeSelf = false)
        {
            //https://stackoverflow.com/questions/1823655/given-a-c-sharp-type-get-its-base-classes-and-implemented-interfaces
            List<Type> allTypes = new List<Type>();

            if (includeSelf) allTypes.Add(type);

            if (type.BaseType == typeof(object))
            {
                allTypes.AddRange(type.GetInterfaces());
            }
            else
            {
                allTypes.AddRange(
                        Enumerable
                        .Repeat(type.BaseType, 1)
                        .Concat(type.GetInterfaces())
                        .Concat(type.BaseType.GetBaseClassesAndInterfaces())
                        .Distinct());
            }

            return allTypes;
        }
        public static void SetFieldOrPropertyValue<T>(string fieldName, object obj, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                field.SetValue(obj, value);
                return;
            }

            PropertyInfo property = obj.GetType().GetProperty(fieldName, bindings);
            if (property != null)
            {
                property.SetValue(obj, value, null);
                return;
            }

            if (includeAllBases)
            {
                foreach (Type type in GetBaseClassesAndInterfaces(obj.GetType()))
                {
                    field = type.GetField(fieldName, bindings);
                    if (field != null)
                    {
                        field.SetValue(obj, value);
                        return;
                    }

                    property = type.GetProperty(fieldName, bindings);
                    if (property != null)
                    {
                        property.SetValue(obj, value, null);
                        return;
                    }
                }
            }
        }
    }
}
