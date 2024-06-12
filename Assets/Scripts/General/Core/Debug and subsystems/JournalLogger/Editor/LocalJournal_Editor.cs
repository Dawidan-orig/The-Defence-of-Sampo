using Sampo.Core.JournalLogger.Behaviours;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sampo.Core.JournalLogger.Editor
{
    [CustomEditor(typeof(JournalComponent))]
    public class LocalJournal_Editor : UnityEditor.Editor
    {
        Vector2 savedScroll;
        bool isAtEnd = false;

        public override void OnInspectorGUI()
        {
            SerializedProperty journalList = serializedObject.FindProperty("_logged");
            JournalComponent.LoggerData data;

            serializedObject.Update();
            //Стандартная в Unity высота строчки
            const float LINE_HEIGHT = 16f;

            EditorGUILayout.BeginVertical(GUILayout.MaxHeight(LINE_HEIGHT * 20));
            Vector2 last = savedScroll;
            savedScroll = EditorGUILayout.BeginScrollView(isAtEnd ? Vector2.positiveInfinity : savedScroll);
            if (Vector2.Distance(last, savedScroll) < 0.001f)            
                isAtEnd = true;            
            else
                isAtEnd = false;
            for (int i = 0; i < journalList.arraySize; i++) 
            {
                SerializedProperty arrayElem = journalList.GetArrayElementAtIndex(i);
                SerializedProperty stringVal = arrayElem.FindPropertyRelative(nameof(data.message));
                int newlines = stringVal.stringValue.Count(item => item == '\n');
                EditorGUILayout.PropertyField(arrayElem, GUILayout.MinHeight(LINE_HEIGHT * newlines));
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }
    }
}