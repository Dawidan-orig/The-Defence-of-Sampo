using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordfighterAI : MonoBehaviour
{
    SwordControlAI swordControl;

    // Start is called before the first frame update
    void Start()
    {
        swordControl = GetComponent<SwordControlAI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
