using UnityEngine;

public class Ability
{
    public Transform user;
    private bool _activated;


    public Ability(Transform user)
    {
        this.user = user;
    }

    public bool Activated { get => _activated; set => _activated = value; }
}
