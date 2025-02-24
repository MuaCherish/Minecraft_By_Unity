using System.Collections;
using UnityEngine;
using MCEntity;
using Homebrew;
using static UnityEngine.Rendering.DebugUI;
using System.Collections.Generic;

namespace MCEntity
{
    [RequireComponent(typeof(MC_Component_Velocity))]
    public class MC_Component_Physics : MonoBehaviour
    {

        #region ״̬

        [Foldout("״̬", true)]
        [ReadOnly] public bool isGround = false;

        void _ReferUpdate_State()
        {
            isGround = collider_Down;
        }


        #endregion


        #region ��������

        MC_Component_Velocity Component_Velocity;
        World world;

        private ManagerHub _managerhub;
        public ManagerHub managerhub
        {
            get
            {
                if (_managerhub == null)
                {
                    _managerhub = SceneData.GetManagerhub();
                }
                return _managerhub;
            }
        }


        private void Awake()
        {
            Component_Velocity = GetComponent<MC_Component_Velocity>();
            world = managerhub.world;

            _ReferAwake_AutoGetModel();
        }


        private void Start()
        {
            isDrawHitBox = managerhub.player.ShowEntityHitbox;
        }

        private void Update()
        {
            if (managerhub.world.game_state == Game_State.Playing)
            {
                _ReferUpdate_State();
                _ReferUpdate_HitBox();
                _ReferUpdate_EntityBounceCheck();
            }
           
        }




        #endregion


        #region �ж��Ƿ��ص�

        [Foldout("ʵ���ص�����", true)]
        [Header("�Ƿ�ɱ���")] public bool canBeRayCastHit = true;
        

        /// <summary>
        /// �ж��õ��Ƿ����ж�����
        /// </summary>
        public bool CheckHitBox(Vector3 _targetPos)
        {
            //��ǰ����-��ֹ���߼��
            if (!canBeRayCastHit)
                return false;

            Vector3 _selfPos = selfPos;
            float selfHalfWidth = hitBoxWidth / 2f;
            float selfHalfHeight = hitBoxHeight / 2f;

            // �ж�Ŀ����Ƿ��ڵ�ǰ�ж�����
            bool isInside =
                (_targetPos.x >= _selfPos.x - selfHalfWidth && _targetPos.x <= _selfPos.x + selfHalfWidth) &&
                (_targetPos.y >= _selfPos.y - selfHalfHeight && _targetPos.y <= _selfPos.y + selfHalfHeight) &&
                (_targetPos.z >= _selfPos.z - selfHalfWidth && _targetPos.z <= _selfPos.z + selfHalfWidth); // �����Ҫ����z��

            return isInside;
        }


        /// <summary>
        /// �ж����������Ƿ��ص�
        /// </summary>
        public Vector3 CheckHitBox(Vector3 _targetPos, float _targetWidth, float _targetHeight)
        {
            Vector3 _selfPos = selfPos;
            float _selfWidth = hitBoxWidth;
            float _selfHeight = hitBoxHeight;

            // ������Ͱ��
            float selfHalfWidth = _selfWidth / 2f;
            float selfHalfHeight = _selfHeight / 2f;
            float targetHalfWidth = _targetWidth / 2f;
            float targetHalfHeight = _targetHeight / 2f;

            // �ж��Ƿ��ص�
            bool isCollision =
                Mathf.Abs(_selfPos.x - _targetPos.x) < (selfHalfWidth + targetHalfWidth) &&
                Mathf.Abs(_selfPos.y - _targetPos.y) < (selfHalfHeight + targetHalfHeight) &&
                Mathf.Abs(_selfPos.z - _targetPos.z) < (selfHalfWidth + targetHalfWidth); // �����Ҫ����z��

            // ���������ײ��������ײ����
            if (isCollision)
            {
                return (_targetPos - _selfPos).normalized;
            }
            else
            {
                return Vector3.zero; // û����ײ
            }
        }

