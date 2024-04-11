using System;
using UnityEngine;

namespace Sampo.Melee.Combos
{
    [Serializable]
    public abstract class ComboBase
    {
        [SerializeField]
        protected Vector3 relativeStart;
        [SerializeField]
        protected Vector3 relativeEnd;
        [Tooltip("Название этого комбо-удара")]
        [SerializeField]
        protected string filename = "Unnamed combo";
        [Tooltip("Будет сохранён в этот путь")]
        [SerializeField]
        protected string savePath = "Assets/Unity Data Forms/Scriptable Objects/Combo library/Sword/";
        [Tooltip("При использовании этого комбо будет указана эта анимация")]
        [SerializeField]
        protected AnimationClip alignedClip = null;
        [Tooltip("Касающаяся только движения оружия анимационная кривая")]
        [SerializeField]
        protected AnimationCurve moveCurve = AnimationCurve.Constant(1,1,1);
        protected MeleeFighter.ActionType type;
    }
}