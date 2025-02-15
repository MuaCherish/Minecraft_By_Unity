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

//�ƶ���ʽ
public enum AIMovingType
{
    Jump,
    Walk,
}

//AIState-Idle��״̬
public enum IdleState
{
    Wait,
    Rotate,
    Moving,
}

//AIState-Chase��״̬
public enum ChaseState
{
    Wait,
    Moving,
}

//AIState-Flee��״̬
public enum FleeState
{
    Wait,
    Rotate,
    Moving,
}