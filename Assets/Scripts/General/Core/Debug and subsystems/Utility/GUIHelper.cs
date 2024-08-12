using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WingedCore.Core.Utility
{
    public class GUIHelper
    {
        public static TextMeshProUGUI CreateText(string text, float duration, Vector3 position)
        {
            GameObject parent = new GameObject($"WillDie : \"{text}\"");
            TextMeshProUGUI res = CreateText(text, parent.transform, textAlignment: TextAlignment.Center);
            parent.transform.position = position;
            if (duration == 0)
                duration = 0.01f;
            res.transform.LookAt(Camera.main.transform);
            Object.Destroy(parent, duration);
            return res;
        }
        public static TextMeshProUGUI CreateText(string text, Transform parent, Vector3 localOffset = default(Vector3), Color? color = null, TextAlignment textAlignment = TextAlignment.Center, int fontSize = 18)
        {
            if (color == null) color = Color.gray;

            GameObject gameObject = new GameObject("TextMesh of " + (parent ? parent.ToString() : "nothing"), typeof(TextMeshProUGUI));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localOffset;
            TextMeshProUGUI textMesh = gameObject.GetComponent<TextMeshProUGUI>();
            textMesh.alignment = (TextAlignmentOptions)textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = (Color)color;
            textMesh.transform.SetAsLastSibling();
            textMesh.raycastTarget = false;
            return textMesh;
        }

        public static Canvas FindCanvas(Transform transform) //Legacy. Окна должны искать не Canvas, а менеджер окон.
        {
            Canvas canvas = null;

            Transform testCanvas = transform.parent;
            while (testCanvas.GetComponent<Canvas>() == null)
            {
                canvas = testCanvas.GetComponentInParent<Canvas>();
                if (canvas)
                    break;
                testCanvas = testCanvas.parent;
            }

            return canvas;
        }

        //Returns 'true' if we touched or hovering on Unity UI element.
        public static bool IsPointerOverUIElement(LayerMask UILayer)
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults(), UILayer);
        }

        //Returns 'true' if we touched or hovering on Unity UI element.
        static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults, LayerMask UIlayer)
        {
            for (int index = 0; index < eventSystemRaysastResults.Count; index++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[index];
                if (curRaysastResult.gameObject.layer == UIlayer)
                    return true;
            }
            return false;
        }

        //Gets all event system raycast results of current mouse or touch position.
        public static List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            return raysastResults;
        }
    }
}