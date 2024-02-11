using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Building
{
    public class WallPylon : BuildableStructure
    {
        [Header("Specific parameters")]
        public GameObject wallSegmentPrefab;
        public bool shouldFocus = true;

        [Header("Editor Pre-Playmode Only")]
        public WallPylon wallToConnect;

        protected override void Start()
        {
            base.Start();
            WallPylon otherWall = wallToConnect ? wallToConnect : BuildingSystem.Instance.CurrentWallInFocus;

            if (otherWall)
            {
                var wall = Instantiate(wallSegmentPrefab, transform.position, Quaternion.identity, transform);
                wall.GetComponent<Faction>().ChangeFactionCompletely(GetComponent<Faction>().FactionType);
                wall.GetComponent<WallSegment>().ArrangeSegment(otherWall.transform.position, wallSegmentPrefab, transform);                
            }

            if (shouldFocus)
                BuildingSystem.Instance.CurrentWallInFocus = this;
            else
                BuildingSystem.Instance.CurrentWallInFocus = null;
        }

        protected override void Build()
        {

        }
    }
}
