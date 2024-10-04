using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundMoving : MonoBehaviour
{
    public float speed = 5f;        // 平移速度
    public float distance = 3f;     // 平移距离
    private Vector3 startPosition;  // 记录初始位置
    private bool movingRight = true; // 方向标志，true表示向右移动
    private ManagerHub managerhub;
    void Start()
    {
        managerhub = VoxelData.GetManagerhub();
        // 记录物体的初始位置
        startPosition = transform.position;
    }

    void Update()
    {
        if (managerhub.world.game_state == Game_State.Start)
        {
            // 计算当前位置到初始位置的距离
            float currentDistance = Vector3.Distance(transform.position, startPosition);

            // 如果移动距离超过设定的平移距离，切换移动方向
            if (currentDistance >= distance)
            {
                movingRight = !movingRight;
            }

            // 根据移动方向进行移动
            if (movingRight)
            {
                transform.position += Vector3.right * speed * Time.deltaTime;
            }
            else
            {
                transform.position += Vector3.left * speed * Time.deltaTime;
            }
        }
    }
}
