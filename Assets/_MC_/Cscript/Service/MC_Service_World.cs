using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理游戏执行逻辑顺序等等，综合调度
/// </summary>
public class MC_Service_World : MonoBehaviour
{
    #region 周期函数

    ManagerHub managerhub;
    Player player;
    MC_Service_Chunk Service_Chunk;
    World world;

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        world = managerhub.world;
        player = managerhub.player;
        world.ChunkParent = SceneData.GetChunkParent();
        Service_Chunk = SceneData.GetService_Chunk();
    }

    bool hasExec_Start = true;
    bool hasExec_Loading = true;
    bool hasExec_Playing = true;
    bool hasExec_Pause = true;
    private void Update()
    {
        switch (world.game_state)
        {
            case Game_State.Start:
                Handle_GameState_Start();
                if (!hasExec_Loading)
                    hasExec_Loading = true;
                if (!hasExec_Playing)
                    hasExec_Playing = true;
                if (!hasExec_Pause)
                    hasExec_Pause = true;
                break;

            case Game_State.Loading:
                Handle_GameState_Loading();
                if (!hasExec_Start)
                    hasExec_Start = true;
                if (!hasExec_Playing)
                    hasExec_Playing = true;
                if (!hasExec_Pause)
                    hasExec_Pause = true;
                break;

            case Game_State.Playing:
                Handle_GameState_Playing();
                if (!hasExec_Start)
                    hasExec_Start = true;
                if (!hasExec_Loading)
                    hasExec_Loading = true;
                if (!hasExec_Pause)
                    hasExec_Pause = true;
                break;

            case Game_State.Pause:
                Handle_GameState_Pause();
                if (!hasExec_Start)
                    hasExec_Start = true;
                if (!hasExec_Loading)
                    hasExec_Loading = true;
                if (!hasExec_Playing)
                    hasExec_Playing = true;
                break;
        }

    }

    void Handle_GameState_Start()
    {
        if (hasExec_Start)
        {
            world.InitWorldManager();
            hasExec_Start = false;
            world.isFinishUpdateEditNumber = false;
        }
    }

    void Handle_GameState_Loading()
    {
        if (hasExec_Loading)
        {
            hasExec_Loading = false;
        }
    }

    void Handle_GameState_Playing()
    {
        if (hasExec_Playing)
        {
            hasExec_Playing = false;
        }
    }

    void Handle_GameState_Pause()
    {
        if (hasExec_Pause)
        {
            hasExec_Pause = false;
        }
    }

    #endregion
}
