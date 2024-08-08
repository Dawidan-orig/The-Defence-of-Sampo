using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.AI.CounterSystem {
    public class CounterTagLocal : MonoBehaviour
    {
        [SerializeField]
        private List<CounterNode> containedTagsRoles = new List<CounterNode>();

        public float Check(CounterTagLocal other) 
        {
            float resModifier = 1;

            //TODO : Придумать оптимизацию. Вызов этой функции наверняка будет делаться очень часто.
            foreach (CounterNode node in containedTagsRoles) 
            {
                foreach(CounterNode otherNode in other.containedTagsRoles) 
                {
                    if(CounterSystem.Instance.IsCounter(node, otherNode, out float mod)) 
                    {
                        resModifier *= mod;
                    }
                }
            }

            return resModifier;
        }

        public void AddNewRole(CounterNode role) 
        {
            containedTagsRoles.Add(role);
        }

        public void RemoveRole(CounterNode role)
        {
            containedTagsRoles.Remove(role);
        }
    }
}