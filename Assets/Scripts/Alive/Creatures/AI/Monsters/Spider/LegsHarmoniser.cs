using System;   
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI.Monsters.Spider
{
    public class LegsHarmoniser : MonoBehaviour
    {
        public List<SpiderLegControl> legs = new();
        public Behaviour current = Behaviour.random;

        [Header("Unification")]
        public float maxDistToNew = 6;
        public float distanceToNew = 4;
        public float moveSpeed = 2;
        public float legsLength = 18;
        public AnimationCurve legMovement;
        public Transform mainBody;

        public List<LegsPair> legPairs = new();

        [SerializeField]
        public List<List<SpiderLegControl>> groups = new();
        private int currentActiveGroup = -1;

        public enum Behaviour
        {
            random, // Каждая отдельная нога добавляется в свою группу
            zigzag, // Все ноги разделяются на две группы по шахматнаму принципу
            parallel // Две группы - левые и правые ноги
        }

        [Serializable]
        public struct LegsPair
        {
            public SpiderLegControl left;
            public SpiderLegControl right;
        }

        private void OnValidate()
        {
            ReGroup();
            AssignValues();
        }

        private void Awake()
        {
            AssignValues();
        }

        private void Start()
        {
            ReGroup();
        }

        void Update()
        {
            if (currentActiveGroup == -1 || currentActiveGroup > groups.Count)
            {
                CheckForNewReadyGroup();
            }
            else
            {
                UpdateActiveGroup();
            }
        }

        private void CheckForNewReadyGroup()
        {
            int groupIndex = 0;
            foreach (List<SpiderLegControl> group in groups)
            {
                foreach (SpiderLegControl leg in group)
                {
                    if (leg.readyToMove)
                    {
                        currentActiveGroup = groupIndex;
                        foreach (SpiderLegControl toMove in groups[currentActiveGroup])
                        {
                            leg.BeginMove();
                        }
                        return;
                    }
                }

                groupIndex++;
            }
        }

        private void UpdateActiveGroup()
        {
            foreach (SpiderLegControl leg in groups[currentActiveGroup])
                if (!leg.stable)
                    return;

            currentActiveGroup = -1;
        }

        //TODO? : Editor, чтобы чётко видеть как в списке идёт разделение на группы
        private void ReGroup()
        {
            groups.Clear();
            legPairs.Clear();

            for (int curIndex = 0; curIndex < legs.Count / 2; curIndex++)
            {
                LegsPair lPair = new LegsPair();
                lPair.left = legs[curIndex];
                lPair.right = legs[curIndex + legs.Count / 2];
                legPairs.Add(lPair);
            }

            if (current == Behaviour.zigzag)
            {
                groups.Add(new List<SpiderLegControl>());
                groups.Add(new List<SpiderLegControl>());

                bool left = false;
                foreach (SpiderLegControl leg in legs)
                {
                    int groupIndex = left ? 1 : 0;

                    groups[groupIndex].Add(leg);

                    left = !left;
                }
            }
            else if (current == Behaviour.random)
            {
                int index = 0;
                foreach (SpiderLegControl leg in legs)
                {
                    groups.Add(new List<SpiderLegControl>());
                    groups[index++].Add(leg);
                }
            }
            else if (current == Behaviour.parallel)
            {
                int curParallelIndex = 0;
                groups.Add(new List<SpiderLegControl>());
                groups.Add(new List<SpiderLegControl>());
                foreach (SpiderLegControl leg in legs)
                    groups[legs.Count / 2 < curParallelIndex++ ? 0 : 1].Add(leg);
            }
        }

        public void AssignValues()
        {
            foreach (SpiderLegControl leg in legs)
            {
                leg.limb.SetHost(mainBody);
                leg.distanceToNew = distanceToNew;
                leg.maxDistToNew = maxDistToNew;
                leg.moveSpeed = moveSpeed;
                leg.legMovement = legMovement;
                leg.legLength = legsLength;
                leg.limb.GetComponent<AliveBeing>().mainBody = mainBody;
            }
        }
    }
}