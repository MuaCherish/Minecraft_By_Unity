using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Entity_Slime : EntityBase
{

    #region 状态

    [Foldout("史莱姆设置", true)]
    [Header("大小")] public Entity_Slime_Type MyType = Entity_Slime_Type.Medium;

    #endregion


    #region 周期函数

    MC_Collider_Component Collider_Component;
    MC_Velocity_Component Velocity_Component;
    World world;
    private void Awake()
    {
        Collider_Component = GetComponent<MC_Collider_Component>();
        Velocity_Component = GetComponent<MC_Velocity_Component>();
        world = Collider_Component.managerhub.world;
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
        _ReferUpdate_BoundWithGround();
    }

    public override void OnStartEntity()
    {

    }

    public override void OnEndEntity()
    {
        CreateChildSlime();
    }


    #endregion



    #region 史莱姆分裂

    [Foldout("分裂设置", true)]
    [Header("分裂范围")] public Vector2Int splitRange = new Vector2Int(3, 5);

    void CreateChildSlime()
    {
        //提前返回-小史莱姆直接跳过
        if (MyType == Entity_Slime_Type.Small)
            return;

        //选择分裂数量
        int splitNumber = Random.Range(splitRange.x, splitRange.y);

        //选择分裂实体ID
        int TargetId = EntityData.Slime_Small;
        switch (MyType)
        {
            case Entity_Slime_Type.Medium:
                TargetId = EntityData.Slime_Small;
                break;
            case Entity_Slime_Type.Big:
                TargetId = EntityData.Slime_Medium;
                break;
        }

        //随机选择Position
        List<Vector3> _vecs = new List<Vector3>();
        float _r = Mathf.Min(Collider_Component.hitBoxWidth, Collider_Component.hitBoxHeight);
        for (int i = 0; i < splitNumber; i++)
        {
            // 获取球体内的随机位置
            Vector3 randomDirection = Random.onUnitSphere; // 在单位球面上生成一个随机点
            Vector3 randomPosition = transform.position + randomDirection * _r; // 乘以半径得到目标位置

            _vecs.Add(randomPosition); // 存储生成的随机位置
        }

        //AddEntity()
        foreach (var item in _vecs)
        {
            world.AddEntity(TargetId, item, out var _result);
        }

        
    }

    #endregion


    #region 落地会弹一下

    [Foldout("弹性设置", true)]
    [Header("是否会发生弹跳")] public bool canBound = true;
    [Header("弹跳力度")] public float elasticity = 10.0f;
    [Header("高度阈值")] public float disY = 1f;
    private float maxY = -Mathf.Infinity;
    private bool hasExec_GroundJump;
    void _ReferUpdate_BoundWithGround()
    {
        //提前返回-不发生弹跳
        if (canBound == false)
            return;

        //弹跳检测
        if (Collider_Component.isGround)
        {
            float _dis = maxY - Collider_Component.FootPoint.y;
            if (hasExec_GroundJump && _dis > disY)
            {
                Velocity_Component.AddForce(new Vector3(0f, 1f, 0f), elasticity);
                hasExec_GroundJump = false;
            }

            //最大高度
            maxY = Collider_Component.FootPoint.y;
        }
        else
        {
            //最大高度
            if (Collider_Component.FootPoint.y > maxY)
                maxY = Collider_Component.FootPoint.y;

            hasExec_GroundJump = true;
        }

        //最大高度
        if (Collider_Component.IsInTheWater(Collider_Component.EyesPoint))
        {
            maxY = Collider_Component.FootPoint.y;
        }

    }

    #endregion


}


[System.Serializable]
public enum Entity_Slime_Type
{
    Big, Medium, Small
}