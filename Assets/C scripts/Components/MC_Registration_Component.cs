using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MC_Collider_Component))]
[RequireComponent(typeof(MC_Animator_Component))]
public class MC_Registration_Component : MonoBehaviour
{

    #region ʵ��״̬

    [Foldout("״̬", true)]
    [Header("ʵ����")][ReadOnly] public int EntityID = -1;


    #endregion


    #region ���ں���


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


    #region ����

    [Foldout("���ٸ��Ի�����", true)]
    [Header("�Ƿ���������, û���κζ���Ĳ���")] public bool isDeadImmediately = false;
    [Header("�Ƿ񲥷���������")] public bool isPlayDeadAnimation = true;
    [Header("�Ƿ񲥷�������Ч")] public bool isPlayDeadMusic = true;
    [Header("�����ӳ�ʱ��")] public float WateToDead_Time = 1f;
    [Header("�Ƿ񲥷���������")] public bool isPlayEvaporationParticle = true;
    [Header("�Ƿ��е�����")] public bool hasDropBox = true;
    [Header("�������б�")] public List<BlockItem> DropBoxList;



    #endregion


    #region ʵ��ע����ע��



    public void RegistEntity(int _id)
    {
        EntityID = _id;
    }


    public void LogOffEntity()
    {

        if(!world.RemoveEntity(EntityID))
        {
            print($"����ʧ�ܣ�ʵ��δע��,id = {EntityID}");
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
        //��������
        Animator_Component.isDead = true;

        //������Ч
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

        //��������
        if (isPlayEvaporationParticle)
        {
            GameObject _particleParent = SceneData.GetParticleParent();
            GameObject deadParticle = GameObject.Instantiate(
                world.Evaporation_Particle,
                transform.position,
                Quaternion.LookRotation(Vector3.up),
                _particleParent.transform  // ���ø�����
            );
        }

        //����������
        if (hasDropBox)
        {

            foreach (var item in DropBoxList)
            {
                Vector3 randomPoint = Random.insideUnitSphere / 2f;
                Collider_Component.managerhub.backpackManager.CreateDropBox(this.transform.position + randomPoint, item, false);
            }

        }
        
        //�������
        Destroy(gameObject);
    }


    #endregion


    #region ʵ�������������


    void _ReferUpdate_DestroyCheck()
    {
        // ���Y������������������
        if (Collider_Component.FootPoint.y <= EntityData.MinYtoRemoveEntity)
        {
            LogOffEntity();
        }

        // �������鱻����
        if (managerhub.world.GetChunkObject(Collider_Component.FootPoint).isShow == false)
        {
            LogOffEntity();
        }

    }


    #endregion

}
