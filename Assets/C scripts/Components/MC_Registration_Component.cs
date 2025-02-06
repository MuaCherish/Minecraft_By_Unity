using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MC_Collider_Component))]
[RequireComponent(typeof(MC_Animator_Component))]
public class MC_Registration_Component : MonoBehaviour
{

    #region 实体状态

    [Foldout("状态", true)]
    [Header("实体编号")][ReadOnly] public int EntityID = -1;


    #endregion


    #region 周期函数


    MC_Collider_Component Collider_Component;
    MC_Animator_Component Animator_Component;
    ManagerHub managerhub;
    World world;
    private void Awake()
    {
        Collider_Component = GetComponent<MC_Collider_Component>();
        managerhub = Collider_Component.managerhub;
        world = managerhub.world;
        Animator_Component = GetComponent<MC_Animator_Component>();
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
        _ReferUpdate_DestroyCheck();
    }

    #endregion


    #region 设置

    [Foldout("销毁个性化设置", true)]
    [Header("是否立即死亡, 没有任何多余的操作")] public bool isDeadImmediately = false;
    [Header("是否播放死亡动画")] public bool isPlayDeadAnimation = true;
    [Header("是否播放死亡音效")] public bool isPlayDeadMusic = true;
    [Header("死亡延迟时间")] public float WateToDead_Time = 1f;
    [Header("是否播放蒸汽粒子")] public bool isPlayEvaporationParticle = true;
    [Header("是否有掉落物")] public bool hasDropBox = true;
    [Header("掉落物列表")] public List<BlockItem> DropBoxList;



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
            print($"销毁失败，实体未注册,id = {EntityID}");
            return;
        }

        if (isDeadImmediately)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(WaitToDead());
        }
        
    }

    IEnumerator WaitToDead()
    {
        //死亡动画
        Animator_Component.isDead = true;

        //死亡音效
        if (isPlayDeadMusic)
        {
            int _index = MusicData.Creeper_Death;
            if (GetComponent<MC_Music_Component>() != null)
            {
                _index = GetComponent<MC_Music_Component>().DeathIndex;
            }
            managerhub.NewmusicManager.Create3DSound(transform.position, _index);

        }

        //Wait
        yield return new WaitForSeconds(WateToDead_Time);

        //蒸汽粒子
        if (isPlayEvaporationParticle)
        {
            GameObject _particleParent = SceneData.GetParticleParent();
            GameObject deadParticle = GameObject.Instantiate(
                world.Evaporation_Particle,
                transform.position,
                Quaternion.LookRotation(Vector3.up),
                _particleParent.transform  // 设置父对象
            );
        }

        //创建掉落物
        if (hasDropBox)
        {

            foreach (var item in DropBoxList)
            {
                Vector3 randomPoint = Random.insideUnitSphere / 2f;
                Collider_Component.managerhub.backpackManager.CreateDropBox(this.transform.position + randomPoint, item, false);
            }

        }
        
        //最后销毁
        Destroy(gameObject);
    }


    #endregion


    #region 实体销毁条件检测


    void _ReferUpdate_DestroyCheck()
    {
        // 检查Y坐标条件，立即销毁
        if (Collider_Component.FootPoint.y <= EntityData.MinYtoRemoveEntity)
        {
            LogOffEntity();
        }

        // 脚下区块被隐藏
        if (managerhub.world.GetChunkObject(Collider_Component.FootPoint).isShow == false)
        {
            LogOffEntity();
        }

    }


    #endregion

}
