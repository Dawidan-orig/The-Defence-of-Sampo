using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

public class GlobalSettings : MonoBehaviour
{
    public int setFramerate = 120;

    // Start is called before the first frame update
    void Update()
    {
        Application.targetFrameRate = setFramerate;
    }
}

#endif