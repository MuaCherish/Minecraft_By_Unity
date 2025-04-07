using MCEntity;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

/// <summary>
/// ���߼����
/// </summary>
public static class MC_Static_Raycast
{
    /// <summary>
    /// ���߼��
    /// </summary>
    /// <param name="_world">world</param>
    /// <param name="_origin">��ʼ��</param>
    /// <param name="_direct">����</param>
    /// <param name="_maxDistance">������󳤶�</param>
    /// <param name="castingEntityId">�ų��Լ���Id</param>
    /// <param name="checkIncrement">������С����</param>
    /// <returns></returns>
    public static MC_RayCastStruct RayCast(ManagerHub _managerhub, MC_RayCast_FindType _FindType, Vector3 _origin, Vector3 _direct, float _maxDistance, int castingEntityId, float checkIncrement, bool debug = false)
    {
        // Ԥ����
        _direct.Normalize();

        // ��ʼ���ṹ��
        float step = 0f;
        Vector3 lastPos = new Vector3();
        Vector3 hitPoint = Vector3.zero;
        byte blockType = 255;
        Vector3 hitNormal = Vector3.zero;
        float rayDistance = _maxDistance;
        byte isHit = 0;
        EntityInfo targetEntity = new EntityInfo(-1, "Unknown Entity", null);

        // ��������㿪ʼ����Ŀ�귽����м��
        while (step < _maxDistance)
        {
            // ��ǰ�������ڵĵ�
            Vector3 pos = _origin + (_direct * step);

            // ��ǰ����-���y����С��0
            if (pos.y < 0)
                pos = new Vector3(pos.x, 0, pos.z);

            // ��ǰ����-����Ѿ�����
            if (isHit != 0)
                break;

            // ʵ�����м��
            if (_FindType != MC_RayCast_FindType.OnlyFindBlock && targetEntity._id == -1)
            {
                // ��ȡ��Χ�ڵ�ʵ��
                if (_managerhub.Service_Entity.GetOverlapSphereEntity(_origin, _maxDistance, out var entitiesInRange))
                {
                    // ����Ƿ���ʵ���������ཻ�����Ҹ�ʵ����������ײ
                    foreach (var entity in entitiesInRange)
                    {
                        // �ų���ǰʵ������
                        if (entity._id == castingEntityId)
                            continue;

                        // ��ȡʵ�����ײ������
                        var collider = entity._obj.GetComponent<MC_Component_Physics>();
                        if (collider != null && collider.CheckHitBox(pos))
                        {
                            targetEntity._id = entity._id;
                            targetEntity._obj = entity._obj;
                            isHit = 2;
                            break; // �ҵ���һ������������ʵ�壬�˳�ѭ��
                        }
                    }
                }
            }

            // �������м��
            if (_FindType != MC_RayCast_FindType.OnlyFindEntity && _managerhub.Service_Chunk.RayCheckForVoxel(pos))
            {
                // ��¼���е�
                hitPoint = pos;
                isHit = 1; // ��¼����

                // ��ȡ���еķ�������
                blockType = _managerhub.Service_Chunk.GetBlockType(pos);

                // �������еķ��߷��򣬻������е�����λ���жϷ��ߵ�λ����
                Vector3 blockCenter = new Vector3(Mathf.Floor(hitPoint.x) + 0.5f, Mathf.Floor(hitPoint.y) + 0.5f, Mathf.Floor(hitPoint.z) + 0.5f);
                Vector3 relativePos = hitPoint - blockCenter;

                // ���㷨��
                if (Mathf.Abs(relativePos.x) > Mathf.Abs(relativePos.y) && Mathf.Abs(relativePos.x) > Mathf.Abs(relativePos.z))
                    hitNormal = new Vector3(Mathf.Sign(relativePos.x), 0, 0);
                else if (Mathf.Abs(relativePos.y) > Mathf.Abs(relativePos.x) && Mathf.Abs(relativePos.y) > Mathf.Abs(relativePos.z))
                    hitNormal = new Vector3(0, Mathf.Sign(relativePos.y), 0);
                else
                    hitNormal = new Vector3(0, 0, Mathf.Sign(relativePos.z));

                // �������߾���
                rayDistance = (pos - _origin).magnitude;

                // ���к�����ѭ��
                break;
            }

            // ����ģʽ����������
            if (debug)
            {
                Debug.DrawRay(_origin, _direct * step, Color.red); // ��������
            }

            // ����
            lastPos = pos;
            step += checkIncrement;
        }

        // ���ؽ��
        return new MC_RayCastStruct
        {
            isHit = isHit,
            rayOrigin = _origin,
            hitPoint = hitPoint,
            hitPoint_Previous = lastPos,
            blockType = blockType,
            hitNormal = hitNormal,
            rayDistance = rayDistance,
            targetEntityInfo = targetEntity._id != -1 ? targetEntity : new EntityInfo(-1, "Unknown Entity", null),
        };
    }


}

//���ؽṹ��
[System.Serializable]
public struct MC_RayCastStruct
{
    /// <summary>
    /// �Ƿ�����: 0û������, 1���з���, 2����ʵ��
    /// </summary>
    public byte isHit;
    public Vector3 rayOrigin; // �������
    public Vector3 hitPoint;// ���е�����
    public Vector3 hitPoint_Previous;// ����ǰһ������
    public byte blockType;  // ���з�������
    public Vector3 hitNormal;// ���з��߷���
    public float rayDistance; // ���߾���
    public EntityInfo targetEntityInfo;// Ŀ��ʵ�壨��Ϊ�գ�

    // ���캯��
    public MC_RayCastStruct(byte isHit, Vector3 rayOrigin, Vector3 hitPoint, Vector3 hitPoint_Previous, byte blockType, Vector3 hitNormal, float rayDistance, EntityInfo targetEntityInfo)
    {
        this.isHit = isHit;
        this.rayOrigin = rayOrigin;
        this.hitPoint = hitPoint;
        this.hitPoint_Previous = hitPoint_Previous;
        this.blockType = blockType;
        this.hitNormal = hitNormal;
        this.rayDistance = rayDistance;
        this.targetEntityInfo = targetEntityInfo;
    }

    // ����ToString���������ڴ�ӡ���
    public override string ToString()
    {
        return $"RayCastStruct: \n" +
               $"  Is Hit: {isHit}\n" +
               $"  Ray Origin: {rayOrigin}\n" +
               $"  Hit Point: {hitPoint}\n" +
               $"  Previous Hit Point: {hitPoint_Previous}\n" +
               $"  Block Type: {blockType}\n" +
               $"  Hit Normal: {hitNormal}\n" +
               $"  Ray Distance: {rayDistance}\n" +
               $"  Target Entity: {targetEntityInfo._id}, {targetEntityInfo._name}, {targetEntityInfo._obj}";
    }
}

//����ģʽ
[System.Serializable]
public enum MC_RayCast_FindType
{
    AllFind,
    OnlyFindBlock,
    OnlyFindEntity,
}
