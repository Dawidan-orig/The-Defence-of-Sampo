using Sampo.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sampo.Player.Economy
{
    public class EconomySystem : MonoBehaviour
    {
        private static EconomySystem _instance;
        public static EconomySystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<EconomySystem>();

                if (_instance == null)
                {
                    GameObject go = new("Economy");
                    _instance = go.AddComponent<EconomySystem>();
                }

                if (EditorApplication.isPlaying)
                {
                    _instance.transform.parent = null;
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

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

        public bool Spend(int amount)
        {
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