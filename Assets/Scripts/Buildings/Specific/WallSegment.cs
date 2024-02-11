using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Building
{
    public class WallSegment : BuildableStructure
    {
        [Header("Specific parameters")]
        public GameObject column;

        public Transform mainObject;
        public GameObject prefabSample;

        static int iterations = 0;

        const float CLOSE_ENOUGH = 0.1f;

        public void ArrangeSegment(Vector3 to, GameObject samplePrefab, Transform parent) 
        {
            iterations = 0;

            mainObject = parent;
            prefabSample = samplePrefab;

            ArrangeSegment(transform.position, to);
        }

        private void ArrangeSegment(Vector3 from, Vector3 to) 
        {
            iterations++;
            const int BREAKPOINT = 100;
            if(iterations > BREAKPOINT) 
            {
                throw new StackOverflowException("—лишком много итераций создани€ стен");
            }

            if (Utilities.ValueInArea(transform.position, to, CLOSE_ENOUGH))
                return;            

            to.y = from.y;

            bool nextPosIsOpen = !ArrangedOnLowGround(from, to);
            if(nextPosIsOpen)
                nextPosIsOpen = !ArrangedOnHighGround(from, to);

            if (nextPosIsOpen)
                DisplaceSegment(to);                
        }

        private bool ArrangedOnLowGround(Vector3 from, Vector3 to) 
        {
            float resolution = 0.25f;
            Vector3 current = from;

            float HEIGHT = 10f;

            while(!Utilities.ValueInArea(current, to, CLOSE_ENOUGH)) 
            {
                current += (to - from).normalized * resolution;

                if (Physics.Raycast(current + Vector3.up * possibleHeightToBuild, Vector3.down, out var hit, HEIGHT, ground))
                {
                    if (hit.point.y < current.y)
                    {
                        DisplaceSegment(hit.point);
                        Vector3 newPos = hit.point + Vector3.down * possibleHeightToBuild/2;

                        GameObject go = Instantiate(prefabSample, newPos, Quaternion.identity, mainObject);

                        WallSegment nextWall = go.GetComponent<WallSegment>();
                        nextWall.mainObject = mainObject;
                        nextWall.prefabSample = prefabSample;
                        nextWall.GetComponent<Faction>().ChangeFactionCompletely(mainObject.GetComponent<Faction>().FactionType);
                        nextWall.ArrangeSegment(newPos, to);
                        return true;
                    }
                }
                else
                {
                    Debug.Log("ѕри поиске возможности строить стену вниз не оказалось земли");
                    return false;
                }
            }

            return false;
        }

        private bool ArrangedOnHighGround(Vector3 from, Vector3 to) 
        {
            Vector3 offset = Vector3.up * possibleHeightToBuild;

            if (Physics.Raycast(from + offset, (to - from + offset).normalized, out var hit, (to - from + offset).magnitude, ground))
            {
                DisplaceSegment(hit.point);
                Vector3 newPos = hit.point;
                var prefabFaction = prefabSample.GetComponent<Faction>();
                prefabFaction.ChangeFactionCompletely(mainObject.GetComponent<Faction>().FactionType);

                GameObject go = Instantiate(prefabSample, newPos, Quaternion.identity, mainObject);

                prefabFaction.ChangeFactionCompletely(Faction.FType.neutral);
                WallSegment nextWall = go.GetComponent<WallSegment>();
                nextWall.mainObject = mainObject;
                nextWall.prefabSample = prefabSample;

                nextWall.ArrangeSegment(newPos, to);
                return true;
            }

            return false;
        }

        protected override void Build()
        {
            
        }

        private void DisplaceSegment(Vector3 to) 
        {
            to.y = 0;
            Vector3 from = transform.position;
            from.y = 0;

            DestructableStructure sturc = GetComponent<DestructableStructure>();
            sturc.health *= Vector3.Distance(from, to);

            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y,
                Vector3.Distance(from, to));

            column.transform.localScale = new Vector3(column.transform.localScale.x, column.transform.localScale.y,
                column.transform.localScale.z * (1 / transform.localScale.z));

            transform.LookAt(to);
            Vector3 euler = transform.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            transform.rotation = Quaternion.Euler(euler);
        }
    }
}