using WingedCore.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sampo.Economy
{
    public class EconomySystem : MonoBehaviour
    {
        //TODO UI : Окно (Панель) экономики.
        [SerializeField]
        private int hunting = 500;
        [SerializeField]
        private int crop = 500;
        [SerializeField]
        private int cattleMeat = 500;
        [SerializeField]
        private int cattleProd = 500;

        /// <summary>
        /// Возвращает наименьшее значение ресурса, устанавливает для всех.
        /// </summary>
        private int Overall
        {
            get {
                return Mathf.Min(Mathf.Min(hunting, crop), Mathf.Min(cattleMeat, cattleProd));
            }
            set
            {
                hunting = value;
                crop = value;
                cattleMeat = value;
                cattleProd = value;
            }
        }
        public void AddToAll(int amount) 
        {
            Overall += amount;
        }
        public bool Spend(int amount)
        {
            WingedCore.Core.JournalLogger.LoggerSystem.DebugLog("Spent " + amount + "of all resources", gameObject);

            if (Overall > amount)
            {
                Overall -= amount;
                return true;
            }
            else
                return false;
        }
    }
}