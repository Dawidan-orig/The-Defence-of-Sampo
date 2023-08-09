using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimationProvider
{
    public abstract Vector3 GetLookTarget();

    public abstract Vector3 GetRightHandTarget();

    public abstract bool IsGrounded();

    public abstract bool IsInJump();
}
