using System;
using UnityEngine;

[Serializable]
public struct PalleteObject
{
    [Range(0f, 1f)]
    public float left;
    [Range(0f, 1f)]
    public float right;
    public UnityEngine.Object obj;

    public int index;

    [SerializeField]
    private bool wasModified; // Ќужен дл€ корректного использовани€ PropertyDrawer, чтобы соблюдать пор€док обновлений.

    public bool WasModified { get => wasModified; set => wasModified = value; }

    public PalleteObject(float left, float right, UnityEngine.Object obj, int index)
    {
        this.left = left;
        this.right = right;
        this.obj = obj;
        this.index = index;
        wasModified = false;
    }

    public override string ToString()
    {
        return $"{left}<{obj}>{right}";
    }

    public override bool Equals(object obj)
    {
        if (obj is not PalleteObject casted) return false;

        return Utilities.ValueInArea(casted.left, left,0.00001f) &&
            Utilities.ValueInArea(casted.right, right, 0.00001f);
    }

    public override int GetHashCode()
    {
        return left.GetHashCode()|right.GetHashCode();
    }
}