        /// <summary>
        /// �������Collider�Ƿ��ص�
        /// </summary>
        /// <returns></returns>
        public bool CheckHitBox(MC_Component_Physics _ColliderA, MC_Component_Physics _ColliderB)
        {
            // ��ȡ����A������B��λ��
            Vector3 positionA = _ColliderA.transform.position;
            Vector3 positionB = _ColliderB.transform.position;

            // ��ȡ��ײ�еĳߴ磨��ȡ��߶Ⱥ���ȣ�
            float widthA = _ColliderA.hitBoxWidth;
            float heightA = _ColliderA.hitBoxHeight;
            float widthB = _ColliderB.hitBoxWidth;
            float heightB = _ColliderB.hitBoxHeight;

            // ������ײ�еĳߴ�
            Vector3 sizeA = new Vector3(widthA, heightA, widthA);  // ��ά�ߴ�
            Vector3 sizeB = new Vector3(widthB, heightB, widthB);  // ��ά�ߴ�

            // ����A��B����ײ�еı߽�
            Vector3 minA = positionA - sizeA * 0.5f;
            Vector3 maxA = positionA + sizeA * 0.5f;

            Vector3 minB = positionB - sizeB * 0.5f;
            Vector3 maxB = positionB + sizeB * 0.5f;

            // ����Ƿ����ص�
            bool isOverlapping = (minA.x < maxB.x && maxA.x > minB.x) &&
                                  (minA.y < maxB.y && maxA.y > minB.y) &&
                                  (minA.z < maxB.z && maxA.z > minB.z);

            return isOverlapping;
        }




        #endregion


        #region �����������ײ���

        public float Delta = 0.01f;
        public float Delta_Pro = 0.0125f;

