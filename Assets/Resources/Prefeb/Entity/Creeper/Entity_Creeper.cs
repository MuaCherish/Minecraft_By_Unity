using MCEntity;
using System.Collections;
using System.Linq.Expressions;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class Entity_Creeper : MonoBehaviour
{

    #region 周期函数

    ManagerHub managerhub;
    MC_Service_World Service_world;
    Player player;
    MC_Component_AI Component_AI;
    MC_Component_Buff Buff_Component;

    private void Awake()
    {
        Service_world = SceneData.GetManagerhub().Service_World;
        Component_AI = GetComponent<MC_Component_AI>();
        managerhub = SceneData.GetManagerhub();
        player = managerhub.player;
        Buff_Component = GetComponent<MC_Component_Buff>();
    }

    private void Update()
    {
        if (MC_Runtime_DynamicData.instance.GetGameState() == Game_State.Playing)
        {
            Handle_GameState_Playing();
        }
    }

    void Handle_GameState_Playing()
    {
        _ReferUpdate_Handle_StartFuse();
    }

    #endregion


    #region Creeper

    // 靠近玩家触发引信
    [Header("是否触发了引信")] public bool startFuse;
    [Header("触发引信距离")] public float DistanceToFuse = 3f;
    [Header("关闭引信距离")] public float DistanceToStopFuse = 5f;
    [Header("引信时间")] public float FuseTime = 3f;

    Coroutine _Coroutine_FuseToExplore;

    void _ReferUpdate_Handle_StartFuse()
    {
        //提前返回-只在生存模式触发引信
        if (MC_Runtime_DynamicData.instance.GetGameMode() != GameMode.Survival)
            return;

        float distanceToPlayer = (transform.position - player.transform.position).magnitude;

        // 如果引信未触发并且玩家进入触发范围
        if (!startFuse && Component_AI.isSeePlayer && distanceToPlayer < DistanceToFuse)
        {
            startFuse = true;
            Component_AI.Debug_PauseAI = true;
            Buff_Component.AddBuff(BuffData.Blink, 3f); // 触发 Blink Buff

            // 确保 Coroutine 只会启动一次
            if (_Coroutine_FuseToExplore == null)
                _Coroutine_FuseToExplore = StartCoroutine(StartFuseToExplore());
        }

        // 如果玩家离开触发范围，不立即停止协程，而是等待当前引信完成
        if (startFuse && distanceToPlayer >= DistanceToStopFuse)
        {
            startFuse = false;
            Component_AI.Debug_PauseAI = false;
            if (_Coroutine_FuseToExplore != null)
            {
                StopCoroutine(_Coroutine_FuseToExplore);
                _Coroutine_FuseToExplore = null;
            }
            
        }
    }

    // 引信达到一定时间触发爆炸
    IEnumerator StartFuseToExplore()
    {
        float _ExploreTimer = 0;

        // 等待一定时间
        while (_ExploreTimer < FuseTime)
        {
            // 提前返回-没有触发引信不计时
            if (!startFuse)
            {
                // 引信取消，协程退出
                _Coroutine_FuseToExplore = null;
                yield break;
            }

            _ExploreTimer += Time.deltaTime;
            yield return null; // 等待下一帧
        }

        // 爆炸触发
        Buff_Component.AddBuff(BuffData.SwellandExplore, 2f); // 触发爆炸 Buff
        _Coroutine_FuseToExplore = null;
    }

    #endregion

}
