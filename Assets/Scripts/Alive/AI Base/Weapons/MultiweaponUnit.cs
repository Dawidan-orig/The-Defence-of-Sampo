using Sampo.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI.Humans
{
    public class MultiweaponUnit : AIBehaviourBase
    {
        [Tooltip("Добавьте в этот список все GameObject-наборы, определяющие юнитов")]
        public List<GameObject> unitReferencePrefabs = new();
        public float behaviourUpdateFrequency = 10;

        private Transform kitContainer;

        private List<AIBehaviourBase> behaviours;
        private Dictionary<AIBehaviourBase, Transform> behaviourToChild;
        private AIBehaviourBase currentBehaviour;

        protected override void Awake()
        {
            base.Awake();

            kitContainer = (new GameObject("Weaponry kits")).transform;

            Initialize();
        }

        private void Initialize()
        {
            behaviours = new List<AIBehaviourBase>();
            behaviourToChild = new Dictionary<AIBehaviourBase, Transform>();

            if (unitReferencePrefabs.Count == 0)
            {
                Debug.LogError("Отсутствуют заданные классы");
                return;
            }

            foreach (var reference in unitReferencePrefabs)
            {
                GameObject copy = Instantiate(reference, kitContainer);
                AIBehaviourBase beh = copy.GetComponent<AIBehaviourBase>();
                behaviours.Add(beh);
                behaviourToChild.Add(beh, copy.transform);
                copy.SetActive(false);
            }

            StartCoroutine(CheckingCycle());
        }

        private IEnumerator CheckingCycle()
        {
            while (true)
            {
                ChooseBestWeapon();

                yield return new WaitForSeconds(behaviourUpdateFrequency);
            }
        }

        private void ChooseBestWeapon()
        {
            behaviours.Sort((beh1, beh2) => beh2.GetCurrentWeaponPoints().CompareTo(beh1.GetCurrentWeaponPoints()));
            ChangeBehavoiur(behaviours[0]);
        }

        private void ChangeBehavoiur(AIBehaviourBase to) 
        {
            currentBehaviour?.gameObject.SetActive(false);
            to.gameObject.SetActive(true);
            currentBehaviour = to;
        }

        public void AddNewBehaviour(GameObject AIKit)
        {
            GameObject copy = Instantiate(AIKit, kitContainer);
            AIBehaviourBase beh = copy.GetComponent<AIBehaviourBase>();
            behaviours.Add(beh);
            behaviourToChild.Add(beh, copy.transform);
        }
        #region AIBehaviour overrides
        public override void ActionUpdate(Transform target)
        {
            currentBehaviour.ActionUpdate(target);
        }

        public override void AttackUpdate(Transform target)
        {
            currentBehaviour.AttackUpdate(target);
        }

        public override Vector3 RelativeRetreatMovement()
        {
            //Это зависит от текщего выбранного оружия
            return currentBehaviour.RelativeRetreatMovement();
        }

        public override Tool ToolChosingCheck(Transform target)
        {
            //TODO : Выбрать наиболее подходящее оружие среди всех Behavour для этого target.
            // Пусть оно будет приоретизироваться среди остальных в виде бонуса очков при выборе.
            // Пусть юнит всё ещё может менять оружие на другое
            return currentBehaviour?.ToolChosingCheck(target);
        }

        public override int GetCurrentWeaponPoints()
        {
            int sum = 0;
            foreach(var beh in behaviours)            
                sum += beh.GetCurrentWeaponPoints();
            
            return sum;
        }
        public override Transform GetRightHandTarget()
        {
            return currentBehaviour.GetRightHandTarget();
        }
        public override Dictionary<Interactable_UtilityAI, int> GetActionsDictionary()
        {
            return currentBehaviour.GetActionsDictionary();
        }
        #endregion
    }
}