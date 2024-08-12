using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.Core.Utility
{
    public class PhysicsHelper : MonoBehaviour
    {
        public static bool GetMouseInWorldObject(int criticalDist, out Transform hitObject)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, criticalDist))
            {
                hitObject = hit.transform;
                return true;
            }

            hitObject = null;
            return false;
        }
        public static bool GetMouseInWorldCollision(int criticalDist, out Vector3 hitPoint)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, criticalDist))
            {
                hitPoint = hit.point;
                return true;
            }

            hitPoint = Vector3.zero;
            return false;
        }
        /// <summary>
        /// Возвращает все слои, пригодные для raycast, в соответствии с коллизиями объекта
        /// </summary>
        /// <param name="ofObject"></param>
        /// <returns></returns>
        public static LayerMask GetRaycastOfCollisionLayers(GameObject ofObject) 
        {
            int objectLayer = ofObject.layer;
            int layerMask = Physics.AllLayers;

            for(int i = 0;i < 32; i++) 
                if (Physics.GetIgnoreLayerCollision(objectLayer, i))
                    layerMask &= ~(1 << i);

            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            layerMask &= ~(1 << ignoreRaycastLayer);

            return layerMask;
        }
    }
}