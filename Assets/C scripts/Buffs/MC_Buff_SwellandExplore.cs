using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MC_Static_BlocksFunction;

/// <summary>
/// Warning!!!
/// 该buff一旦触发会销毁该实体
/// </summary>
public class MC_Buff_SwellandExplore : MC_Buff_Base
{

    #region 周期函数

    MC_Component_Physics Component_Physics;
    ManagerHub managerhub;

    private void Awake()
    {
        Component_Physics = GetComponent<MC_Component_Physics>();
        managerhub = Component_Physics.managerhub;
    }

    private void Update()
    {
        
    }

    #endregion

    /// <summary>
    /// 膨胀到一定大小，随后爆炸，造成方块和实体的伤害
    /// </summary>
    public override IEnumerator StartBuffEffect()
    {
        // 设定膨胀时间和目标比例
        float SwellTimer = 0;
        float _swellDuration = 0.3f;
        GameObject _Model = Component_Physics.Model;
        Vector3 originalScale = _Model.transform.localScale;  // 初始缩放
        Vector3 targetScale = originalScale * 1.1f;  // 目标缩放（膨胀到1.1倍）

        // 膨胀过程
        while (SwellTimer < _swellDuration)
        {
            float lerpValue = SwellTimer / _swellDuration;  // 计算当前膨胀进度
            _Model.transform.localScale = Vector3.Lerp(originalScale, targetScale, lerpValue);  // 平滑过渡缩放

            SwellTimer += Time.deltaTime;
            yield return null;
        }

        // 爆炸部分
        Handle_Explore();
    }


    //爆炸处理
    void Handle_Explore()
    {
        //数值
        Vector3 _center = managerhub.player.GetCenterPoint(transform.position);

        //爆炸粒子效果
        GameObject particle_explore = GameObject.Instantiate(managerhub.player.particle_explosion);
        particle_explore.transform.position = this.transform.position;
        GameObject particleInstance = Instantiate(managerhub.player.Particle_TNT_Prefeb);
        particleInstance.transform.parent = managerhub.player.particel_Broken_transform;
        particleInstance.transform.position = _center;
        particleInstance.GetComponent<ParticleCollision>().Particle_Play(VoxelData.TNT);

        // 玩家炸飞
        Vector3 _Direction = managerhub.player.cam.transform.position - _center;  //炸飞方向
        float BloodValue = _Direction.magnitude / 6f;
        float _value = _Direction.magnitude / 3;  //距离中心点程度[0,1]
        _Direction.y = Mathf.Lerp(0, 1, _value);
        float Distance = Mathf.Lerp(3, 0, _value);
        managerhub.player.ForceMoving(_Direction, Distance, 0.1f);

        //玩家扣血
        if (managerhub.Service_World.game_mode == GameMode.Survival && _Direction.magnitude <= 6)
            managerhub.lifeManager.UpdatePlayerBlood((int)Mathf.Lerp(52, 5, BloodValue), true, true);

        //Music
        managerhub.NewmusicManager.PlayOneShot(MusicData.explore);


        //激活范围内的所有TNT
        GetAllTNTPositions(transform.position, out List<Vector3> TNTpositions);
        if (TNTpositions.Count != 0)
        {
            //print($"搜索到了{TNTpositions.Count}个TNT");

            foreach (Vector3 item in TNTpositions)
            {
                Vector3 _direct = (item - transform.position).normalized;
                float value = _direct.magnitude / TNT_explore_Radius;
                managerhub.player.CreateTNT(item, true);
            }

        }


        //搜索范围内所有实体
        if (managerhub.Service_Entity.GetOverlapSphereEntity(_center, TNT_explore_Radius + 2f, GetComponent<MC_Component_Registration>().GetEntityId()._id, out List<EntityInfo> _entities))
        {
            foreach (var item in _entities)
            {
                Vector3 _forceDirect = (item._obj.transform.position - _center).normalized;
                _forceDirect.y = 0.8f;
                _forceDirect = _forceDirect.normalized;
                int updateBlood = 0;

                //施加力度
                float _dis = (item._obj.transform.position - _center).magnitude;
                float _forceValue = 0f;

                // 如果距离在0到4米之间，力值从400到160之间变化
                if (_dis >= 0f && _dis <= TNT_explore_Radius)
                {
                    _forceValue = Mathf.Lerp(400f, 160f, _dis / TNT_explore_Radius);
                    updateBlood = (int)Mathf.Lerp(32, 10, _dis / TNT_explore_Radius);
                }
                // 如果距离在4到6米之间，力值固定为50
                else if (_dis > TNT_explore_Radius && _dis <= TNT_explore_Radius + 2f)
                {
                    _forceValue = 50f;
                    updateBlood = (int)Mathf.Lerp(10, 0, (_dis - TNT_explore_Radius) / 2f);
                }
                // 如果距离超过6米，力值为0或其他
                else
                {
                    updateBlood = 0;
                    _forceValue = 0f;  // 或者设置为你需要的默认值
                }

                if (item._obj.GetComponent<MC_Component_Life>() != null)
                {
                    item._obj.GetComponent<MC_Component_Life>().UpdateEntityLife(-updateBlood, _forceDirect * _forceValue);
                }
                else
                {
                    item._obj.GetComponent<MC_Component_Velocity>().AddForce(_forceDirect, _forceValue);
                }

            }
        }


        //Chunk
        if (!Component_Physics.IsInTheWater(Component_Physics.FootPoint + new Vector3(0f, 0.125f, 0f)))
            Boom(_center);


        GetComponent<MC_Component_Registration>().LogOffEntity(true);
    }



    /// <summary>
    /// 不需要解除buff了，直接销毁自己所有东西
    /// </summary>
    public override void EndBuffEffect()
    {
        //Pass
    }


    /// <summary>
    /// 让重置时间函数失效
    /// </summary>
    public override void ResetBuffDuration()
    {
        //pass
    }

}
