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

    [SerializeReference] // „тобы не было цикла сериализации
    private Pallete container;
    public int index;

    private bool wasModified; // Ќужен дл€ корректного использовани€ PropertyDrawer, чтобы соблюдать пор€док обновлений.

    public bool WasModified { get => wasModified; set => wasModified = value; }

    public PalleteObject(float left, float right, UnityEngine.Object obj, Pallete container, int index)
    {
        this.left = left;
        this.right = right;
        this.obj = obj;
        this.container = container;
        this.index = index;
        wasModified = false;
    }

    public void OnValidate() 
    {
        container.Validate(this);
    }

    public override string ToString()
    {
        return $"{left}<{obj}>{right}";
    }

    public override bool Equals(object obj)
    {
        if (obj is not PalleteObject casted) return false;

        return Utilities.ValueInArea(casted.left, left,0.000001f) &&
            Utilities.ValueInArea(casted.right, right, 0.000001f);
    }

    public override int GetHashCode()
    {
        return left.GetHashCode()|right.GetHashCode();
    }
}
