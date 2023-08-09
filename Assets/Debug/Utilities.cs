using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Utilities
{
    //Краткая запись, дабы сократить сильнее нужен TODO : Ray.
    public static bool VisualisedBoxCast(Vector3 center, Vector3 halfExtends, Vector3 direction, float maxDistance, LayerMask layerMask, bool drawHit, Color? color = null, float duration = 0)
    {
        return VisualisedBoxCast(center, halfExtends, direction, out _, Quaternion.identity, maxDistance, layerMask, drawHit, color, duration);
    }

    //TODO : Поменять местами out и Quaternion
    public static bool VisualisedBoxCast(Vector3 center, Vector3 halfExtends, Vector3 direction, out RaycastHit hitInfo, Quaternion orientation, float maxDistance, LayerMask layerMask, bool drawHit = true, Color? color = null, float duration = 0, bool visualise = true)
    {
        //TODO: Визуализация поворота коробки
        if (color == null)
            color = Color.white;

        if (visualise)
        {
            DrawSphere(center, 0.1f, color, duration);
            Debug.DrawRay(center, direction * maxDistance, color.Value, duration);
        }

        /*
         Utilities.DrawSphere(transform.position + movementDirection.normalized * stepDistance, 0.01f);
        Debug.DrawRay(transform.position + movementDirection.normalized * stepDistance, halfWidth + halfLength + Vector3.up * 0.01f);
        Debug.DrawRay(transform.position + movementDirection.normalized * stepDistance, Vector3.up* toGroundStickDistance);
         */

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

    //TODO : Сделать всё с Ray!
    public static bool VisualisedRaycast(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit hit, LayerMask layerMask, bool drawHit = true, Color? color = null, float duration = 0, bool visualise = true)
    {
        if (color == null)
            color = Color.white;

        bool result = Physics.Raycast(origin, direction, out hit, maxDistance, layerMask);

        if (drawHit && result)
        {
            Debug.DrawLine(origin, hit.point, (Color)color, duration);
            DrawSphere(hit.point, 0.075f, (Color)color, duration);
        }

        if (!result)
        {
            Debug.DrawRay(origin, direction.normalized * maxDistance, Color.red, duration);
        }

        return result;
    }

    //С подсчётом угла
    public static bool VisualisedRaycast(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit hit, out float angle, LayerMask layerMask, bool drawAngle = true, bool drawHit = true, Color? color = null, float duration = 0, bool visualise = true)
    {
        bool result = VisualisedRaycast(origin, direction, maxDistance, out hit, layerMask, drawHit, color, duration, visualise);

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
                CreateTextInWorld(angle.ToString(), duration, hit.point + Vector3.left);
            }
        }
        else
        {
            angle = -1;
        }

        return result;
    }

    public static TextMesh CreateTextInWorld(string text, float duration, Vector3 position)
    {
        GameObject parent = new GameObject($"WillDie : \"{text}\"");
        TextMesh res = CreateTextInWorld(text, parent.transform, textAnchor: TextAnchor.MiddleCenter, textAlignment: TextAlignment.Center);
        parent.transform.position = position;
        if (duration == 0)
            duration = 0.05f;
        res.transform.LookAt(Camera.main.transform);
        Object.Destroy(parent, 0.1f);
        return res;
    }
    public static TextMesh CreateTextInWorld(string text, Transform parent = null, Vector3 localOffset = default(Vector3), Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left, int fontSize = 40, int sortingOrder = 5000)
    {
        if (color == null) color = Color.gray;

        GameObject gameObject = new GameObject("TextMesh of " + (parent ? parent.ToString() : "nothing"), typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localOffset;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = (Color)color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }

    public static void DrawSphere(Vector3 center, float radius = 0.075f, Color? color = null, float duration = 0) //TODO : Не проверено! 
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 200))
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
        public static bool IsPointerOverUIElement()
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults());
        }

        //Returns 'true' if we touched or hovering on Unity UI element.
        static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
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
