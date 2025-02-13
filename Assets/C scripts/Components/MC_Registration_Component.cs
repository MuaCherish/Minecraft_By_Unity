using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

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


    #region ʵ��ע��

    public int GetEntityId()
    {
        return EntityID;
    }

    public void RegistEntity(int _id)
    {
        EntityID = _id;
    }

    #endregion


    #region ʵ��ע��

    private bool isRemoveEntity = false;
    public void LogOffEntity()
    {
        //��ǰ����-�Ѿ�����ʵ��
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
        //��������
        //Animator_Component.isDead = true;
        DeadAnimation();

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

        //�Զ���OnEndEntity
        EntityBase entityBase = GetComponent<EntityBase>();
        if (entityBase != null)
        {
            entityBase.OnEndEntity();
        }

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


    #region ��������

    [Foldout("��������", true)]
    [Header("����ʱ��")] public float DeadRotationDuration = 0.5f;

    void DeadAnimation()
    {
        GameObject _Model = GameObject.Find("Model");

        if (_Model == null)
        {
            print("�Ҳ���Model");
            return;
        }
        
        StartCoroutine(RotateCubeAroundPoint(_Model, 90f, DeadRotationDuration));
    }

    IEnumerator RotateCubeAroundPoint(GameObject obj, float angle, float duration)
    {
        // �ҵ����ڵ�
        Vector3 footRoot = Collider_Component.FootPoint;

        // ��ȡ��ʼ��ת
        Quaternion startRotation = obj.transform.rotation;

        // ����Ŀ����ת������ Cube �� forward ����ת
        Quaternion endRotation = startRotation * Quaternion.Euler(angle, 0, 0);

        // ������ת��Ϊ Cube.transform.forward
        Vector3 rotationAxis = obj.transform.forward;

        // ��ת��ʼʱ��
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;

            // ����ת�ǶȲ�ֵ��ʹ�� Slerp ��ƽ����ת
            obj.transform.RotateAround(footRoot, rotationAxis, angle * Time.deltaTime / duration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // ȷ�����յ���ת�Ƕ�
        obj.transform.RotateAround(footRoot, rotationAxis, angle * Time.deltaTime / duration);
    }


    #endregion

}
