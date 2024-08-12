using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace WingedCore.Core.Utility
{
    public class MathHelper : MonoBehaviour
    {
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
    }
}