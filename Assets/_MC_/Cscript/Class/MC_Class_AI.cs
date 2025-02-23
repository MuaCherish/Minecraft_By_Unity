using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region AI��Ϊ��ʽ

//�ƶ���ʽ
public enum AIMovingType
{
    Jump,
    Walk,
}

#endregion


#region AI״̬��

//AIState
public enum AIState
{
    Idle,
    Chase,
    Flee,
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

#endregion
