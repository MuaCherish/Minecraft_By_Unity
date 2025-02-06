using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public enum AIState
{
    Idle,
    Chase,
    Flee,
}

[SerializeField]
public enum AIMovingType
{
    Jump,
    Walk,
}

[SerializeField]
public enum IdleState
{
    Wait,
    Rotate,
    Moving,
}

[SerializeField]
public enum ChaseState
{
    Wait,
    Moving,
}

[SerializeField]
public enum FleeState
{
    Wait,
    Rotate,
    Moving,
}