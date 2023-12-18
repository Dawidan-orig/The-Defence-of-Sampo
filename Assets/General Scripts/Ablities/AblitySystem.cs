using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AblitySystem : MonoBehaviour
{
    //TODO : REFACTOR!!!!
    public GameObject slashPrefab; // TODO : Переместить в ScriptableObject, добавив его в ProceedingSlash. это уберёт это поле отсюда, оно лишнее и не связанно с системой - только с Proceeding Slash
    public Ability[] abilities;
    public LayerMask mask;
    /*
    public ProceedingSlash slash;
    public Blow blow;
    public WindSlide windSlide;
    public FixedAscention ascention_Ult;*/

    private void Awake()
    {
        abilities = new Ability[4];

        abilities[0] = new ProceedingSlash(transform, slashPrefab);
        ((ProceedingSlash)abilities[0]).layers = mask;
        abilities[1] = new Blow(transform);
        abilities[2] = new WindSlide(transform);
        abilities[3] = new FixedAscention(transform);

        /*
        slash = (ProceedingSlash)abilities[0];
        blow = (Blow)abilities[1];
        windSlide = (WindSlide)abilities[2];
        ascention_Ult = (FixedAscention)abilities[3];*/
    }

    private void Start()
    {
        foreach (var ability in abilities) { ability.Enable(); }
    }

    private void Update()
    {
        foreach(var ability in abilities) { ability.Update(); }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            abilities[0].Activate();
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            abilities[1].Activate();
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            abilities[2].Activate();
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            abilities[3].Activate();
    }

    private void FixedUpdate()
    {
        foreach (var ability in abilities) { ability.FixedUpdate(); }
    }
}
