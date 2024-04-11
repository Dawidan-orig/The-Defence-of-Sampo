using Sampo.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Building
{
    /// <summary>
    /// Структура, что способна удерживать в себе неактивных юнитов.
    /// Это позволит запасать их для разных задач, а так же манипулировать как группами
    /// </summary>
    public class BufferingHouse : BuildableStructure, IInteractable
    {
        public int bufferingAmount = 10;
        public Transform releasePos;
        public Transform unitContainer;

        List<GameObject> contained;

        protected override void Build()
        {
            contained = new List<GameObject>();

            unitContainer = Variable_Provider.Instance.unitsContainer;
        }

        public void Contain(GameObject obj) 
        {
            obj.SetActive(false);
            obj.transform.parent = transform;
            contained.Add(obj);
        }

        public void Release(GameObject obj) 
        {
            obj.transform.position = releasePos.position;
            obj.transform.rotation = releasePos.rotation;
            obj.transform.parent = unitContainer;
            obj.SetActive(true);
        }

        public void ReleaseAll() 
        {
            foreach(GameObject obj in contained) 
            {
                Release(obj);
            }
        }

        private void OnDestroy()
        {
            ReleaseAll();
        }

        public void Interact(Transform interactor)
        {
            Contain(interactor.gameObject);
        }

        public void PlayerInteract()
        {
            //TODO : Настройка через интерфейс
        }
    }
}