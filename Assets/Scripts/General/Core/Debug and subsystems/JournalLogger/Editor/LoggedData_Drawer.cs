using Sampo.Core.JournalLogger.Behaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sampo.Core.JournalLogger.Editor
{
    [CustomPropertyDrawer(typeof(JournalComponent.LoggerData))]
    public class LoggedData_Drawer : PropertyDrawer
    {
        /*
         public DateTime timeWhenHappened;
         public string message;
         public GameObject context;
        */

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float DATE_WIDTH_FACTOR = 0.1f;
            const float CONTEXT_WIDTH_FACTOR = 0.2f;

            Rect dateRect = new Rect(position);
            dateRect.width *= DATE_WIDTH_FACTOR;
            Rect contextRect = new Rect(position);
            contextRect.width *= CONTEXT_WIDTH_FACTOR;
            Rect messageRect = new Rect(position);
            messageRect.x = dateRect.xMin + dateRect.width;
            messageRect.width = position.width - contextRect.width - dateRect.width;
            contextRect.x = messageRect.x + messageRect.width;

            JournalComponent.LoggerData data;
            SerializedProperty date = property.FindPropertyRelative(nameof(data.timeWhenHappened));
            SerializedProperty msg = property.FindPropertyRelative(nameof(data.message));
            SerializedProperty ctx = property.FindPropertyRelative(nameof(data.context));

            GUIStyleState read = new GUIStyleState
            {
                background = Texture2D.grayTexture
            };

            GUIStyle style = new GUIStyle()
            {
                normal = read,
            };

            EditorGUI.TextField(dateRect, date.stringValue, style);
            EditorGUI.TextField(messageRect, msg.stringValue, style);
            EditorGUI.ObjectField(contextRect, ctx);
        }
    }
}
