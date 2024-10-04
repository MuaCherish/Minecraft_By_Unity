using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundMoving : MonoBehaviour
{
    public float speed = 5f;        // ƽ���ٶ�
    public float distance = 3f;     // ƽ�ƾ���
    private Vector3 startPosition;  // ��¼��ʼλ��
    private bool movingRight = true; // �����־��true��ʾ�����ƶ�
    private ManagerHub managerhub;
    void Start()
    {
        managerhub = VoxelData.GetManagerhub();
        // ��¼����ĳ�ʼλ��
        startPosition = transform.position;
    }

    void Update()
    {
        if (managerhub.world.game_state == Game_State.Start)
        {
            // ���㵱ǰλ�õ���ʼλ�õľ���
            float currentDistance = Vector3.Distance(transform.position, startPosition);

            // ����ƶ����볬���趨��ƽ�ƾ��룬�л��ƶ�����
            if (currentDistance >= distance)
            {
                movingRight = !movingRight;
            }

            // �����ƶ���������ƶ�
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
