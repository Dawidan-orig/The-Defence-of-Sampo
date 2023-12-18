using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneTimeCollision : MonoBehaviour
{
    public LayerMask ignored;

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & ignored) != 0)
            return;

        Destroy(gameObject);
    }
}
