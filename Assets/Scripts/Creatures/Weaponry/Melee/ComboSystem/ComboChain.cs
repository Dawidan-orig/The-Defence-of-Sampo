using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sampo.Melee.Combos
{
    public class ComboChain : IEnumerator
    {
        private ComboBase[] _chain;
        private int _chainIndex = 0;

        public ComboBase[] Chain { get => _chain;}

        public object Current => _chain[_chainIndex];

        public bool MoveNext()
        {
            _chainIndex++;

            if (_chainIndex >= _chain.Length)
                return false;

            return true;
        }

        public void Reset()
        {
            _chainIndex = 0;
        }

        public ComboChain(ComboBase[] chainedAttacks)
        {
            _chain = chainedAttacks;
        }
    }
}