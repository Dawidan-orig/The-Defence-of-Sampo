using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WingedCore.Core.Utility;

namespace WingedCore.Core.Utility
{
    public class BoxCastHelper
    {
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
                DebugVisualsHelper.DrawSphere(center, 0.1f, color, duration);
                Debug.DrawRay(center, direction * maxDistance, color.Value, duration);
            }

            bool result = Physics.BoxCast(center, halfExtends, direction, out hitInfo, orientation, maxDistance, layerMask);

            if (drawHit && result && visualise)
            {
                DebugVisualsHelper.DrawSphere(hitInfo.point, 0.1f, Color.white, duration);
            }

            if (visualise)
            {
                if (result)
                {
                    DebugVisualsHelper.DrawBox(center, halfExtends, orientation, Color.green, duration);
                    Debug.DrawLine(center, hitInfo.point, (Color)color, duration);
                }
                else
                    DebugVisualsHelper.DrawBox(center, halfExtends, orientation, Color.red);
            }

            return result;
        }

    }
}