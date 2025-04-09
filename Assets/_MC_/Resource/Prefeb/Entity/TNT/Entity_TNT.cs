using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCEntity;
using static MC_Static_BlocksFunction;

[RequireComponent(typeof(MC_Component_Physics))]
public class Entity_TNT : MonoBehaviour, IEntityBrain
{


    #region 周期函数

    ManagerHub managerhub;
    MC_Component_Velocity Component_Velocity;
    MC_Component_Physics Component_Physics;
    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        Component_Velocity = GetComponent<MC_Component_Velocity>();
        Component_Physics = GetComponent<MC_Component_Physics>();
    }

    public void OnStartEntity()
    {
        //throw new System.NotImplementedException();
    }

    // 带参数的重载方法
    public void OnStartEntity(Vector3 _pos, bool _ActByTNT)
    {

        InitTNT();
        transform.position = _pos;

        managerhub.NewmusicManager.PlayOneShot(MusicData.TNT_Fuse);

        if (_ActByTNT)
        {
            FuseDuration = Random.Range(1f, 3f);
        }

        TNTjump();
        StartCoroutine(TNTBlink());
    }

    
    //初始化
    void InitTNT()
    {
        ExploreWhite = crustRenderer.material;

        // 获取Crust的Renderer组件
        originalMaterial = crustRenderer.material; // 保存原始材质
    }


    #endregion


    #region 1.点燃跳跃

    [Header("跳跃力")] public float forcevalue = 45f; // 施加的力
    [Header("跳跃高度")] public float jumpheight = 0.9f; // 跳跃高度
    void TNTjump(Vector3? _customDirection = null)
    {
        Vector3 direction;

        if (_customDirection.HasValue) // 如果有传入自定义方向
        {
            direction = _customDirection.Value; // 使用自定义方向
            Component_Velocity.AddForce(direction, 1); // 直接施加力，不需要标准化
        }
        else
        {
            //direction = Random.onUnitSphere * 0.5f; // 使用随机方向
            //direction.y = jumpheight; // 设置Y轴为jumpheight
            //Vector3 direct = direction.normalized; // 标准化向量
            //Component_Velocity.AddForce(direct, forcevalue); // 施加力
            Component_Velocity.EntityRandomJump(forcevalue);
        }
    }




    #endregion


    #region 2.TNT闪烁


    [Header("闪烁次数")] public int frequency = 9; // 闪烁次数
    [Header("白色持续时间")] public float whiteDelay = 0.5f; // 闪烁延迟时间
    [Header("普通持续时间")] public float normalDelay = 0.5f; // 闪烁延迟时间
    [Header("最大亮度")] public float ColorA = 150f; // 闪烁延迟时间
    [Header("引信时间")] public float FuseDuration = 4f;
    private Material ExploreWhite; // 预设的闪烁材质
    private Material originalMaterial; // 原始材质
    public Renderer crustRenderer; // crust的Renderer组件

    IEnumerator TNTBlink()
    {
        if (crustRenderer == null || ExploreWhite == null)
        {
            print("crustRenderer == null || ExploreWhite == null");
            yield break; // 如果没有Renderer或材质，退出协程
        }

        // 创建ExploreWhite的材质实例
        Material blinkMaterial = new Material(ExploreWhite);
        float elapsetime = 0f;

        for (int i = 0; i < frequency; i++)
        {
            // 给crust赋上实例材质
            crustRenderer.material = blinkMaterial;

            // 透明度调整为1
            Color color = blinkMaterial.color;
            color.a = ColorA / 255f; // 设置透明度为1
            blinkMaterial.color = color;

            //开始膨胀
            if (i == frequency - 1 || elapsetime > FuseDuration)
            {
                StartCoroutine(TNTSwell());
                yield break;
            }


            yield return new WaitForSeconds(whiteDelay);
            elapsetime += whiteDelay;
            // 透明度调整为0
            color.a = 0f; // 设置透明度为0
            blinkMaterial.color = color;

            yield return new WaitForSeconds(normalDelay);
            elapsetime += normalDelay;
        }

        // 恢复原始材质
        if (crustRenderer != null && originalMaterial != null)
        {
            crustRenderer.material = originalMaterial;
        }

        //OnEndEntity();
        

    }

    #endregion


    #region 3.TNT膨胀

    [Header("膨胀时间")] public float SwellDuration = 0.1f;
    [Header("膨胀大小")] public float SwellMaxScale = 1.2f;


    IEnumerator TNTSwell()
    {
        // 获取当前物体的原始缩放
        Vector3 originalScale = transform.localScale;
        // 计算膨胀后的缩放值
        Vector3 targetScale = originalScale * SwellMaxScale;

        float elapsedTime = 0f; // 记录经过的时间

        while (elapsedTime < SwellDuration)
        {
            // 计算当前的比例
            float t = elapsedTime / SwellDuration;

            // 使用 Lerp 来平滑过渡缩放
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);

            elapsedTime += Time.deltaTime; // 增加经过的时间
            yield return null; // 等待下一帧
        }

        // 确保最后的缩放值为目标缩放值
        transform.localScale = targetScale;

        OnEndEntity();
        
    }

    #endregion


    #region 3.TNT爆炸

    public void OnEndEntity()
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
        float _value = _Direction.magnitude / 3;  //距离中心点程度[0,1]
        _Direction.y = Mathf.Lerp(0, 1, _value);
        float Distance = Mathf.Lerp(3, 0, _value);
        managerhub.player.ForceMoving(_Direction, Distance, 0.1f);

        //玩家扣血
        if (managerhub.Service_World.game_mode == GameMode.Survival && _Direction.magnitude <= 4)
            managerhub.lifeManager.UpdatePlayerBlood((int)Mathf.Lerp(30, 10, _value), true, true);

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

                MC_Component_Life Component_Life = item._obj.GetComponent<MC_Component_Life>();
                if (Component_Life != null) 
                {
                    Component_Life.UpdateEntityLife(-updateBlood, _forceDirect * _forceValue);
                }
                else
                {
                    item._obj.GetComponent<MC_Component_Velocity>().AddForce(_forceDirect , _forceValue);
                }
                
            }
        }


        //Chunk
        if (!Component_Physics.IsInTheWater(Component_Physics.FootPoint + new Vector3(0f, 0.125f, 0f)))
            Boom(_center);


        GetComponent<MC_Component_Registration>().LogOffEntity();
        
    }


    #endregion


    #region Debug

    //[Header("Debug")] public bool 点燃一次;
    //private void Update()
    //{
    //    if (点燃一次)
    //    {
    //        OnStartEntity(transform.position, false);
    //        点燃一次 = false;
    //    }
    //}

    #endregion


}
