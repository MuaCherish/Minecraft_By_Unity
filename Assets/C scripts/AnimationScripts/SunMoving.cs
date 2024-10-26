using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class SunMoving : MonoBehaviour
{
    public bool isOpenLightCast;

    [Header("引用")]
    public ManagerHub managerhub;
    public Transform Sun;
    public Transform Moon;
    public Transform DirectionalLight; Light directionalLight;
    public Transform DirectionalLightMain; Light directionalLightMain;
    private Vector3 playerPosition;


    [Header("太阳参数")]
    private float time; // time在0~24之间，其中12的时候time在玩家正上方
    public float radius; // 距离玩家多远

    private bool hasExec_Update = true;


    private void Awake()
    {
        directionalLightMain = DirectionalLightMain.GetComponent<Light>();
        directionalLight = DirectionalLight.GetComponent<Light>();
    }


    private void Update()
    {
        // 游戏开始
        if (managerhub.world.game_state == Game_State.Playing)
        {
            // 一次性代码
            if (hasExec_Update)
            {
                hasExec_Update = false;
            }

            DynamicLightCast();


            // 获取数据
            playerPosition = managerhub.player.transform.position;
            time = managerhub.timeManager.GetCurrentTime();

            // 设置太阳的位置
            // 计算太阳的角度：将time值映射到0~360度（0点和24点相当于日落，12点为正上方）
            float angle = (time / 24f) * 360f;

            // 计算太阳相对玩家的位置
            float sunX = playerPosition.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float sunZ = playerPosition.z + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            float sunY = playerPosition.y + radius * Mathf.Sin((angle - 90f) * Mathf.Deg2Rad); // 12点时太阳在正上方

            // 设置太阳的位置
            Sun.transform.position = new Vector3(sunX, sunY, sunZ);

            // 让太阳一直面向玩家
            Sun.transform.LookAt(playerPosition);


            

            

            // 设置月亮的位置
            // 计算与太阳相对的角度
            float moonAngle = angle + 180f; // 与太阳相对
            float moonX = playerPosition.x + radius * Mathf.Cos(moonAngle * Mathf.Deg2Rad);
            float moonZ = playerPosition.z + radius * Mathf.Sin(moonAngle * Mathf.Deg2Rad);
            float moonY = playerPosition.y + radius * Mathf.Sin((moonAngle - 90f) * Mathf.Deg2Rad); // 与太阳高度相反

            // 设置月亮的位置
            Moon.transform.position = new Vector3(moonX, moonY, moonZ);

            // 让月亮一直面向玩家
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

            // 获取 Directional Light 的 Light 组件
            // 使用 Mathf.Lerp 在不同时间平滑调整强度
            // 设置灯光强度
            
            


            // 使用 Quaternion.LookRotation 使灯光 Z 轴对齐到方向向量
            // 如果需要调整灯光的朝向，可以通过第二个参数传入一个自定义的“上”方向（默认为 Vector3.up）
            // 计算从 Sun 到 Player 的方向向量
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
    /// 改变灯光
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
