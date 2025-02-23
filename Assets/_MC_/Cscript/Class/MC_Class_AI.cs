using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region AI行为方式

//移动方式
public enum AIMovingType
{
    Jump,
    Walk,
}

#endregion


#region AI状态机

//AIState
public enum AIState
{
    Idle,
    Chase,
    Flee,
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

#endregion
