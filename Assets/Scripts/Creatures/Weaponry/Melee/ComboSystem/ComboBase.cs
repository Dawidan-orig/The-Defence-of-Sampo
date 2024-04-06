using Sampo.Melee.Sword;
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
        [Tooltip("�������� ����� �����-�����")]
        [SerializeField]
        protected string filename = "Unnamed combo";
        [Tooltip("����� ������� � ���� ����")]
        [SerializeField]
        protected string savePath = "Assets/Unity Data Forms/Scriptable Objects/Combo library/Sword/";
        [Tooltip("��� ������������� ����� ����� ����� ������� ��� ��������")]
        [SerializeField]
        protected AnimationClip alignedClip = null;
        [Tooltip("���������� ������ �������� ������ ������������ ������")]
        [SerializeField]
        protected AnimationCurve moveCurve = AnimationCurve.Constant(1,1,1);
        protected MeleeFighter.ActionType type;
    }
}