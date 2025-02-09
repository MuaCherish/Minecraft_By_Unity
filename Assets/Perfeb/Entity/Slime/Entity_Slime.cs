using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Entity_Slime : EntityBase
{

    #region ״̬

    [Foldout("ʷ��ķ����", true)]
    [Header("��С")] public Entity_Slime_Type MyType = Entity_Slime_Type.Medium;

    #endregion


    #region ���ں���

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



    #region ʷ��ķ����

    [Foldout("��������", true)]
    [Header("���ѷ�Χ")] public Vector2Int splitRange = new Vector2Int(3, 5);

    void CreateChildSlime()
    {
        //��ǰ����-Сʷ��ķֱ������
        if (MyType == Entity_Slime_Type.Small)
            return;

        //ѡ���������
        int splitNumber = Random.Range(splitRange.x, splitRange.y);

        //ѡ�����ʵ��ID
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

        //���ѡ��Position
        List<Vector3> _vecs = new List<Vector3>();
        float _r = Mathf.Min(Collider_Component.hitBoxWidth, Collider_Component.hitBoxHeight);
        for (int i = 0; i < splitNumber; i++)
        {
            // ��ȡ�����ڵ����λ��
            Vector3 randomDirection = Random.onUnitSphere; // �ڵ�λ����������һ�������
            Vector3 randomPosition = transform.position + randomDirection * _r; // ���԰뾶�õ�Ŀ��λ��

            _vecs.Add(randomPosition); // �洢���ɵ����λ��
        }

        //AddEntity()
        foreach (var item in _vecs)
        {
            world.AddEntity(TargetId, item, out var _result);
        }

        
    }

    #endregion


    #region ��ػᵯһ��

    [Foldout("��������", true)]
    [Header("�Ƿ�ᷢ������")] public bool canBound = true;
    [Header("��������")] public float elasticity = 10.0f;
    [Header("�߶���ֵ")] public float disY = 1f;
    private float maxY = -Mathf.Infinity;
    private bool hasExec_GroundJump;
    void _ReferUpdate_BoundWithGround()
    {
        //��ǰ����-����������
        if (canBound == false)
            return;

        //�������
        if (Collider_Component.isGround)
        {
            float _dis = maxY - Collider_Component.FootPoint.y;
            if (hasExec_GroundJump && _dis > disY)
            {
                Velocity_Component.AddForce(new Vector3(0f, 1f, 0f), elasticity);
                hasExec_GroundJump = false;
            }

            //���߶�
            maxY = Collider_Component.FootPoint.y;
        }
        else
        {
            //���߶�
            if (Collider_Component.FootPoint.y > maxY)
                maxY = Collider_Component.FootPoint.y;

            hasExec_GroundJump = true;
        }

        //���߶�
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