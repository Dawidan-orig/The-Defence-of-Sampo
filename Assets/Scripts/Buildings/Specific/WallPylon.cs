using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Building
{
    public class WallPylon : BuildableStructure
    {
        [Header("Specific parameters")]
        public GameObject wallSegmentPrefab;

        private List<GameObject> _underControl = new();

        protected override void Start()
        {
            base.Start();
            WallPylon otherWall = BuildingSystem.Instance.CurrentWallInFocus;

            if (otherWall)
            {
                var wall = Instantiate(wallSegmentPrefab, transform.position, Quaternion.identity, transform);
                wall.GetComponent<Faction>().ChangeFactionCompletely(GetComponent<Faction>().FactionType);
                _underControl.Add(wall);
                wall.GetComponent<WallSegment>().ArrangeSegment(otherWall.transform.position, wallSegmentPrefab, transform);                
            }
            
            BuildingSystem.Instance.CurrentWallInFocus = this;
        }

        protected override void Build()
        {

        }
    }
}
