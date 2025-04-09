using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MC_Component_Physics))]
public class MC_Component_Registration : MonoBehaviour
{

    #region ״̬

    [Foldout("״̬", true)]
    [Header("ID")] [SerializeField] private EntityInfo currentID;

    #endregion


    #region ���ں���

    MC_Component_Physics Component_Physics;
    ManagerHub managerhub;
    MC_Service_World Service_World;

    private void Awake()
    {
        Component_Physics = GetComponent<MC_Component_Physics>();
        managerhub = Component_Physics.managerhub;
        Service_World = managerhub.Service_World;
    }

    private void Update()
    {
        switch (MC_Runtime_DynamicData.instance.GetGameState())
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

    
    public EntityInfo GetEntityId()
    {
        return currentID;
    }

    public void RegistEntity(EntityInfo _ID)
    {
        currentID = new EntityInfo(_ID._id, _ID._name, _ID._obj);
    }

    #endregion


    #region ʵ��ע��

    private bool isRemoveEntity = false;
    public void LogOffEntity()
    {
        //��ǰ����-�Ѿ�����ʵ��
        if (isRemoveEntity)
            return;

        isRemoveEntity = managerhub.Service_Entity.RemoveEntity(currentID);

        if (isDeadImmediately)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(WaitToDead());
        }
        
    }

    /// <summary>
    /// ��Buff���߱����������Ƿ���������
    /// </summary>
    /// <param name="_ImediateDestroy"></param>
    public void LogOffEntity(bool _ImediateDestroy)
    {
        //��ǰ����-�Ѿ�����ʵ��
        if (isRemoveEntity)
            return;

        isRemoveEntity = managerhub.Service_Entity.RemoveEntity(currentID);

        if (isDeadImmediately || _ImediateDestroy)
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
            MC_Component_Music Music_Component = GetComponent<MC_Component_Music>();
            if (Music_Component != null)
            {
                managerhub.Service_Music.Create3DSound(transform.position, Music_Component.DeathClip);
            }
            

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
                managerhub.Service_Entity.Evaporation_Particle,
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
                Component_Physics.managerhub.backpackManager.CreateDropBox(this.transform.position + randomPoint, item, false);
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
        if (Component_Physics.FootPoint.y <= EntityData.MinYtoRemoveEntity)
        {
            LogOffEntity();
        }

        // �������鱻����
        if (managerhub.Service_World.GetChunkObject(Component_Physics.FootPoint).isShow == false)
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
        GameObject _Model = Component_Physics.Model;
        
        StartCoroutine(RotateCubeAroundPoint(_Model, 90f, DeadRotationDuration));
    }

    IEnumerator RotateCubeAroundPoint(GameObject obj, float angle, float duration)
    {

        // �ҵ����ڵ�
        Vector3 footRoot = Component_Physics.FootPoint;

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
