using Sampo.Core.JournalLogger;
using Sampo.Weaponry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sampo.AI.Humans
{
    /// <summary>
    /// Умеет выбирать оружие относительно их настроек
    /// </summary>
    public class MultiweaponUnit : AIBehaviourBase
    {
        //TODO : Если пуст - добавить NullUnitKit через Resources

        // Пример MultiweaponUnit'а
        // Можно атаковать магией,
        // Можно действовать - активировать способность заморозки
        // Можно действовать - активировать другую способность поле отравления
        // Или атаковать мечом.
        // И всё это на одну цель
        [Tooltip("Добавьте в этот список все GameObject-наборы, определяющие юнитов")]
        public List<GameObject> unitReferencePrefabs = new();
        public float behaviourUpdateFrequency = 10;

        private Transform kitContainer;

        [SerializeField]
        private List<AIBehaviourBase> behaviours;
        private Dictionary<AIBehaviourBase, Transform> behaviourToChild;
        [SerializeField]
        private AIBehaviourBase currentBehaviour;

        public override Tool BehaviourWeapon => currentBehaviour.BehaviourWeapon;

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
                //TODO : Сделать сюда автоматическую загрузку null-unit
                Debug.LogError("Отсутствуют заданные классы");
                return;
            }

            foreach (var reference in unitReferencePrefabs)
                AddNewBehaviour(reference);

            _AITargeting.ChangedToNewAction += OnNewAction;

            StartCoroutine(CheckingCycle());
        }
        private void OnNewAction(object sender, EventArgs _) 
        {
            ChangeToBestWeapon();
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
            string dataToDebugLog = "Выбор лучшего оружия:\n";

            //Event не успевает удалить действие у TargetingAI,
            // а тут уже происходит выбор по target=null
            if (CurrentActivity.target == null)
            {
                dataToDebugLog += "Отмена, цели нет";
                LoggerSingleton.DebugLog(dataToDebugLog, gameObject);
                return currentBehaviour;
            }

            List<AIBehaviourBase> behavioursSorted = behaviours.Where(beh => beh.IsTargetPassing(CurrentActivity.target))
                .ToList();

            dataToDebugLog += "Для цели " + CurrentActivity.target.name + "\n";

            //TODO : Костыльная затычка. Следует исправить.
            // behavioursSorted = null почему-то, но не всегда. 
            // С чем это связано - предстоит узнать, т.к. ломается вообще всё
            if (behavioursSorted.Count == 0)
                return currentBehaviour;

            if (behavioursSorted.Count > 1)
                behavioursSorted.Sort((beh1, beh2) => beh2.GetCurrentWeaponPoints().CompareTo(beh1.GetCurrentWeaponPoints()));

            foreach(var behaviour in behavioursSorted) 
            {
                dataToDebugLog += behaviour.GetType().ToString() + "->" + behaviour.GetCurrentWeaponPoints() + "\n";
            }
            LoggerSingleton.DebugLog(dataToDebugLog, gameObject, CurrentActivity.target.gameObject);

            return behavioursSorted[0];
        }
        private void ChangeToBestWeapon()
        {
            ChangeBehavoiur(ChooseBestWeapon());
            HasCongestion = currentBehaviour.HasCongestion;
            congestionInfluence = currentBehaviour.congestionInfluence;
            distanceInfluence = currentBehaviour.distanceInfluence;
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
            if (!currentBehaviour)
                currentBehaviour = beh;
            if (beh.BehaviourWeapon)
                beh.BehaviourWeapon.Host = GetMainTransform().transform;
            behaviours.Add(beh);
            behaviourToChild.Add(beh, copy.transform);
            copy.SetActive(false);

            _AITargeting.AddNewActionsFromBehaviour(beh);

            ChangeToBestWeapon();
        }

        #region AIBehaviour overrides
        public override Vector3 RelativeRetreatMovement()
        {
            //Это зависит от текщего выбранного оружия
            return currentBehaviour.RelativeRetreatMovement();
        }
        public override int GetCurrentWeaponPoints()
        {
            int sum = 0;
            foreach (var beh in behaviours)
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
                foreach (var kvp in beh.GetActionsDictionary())
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
        #endregion
    }
}