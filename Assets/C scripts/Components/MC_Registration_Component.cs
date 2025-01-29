using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MC_Collider_Component))]
public class MC_Registration_Component : MonoBehaviour
{

    #region ʵ��״̬

    [Foldout("״̬", true)]
    [Header("ʵ����")][ReadOnly] public int EntityID = -1;


    #endregion


    #region ���ں���


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
        // ÿ�� checkInterval ����һ�� Chunk ����ʾ״̬
        if (Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time; // �����ϴμ���ʱ��

            DestroyCheck_YtooSlow();
            DestroyCheck_ChunkHide();
        }
    }

    #endregion


    #region ����

    // �����ʱ�䣨��λ���룩
    [Foldout("����", true)]
    [Header("�������������")] public float checkInterval = 3f; private float lastCheckTime = -5f; // ��ʼ��Ϊ��ֵ��ȷ���״μ��
    [Header("�����ӳ�ʱ��")] public float WateToDead_Time = 2f;


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
            print("����ʧ�ܣ�ʵ��δע��");
            return;
        }

       
        StartCoroutine(WaitToDead());
    }

    IEnumerator WaitToDead()
    {
        //��������
        if (animationCoponent != null && animationCoponent.GetClip("EntityDead") != null)
        {
            animationCoponent.Play("EntityDead");
        }
        else
        {
            print("�Ҳ���");
        }

        //������Ч
        int _index = MusicData.Creeper_Death;
        if (GetComponent<MC_Music_Component>() != null)
        {
            _index = GetComponent<MC_Music_Component>().DeathIndex;
        }
        managerhub.NewmusicManager.Create3DSound(transform.position, _index);


        //Wait
        yield return new WaitForSeconds(WateToDead_Time);


        //��������
        // ����ʵ������������������Ϊ particleParent
        GameObject _particleParent = SceneData.GetParticleParent();
        GameObject deadParticle = GameObject.Instantiate(
            world.Evaporation_Particle,
            transform.position,
            Quaternion.LookRotation(Vector3.up),
            _particleParent.transform  // ���ø�����
        );


        //����������
        Vector3 randomPoint = Random.insideUnitSphere / 2f;
        Collider_Component.managerhub.backpackManager.CreateDropBox(this.transform.position, new BlockItem(VoxelData.Slimeball, 1), false);
        Collider_Component.managerhub.backpackManager.CreateDropBox(this.transform.position + randomPoint, new BlockItem(VoxelData.Apple, 2), false);

        Destroy(this.gameObject);
    }


    #endregion


    #region ʵ�������������

  
    void DestroyCheck_YtooSlow()
    {
        //Destroy_OutOfPlayer();

        // ���Y������������������
        if (Collider_Component.FootPoint.y <= EntityData.MinYtoRemoveEntity)
        {
            
            LogOffEntity();
            return;
        }


    }


    void DestroyCheck_ChunkHide()
    {
        // ������������ʱ����
        if (managerhub.world.GetChunkObject(Collider_Component.FootPoint).isShow == false)
        {
            LogOffEntity();
        }

    }


    #endregion

}
