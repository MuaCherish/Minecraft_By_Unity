using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_CollisionCheck : MonoBehaviour
{
    public MC_Component_Physics ColliderA;
    public MC_Component_Physics ColliderB;

    public bool isCollision;

    void Update()
    {

        CollisionCheck();

    }

    void CollisionCheck()
    {

        if (ColliderA == null || ColliderB == null)
            return;

        if (ColliderA.CheckHitBox(ColliderA, ColliderB))
        {
            isCollision = true;
        }
        else
        {
            isCollision = false;
        }
    }

}
