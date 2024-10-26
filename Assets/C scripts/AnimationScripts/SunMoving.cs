using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class SunMoving : MonoBehaviour
{
    public bool isOpenLightCast;

    [Header("����")]
    public ManagerHub managerhub;
    public Transform Sun;
    public Transform Moon;
    public Transform DirectionalLight; Light directionalLight;
    public Transform DirectionalLightMain; Light directionalLightMain;
    private Vector3 playerPosition;


    [Header("̫������")]
    private float time; // time��0~24֮�䣬����12��ʱ��time��������Ϸ�
    public float radius; // ������Ҷ�Զ

    private bool hasExec_Update = true;


    private void Awake()
    {
        directionalLightMain = DirectionalLightMain.GetComponent<Light>();
        directionalLight = DirectionalLight.GetComponent<Light>();
    }


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

            DynamicLightCast();


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
        else
        {

            
        }
    }





    void DynamicLightCast()
    {
        if (isOpenLightCast)
        {
            if (managerhub.crepuscularScript != null && !managerhub.crepuscularScript.enabled)
            {
                managerhub.crepuscularScript.enabled = true;
                DirectionalLightMain.gameObject.SetActive(false);
            }

            // ��ȡ Directional Light �� Light ���
            // ʹ�� Mathf.Lerp �ڲ�ͬʱ��ƽ������ǿ��
            // ���õƹ�ǿ��
            
            


            // ʹ�� Quaternion.LookRotation ʹ�ƹ� Z ����뵽��������
            // �����Ҫ�����ƹ�ĳ��򣬿���ͨ���ڶ�����������һ���Զ���ġ��ϡ�����Ĭ��Ϊ Vector3.up��
            // ����� Sun �� Player �ķ�������
            Vector3 Lightdirection = Vector3.zero;
            if (!managerhub.timeManager.isNight)
            {
                Lightdirection = playerPosition - Sun.transform.position;
            }
            else
            {
                Lightdirection = playerPosition - Moon.transform.position;
            }

            DirectionalLight.transform.rotation = Quaternion.LookRotation(Lightdirection, Vector3.up);
        }
        else
        {
            if (managerhub.crepuscularScript != null && managerhub.crepuscularScript.enabled)
            {
                managerhub.crepuscularScript.enabled = false;
                DirectionalLightMain.gameObject.SetActive(true);
            }

            
            
        }
    }

    /// <summary>
    /// �ı�ƹ�
    /// </summary>
    /// <param name="_value"></param>
    public void SetLightInten()
    {
        float newIntensity = Mathf.Lerp(0f, 1.7f, managerhub.timeManager.timeStruct._time.value);

        if (!isOpenLightCast)
        {
            directionalLightMain.intensity = newIntensity;
        }
        else
        {
            directionalLight.intensity = newIntensity;
        }
        
    }
}
