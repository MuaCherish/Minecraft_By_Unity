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
        Entity_Parent = SceneData.GetEntityParent();
        world = managerhub.world;

        isNaturalSpawnEnabled = managerhub.������Ȼ����;

        _ReferAwake_InitService();
    }



    private void Update()
    {
        switch (world.game_state)
        {
            case Game_State.Loading:
                Handle_GameState_Loading();
                break;
            case Game_State.Playing:
                Handle_GameState_Playing();
                break;
        }
    }

    void Handle_GameState_Loading()
    {
        
    }


    void Handle_GameState_Playing()
    {
        if (isNaturalSpawnEnabled && _Coroutine_Service_DynamicAddEntity == null)
            _Coroutine_Service_DynamicAddEntity = StartCoroutine(Service_DynamicAddEntity());

        _ReferUpdate_CheckShowEntityHitbox();
    }


    #endregion


    #region ��פ����_��̬����ʵ��

    [Foldout("��פ����_��̬����ʵ��", true)]
    [Header("��Ȼ����")] private bool isNaturalSpawnEnabled;
    [Header("ʵ�������ӳٷ�Χ")] public Vector2 AddEntityDelayRange = new Vector2(60f, 120f);
    [Header("ʵ�����ɰ뾶��Χ")] public Vector2 spawnRadiusRange = new Vector2(10f, 16f); //ʵ�����ɰ뾶��Χ(����Ȧ)
    [Header("ʵ������diffY���ʾ���")] public float maxSpawnHeightDifference;  //ʵ��Y - ���Y < ʵ������diffY���ʾ���
    [Header("��Ȼ����ʵ���ź���")] public List<int> Mutex_Entities;

    void _ReferAwake_InitService()
    {
        foreach (var item in Entity_Prefebs)
            Mutex_Entities.Add(item.maxGenNumer);
    }

    Coroutine _Coroutine_Service_DynamicAddEntity;
    IEnumerator Service_DynamicAddEntity()
    {
        while (true)
        {
            //��ǰ�˳�-�����Start���˳�
            if (world.game_state == Game_State.Start)
            {
                _Coroutine_Service_DynamicAddEntity = null;
                break;
            }

            //����������������Ѿ�����
            if (isFullofAllEntities())
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            //��ȷ��λ�� 
            int NextEntityIndex = DynamicAddEntity_FindEntityPrefebIndex();
            Vector3 NextEntityPos = DynamicAddEntity_FindPos();

            //�ӳ�������
            yield return new WaitForSeconds(Random.Range(AddEntityDelayRange.x, AddEntityDelayRange.y));

            //AddEntity
            //print($"Ͷ��ʵ��, index:{NextEntityIndex}, pos:{NextEntityPos}");
            AddEntity(NextEntityIndex, NextEntityPos, out var entity);
        }
    }


    //Ѱ�Һ��ʵ����� 
    Vector3 RandomSpawnPos;
    Vector3 DynamicAddEntity_FindPos()
    {
        // ��ȡ���λ��
        Vector3 playerPos = managerhub.player.transform.position;

        // ����һ��������Ȧ��Χ�ڵ������
        float angle = Random.Range(0f, Mathf.PI * 2); // ����Ƕ�
        float radius = Mathf.Sqrt(Random.Range(spawnRadiusRange.x * spawnRadiusRange.x, spawnRadiusRange.y * spawnRadiusRange.y)); // ȷ�����ȷֲ�
        RandomSpawnPos = new Vector3(playerPos.x + Mathf.Cos(angle) * radius, playerPos.y, playerPos.z + Mathf.Sin(angle) * radius);

        // ����World�Ļ�ȡ���ó����㺯��
        world.GetSpawnPos(RandomSpawnPos, out List<Vector3> _Result);

        // ForeachList: �ж������Ƿ������ [���������׶����] [������ҵ�Y�ں��ʷ�Χ]
        foreach (var _pos in _Result)
        {
            if (CheckCanAddEntity(_pos))
                return _pos;
        }

        return Vector3.zero;
    }

    //ÿ��������������
    bool isFullofAllEntities()
    {
        foreach (var item in Mutex_Entities)
        {
            if (item != 0)
                return false;
        }

        return true;
    } 

    // Ѱ�Һ��ʵ�indexȥͶ��
    int DynamicAddEntity_FindEntityPrefebIndex()
    {
        // ���ռ����п��õ�����
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < Mutex_Entities.Count; i++)
        {
            if (Mutex_Entities[i] > 0)
                availableIndices.Add(i);
        }

        // ���û�п���ʵ�壬���� -1 ��Ϊ�����ʶ
        if (availableIndices.Count == 0)
        {
            print("���������쳣");
            return -1;
        }

        // ���ѡ��һ�����õ�����
        int _randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];

        // �ݼ�����
        Mutex_Entities[_randomIndex]--;
        return _randomIndex;
    }


    //����Ƿ��������ʵ��
    bool CheckCanAddEntity(Vector3 _pos)
    {
        Vector3 _playerPos = managerhub.player.transform.position;
        Camera _camera = managerhub.player.eyes;
        bool _Pass = true;

        // �������Yֵ�������ʷ�Χ
        if (Mathf.Abs(_pos.y - _playerPos.y) > maxSpawnHeightDifference)
            _Pass = false;

        // ���������׶����
        Vector3 viewportPoint = _camera.WorldToViewportPoint(_pos);
        if (viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1)
            _Pass = false;

        return _Pass;
    }



    #endregion


    #region ʵ�����

    [Foldout("ʵ�����", true)]
    [Header("��������")] public GameObject Evaporation_Particle;
    [Header("ʵ��Ԥ����")] public EntityPrefebStruct[] Entity_Prefebs;
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
    public bool AddEntity(int _PrefebIndex, Vector3 _Startpos, out EntityInfo _Result)
    {
        // ���𷵻�-���ʵ�������Ƿ�ﵽ���ֵ
        if (AllEntity.Count >= maxSize)
        {
            Debug.LogWarning("ʵ�������Ѵﵽ���ֵ���޷������ʵ�壡");
            _Result = null;
            return false;
        }

        // ���𷵻�-����±��Ƿ���Ч
        if (_PrefebIndex < 0 || _PrefebIndex >= Entity_Prefebs.Length)
        {
            Debug.LogError("����������Χ�����ṩ��Ч��Ԥ����������");
            _Result = null;
            return false;
        }

        //��ǰ����-��ǰ����û���������
        if (!world.TryGetChunkObject(_Startpos, out Chunk chunktemp))
        {
            _Result = null;
            return false;
        }

        //��ǰ����-���鱻����
        if (chunktemp != null && chunktemp.isShow == false)
        {
            _Result = null;
            return false;
        }


        // ʵ����Ԥ����
        GameObject newEntity = Instantiate(Entity_Prefebs[_PrefebIndex].prefeb);

        // ���𷵻�-���û��ע�����
        if (newEntity.GetComponent<MC_Component_Registration>() == null)
        {
            print($"{_PrefebIndex}ʵ��δ���ע��������޷���ӵ���Ϸ��");
            _Result = null;
            Destroy(newEntity);
            return false;
        }


        // ����һ��Ψһ��ID
        int entityId = Unique_Id++;

        //��������
        string entityName = EntityData.GetEntityName(_PrefebIndex);

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
            //Debug.Log($"ʵ�� {_EntityID._name} ���Ƴ��ɹ���");
            Mutex_Entities[EntityData.GetEntityPrefebIndex(entityToRemove._name)] ++;

            //End
            AllEntity.Remove(entityToRemove);
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


    #region Debug

    [Foldout("Debug", true)]
    [Header("���ƿ�����ʵ��ķ�Χ")] public bool Debug_DrawAddEntityRange;
    [Header("ʵ������λ��Ԥ��")] public bool Debug_PredictEntityPos;

    private void OnDrawGizmos()
    {
        if (Debug_DrawAddEntityRange && player != null)
        {
            Vector3 playerPos = player.transform.position;
            Camera cam = player.eyes; // ��ȡ������

            if (cam != null)
            {
                DrawWireCircle(playerPos, spawnRadiusRange.x, cam);
                DrawWireCircle(playerPos, spawnRadiusRange.y, cam);
                DrawFOVLines(playerPos, cam, spawnRadiusRange.y); // ����FOV����
            }
        }

        if (Debug_PredictEntityPos)
        {
            Vector3 StartPos = RandomSpawnPos; StartPos.y = 128f;
            Vector3 EndPos = RandomSpawnPos; EndPos.y = 0f;
            Debug.DrawLine(StartPos, EndPos, Color.red);
        }

    }

    // ��XOZƽ���ϻ���һ��Բ��������Ұ��Χ������ɫ
    private void DrawWireCircle(Vector3 center, float radius, Camera cam, int segments = 32)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0); // ��ʼ��

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

            // �����е㣬������Ƿ�����Ұ��Χ��
            Vector3 midPoint = (prevPoint + newPoint) * 0.5f;
            bool inView = IsPointInCameraView(cam, center, midPoint);

            // ������ɫ
            Gizmos.color = inView ? Color.red : Color.green;
            Gizmos.DrawLine(prevPoint, newPoint);

            prevPoint = newPoint;
        }
    }

    // �ж�һ�����Ƿ�����ҵ�FOV��Χ��
    private bool IsPointInCameraView(Camera cam, Vector3 playerPos, Vector3 point)
    {
        Vector3 toPoint = (point - playerPos).normalized; // ��ҵ���ķ���
        Vector3 forward = cam.transform.forward.normalized; // ����ӽǷ���

        float angleToPoint = Vector3.Angle(forward, toPoint); // ����н�
        float halfFOV = cam.fieldOfView / 2f; // ��Ұ�ǵ�һ��

        return angleToPoint < halfFOV;
    }

    // ����FOV���Σ�ֻ��XOZƽ�棩
    private void DrawFOVLines(Vector3 playerPos, Camera cam, float maxRadius)
    {
        Vector3 forward = cam.transform.forward;
        forward.y = 0; // ֻ����XOZƽ�淽��
        forward.Normalize();

        // ���� FOV ���ұ߽緽����Y����ת��
        Quaternion leftRotation = Quaternion.AngleAxis(-cam.fieldOfView / 2f, Vector3.up);
        Quaternion rightRotation = Quaternion.AngleAxis(cam.fieldOfView / 2f, Vector3.up);

        Vector3 leftDir = leftRotation * forward;
        Vector3 rightDir = rightRotation * forward;

        // ���Ʒ�����XOZƽ�沢��һ��
        leftDir.y = 0;
        rightDir.y = 0;
        leftDir = leftDir.normalized;
        rightDir = rightDir.normalized;

        // ������Ұ�߽��
        Vector3 leftPoint = playerPos + leftDir * maxRadius;
        Vector3 rightPoint = playerPos + rightDir * maxRadius;

        // �������ߣ�ֻ��XOZƽ�棩
        Gizmos.color = Color.red;
        Gizmos.DrawLine(playerPos, leftPoint);
        Gizmos.DrawLine(playerPos, rightPoint);
    }


    #endregion

}

[System.Serializable]
public class EntityPrefebStruct
{
    public GameObject prefeb;
    public int maxGenNumer;
}