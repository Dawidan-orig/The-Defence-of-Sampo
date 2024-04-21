using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.AI.Humans
{
    public class MultiweaponUnit : AIBehaviourBase
    {
        [Tooltip("�������� � ���� ������ ��� GameObject-������, ������������ ������")]
        public List<GameObject> unitReferencePrefabs = new();
        public float behaviourUpdateFrequency = 10;

        private Transform kitContainer;

        [SerializeField]
        private List<AIBehaviourBase> behaviours;
        private Dictionary<AIBehaviourBase, Transform> behaviourToChild;
        [SerializeField]
        private AIBehaviourBase currentBehaviour;

        protected override void Awake()
        {
            base.Awake();

            if (!kitContainer)
            {
                kitContainer = (new GameObject("Weaponry kits")).transform;
                kitContainer.parent = transform;
                kitContainer.localPosition = Vector3.zero;
            }

            Initialize();
        }

        private void Initialize()
        {
            behaviours = new List<AIBehaviourBase>();
            behaviourToChild = new Dictionary<AIBehaviourBase, Transform>();

            if (unitReferencePrefabs.Count == 0)
            {
                Debug.LogError("����������� �������� ������");
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
                ChangeToBestWeapon();

                yield return new WaitForSeconds(behaviourUpdateFrequency);
            }
        }

        private AIBehaviourBase ChooseBestWeapon() 
        {
            if (behaviours.Count > 1)
                behaviours.Sort((beh1, beh2) => beh2.GetCurrentWeaponPoints().CompareTo(beh1.GetCurrentWeaponPoints()));
            return behaviours[0];
        }

        private void ChangeToBestWeapon()
        {
            ChangeBehavoiur(ChooseBestWeapon());
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
            beh.BehaviourWeapon.Host = GetMainTransform().transform;
            behaviours.Add(beh);
            behaviourToChild.Add(beh, copy.transform);

            _AITargeting.AddNewActionsFromBehaviour(beh);
        }
        #region AIBehaviour overrides
        public override void ActionUpdate(Transform target)
        {
            currentBehaviour.ActionUpdate(target);
        }

        public override Vector3 RelativeRetreatMovement()
        {
            //��� ������� �� ������� ���������� ������
            return currentBehaviour.RelativeRetreatMovement();
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
            Dictionary<Interactable_UtilityAI, int> res = new();

            foreach (var beh in behaviours)
                foreach(var kvp in beh.GetActionsDictionary())
                res.Add(kvp.Key, kvp.Value);

            return res;
        }
        public override bool IsTargetPassing(Transform target)
        {
            foreach (var beh in behaviours)
                if (beh.IsTargetPassing(target))
                    return true;

            return false;
        }
        // ������ MultiweaponUnit'�W
        // ����� ��������� ������,
        // ����� ����������� - ������������ ����������� ���������
        // ����� ����������� - ������������ ������ ����������� ���� ����������
        // ��� ��������� �����.
        // � �� ��� �� ���� ����
        
        // TODO : IsTargetPassing � ��� ���� ���-������ ����������
        public override AIBehaviourBase TargetReaction(Transform target)
        {
            return ChooseBestWeapon();
        }
        #endregion
    }
}