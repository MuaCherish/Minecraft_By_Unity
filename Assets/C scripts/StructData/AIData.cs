using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//AIState
public enum AIState
{
    Idle,
    Chase,
    Flee,
}

//移动方式
public enum AIMovingType
{
    Jump,
    Walk,
}

//AIState-Idle子状态
public enum IdleState
{
    Wait,
    Rotate,
    Moving,
}

//AIState-Chase子状态
public enum ChaseState
{
    Wait,
    Moving,
}

//AIState-Flee子状态
public enum FleeState
{
    Wait,
    Rotate,
    Moving,
}