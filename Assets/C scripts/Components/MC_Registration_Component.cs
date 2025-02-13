using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

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

    private void Awake()
    {
        Collider_Component = GetComponent<MC_Collider_Component>();
        managerhub = Collider_Component.managerhub;
        world = managerhub.world;
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


    #region 实体注册

    public int GetEntityId()
    {
        return EntityID;
    }

    public void RegistEntity(int _id)
    {
        EntityID = _id;
    }

    #endregion


    #region 实体注销

    private bool isRemoveEntity = false;
    public void LogOffEntity()
    {
        //提前返回-已经销毁实体
        if (isRemoveEntity)
            return;

        isRemoveEntity = world.RemoveEntity(EntityID);

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
        //Animator_Component.isDead = true;
        DeadAnimation();

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

        //自定义OnEndEntity
        EntityBase entityBase = GetComponent<EntityBase>();
        if (entityBase != null)
        {
            entityBase.OnEndEntity();
        }

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


    #region 死亡动画

    [Foldout("死亡动画", true)]
    [Header("动画时间")] public float DeadRotationDuration = 0.5f;

    void DeadAnimation()
    {
        GameObject _Model = GameObject.Find("Model");

        if (_Model == null)
        {
            print("找不到Model");
            return;
        }
        
        StartCoroutine(RotateCubeAroundPoint(_Model, 90f, DeadRotationDuration));
    }

    IEnumerator RotateCubeAroundPoint(GameObject obj, float angle, float duration)
    {
        // 找到根节点
        Vector3 footRoot = Collider_Component.FootPoint;

        // 获取起始旋转
        Quaternion startRotation = obj.transform.rotation;

        // 计算目标旋转，绕着 Cube 的 forward 轴旋转
        Quaternion endRotation = startRotation * Quaternion.Euler(angle, 0, 0);

        // 计算旋转轴为 Cube.transform.forward
        Vector3 rotationAxis = obj.transform.forward;

        // 旋转开始时间
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;

            // 将旋转角度插值，使用 Slerp 来平滑旋转
            obj.transform.RotateAround(footRoot, rotationAxis, angle * Time.deltaTime / duration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // 确保最终的旋转角度
        obj.transform.RotateAround(footRoot, rotationAxis, angle * Time.deltaTime / duration);
    }


    #endregion

}
