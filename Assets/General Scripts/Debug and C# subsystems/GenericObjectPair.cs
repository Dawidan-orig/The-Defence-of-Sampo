using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GenericObjectPair<TObject>
{
    private TObject right;
    private TObject left;

    public GenericObjectPair(TObject left, TObject right)
    {
        this.left = left;
        this.right = right;
    }

    public override string ToString()
    {
        return $"{left}->{right}";
    }

    public TObject From { get => left; set => left = value; }
    public TObject To { get => right; set => right = value; }

    public TObject Left { get => left; set => left = value; }
    public TObject Right { get => right; set => right = value; }
}
