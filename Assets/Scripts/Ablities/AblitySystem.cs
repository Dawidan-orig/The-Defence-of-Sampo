using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Abilities
{
    public class AblitySystem : MonoBehaviour
    {
        //TODO dep. Player : добавление новых способностей hardcoded в Awake, а через внешнюю систему. Сделать это, когда будет готова система классов игрока
        public List<Ability> abilities;
        public LayerMask Collidables;

        private void Awake()
        {
            abilities = new List<Ability>();

            ProceedingSlash slash = new ProceedingSlash(transform);
            Blow blow = new Blow(transform);
            WindSlide slide = new WindSlide(transform);
            FixedAscention ult_Ascension = new FixedAscention(transform);

            AddNewAbility(slash);
            AddNewAbility(blow);
            AddNewAbility(slide);
            AddNewAbility(ult_Ascension);

            slash.layers = Collidables;
        }

        private void Start()
        {
            foreach (var ability in abilities) { ability.Enable(); }
        }

        private void Update()
        {
            foreach (var ability in abilities) { ability.Update(); }

            int inputAbility = (int)KeyCode.Alpha1;

            for (int i = 0; i < abilities.Count; i++)
                if (Input.GetKeyDown((KeyCode)(i + inputAbility)))
                    abilities[i].Activate();
        }

        private void FixedUpdate()
        {
            foreach (var ability in abilities) { ability.FixedUpdate(); }
        }

        public void AddNewAbility(Ability a) 
        {
            abilities.Add(a);
        }
    }
}