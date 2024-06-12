using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Utilities
{
    //public static GameObject utility = new("Utilitary");

    public static bool VisualisedBoxCast(Vector3 center, Vector3 halfExtends, Vector3 direction, float maxDistance, LayerMask layerMask = default, bool drawHit = false, Color? color = null, float duration = 0)
    {
        return VisualisedBoxCast(center, halfExtends, direction, out _, Quaternion.identity, maxDistance, layerMask, drawHit, color, duration);
    }
    public static bool VisualisedBoxCast(Vector3 center, Vector3 halfExtends, Vector3 direction, out RaycastHit hitInfo, Quaternion orientation, float maxDistance, LayerMask layerMask = default, bool drawHit = true, Color? color = null, float duration = 0, bool visualise = true)
    {
        if (color == null)
            color = Color.white;

        if (visualise)
        {
            DrawSphere(center, 0.1f, color, duration);
            Debug.DrawRay(center, direction * maxDistance, color.Value, duration);
        }

        bool result = Physics.BoxCast(center, halfExtends, direction, out hitInfo, orientation, maxDistance, layerMask);

        if (drawHit && result && visualise)
        {
            DrawSphere(hitInfo.point, 0.1f, Color.white, duration);
        }

        if (visualise)
        {
            if (result)
            {
                DrawBox(center, halfExtends, orientation, Color.green, duration);
                Debug.DrawLine(center, hitInfo.point, (Color)color, duration);
            }
            else
                DrawBox(center, halfExtends, orientation, Color.red);
        }

        return result;
    }
    public static bool VisualizedRaycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance, LayerMask? layerMask = null, bool drawHit = true, Color? color = null, float duration = 0, bool visualise = true)
    {
        if (color == null)
            color = Color.white;

        bool result;
        if (layerMask != null)
            result = Physics.Raycast(origin, direction, out hit, maxDistance, (LayerMask)layerMask);
        else
            result = Physics.Raycast(origin, direction, out hit, maxDistance);

        if (drawHit && result && visualise)
        {
            Debug.DrawLine(origin, hit.point, (Color)color, duration);
            DrawSphere(hit.point, 0.075f, (Color)color, duration);
        }

        if (!result && visualise)
        {
            Debug.DrawRay(origin, direction.normalized * maxDistance, Color.red, duration);
        }

        return result;
    }
    public static bool VisualisedRaycast(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit hit, out float angle, LayerMask layerMask,  Color? color = null, float duration = 0, bool drawAngle = true, bool drawHit = true, bool visualise = true)
    {
        bool result = VisualizedRaycast(origin, direction, out hit, maxDistance, layerMask, drawHit, color, duration, visualise);

        if (color == null)
            color = Color.white;

        if (result)
        {
            Vector3 offsetDir = direction + Vector3.up * 0.01f;
            Physics.Raycast(origin, offsetDir, out RaycastHit angleHit);
            Vector3 angleLine = angleHit.point - hit.point;

            angle = Vector3.Angle(angleLine, new Vector3(angleLine.x, 0, angleLine.z));
            angle = Mathf.Round(angle * 100) / 100;

            if (drawAngle && visualise)
            {
                Debug.DrawLine(hit.point, angleHit.point, (Color)color);
                CreateTextInWorld(angle.ToString(), duration: duration, position: hit.point + Vector3.left);
            }
        }
        else
        {
            angle = -1;
        }

        return result;
    }
    public static void CreateFlowText(string text, float duration, Vector3 position, Color? color = null)
    {
        var tMesh = CreateTextInWorld(text, duration: duration, position: position, color: color);
        tMesh.gameObject.AddComponent<TextFlow>();
    }
    public static TextMesh CreateTextInWorld(string text, Transform parent = null, float duration = 0, Vector3 position = default(Vector3), Color? color = null, TextAnchor textAnchor = TextAnchor.MiddleCenter, TextAlignment textAlignment = TextAlignment.Center, int fontSize = 40, int sortingOrder = 5000)
    {
        if (color == null) color = Color.white;

        GameObject gameObject = new GameObject("TextMesh of " + (parent ? parent.ToString() : "nothing"), typeof(TextMesh));
        gameObject.AddComponent<TextFaceCamera>();
        Transform transform = gameObject.transform;
        if (parent != null)
            transform.SetParent(parent);
        else
            transform.SetParent(/*utility.transform*/ null);
        transform.position = position;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.characterSize = 0.1f;
        textMesh.color = (Color)color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        UnityEngine.Object.Destroy(gameObject, duration == 0 ? Time.deltaTime * 2 : duration);
        return textMesh;
    }
    public static void DrawLineWithDistance(Vector3 start, Vector3 end, Color? color = null, Transform parent = null, float duration = 0)
    {
        if (color == null) color = Color.white;
        string text = Vector3.Distance(start, end).ToString();
        CreateTextInWorld(text, duration: duration, position: Vector3.Lerp(start, end, 0.5f));
        Debug.DrawLine(start, end, (Color)color, duration);
    }
    public static void DrawSphere(Vector3 center, float radius = 0.075f, Color? color = null, float duration = 0) 
    {
        if (color == null)
            color = Color.white;
        DrawEllipse(center, Vector3.forward, Vector3.up, radius, radius, 30, (Color)color, duration);
        DrawEllipse(center, Vector3.up, Vector3.up, radius, radius, 30, (Color)color, duration);
        DrawEllipse(center, Vector3.left, Vector3.up, radius, radius, 30, (Color)color, duration);
    }
    public static void DrawBox(Vector3 center, Vector3 halfSizes, Quaternion rotation, Color? color = null, float duration = 0)
    {
        if (color == null)
            color = Color.white;

        //Смотрим сверху так, что ось X идёт вверх
        Vector3 upCenter = center + rotation * new Vector3(0, halfSizes.y, 0);
        Vector3 down = rotation * Vector3.down * halfSizes.y * 2;
        //Левый верхний
        Vector3 leftUp = rotation * Vector3.left * halfSizes.x + rotation * Vector3.forward * halfSizes.z;
        Debug.DrawLine(upCenter + leftUp, upCenter + leftUp + down, (Color)color, duration);
        //Правый Верхний
        Vector3 rightUp = rotation * Vector3.right * halfSizes.x + rotation * Vector3.forward * halfSizes.z;
        Debug.DrawLine(upCenter + rightUp, upCenter + rightUp + down, (Color)color, duration);
        //Нижний правый
        Vector3 rightBack = rotation * Vector3.right * halfSizes.x + rotation * Vector3.back * halfSizes.z;
        Debug.DrawLine(upCenter + rightBack, upCenter + rightBack + down, (Color)color, duration);
        //Нижний левый
        Vector3 leftBack = rotation * Vector3.left * halfSizes.x + rotation * Vector3.back * halfSizes.z;
        Debug.DrawLine(upCenter + leftBack, upCenter + leftBack + down, (Color)color, duration);

        //Верхние соединения
        Debug.DrawLine(upCenter + leftUp, upCenter + rightUp, (Color)color, duration);
        Debug.DrawLine(upCenter + rightUp, upCenter + rightBack, (Color)color, duration);
        Debug.DrawLine(upCenter + rightBack, upCenter + leftBack, (Color)color, duration);
        Debug.DrawLine(upCenter + leftBack, upCenter + leftUp, (Color)color, duration);

        //Нижние
        Debug.DrawLine(upCenter + leftUp + down, upCenter + rightUp + down, (Color)color, duration);
        Debug.DrawLine(upCenter + rightUp + down, upCenter + rightBack + down, (Color)color, duration);
        Debug.DrawLine(upCenter + rightBack + down, upCenter + leftBack + down, (Color)color, duration);
        Debug.DrawLine(upCenter + leftBack + down, upCenter + leftUp + down, (Color)color, duration);
    }
    public static void DrawEllipse(Vector3 pos, Vector3 forward, Vector3 up, float radiusX, float radiusY, int segments, Color color, float duration = 0)
    {
        float angle = 0f;
        Quaternion rot = Quaternion.LookRotation(forward, up);
        Vector3 lastPoint = Vector3.zero;
        Vector3 thisPoint = Vector3.zero;

        for (int i = 0; i < segments + 1; i++)
        {
            thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusX;
            thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusY;

            if (i > 0)
            {
                Debug.DrawLine(rot * lastPoint + pos, rot * thisPoint + pos, color, duration);
            }

            lastPoint = thisPoint;
            angle += 360f / segments;
        }
    }
    public static void DrawAxisVector(Vector3 vector, Vector3 from, Color? color = null, float duration = 0)
    {
        if (color == null)
            color = Color.white;

        Debug.DrawRay(from, vector.x * Vector3.right, (Color)color, duration);
        Debug.DrawRay(from, vector.y * Vector3.up, (Color)color, duration);
        Debug.DrawRay(from, vector.z * Vector3.forward, (Color)color, duration);
    }
    public static bool GetMouseInWorldObject(out Transform hitObject)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hitObject = hit.transform;
            return true;
        }

        hitObject = null;
        return false;
    }
    public static bool GetMouseInWorldCollision(out Vector3 hitPoint)
    {
        const float FAR_AWAY = 300;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, FAR_AWAY))
        {
            hitPoint = hit.point;
            return true;
        }

        hitPoint = Vector3.zero;
        return false;
    }
    public static bool ValueInArea(Vector3 input, Vector3 targetValue, float area)
    {
        return Vector3.Distance(input, targetValue) < area;
    }
    public static bool ValueInArea(float input, float targetValue, float area)
    {
        return (input >= targetValue - area) && (input <= targetValue + area);
    }
    public static bool ValueInArea(float input, float targetValue, float area, Vector2 loopBorders)
    {
        if (targetValue + area > loopBorders.y)
            return ((input >= targetValue - area) && ((input <= targetValue + area) || (input <= loopBorders.x + Mathf.Abs(targetValue + area - loopBorders.y))));
        if (targetValue - area < loopBorders.x)
            return (((input >= targetValue - area) || (input >= loopBorders.y - Mathf.Abs(loopBorders.x - targetValue - area))) && (input <= loopBorders.x + area));

        return ValueInArea(input, targetValue, area);
    }
    public static float NavMeshPathLength(NavMeshPath path)
    {
        float res = -1;
        Vector3 prevPoint = Vector3.zero;
        foreach (Vector3 point in path.corners)
        {
            if (res == -1)
            {
                res = 0;
                prevPoint = point;
                continue;
            }

            res += Vector3.Distance(prevPoint, point);

            prevPoint = point;
        }

        return res;
    }
    /// <summary>
    /// Поиск ближайшей точки на линии
    /// </summary>
    /// <param name="pointOnLine">Точка, через которую проходит линяя</param>
    /// <param name="lineDir">Направление линии</param>
    /// <param name="targetPoint">Относительно этой точки ищем ближайшую на линии</param>
    /// <returns></returns>
    public static Vector3 NearestPointOnLine(Vector3 pointOnLine, Vector3 lineDir, Vector3 targetPoint)
    {
        lineDir.Normalize();//this needs to be a unit vector
        var v = targetPoint - pointOnLine;
        var d = Vector3.Dot(v, lineDir);
        return pointOnLine + lineDir * d;
    }
    public static void DrawArrow(Vector3 from, Vector3 to, float duration = 0, Color? color = null)
    {
        Color usedColor = color == null ? Color.white : color.Value;

        const int SEGMENTS = 3;

        Debug.DrawLine(from, to, usedColor, duration);
        Vector3 circleCenter = Vector3.Lerp(from, to, 0.9f);
        float circleRadius = (to - circleCenter).magnitude / 2;

        #region modified Circle Draw (Conus) 

        float angle = 0f;
        Vector3 direction = (to - from).normalized;
        Quaternion rot = direction == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(direction, Vector3.up);
        Vector3 lastPoint = Vector3.zero;
        Vector3 thisPoint = Vector3.zero;

        for (int i = 0; i < SEGMENTS + 1; i++)
        {
            thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * circleRadius;
            thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * circleRadius;
            Debug.DrawLine(circleCenter+rot*thisPoint, to, usedColor, duration);
            if (i > 0)
            {
                Debug.DrawLine(rot * lastPoint + circleCenter, rot * thisPoint + circleCenter, usedColor, duration);
            }

            lastPoint = thisPoint;
            angle += 360f / SEGMENTS;
        }

        #endregion
    }
    public class GUI
    {
        public static TextMeshProUGUI CreateText(string text, float duration, Vector3 position)
        {
            GameObject parent = new GameObject($"WillDie : \"{text}\"");
            TextMeshProUGUI res = CreateText(text, parent.transform, textAlignment: TextAlignment.Center);
            parent.transform.position = position;
            if (duration == 0)
                duration = 0.01f;
            res.transform.LookAt(Camera.main.transform);
            GameObject.Destroy(parent, duration);
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
        public static bool IsPointerOverUIElement()
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults());
        }
        /// <summary>
        /// Returns 'true' if we touched or hovering on Unity UI element.
        /// </summary>
        /// <param name="eventSystemRaysastResults"></param>
        /// <returns></returns>
        public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
        {
            int UILayer = LayerMask.NameToLayer("UI");

            for (int index = 0; index < eventSystemRaysastResults.Count; index++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[index];
                if (curRaysastResult.gameObject.layer == UILayer)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all event system raycast results of current mouse or touch position.
        /// </summary>
        /// <returns></returns>
        public static List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            return raysastResults;
        }
    }
    public class Editor
    {
        //https://discussions.unity.com/t/convert-serializedproperty-to-custom-class/94163/4

        private static int serializationDepth = 0; //Переменная-регистр, что постоянно используется в сериализации
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
                char[] indexChar = splitted[serializationDepth+1].ToString().Where(c => char.IsDigit(c)).ToArray();

                int index = Convert.ToInt32(new string(indexChar));
                var handledList = (IList)obj;
                serializationDepth++; // Skipping one step, because array elements are formed by Array.data[{index}], two things.
                return (T)handledList[index];
            }

            //TODO : При решении закинуть его на форум (ссылка в начале Utilities.Editor)

            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field != null) return (T)field.GetValue(obj);

            PropertyInfo property = obj.GetType().GetProperty(fieldName, bindings);
            if (property != null) return (T)property.GetValue(obj, null);

            if (includeAllBases)
            {
                foreach (Type type in GetBaseClassesInterfacesExtensions.GetBaseClassesAndInterfaces(obj.GetType()))
                {
                    field = type.GetField(fieldName, bindings);
                    if (field != null) return (T)field.GetValue(obj);

                    property = type.GetProperty(fieldName, bindings);
                    if (property != null) return (T)property.GetValue(obj, null);
                }
            }

            return default;
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
                foreach (Type type in GetBaseClassesInterfacesExtensions.GetBaseClassesAndInterfaces(obj.GetType()))
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
