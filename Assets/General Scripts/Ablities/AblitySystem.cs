using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AblitySystem : MonoBehaviour
{
    public GameObject slashPrefab;
    //TODO DESIGN : У нас тут компонент, который лишь позволяет работать с какими угодно способностями.
    // Способности знают всё сами о себе, этот компонент их - активирует
    // SlashPrefab тут - лишний.
    //TODO DESIGN :  Кроме того, надо добавить систему, при которой эти способности отсюда можно легко добавлять-удалять в runtime.
    // Пока что всё в Hardcode
    public Ability[] abilities;
    public LayerMask Collidables;

    private void Awake()
    {
        abilities = new Ability[4];

        abilities[0] = new ProceedingSlash(transform, slashPrefab);
        ((ProceedingSlash)abilities[0]).layers = Collidables;
        abilities[1] = new Blow(transform);
        abilities[2] = new WindSlide(transform);
        abilities[3] = new FixedAscention(transform);
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
