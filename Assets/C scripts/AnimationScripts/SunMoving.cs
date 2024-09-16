using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunMoving : MonoBehaviour
{
    [Header("����")]
    public ManagerHub managerhub;
    public Transform Sun;
    public Transform Moon;
    private Vector3 playerPosition;

    [Header("̫������")]
    private float time; // time��0~24֮�䣬����12��ʱ��time��������Ϸ�
    public float radius; // ������Ҷ�Զ

    private bool hasExec_Update = true;

    private void Update()
    {
        // ��Ϸ��ʼ
        if (managerhub.world.game_state == Game_State.Playing)
        {
            // һ���Դ���
            if (hasExec_Update)
            {
                hasExec_Update = false;
            }

            // ��ȡ����
            playerPosition = managerhub.player.transform.position;
            time = managerhub.timeManager.GetCurrentTime();

            // ����̫����λ��
            // ����̫���ĽǶȣ���timeֵӳ�䵽0~360�ȣ�0���24���൱�����䣬12��Ϊ���Ϸ���
            float angle = (time / 24f) * 360f;

            // ����̫�������ҵ�λ��
            float sunX = playerPosition.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float sunZ = playerPosition.z + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            float sunY = playerPosition.y + radius * Mathf.Sin((angle - 90f) * Mathf.Deg2Rad); // 12��ʱ̫�������Ϸ�

            // ����̫����λ��
            Sun.transform.position = new Vector3(sunX, sunY, sunZ);

            // ��̫��һֱ�������
            Sun.transform.LookAt(playerPosition);

            // ����������λ��
            // ������̫����ԵĽǶ�
            float moonAngle = angle + 180f; // ��̫�����
            float moonX = playerPosition.x + radius * Mathf.Cos(moonAngle * Mathf.Deg2Rad);
            float moonZ = playerPosition.z + radius * Mathf.Sin(moonAngle * Mathf.Deg2Rad);
            float moonY = playerPosition.y + radius * Mathf.Sin((moonAngle - 90f) * Mathf.Deg2Rad); // ��̫���߶��෴

            // ����������λ��
            Moon.transform.position = new Vector3(moonX, moonY, moonZ);

            // ������һֱ�������
            Moon.transform.LookAt(playerPosition);
        }
    }
}
