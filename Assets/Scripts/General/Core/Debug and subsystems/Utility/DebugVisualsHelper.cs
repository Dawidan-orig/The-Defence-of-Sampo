using TMPro;
using UnityEditor;
using UnityEngine;

namespace WingedCore.Core.Utility
{
    public class DebugVisualsHelper
    {
        public static void CreateFlowText(string text, float duration, Vector3 position, Color? color = null)
        {
            var tMesh = CreateTextInWorld(text, duration: duration, position: position, color: color);
            tMesh.gameObject.AddComponent<TextFlow>();
        }
        public static TextMeshPro CreateTextInWorld(string text, Transform parent = null, float duration = 0, Vector3 position = default(Vector3), Color? color = null, TextAnchor textAnchor = TextAnchor.MiddleCenter, TextAlignmentOptions textAlignment = TextAlignmentOptions.Center, int fontSize = 40, int sortingOrder = 5000)
        {
            if (!(EditorApplication.isPlaying && !EditorApplication.isPaused)) return null;

            if (color == null) color = Color.white;

            GameObject gameObject = new GameObject("TextMesh of " + (parent ? parent.ToString() : "nothing"), typeof(TextMesh));
            gameObject.AddComponent<TextFaceCamera>();
            Transform transform = gameObject.transform;
            if (parent != null)
                transform.SetParent(parent);
            else
                transform.SetParent(/*utility.transform*/ null);
            transform.position = position;
            TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
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
                Debug.DrawLine(circleCenter + rot * thisPoint, to, usedColor, duration);
                if (i > 0)
                {
                    Debug.DrawLine(rot * lastPoint + circleCenter, rot * thisPoint + circleCenter, usedColor, duration);
                }

                lastPoint = thisPoint;
                angle += 360f / SEGMENTS;
            }

            #endregion
        }
    }
}