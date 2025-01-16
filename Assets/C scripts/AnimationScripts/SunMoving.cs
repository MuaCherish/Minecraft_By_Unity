using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class SunMoving : MonoBehaviour
{

    #region ״̬


    [Foldout("״̬", true)]
    public bool isOpenLightCast;


    #endregion


    #region ���ں���

    ManagerHub managerhub;


    private void Awake()
    {
        managerhub = GlobalData.GetManagerhub();

        directionalLightMain = DirectionalLightMain.GetComponent<Light>();
        directionalLight = DirectionalLight.GetComponent<Light>();
    }


    private void Update()
    {

        switch (managerhub.world.game_state)
        {
            case Game_State.Playing:
                Handle_GameState_Playing();
                break;
        }

        
        
    }


    void Handle_GameState_Playing()
    {
        DynamicLightCast();

        SunMoon_Moving();

    }


    #endregion


    #region �����ƶ�

    [Foldout("�����ƶ�", true)]

    public Transform Sun;
    public Transform Moon;
    public Transform DirectionalLight; Light directionalLight;
    public Transform DirectionalLightMain; Light directionalLightMain;
    private Vector3 playerPosition;


    [Header("̫������")]
    private float time; // time��0~24֮�䣬����12��ʱ��time��������Ϸ�
    public float radius; // ������Ҷ�Զ


    void SunMoon_Moving()
    {
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

    #endregion


    #region ���ߺ���


    void DynamicLightCast()
    {
        if (isOpenLightCast)
        {
            if (managerhub.crepuscularScript != null && !managerhub.crepuscularScript.enabled)
            {
                managerhub.crepuscularScript.enabled = true;
                DirectionalLight.gameObject.SetActive(true);
                DirectionalLightMain.gameObject.SetActive(false);
            }

            // ��ȡ Directional Light �� Light ���
            // ʹ�� Mathf.Lerp �ڲ�ͬʱ��ƽ������ǿ��
            // ���õƹ�ǿ��
            
            


            // ʹ�� Quaternion.LookRotation ʹ�ƹ� Z ����뵽��������
            // �����Ҫ�����ƹ�ĳ��򣬿���ͨ���ڶ�����������һ���Զ���ġ��ϡ�����Ĭ��Ϊ Vector3.up��
            // ����� Sun �� Player �ķ�������
            Vector3 Lightdirection = Vector3.zero;
            if (!managerhub.timeManager.Check_isNight())
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
                DirectionalLight.gameObject.SetActive(false);
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



    public Material Mat_LightCast;
    public void SetLightCastDensity()
    {
        float result = 4 * Mathf.Pow(managerhub.timeManager.timeStruct._time.value - 0.5f, 2);
        Mat_LightCast.SetFloat("_Density", result);
    }


    #endregion


}
