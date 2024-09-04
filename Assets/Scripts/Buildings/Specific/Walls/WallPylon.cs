using WingedCore.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.Building
{
    public class WallPylon : BuildableStructure
    {
        [Header("Specific parameters")]
        public GameObject wallSegmentPrefab;
        public bool shouldFocus = true;

        [Header("Editor Pre-Playmode Only")]

#if UNITY_EDITOR
        //TODO : Добавить этот функционал для тестирований.
        public List<GameObject> wallsToConnect = new List<GameObject>();
#endif

        protected override void Start()
        {
            base.Start();

            if (wallsToConnect.Count > 0)
            {
                foreach (var pylon in wallsToConnect)
                {
                    var wall = Instantiate(wallSegmentPrefab, transform.position, Quaternion.identity, transform);
                    wall.GetComponent<AITarget>().ChangeFactionCompletely(GetComponent<AITarget>().FactionType);
                    wall.GetComponent<WallSegment>().ArrangeSegment(pylon.transform.position, wallSegmentPrefab, transform);
                }
                return;
            }

            WallPylon otherWall = MonoBehaviourSingleton<BuildingSystem>.Instance.CurrentWallInFocus;

            if (otherWall)
            {
                var wall = Instantiate(wallSegmentPrefab, transform.position, Quaternion.identity, transform);
                wall.GetComponent<AITarget>().ChangeFactionCompletely(GetComponent<AITarget>().FactionType);
                wall.GetComponent<WallSegment>().ArrangeSegment(otherWall.transform.position, wallSegmentPrefab, transform);                
            }

            if (shouldFocus)
                MonoBehaviourSingleton<BuildingSystem>.Instance.CurrentWallInFocus = this;
            else
                MonoBehaviourSingleton<BuildingSystem>.Instance.CurrentWallInFocus = null;
        }

        protected override void Build()
        {

        }
    }
}
