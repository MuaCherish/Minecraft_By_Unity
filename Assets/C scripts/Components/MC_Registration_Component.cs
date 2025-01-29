using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MC_Collider_Component))]
public class MC_Registration_Component : MonoBehaviour
{

    #region 实体状态

    [Foldout("状态", true)]
    [Header("实体编号")][ReadOnly] public int EntityID = -1;


    #endregion


    #region 周期函数


    MC_Collider_Component Collider_Component;
    ManagerHub managerhub;
    World world;
    Animation animationCoponent;
    private void Awake()
    {
        Collider_Component = GetComponent<MC_Collider_Component>();
        managerhub = Collider_Component.managerhub;
        world = managerhub.world;
        animationCoponent = GetComponent<Animation>();
    }

    private void Update()
    {
        switch (world.game_state)
        {
            case Game_State.Playing:
                Handle_GameState_Playing();
                break;
        }
    }

    void Handle_GameState_Playing()
    {
        // 每隔 checkInterval 秒检查一次 Chunk 的显示状态
        if (Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time; // 更新上次检查的时间

            DestroyCheck_YtooSlow();
            DestroyCheck_ChunkHide();
        }
    }

    #endregion


    #region 设置

    // 检查间隔时间（单位：秒）
    [Foldout("设置", true)]
    [Header("销毁条件检测间隔")] public float checkInterval = 3f; private float lastCheckTime = -5f; // 初始化为负值以确保首次检测
    [Header("死亡延迟时间")] public float WateToDead_Time = 2f;


    #endregion


    #region 实体注册与注销



    public void RegistEntity(int _id)
    {
        EntityID = _id;
    }


    public void LogOffEntity()
    {

        if(!world.RemoveEntity(EntityID))
        {
            print("销毁失败，实体未注册");
            return;
        }

       
        StartCoroutine(WaitToDead());
    }

    IEnumerator WaitToDead()
    {
        //死亡动画
        if (animationCoponent != null && animationCoponent.GetClip("EntityDead") != null)
        {
            animationCoponent.Play("EntityDead");
        }
        else
        {
            print("找不到");
        }

        //死亡音效
        int _index = MusicData.Creeper_Death;
        if (GetComponent<MC_Music_Component>() != null)
        {
            _index = GetComponent<MC_Music_Component>().DeathIndex;
        }
        managerhub.NewmusicManager.Create3DSound(transform.position, _index);


        //Wait
        yield return new WaitForSeconds(WateToDead_Time);


        //蒸汽粒子
        // 创建实例，并将父对象设置为 particleParent
        GameObject _particleParent = SceneData.GetParticleParent();
        GameObject deadParticle = GameObject.Instantiate(
            world.Evaporation_Particle,
            transform.position,
            Quaternion.LookRotation(Vector3.up),
            _particleParent.transform  // 设置父对象
        );


        //创建掉落物
        Vector3 randomPoint = Random.insideUnitSphere / 2f;
        Collider_Component.managerhub.backpackManager.CreateDropBox(this.transform.position, new BlockItem(VoxelData.Slimeball, 1), false);
        Collider_Component.managerhub.backpackManager.CreateDropBox(this.transform.position + randomPoint, new BlockItem(VoxelData.Apple, 2), false);

        Destroy(this.gameObject);
    }


    #endregion


    #region 实体销毁条件检测

  
    void DestroyCheck_YtooSlow()
    {
        //Destroy_OutOfPlayer();

        // 检查Y坐标条件，立即销毁
        if (Collider_Component.FootPoint.y <= EntityData.MinYtoRemoveEntity)
        {
            
            LogOffEntity();
            return;
        }


    }


    void DestroyCheck_ChunkHide()
    {
        // 仅在满足条件时销毁
        if (managerhub.world.GetChunkObject(Collider_Component.FootPoint).isShow == false)
        {
            LogOffEntity();
        }

    }


    #endregion

}
