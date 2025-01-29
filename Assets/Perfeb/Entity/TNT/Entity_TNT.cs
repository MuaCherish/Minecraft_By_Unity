using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCEntity;


public class Entity_TNT : MonoBehaviour, IEntityLifecycle
{


    #region 周期函数

    ManagerHub managerhub;
    MC_Velocity_Component Velocity_Component;
    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        Velocity_Component = GetComponent<MC_Velocity_Component>();
    }


    public int MyEntityID;

    public void OnStartEntity()
    {
        //throw new System.NotImplementedException();
    }

    // 带参数的重载方法
    public void OnStartEntity(int _id, Vector3 _pos, bool _ActByTNT)
    {
        MyEntityID = _id;

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
        if (managerhub.world.game_mode == GameMode.Survival && _Direction.magnitude <= 4)
        {
            managerhub.lifeManager.UpdatePlayerBlood((int)Mathf.Lerp(30, 10, _value), true, true);
        }

        //Music
        managerhub.NewmusicManager.PlayOneShot(MusicData.explore);


        //激活范围内的所有TNT
        BlocksFunction.GetAllTNTPositions(this.transform.position, out List<Vector3> TNTpositions);
        if (TNTpositions.Count != 0)
        {
            //print($"搜索到了{TNTpositions.Count}个TNT");

            foreach (Vector3 item in TNTpositions)
            {
                Vector3 _direct = (item - transform.position).normalized;
                float value = _direct.magnitude / BlocksFunction.TNT_explore_Radius;

                //爆炸半径为4m
                //如果force
                float _force = Mathf.Lerp(500, 10, value);

                managerhub.player.CreateTNT(item, true);
            }

        }


        //搜索范围内所有实体
        List<EntityStruct> _entities = managerhub.world.GetOverlapSphereEntity(_center, BlocksFunction.TNT_explore_Radius);

        foreach (var item in _entities)
        {
            Vector3 _forceDirect = item._obj.transform.position - _center;
            _forceDirect.y = 0.8f;
            _forceDirect = _forceDirect.normalized;

            float _forceValue = 150f;

            item._obj.GetComponent<MC_Velocity_Component>().AddForce(_forceDirect, _forceValue);
        }

        //Chunk
        BlocksFunction.Boom(_center);


        //结束注册
        managerhub.world.RemoveEntity(MyEntityID);

        Destroy(gameObject); // 销毁当前对象
    }

    //初始化
    void InitTNT()
    {
        ExploreWhite = transform.Find("Crust").gameObject.GetComponent<MeshRenderer>().material;

        // 获取Crust的Renderer组件
        GameObject crust = transform.Find("Crust").gameObject;
        if (crust != null)
        {
            crustRenderer = crust.GetComponent<Renderer>();
            if (crustRenderer != null)
            {
                originalMaterial = crustRenderer.material; // 保存原始材质
            }
        }
        else
        {
            Debug.LogError("未找到Crust对象");
        }
    }


    #endregion


    #region TNTjump

    [Header("跳跃力")] public float forcevalue = 45f; // 施加的力
    [Header("跳跃高度")] public float jumpheight = 0.9f; // 跳跃高度
    void TNTjump(Vector3? _customDirection = null)
    {
        Vector3 direction;

        if (_customDirection.HasValue) // 如果有传入自定义方向
        {
            direction = _customDirection.Value; // 使用自定义方向
            Velocity_Component.AddForce(direction, 1); // 直接施加力，不需要标准化
        }
        else
        {
            //direction = Random.onUnitSphere * 0.5f; // 使用随机方向
            //direction.y = jumpheight; // 设置Y轴为jumpheight
            //Vector3 direct = direction.normalized; // 标准化向量
            //Velocity_Component.AddForce(direct, forcevalue); // 施加力
            Velocity_Component.EntityRandomJump(forcevalue);
        }
    }




    #endregion


    #region TNTBlink


    [Header("闪烁次数")] public int frequency = 9; // 闪烁次数
    [Header("白色持续时间")] public float whiteDelay = 0.5f; // 闪烁延迟时间
    [Header("普通持续时间")] public float normalDelay = 0.5f; // 闪烁延迟时间
    [Header("最大亮度")] public float ColorA = 150f; // 闪烁延迟时间
    [Header("引信时间")] public float FuseDuration = 4f;
    private Material ExploreWhite; // 预设的闪烁材质
    private Material originalMaterial; // 原始材质
    private Renderer crustRenderer; // crust的Renderer组件


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


    #region TNTswell

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
