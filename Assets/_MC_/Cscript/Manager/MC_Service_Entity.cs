using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MC_Service_Entity : MonoBehaviour
{

    #region ���ں���

    ManagerHub managerhub;
    Player player;
    World world;
    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        player = managerhub.player;
        Entity_Parent = GameObject.Find("Entity");
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
        _ReferUpdate_CheckShowEntityHitbox();
    }


    #endregion


    #region ʵ�����

    [Foldout("ʵ�����", true)]
    [Header("��������")] public GameObject Evaporation_Particle;
    [Header("ʵ��Ԥ����")] public GameObject[] Entity_Prefeb;
    [Header("���ŵ�����ʵ��")] public List<EntityInfo> AllEntity = new List<EntityInfo>();
    [Header("���ʵ������")][SerializeField] private int maxSize = 100; // Ĭ��ֵΪ100������Inspector�е���
    private int Unique_Id = 0; // ���������µ�ΨһID
    GameObject Entity_Parent;

    /// <summary>
    /// ����idѰ��ʵ��
    /// </summary>
    public GameObject FindEntity(int _id)
    {
        foreach (var entity in AllEntity)
        {
            if (entity._id == _id)
            {
                return entity._obj;
            }
        }
        Debug.LogWarning($"ʵ��ID {_id} �����ڣ�");
        return null;
    }

    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="newMaxSize">�µ��������</param>
    public void SetMaxSize(int newMaxSize)
    {
        if (AllEntity.Count >= newMaxSize)
        {
            Debug.LogWarning("�����õ��������С�ڵ�ǰʵ��������������Ч��");
            return;
        }

        maxSize = newMaxSize;
        Debug.Log($"��������ѵ���Ϊ {maxSize}��");
    }

    /// <summary>
    /// ���ʵ�嵽������
    /// </summary>
    /// <param name="_index">��Ҫ��ӵ�Ԥ������±�</param>
    /// <param name="_Startpos">ʵ�����ʼλ��</param>
    /// <returns>�Ƿ���ӳɹ�</returns>
    public bool AddEntity(int _index, Vector3 _Startpos, out EntityInfo _Result)
    {
        // ���𷵻�-���ʵ�������Ƿ�ﵽ���ֵ
        if (AllEntity.Count >= maxSize)
        {
            Debug.LogWarning("ʵ�������Ѵﵽ���ֵ���޷������ʵ�壡");
            _Result = null;
            return false;
        }

        // ���𷵻�-����±��Ƿ���Ч
        if (_index < 0 || _index >= Entity_Prefeb.Length)
        {
            Debug.LogError("����������Χ�����ṩ��Ч��Ԥ����������");
            _Result = null;
            return false;
        }

        // ʵ����Ԥ����
        GameObject newEntity = Instantiate(Entity_Prefeb[_index]);

        // ���𷵻�-���û��ע�����
        if (newEntity.GetComponent<MC_Component_Registration>() == null)
        {
            print($"{_index}ʵ��δ���ע��������޷���ӵ���Ϸ��");
            _Result = null;
            Destroy(newEntity);
            return false;
        }


        // ����һ��Ψһ��ID
        int entityId = Unique_Id++;

        //��������
        string entityName = EntityData.GetEntityName(_index);

        if (entityName == "Unknown Entity")
            print("��ʵ��û�����֣���");

        //����Struct
        EntityInfo _entityInfo = new EntityInfo(entityId, entityName, newEntity);

        //��ʵ�����ע��
        newEntity.transform.SetParent(Entity_Parent.transform);
        newEntity.transform.position = _Startpos;
        newEntity.name = $"[{entityId}] {entityName}";
        newEntity.GetComponent<MC_Component_Registration>().RegistEntity(_entityInfo);

        // ����ʵ���������ݽṹ
        _Result = _entityInfo;
        AllEntity.Add(_Result);

        //Debug.Log($"ʵ�� {newEntity.name} ����ӳɹ���IDΪ{entityId}��");

        return true;
    }

    /// <summary>
    /// �ӹ������Ƴ�ʵ��
    /// </summary>
    /// <param name="entity">��Ҫ�Ƴ���ʵ��</param>
    /// <returns>�Ƿ��Ƴ��ɹ�</returns>
    public bool RemoveEntity(EntityInfo _EntityID)
    {
        EntityInfo entityToRemove = null;

        // ��ȡ��ʵ���Ӧ��EntityStruct
        foreach (var entityStruct in AllEntity)
        {
            if (entityStruct._obj == _EntityID._obj)
            {
                entityToRemove = entityStruct;
                break;
            }
        }

        if (entityToRemove != null)
        {
            AllEntity.Remove(entityToRemove);
            //Debug.Log($"ʵ�� {_EntityID._name} ���Ƴ��ɹ���");
            return true;
        }

        Debug.LogWarning("��ʵ�岻�ڹ����У�");
        return false;
    }

    /// <summary>
    /// ��⵱ǰʵ������
    /// </summary>
    /// <returns>ʵ������</returns>
    public int GetEntityCount()
    {
        return AllEntity.Count;
    }

    /// <summary>
    /// �������ʵ��
    /// </summary>
    public void ClearEntities()
    {
        AllEntity.Clear();
        Debug.Log("����ʵ������գ�");
    }

    /// <summary>
    /// ��ȡһ����Χ�ڵ�ʵ�壨����λ�þ�����㣩
    /// </summary>
    /// <param name="center">��Χ��������</param>
    /// <param name="_r">��ⷶΧ�İ뾶</param>
    /// <returns>��Χ�ڵ�ʵ���б�</returns>
    public bool GetOverlapSphereEntity(Vector3 center, float _r, out List<EntityInfo> result)
    {
        result = new List<EntityInfo>();  // ��ʼ������б�
        float sqrRadius = _r * _r; // ʹ��ƽ���뾶�Ա����ظ���ƽ������
        bool hasEntitiesInRange = false;  // ��¼�Ƿ���ʵ���ڷ�Χ��

        foreach (var entityStruct in AllEntity)
        {
            if (entityStruct._obj == null)
                continue; // ��ֹ�����ô���

            // ����ʵ�������ĵľ���
            Vector3 offset = entityStruct._obj.transform.position - center;

            // ���ʵ���ƽ������С�ڵ��ڼ�ⷶΧ��ƽ���뾶����ӵ�����б�
            if (offset.sqrMagnitude <= sqrRadius)
            {
                result.Add(entityStruct);
                hasEntitiesInRange = true;  // ��ʵ���ڷ�Χ��
            }
        }

        return hasEntitiesInRange;
    }

    /// <summary>
    /// ���أ����Ժ����Լ�
    /// </summary>
    /// <param name="center"></param>
    /// <param name="_r"></param>
    /// <param name="_myID"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool GetOverlapSphereEntity(Vector3 center, float _r, int _myID, out List<EntityInfo> result)
    {
        result = new List<EntityInfo>();  // ��ʼ������б�
        float sqrRadius = _r * _r; // ʹ��ƽ���뾶�Ա����ظ���ƽ������
        bool hasEntitiesInRange = false;  // ��¼�Ƿ���ʵ���ڷ�Χ��

        foreach (var entityStruct in AllEntity)
        {
            if (entityStruct._obj == null || entityStruct._id == _myID)
                continue; // ��ֹ�����ô���ͬʱ��������

            // ����ʵ�������ĵľ���
            Vector3 offset = entityStruct._obj.transform.position - center;

            // ���ʵ���ƽ������С�ڵ��ڼ�ⷶΧ��ƽ���뾶����ӵ�����б�
            if (offset.sqrMagnitude <= sqrRadius)
            {
                result.Add(entityStruct);
                hasEntitiesInRange = true;  // ��ʵ���ڷ�Χ��
            }
        }

        return hasEntitiesInRange;
    }



    #endregion


    #region ����ײ��


    bool hasExec_ShowEntityHitbox = true;
    void _ReferUpdate_CheckShowEntityHitbox()
    {

        if (player.ShowEntityHitbox)
        {
            if (hasExec_ShowEntityHitbox)
            {

                SwitchAllEntityHitbox(true);

                hasExec_ShowEntityHitbox = false;
            }
        }
        else
        {
            if (hasExec_ShowEntityHitbox == false)
            {
                SwitchAllEntityHitbox(false);
                hasExec_ShowEntityHitbox = true;
            }
        }


    }

    /// <summary>
    /// ���������ײ�У��������ǵ�hitboxѡ��
    /// </summary>
    /// <param name="_bool"></param>
    void SwitchAllEntityHitbox(bool _bool)
    {
        foreach (var item in AllEntity)
        {
            item._obj.GetComponent<MC_Component_Physics>().isDrawHitBox = _bool;
        }
    }

    #endregion

}
