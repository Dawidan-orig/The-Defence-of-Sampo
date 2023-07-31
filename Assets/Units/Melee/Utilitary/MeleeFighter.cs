using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeFighter : MonoBehaviour
{
    public abstract void Swing(Vector3 toPoint);
    public abstract void Block(Vector3 start, Vector3 end, Vector3 SlashingDir);
}