        // ǰ��
        public bool collider_Front
        {
            get
            {
                if (isCollisionLocked) return false;

                if (managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(-Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(Delta, Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(-Delta, Delta, Delta)))
                {

                    //����һ��΢��
                    if (!AdjustLock_Collsion_Front && Component_Velocity.GetVelocity().z > 0)
                    {
                        // ���� X �������
                        //int _selfZ = (int)(GetPoint_HitBoxEdge(BlockDirection.ǰ).z + Vector3.forward.z * Delta_Pro);
                        //float _blockZ = GetTargetBlockHeightAndWidth(BlockDirection.ǰ);
                        //transform.position = new Vector3(selfPos.x, selfPos.y, _selfZ + _blockZ - hitBoxWidth / 2);
                        //print($"����һ��λ������,_selyX: {_selfZ},  _blockX:{_blockZ}");

                        // ����λ�ú����������ظ�����
                        AdjustLock_Collsion_Front = true;

                        //print("Left");
                    }


                    return true;
                }
                else
                {
                    // ��δ��ײ�����棬����λ�õ���
                    if (AdjustLock_Collsion_Front)
                    {
                        AdjustLock_Collsion_Front = false;
                    }

                    return false;
                }
            }
        }

        // ��
        public bool collider_Back
        {
            get
            {
                if (isCollisionLocked) return false;

                if (managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(Delta, -Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(-Delta, -Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(Delta, Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(-Delta, Delta, -Delta)))
                {

                    //����һ��΢��
                    if (!AdjustLock_Collsion_Back && Component_Velocity.GetVelocity().z < 0)
                    {
                        // ���� X �������
                        //int _selfZ = (int)(GetPoint_HitBoxEdge(BlockDirection.��).z + Vector3.back.z * Delta_Pro);
                        //float _blockZ = GetTargetBlockHeightAndWidth(BlockDirection.��);
                        //transform.position = new Vector3(selfPos.x, selfPos.y, _selfZ + _blockZ + hitBoxWidth / 2);
                        //print($"����һ��λ������,_selyX: {_selfZ},  _blockX:{_blockZ}");

                        // ����λ�ú����������ظ�����
                        AdjustLock_Collsion_Back = true;

                        //print("Back");
                    }


                    return true;
                }
                else
                {
                    // ��δ��ײ�����棬����λ�õ���
                    if (AdjustLock_Collsion_Back)
                    {
                        AdjustLock_Collsion_Back = false;
                    }

                    return false;
                }
            }
        }

        // ��
        public bool collider_Left
        {
            get
            {
                if (isCollisionLocked) return false;

                if (managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(-Delta, -Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(-Delta, Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(-Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(-Delta, Delta, Delta)))
                {

                    //����һ��΢��
                    if (!AdjustLock_Collsion_Left && Component_Velocity.GetVelocity().x < 0)
                    {
                        // ���� X �������
                        //int _selyX = (int)(GetPoint_HitBoxEdge(BlockDirection.��).x + Vector3.left.x * Delta_Pro);
                        //float _blockX = GetTargetBlockHeightAndWidth(BlockDirection.��);
                        //transform.position = new Vector3(_selyX + _blockX + hitBoxWidth / 2, selfPos.y, selfPos.z);
                        //print($"����һ��λ������,_selyX: {_selyX},  _blockX:{_blockX}");

                        // ����λ�ú����������ظ�����
                        AdjustLock_Collsion_Left = true;

                        //print("Left");
                    }


                    return true;
                }
                else
                {
                    // ��δ��ײ�����棬����λ�õ���
                    if (AdjustLock_Collsion_Left)
                    {
                        AdjustLock_Collsion_Left = false;
                    }

                    return false;
                }
            }
        }

        // �ҷ�
        public bool collider_Right
        {
            get
            {
                if (isCollisionLocked) return false;

                if (managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(Delta, -Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(Delta, Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(Delta, Delta, Delta)))
                {

                    


                    //����һ��΢��
                    if (!AdjustLock_Collsion_Right && Component_Velocity.GetVelocity().x > 0)
                    {

                        // ���� X �������
                        //int _selyX = (int)(GetPoint_HitBoxEdge(BlockDirection.��).x + Vector3.right.x * Delta_Pro);
                        //float _blockX = GetTargetBlockHeightAndWidth(BlockDirection.��);
                        //transform.position = new Vector3(_selyX + _blockX - hitBoxWidth / 2, selfPos.y, selfPos.z);
                        //print($"����һ��λ������,_selyX: {_selyX},  _blockX:{_blockX}");

                        // ����λ�ú����������ظ�����
                        AdjustLock_Collsion_Right = true;

                        //print("Right");
                    }


                    return true;
                }
                else
                {
                    // ��δ��ײ�����棬����λ�õ���
                    if (AdjustLock_Collsion_Right)
                    {
                        AdjustLock_Collsion_Right = false;
                    }

                    return false;
                }
            }
        }

        // �Ϸ�
        public bool collider_Up
        {
            get
            {
                if (isCollisionLocked) return false;

                if (managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(Delta, Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(Delta, Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(-Delta, Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(-Delta, Delta, -Delta)))
                {

                    //����һ��΢��
                    if (!AdjustLock_Collsion_Up && Component_Velocity.GetVelocity().y > 0)
                    {
                        // ���� Y �������
                        //int _selyY = (int)(GetPoint_HitBoxEdge(BlockDirection.��).y + Vector3.up.y * Delta_Pro);
                        //float _blockY = GetTargetBlockHeightAndWidth(BlockDirection.��);
                        //transform.position = new Vector3(selfPos.x, _selyY + _blockY - hitBoxHeight / 2, selfPos.z);
                        //print($"����һ��λ������,_selyY: {_selyY},  _blockY:{_blockY}");

                        // ����λ�ú����������ظ�����
                        AdjustLock_Collsion_Up = true;
                    }

                    
                    return true;
                }
                else
                {
                    // ��δ��ײ�����棬����λ�õ���
                    if (AdjustLock_Collsion_Up)
                    {
                        AdjustLock_Collsion_Up = false;
                    }
                    
                    return false;
                }
            }
        }

        // �·�

        public bool collider_Down
        {
            get
            {
                // ����ײ��������У�鱻������ֱ�ӷ��� false
                if (isCollisionLocked) return false;

                // ������ײ���
                if (managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(Delta, -Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(-Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(-Delta, -Delta, -Delta)))
                {

                    //����һ��΢��
                    if (!AdjustLock_Collsion_Down && Component_Velocity.GetVelocity().y < 0)
                    {
                        // ���� Y �������
                        int _selyY = (int)(GetPoint_HitBoxEdge(BlockDirection.��).y + Vector3.down.y * Delta_Pro);
                        float _blockY = GetTargetBlockHeightAndWidth(BlockDirection.��);
                        transform.position = new Vector3(selfPos.x, _selyY + _blockY + hitBoxHeight / 2, selfPos.z);
                        //print($"λ������, _selfY: {_selyY},  _blockY:{_blockY}");

                        // ����λ�ú����������ظ�����
                        AdjustLock_Collsion_Down = true;

                        //print("Down");
                    }

                   
                    return true;
                }
                else
                {
                    
                    // ��δ��ײ�����棬����λ�õ���
                    if (AdjustLock_Collsion_Down)
                    {
                        //print("����");
                        AdjustLock_Collsion_Down = false;
                    }
                    
                    return false;
                }
            }
        }


        //��Χ
        public bool Collider_Surround
        {
            get
            {
                return !isCollisionLocked && (collider_Front || collider_Back || collider_Left || collider_Right);
            }
        }


        //��̬��ײϸ��
        private bool DynamicSubdivision_CollisionCheck(BlockDirection _DIRECT)
        {
            //������
            float _DynamicWidth = 1f;
            float _DynamicHeight = 1f;

            //���ĵ�
            Vector3 _Center = Vector3.zero; 

            //Block
            //byte _targetBlockType = 0;
            //CollosionRange _targetBlockRange;

            //��ȷ��Width
            switch (_DIRECT)
            {
                case BlockDirection.ǰ:
                    break;
                case BlockDirection.��:
                    break;
                case BlockDirection.��:
                    break;
                case BlockDirection.��:
                    break;
                case BlockDirection.��:





                    //����width������ײ����-Front
                    float _stepX = 1f / _DynamicWidth;
                    float _stepZ = 1f / _DynamicHeight;

                    //����ÿ������
                    for (float _z = 0; _z < _stepZ; _z += _DynamicHeight)
                    {
                        for (float _x = 0; _x < _stepX; _x += _DynamicWidth)
                        {
                            //managerhub.world.CollisionCheckForVoxel(_���� + new Vector3(_x, _y, 1) * _DynamicWidth);
                        }
                    }

                    break;
            }

            

            return false;
        }



        #endregion


        #region λ������

        //λ������
        private bool AdjustLock_Collsion_Front = false;
        private bool AdjustLock_Collsion_Back = false;
        private bool AdjustLock_Collsion_Left = false;
        private bool AdjustLock_Collsion_Right = false;
        private bool AdjustLock_Collsion_Up = false;
        private bool AdjustLock_Collsion_Down = false;

        /// <summary>
        /// ��ȡĿ�귽�򷽿�ĸ߶Ȼ��߿��
        /// </summary>
        /// <param name="_DIRECT">����ö��</param>
        float GetTargetBlockHeightAndWidth(BlockDirection _DIRECT)
        {
            //�Ƚ�ֵ
            float _value = 0f;

            //�߽����
            Vector3 _00 = Vector3.zero;
            Vector3 _11 = Vector3.zero;

            switch (_DIRECT)
            {
                case BlockDirection.ǰ:

                    //ȷ����Χ
                    _00 = ǰ_����;
                    _11 = ǰ_����;

                    //print($"_00: {_00}, _11: {_11}");

                    //�����½ǽ��б���������Ϊ1f
                    for (float _y = _00.y; _y <= _11.y; _y++)
                    {
                        for (float _x = _00.x; _x <= _11.x; _x++)
                        {
                            //��ȡ����
                            Vector3 _pos = new Vector3(_x, _y, _00.z + Delta);

                            //��ȡ��������
                            byte _targetType = managerhub.world.GetBlockType(_pos);

                            //��ǰ����-��������Ҫ����
                            if (_targetType == VoxelData.Air)
                            {
                                continue;
                            }

                            //��ȡĿ�����߶�
                            float _blockY = managerhub.world.blocktypes[_targetType].CollosionRange.zRange.x;

                            //print($"������{_pos}, ��ǰ��������:{_targetType}, ��ǰ����߶�: {_blockY}");

                            //�Ƚ�Yrange.Yȡ���ֵ
                            if (_blockY < _value)
                            {
                                _value = _blockY;
                            }
                        }
                    }
                    break;
                case BlockDirection.��:
                    //ȷ����Χ
                    _00 = ��_����;
                    _11 = ��_����;

                    //print($"_00: {_00}, _11: {_11}");

                    //�����½ǽ��б���������Ϊ1f
                    for (float _y = _00.y; _y <= _11.y; _y++)
                    {
                        for (float _x = _00.x; _x <= _11.x; _x++)
                        {   
                            //��ȡ����
                            Vector3 _pos = new Vector3(_x, _y, _00.z - Delta);

                            //��ȡ��������
                            byte _targetType = managerhub.world.GetBlockType(_pos);

                            //��ǰ����-��������Ҫ����
                            if (_targetType == VoxelData.Air)
                            {
                                continue;
                            }

                            //��ȡĿ�����߶�
                            float _blockY = managerhub.world.blocktypes[_targetType].CollosionRange.zRange.y;

                            //print($"������{_pos}, ��ǰ��������:{_targetType}, ��ǰ����߶�: {_blockY}");

                            //�Ƚ�Yrange.Yȡ���ֵ
                            if (_blockY > _value)
                            {
                                _value = _blockY;
                            }
                        }
                    }
                    break;
                case BlockDirection.��:

                    //ȷ����Χ
                    _00 = ��_����;
                    _11 = ǰ_����;

                    //print($"_00: {_00}, _11: {_11}");

                    //�����½ǽ��б���������Ϊ1f
                    for (float _z = _00.z; _z <= _11.z; _z++)
                    {
                        for (float _y = _00.y; _y <= _11.y; _y++)
                        {
                            //��ȡ����
                            Vector3 _pos = new Vector3(_00.x - Delta, _y, _z);

                            //��ȡ��������
                            byte _targetType = managerhub.world.GetBlockType(_pos);

                            //��ǰ����-��������Ҫ����
                            if (_targetType == VoxelData.Air)
                            {
                                continue;
                            }

                            //��ȡĿ�����߶�
                            float _blockY = managerhub.world.blocktypes[_targetType].CollosionRange.xRange.y;

                            //print($"������{_pos}, ��ǰ��������:{_targetType}, ��ǰ����߶�: {_blockY}");

                            //�Ƚ�Yrange.Yȡ���ֵ
                            if (_blockY > _value)
                            {
                                _value = _blockY;
                            }
                        }
                    }
                    break;
                case BlockDirection.��:

                    //ȷ����Χ
                    _00 = ��_����;
                    _11 = ǰ_����;

                    //�����½ǽ��б���������Ϊ1f
                    for (float _z = _00.z; _z <= _11.z; _z++)
                    {
                        for (float _y = _00.y; _y <= _11.y; _y++)
                        {
                            //��ȡ����
                            Vector3 _pos = new Vector3(_00.x + Delta, _y, _z);

                            //��ȡ��������
                            byte _targetType = managerhub.world.GetBlockType(_pos);

                            //��ǰ����-��������Ҫ����
                            if (_targetType == VoxelData.Air)
                            {
                                continue;
                            }

                            //��ȡĿ�����߶�
                            float _blockY = managerhub.world.blocktypes[_targetType].CollosionRange.xRange.x;

                            //print($"������{_pos}, ��ǰ��������:{_targetType}, ��ǰ����߶�: {_blockY}");

                            //�Ƚ�Yrange.Yȡ���ֵ
                            if (_blockY < _value)
                            {
                                _value = _blockY;
                            }
                        }
                    }
                    break;
                case BlockDirection.��:

                    //ȷ����Χ
                    _00 = ��_����;
                    _11 = ǰ_����;

                    //�����½ǽ��б���������Ϊ1f
                    for (float _z = _00.z; _z <= _11.z; _z++)
                    {
                        for (float _x = _00.x; _x <= _11.x; _x++)
                        {
                            //��ȡ����
                            Vector3 _pos = new Vector3(_x, _00.y + Delta, _z);

                            //��ȡ��������
                            byte _targetType = managerhub.world.GetBlockType(_pos);

                            //��ǰ����-��������Ҫ����
                            if (_targetType == VoxelData.Air)
                            {
                                continue;
                            }

                            //��ȡĿ����͸߶�
                            float _blockY = managerhub.world.blocktypes[_targetType].CollosionRange.yRange.x;

                            print($"������{_pos}, ��ǰ��������:{_targetType}, ��ǰ����߶�: {_blockY}");

                            //�Ƚ�Yrange.Yȡ���ֵ
                            if (_blockY < _value)
                            {
                                _value = _blockY;
                            }
                        }
                    }

                    break;
                case BlockDirection.��:

                    //ȷ����Χ
                    _00 = ��_����;
                    _11 = ǰ_����;

                    //�����½ǽ��б���������Ϊ1f
                    for (float _z = _00.z; _z <= _11.z; _z++)
                    {
                        for (float _x = _00.x; _x <= _11.x; _x++)
                        {
                            //��ȡ����
                            Vector3 _pos = new Vector3(_x, _00.y - Delta, _z);

                            //��ȡ��������
                            byte _targetType = managerhub.world.GetBlockType(_pos);

                            //��ǰ����-��������Ҫ����
                            if (_targetType == VoxelData.Air)
                            {
                                continue;
                            }

                            //��ȡĿ�����߶�
                            float _blockY = managerhub.world.blocktypes[_targetType].CollosionRange.yRange.y;

                            //print($"������{_pos}, ��ǰ��������:{_targetType}, ��ǰ����߶�: {_blockY}");

                            //�Ƚ�Yrange.Yȡ���ֵ
                            if (_blockY > _value)
                            {
                                _value = _blockY;
                            }
                        }
                    }

                    break;

                default:
                    print("GetTargetBlockHeightAndWidth��������������");
                    break;
            }


            return _value;
        }


        #endregion


        #region ʵ������ṹ

        [Foldout("(������)ʵ������ṹ", true)]
        [Header("ģ��(ͷ+��ĸ���)")] public GameObject Model; //ֻ������������
        [Header("ͷ��")] public GameObject Head;
        [Header("����(����ͷ��������)")] public GameObject Body;


        //�Զ���ȡ����ṹ
        void _ReferAwake_AutoGetModel()
        {
            Model = transform.Find("Model").gameObject;
            Head = transform.Find("Model/Head").gameObject;
            Body = transform.Find("Model/Body").gameObject;
           


            //���Model�Ҳ����Ż���ʾInfo
            if(Model == null || Head == null || Body == null)
            {
                MC_Component_Registration Component_Registration = GetComponent<MC_Component_Registration>();

                //��ǰ����-û��ע�����
                if (Component_Registration == null)
                {
                    print("ʵ��δ����ע��������Ҳ���Model");
                    return;
                }

                EntityInfo _info = Component_Registration.GetEntityId();
                if (Model == null)
                    print($"Model��������, id:{_info._id}, name:{_info._name}");

                if (Head == null)
                    print($"Head��������, id:{_info._id}, name:{_info._name}");

                if (Body == null)
                    print($"Body��������, id:{_info._id}, name:{_info._name}");

            }


            
        }


        #endregion


        #region ��ʱ�ر���ײ���


        // ��ʱ�ر����е����ײ���
        public void CloseCollisionForAWhile(float _time)
        {
            isCollisionLocked = true;
            collisionLockTimer = _time;
            StartCoroutine(CollisionLockTimerCoroutine());
        }


        // ������ʱֹͣ��ײ���Ĵ���
        private bool isCollisionLocked = false;
        private float collisionLockTimer = 0f;
        private IEnumerator CollisionLockTimerCoroutine()
        {
            while (collisionLockTimer > 0f)
            {
                collisionLockTimer -= Time.deltaTime;
                yield return null;
            }
            isCollisionLocked = false;
        }


        #endregion


        #region �ж���

        // �ж�������
        [Foldout("�ж���", true)]
        [Header("<!����!>���ж���ߴ���ò�Ҫ����1.2m")]
        [Header("- ��Ȼ���ܻ�������֮���Bug")]
        [Space]
        [Header("���������ж���")] public bool LockDrawHitBox;
        [Header("�����ж���")] public bool isDrawHitBox; private bool hasExec_isDrawHitBox = true;
        [Header("�ж����۾���Ը߶�")] public float hitBoxEyes = 0.8f;
        [Header("�ж�����")] public float hitBoxWidth = 1f;
        [Header("�ж���߶�")] public float hitBoxHeight = 1f;
        private GameObject BodyObject;
        private GameObject EyesObject;
        private Mesh mesh;

       

        //����Update
        void _ReferUpdate_HitBox()
        {
            //�����ж���
            DrawHitBox();
        }


        


        //�����ж���
        void DrawHitBox()
        {

            //��ǰ����-����������ײ��
            if (LockDrawHitBox)
                return;


            if (isDrawHitBox)
            {
                _DrawHitBox_DrawBox();

            }
            else
            {
                if (BodyObject != null )
                {
                    Destroy(BodyObject);
                    BodyObject = null;
                }

                if (EyesObject != null)
                {
                    Destroy(EyesObject);
                    EyesObject = null;
                }

                if (hasExec_isDrawHitBox == false)
                {
                    hasExec_isDrawHitBox = true;
                }
            }
        }

        //�����ж���-Mesh�߼�
        void _DrawHitBox_DrawBox()
        {
            if (hasExec_isDrawHitBox)
            {
                // �����߿�� GameObject �� Mesh
                BodyObject = new GameObject("HitBoxLine");
                BodyObject.transform.parent = transform;
                BodyObject.transform.position = transform.position;

                Mesh whiteMesh = new Mesh();
                Mesh redMesh = new Mesh();

                // ��Ӱ�ɫ���ֵ� MeshRenderer �Ͳ���
                MeshFilter whiteMeshFilter = BodyObject.AddComponent<MeshFilter>();
                whiteMeshFilter.mesh = whiteMesh;
                MeshRenderer whiteMeshRenderer = BodyObject.AddComponent<MeshRenderer>();
                whiteMeshRenderer.material = new Material(Shader.Find("Unlit/Color")) { color = Color.white };

                // �������۾������ֵ� GameObject �� MeshRenderer
                EyesObject = new GameObject("HitBoxEyeLine");
                EyesObject.transform.parent = transform;
                EyesObject.transform.position = transform.position;

                MeshFilter redMeshFilter = EyesObject.AddComponent<MeshFilter>();
                redMeshFilter.mesh = redMesh;
                MeshRenderer redMeshRenderer = EyesObject.AddComponent<MeshRenderer>();
                redMeshRenderer.material = new Material(Shader.Find("Unlit/Color")) { color = Color.red };

                Vector3 center = BodyObject.transform.localPosition;

                // ����
                Vector3[] positions = new Vector3[8];
                Vector3[] eyePositions = new Vector3[4];

                // �����ĸ���
                positions[0] = center + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2, hitBoxWidth / 2);
                positions[1] = center + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2, hitBoxWidth / 2);
                positions[2] = center + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2, -hitBoxWidth / 2);
                positions[3] = center + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2, -hitBoxWidth / 2);

                // �����ĸ���
                positions[4] = positions[0] + Vector3.up * hitBoxHeight;
                positions[5] = positions[1] + Vector3.up * hitBoxHeight;
                positions[6] = positions[2] + Vector3.up * hitBoxHeight;
                positions[7] = positions[3] + Vector3.up * hitBoxHeight;

                // �۾����ֵ��ĸ���
                eyePositions[0] = positions[0] + Vector3.up * hitBoxHeight * hitBoxEyes;
                eyePositions[1] = positions[1] + Vector3.up * hitBoxHeight * hitBoxEyes;
                eyePositions[2] = positions[2] + Vector3.up * hitBoxHeight * hitBoxEyes;
                eyePositions[3] = positions[3] + Vector3.up * hitBoxHeight * hitBoxEyes;

                // ��ɫ���ֵ�����
                int[] indices = new int[]
                {
            // ����
            0, 1,
            1, 2,
            2, 3,
            3, 0,
            // ����
            4, 5,
            5, 6,
            6, 7,
            7, 4,
            // ����
            0, 4,
            1, 5,
            2, 6,
            3, 7
                };

                // ��ɫ���֣��۾���������
                int[] eyeIndices = new int[]
                {
            0, 1,
            1, 2,
            2, 3,
            3, 0
                };

                // ���� Mesh ����
                whiteMesh.Clear();
                whiteMesh.vertices = positions;
                whiteMesh.SetIndices(indices, MeshTopology.Lines, 0);

                redMesh.Clear();
                redMesh.vertices = eyePositions;
                redMesh.SetIndices(eyeIndices, MeshTopology.Lines, 0);

                whiteMesh.RecalculateBounds();
                redMesh.RecalculateBounds();

                hasExec_isDrawHitBox = false;
            }
        }


        #endregion


        #region ʵ�強ѹ

        [Foldout("ʵ�強ѹ", true)]
        [Header("�Ƿ�ᱻ����ѹ")] public bool canBeBounced;
        [Header("�����ٶ�")] public float BounceSpeed = 2f;

        //�ú�����Update��
        void _ReferUpdate_EntityBounceCheck()
        {
            // ��ǰ���� - ����Լ����ᱻ����ѹ
            if (!canBeBounced)
                return;

            // ��ȡ��Χ�ڵ�ʵ�壬���û������ǰ����
            float _maxR = Mathf.Max(hitBoxWidth, hitBoxHeight);
            if (!managerhub.Service_Entity.GetOverlapSphereEntity(transform.position, _maxR, GetComponent<MC_Component_Registration>().GetEntityId()._id, out List<EntityInfo> _entities))
                return;

            // ��һ�����ˣ��޳�û�к��Լ��ص���ʵ��
            List<EntityInfo> overlappingEntities = new List<EntityInfo>();
            foreach (var item in _entities)
            {
                bool a = CheckHitBox(this, item._obj.GetComponent<MC_Component_Physics>());
                if (a) // ������ص�����
                {
                    overlappingEntities.Add(item);
                }
            }

            // ���û���ص���ʵ�壬ֱ�ӷ���
            if (overlappingEntities.Count == 0)
                return;

            // ���������ص�ʵ��ķ�����
            Vector3 _backDirect = Vector3.zero;
            foreach (var item in overlappingEntities)
            {
                // �����뵱ǰʵ��ķ�������������X��Y��Z����
                Vector3 direction = transform.position - item._obj.transform.position;
                _backDirect += direction;  // ����������������X, Y, Z��

            }

            // ���ܵķ������׼�������Ϸ��ٶ�
            _backDirect = _backDirect.normalized * BounceSpeed;

            // ��������Ӧ�õ�ʵ����ٶ������X��Y��Z����
            Component_Velocity.SetVelocity("x", _backDirect.x);
            //Component_Velocity.SetVelocity("y", _backDirect.y);
            Component_Velocity.SetVelocity("z", _backDirect.z);

        }



        #endregion


        #region ��ȡ��


        /// <summary>
        /// ��ȡ����Block��Ŀ�귽���,����Ϊ1m
        /// </summary>
        /// <param name="_direct">��������</param>
        /// <returns>Ŀ���</returns>
        public Vector3 GetPoint_Direct_1m(BlockDirection _DIRECT)
        {

            switch (_DIRECT)
            {
                case BlockDirection.ǰ:
                    return selfPos + Vector3.forward;
                case BlockDirection.��:
                    return selfPos + Vector3.back;
                case BlockDirection.��:
                    return selfPos + Vector3.left;
                case BlockDirection.��:
                    return selfPos + Vector3.right;
                case BlockDirection.��:
                    return selfPos + Vector3.up;
                case BlockDirection.��:
                    return selfPos + Vector3.down;
                default:
                    print("GetPoint_Direct_1m��������������");
                    return Vector3.zero;
            }


        }


        /// <summary>
        /// ��ȡ����HitBox�ϵı�Ե��
        /// </summary>
        /// <param name="_direct">��������</param>
        /// <returns>Ŀ���</returns>
        public Vector3 GetPoint_HitBoxEdge(BlockDirection _DIRECT)
        {

            switch (_DIRECT)
            {
                case BlockDirection.ǰ:
                    return selfPos + Vector3.forward * hitBoxWidth / 2f;
                case BlockDirection.��:
                    return selfPos + Vector3.back * hitBoxWidth / 2f;
                case BlockDirection.��:
                    return selfPos + Vector3.left * hitBoxWidth / 2f;
                case BlockDirection.��:
                    return selfPos + Vector3.right * hitBoxWidth / 2f;
                case BlockDirection.��:
                    return selfPos + Vector3.up * hitBoxHeight / 2f;
                case BlockDirection.��:
                    return selfPos + Vector3.down * hitBoxHeight / 2f;                   
                default:
                    print("GetPoint_HitBoxEdge��������������");
                    return Vector3.zero;
            }


        }


        //��������
        public Vector3 selfPos
        {
            get { return transform.position; }
        }

        public Vector3 EyesPoint
        {
            get
            {
                return transform.position + Vector3.up * hitBoxHeight / 2f * hitBoxEyes;
            }
        }

        //ͷ��
        public Vector3 HeadPoint
        {
            get
            {
                return selfPos + Vector3.up * hitBoxHeight / 2f;
            }
        }



        //���µ�
        public Vector3 FootPoint
        {
            get
            {
                return selfPos + Vector3.down * hitBoxHeight / 2f;
            }
        }

        public byte FootBlockType
        {
            get
            {
                return managerhub.world.GetBlockType(FootPoint);
            }
        }

        // ǰ����
        public Vector3 ǰ_����
        {
            get
            {
                return selfPos + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2, hitBoxWidth / 2);
            }
        }

        // ǰ����
        public Vector3 ǰ_����
        {
            get
            {
                return selfPos + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2, hitBoxWidth / 2);
            }
        }

        // ǰ����
        public Vector3 ǰ_����
        {
            get
            {
                return selfPos + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2 + hitBoxHeight, hitBoxWidth / 2);
            }
        }

        // ǰ����
        public Vector3 ǰ_����
        {
            get
            {
                return selfPos + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2 + hitBoxHeight, hitBoxWidth / 2);
            }
        }

        // ������ 
        public Vector3 ��_����
        {
            get
            {
                return selfPos + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2, -hitBoxWidth / 2);
            }
        }

        // ������
        public Vector3 ��_����
        {
            get
            {
                return selfPos + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2, -hitBoxWidth / 2);
            }
        }

        // ������
        public Vector3 ��_����
        {
            get
            {
                return selfPos + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2 + hitBoxHeight, -hitBoxWidth / 2);
            }
        }

        // ������
        public Vector3 ��_����
        {
            get
            {
                return selfPos + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2 + hitBoxHeight, -hitBoxWidth / 2);
            }
        }








        #endregion


        #region ��ȡ����

        //ʵ���泯����
        public Vector3 EntityFaceForward
        {
            get
            {
                return Head.transform.forward;
            }
        }

        #endregion


        #region ���⹤��

        /// <summary>
        /// �Ƿ���ˮ��
        /// </summary>
        public bool IsInTheWater(Vector3 _pos)
        {
            if (managerhub.world.GetBlockType(_pos) == VoxelData.Water)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion


    }
}