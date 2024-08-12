using UnityEngine;
using WingedCore.Core.Utility;

namespace WingedCore.Core.Utility
{
    public class RaycastHelper
    {
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
                DebugVisualsHelper.DrawSphere(hit.point, 0.075f, (Color)color, duration);
            }

            if (!result && visualise)
            {
                Debug.DrawRay(origin, direction.normalized * maxDistance, Color.red, duration);
            }

            return result;
        }
        public static bool VisualisedRaycast(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit hit, out float angle, LayerMask layerMask, Color? color = null, float duration = 0, bool drawAngle = true, bool drawHit = true, bool visualise = true)
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
                    DebugVisualsHelper.CreateTextInWorld(angle.ToString(), duration: duration, position: hit.point + Vector3.left);
                }
            }
            else
            {
                angle = -1;
            }

            return result;
        }
    }
}