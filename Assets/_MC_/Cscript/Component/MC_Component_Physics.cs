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

        #region 状态

        [Foldout("状态", true)]
        [ReadOnly] public bool isGround = false;

        void _ReferUpdate_State()
        {
            isGround = collider_Down;
        }


        #endregion


        #region 生命周期

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


        #region 判断是否重叠

        [Foldout("实体重叠参数", true)]
        [Header("是否可被打到")] public bool canBeRayCastHit = true;
        

        /// <summary>
        /// 判定该点是否在判定向内
        /// </summary>
        public bool CheckHitBox(Vector3 _targetPos)
        {
            //提前返回-禁止射线检测
            if (!canBeRayCastHit)
                return false;

            Vector3 _selfPos = selfPos;
            float selfHalfWidth = hitBoxWidth / 2f;
            float selfHalfHeight = hitBoxHeight / 2f;

            // 判断目标点是否在当前判定箱内
            bool isInside =
                (_targetPos.x >= _selfPos.x - selfHalfWidth && _targetPos.x <= _selfPos.x + selfHalfWidth) &&
                (_targetPos.y >= _selfPos.y - selfHalfHeight && _targetPos.y <= _selfPos.y + selfHalfHeight) &&
                (_targetPos.z >= _selfPos.z - selfHalfWidth && _targetPos.z <= _selfPos.z + selfHalfWidth); // 如果需要考虑z轴

            return isInside;
        }


        /// <summary>
        /// 判定两个箱子是否重叠
        /// </summary>
        public Vector3 CheckHitBox(Vector3 _targetPos, float _targetWidth, float _targetHeight)
        {
            Vector3 _selfPos = selfPos;
            float _selfWidth = hitBoxWidth;
            float _selfHeight = hitBoxHeight;

            // 计算半宽和半高
            float selfHalfWidth = _selfWidth / 2f;
            float selfHalfHeight = _selfHeight / 2f;
            float targetHalfWidth = _targetWidth / 2f;
            float targetHalfHeight = _targetHeight / 2f;

            // 判断是否重叠
            bool isCollision =
                Mathf.Abs(_selfPos.x - _targetPos.x) < (selfHalfWidth + targetHalfWidth) &&
                Mathf.Abs(_selfPos.y - _targetPos.y) < (selfHalfHeight + targetHalfHeight) &&
                Mathf.Abs(_selfPos.z - _targetPos.z) < (selfHalfWidth + targetHalfWidth); // 如果需要考虑z轴

            // 如果发生碰撞，返回碰撞方向
            if (isCollision)
            {
                return (_targetPos - _selfPos).normalized;
            }
            else
            {
                return Vector3.zero; // 没有碰撞
            }
        }

        /// <summary>
        /// 检查两个Collider是否重叠
        /// </summary>
        /// <returns></returns>
        public bool CheckHitBox(MC_Component_Physics _ColliderA, MC_Component_Physics _ColliderB)
        {
            // 获取物体A和物体B的位置
            Vector3 positionA = _ColliderA.transform.position;
            Vector3 positionB = _ColliderB.transform.position;

            // 获取碰撞盒的尺寸（宽度、高度和深度）
            float widthA = _ColliderA.hitBoxWidth;
            float heightA = _ColliderA.hitBoxHeight;
            float widthB = _ColliderB.hitBoxWidth;
            float heightB = _ColliderB.hitBoxHeight;

            // 计算碰撞盒的尺寸
            Vector3 sizeA = new Vector3(widthA, heightA, widthA);  // 三维尺寸
            Vector3 sizeB = new Vector3(widthB, heightB, widthB);  // 三维尺寸

            // 计算A和B的碰撞盒的边界
            Vector3 minA = positionA - sizeA * 0.5f;
            Vector3 maxA = positionA + sizeA * 0.5f;

            Vector3 minB = positionB - sizeB * 0.5f;
            Vector3 maxB = positionB + sizeB * 0.5f;

            // 检查是否有重叠
            bool isOverlapping = (minA.x < maxB.x && maxA.x > minB.x) &&
                                  (minA.y < maxB.y && maxA.y > minB.y) &&
                                  (minA.z < maxB.z && maxA.z > minB.z);

            return isOverlapping;
        }




        #endregion


        #region 各个方向的碰撞检测

        public float Delta = 0.01f;
        public float Delta_Pro = 0.0125f;

        // 前方
        public bool collider_Front
        {
            get
            {
                if (isCollisionLocked) return false;

                if (managerhub.world.CollisionCheckForVoxel(前_左上 + new Vector3(Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(前_右上 + new Vector3(-Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(前_左下 + new Vector3(Delta, Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(前_右下 + new Vector3(-Delta, Delta, Delta)))
                {

                    //进行一次微调
                    if (!AdjustLock_Collsion_Front && Component_Velocity.GetVelocity().z > 0)
                    {
                        // 计算 X 坐标调整
                        //int _selfZ = (int)(GetPoint_HitBoxEdge(BlockDirection.前).z + Vector3.forward.z * Delta_Pro);
                        //float _blockZ = GetTargetBlockHeightAndWidth(BlockDirection.前);
                        //transform.position = new Vector3(selfPos.x, selfPos.y, _selfZ + _blockZ - hitBoxWidth / 2);
                        //print($"进行一次位置修正,_selyX: {_selfZ},  _blockX:{_blockZ}");

                        // 调整位置后锁定避免重复调用
                        AdjustLock_Collsion_Front = true;

                        //print("Left");
                    }


                    return true;
                }
                else
                {
                    // 若未碰撞到地面，解锁位置调整
                    if (AdjustLock_Collsion_Front)
                    {
                        AdjustLock_Collsion_Front = false;
                    }

                    return false;
                }
            }
        }

        // 后方
        public bool collider_Back
        {
            get
            {
                if (isCollisionLocked) return false;

                if (managerhub.world.CollisionCheckForVoxel(后_左上 + new Vector3(Delta, -Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(后_右上 + new Vector3(-Delta, -Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(后_左下 + new Vector3(Delta, Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(后_右下 + new Vector3(-Delta, Delta, -Delta)))
                {

                    //进行一次微调
                    if (!AdjustLock_Collsion_Back && Component_Velocity.GetVelocity().z < 0)
                    {
                        // 计算 X 坐标调整
                        //int _selfZ = (int)(GetPoint_HitBoxEdge(BlockDirection.后).z + Vector3.back.z * Delta_Pro);
                        //float _blockZ = GetTargetBlockHeightAndWidth(BlockDirection.后);
                        //transform.position = new Vector3(selfPos.x, selfPos.y, _selfZ + _blockZ + hitBoxWidth / 2);
                        //print($"进行一次位置修正,_selyX: {_selfZ},  _blockX:{_blockZ}");

                        // 调整位置后锁定避免重复调用
                        AdjustLock_Collsion_Back = true;

                        //print("Back");
                    }


                    return true;
                }
                else
                {
                    // 若未碰撞到地面，解锁位置调整
                    if (AdjustLock_Collsion_Back)
                    {
                        AdjustLock_Collsion_Back = false;
                    }

                    return false;
                }
            }
        }

        // 左方
        public bool collider_Left
        {
            get
            {
                if (isCollisionLocked) return false;

                if (managerhub.world.CollisionCheckForVoxel(前_左上 + new Vector3(-Delta, -Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(前_左下 + new Vector3(-Delta, Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(后_左上 + new Vector3(-Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(后_左下 + new Vector3(-Delta, Delta, Delta)))
                {

                    //进行一次微调
                    if (!AdjustLock_Collsion_Left && Component_Velocity.GetVelocity().x < 0)
                    {
                        // 计算 X 坐标调整
                        //int _selyX = (int)(GetPoint_HitBoxEdge(BlockDirection.左).x + Vector3.left.x * Delta_Pro);
                        //float _blockX = GetTargetBlockHeightAndWidth(BlockDirection.左);
                        //transform.position = new Vector3(_selyX + _blockX + hitBoxWidth / 2, selfPos.y, selfPos.z);
                        //print($"进行一次位置修正,_selyX: {_selyX},  _blockX:{_blockX}");

                        // 调整位置后锁定避免重复调用
                        AdjustLock_Collsion_Left = true;

                        //print("Left");
                    }


                    return true;
                }
                else
                {
                    // 若未碰撞到地面，解锁位置调整
                    if (AdjustLock_Collsion_Left)
                    {
                        AdjustLock_Collsion_Left = false;
                    }

                    return false;
                }
            }
        }

        // 右方
        public bool collider_Right
        {
            get
            {
                if (isCollisionLocked) return false;

                if (managerhub.world.CollisionCheckForVoxel(前_右上 + new Vector3(Delta, -Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(前_右下 + new Vector3(Delta, Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(后_右上 + new Vector3(Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(后_右下 + new Vector3(Delta, Delta, Delta)))
                {

                    


                    //进行一次微调
                    if (!AdjustLock_Collsion_Right && Component_Velocity.GetVelocity().x > 0)
                    {

                        // 计算 X 坐标调整
                        //int _selyX = (int)(GetPoint_HitBoxEdge(BlockDirection.右).x + Vector3.right.x * Delta_Pro);
                        //float _blockX = GetTargetBlockHeightAndWidth(BlockDirection.右);
                        //transform.position = new Vector3(_selyX + _blockX - hitBoxWidth / 2, selfPos.y, selfPos.z);
                        //print($"进行一次位置修正,_selyX: {_selyX},  _blockX:{_blockX}");

                        // 调整位置后锁定避免重复调用
                        AdjustLock_Collsion_Right = true;

                        //print("Right");
                    }


                    return true;
                }
                else
                {
                    // 若未碰撞到地面，解锁位置调整
                    if (AdjustLock_Collsion_Right)
                    {
                        AdjustLock_Collsion_Right = false;
                    }

                    return false;
                }
            }
        }

        // 上方
        public bool collider_Up
        {
            get
            {
                if (isCollisionLocked) return false;

                if (managerhub.world.CollisionCheckForVoxel(后_左上 + new Vector3(Delta, Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(前_左上 + new Vector3(Delta, Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(后_右上 + new Vector3(-Delta, Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(前_右上 + new Vector3(-Delta, Delta, -Delta)))
                {

                    //进行一次微调
                    if (!AdjustLock_Collsion_Up && Component_Velocity.GetVelocity().y > 0)
                    {
                        // 计算 Y 坐标调整
                        //int _selyY = (int)(GetPoint_HitBoxEdge(BlockDirection.上).y + Vector3.up.y * Delta_Pro);
                        //float _blockY = GetTargetBlockHeightAndWidth(BlockDirection.上);
                        //transform.position = new Vector3(selfPos.x, _selyY + _blockY - hitBoxHeight / 2, selfPos.z);
                        //print($"进行一次位置修正,_selyY: {_selyY},  _blockY:{_blockY}");

                        // 调整位置后锁定避免重复调用
                        AdjustLock_Collsion_Up = true;
                    }

                    
                    return true;
                }
                else
                {
                    // 若未碰撞到地面，解锁位置调整
                    if (AdjustLock_Collsion_Up)
                    {
                        AdjustLock_Collsion_Up = false;
                    }
                    
                    return false;
                }
            }
        }

        // 下方

        public bool collider_Down
        {
            get
            {
                // 若碰撞已锁定或校验被锁定则直接返回 false
                if (isCollisionLocked) return false;

                // 进行碰撞检测
                if (managerhub.world.CollisionCheckForVoxel(后_左下 + new Vector3(Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(前_左下 + new Vector3(Delta, -Delta, -Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(后_右下 + new Vector3(-Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(前_右下 + new Vector3(-Delta, -Delta, -Delta)))
                {

                    //进行一次微调
                    if (!AdjustLock_Collsion_Down && Component_Velocity.GetVelocity().y < 0)
                    {
                        // 计算 Y 坐标调整
                        int _selyY = (int)(GetPoint_HitBoxEdge(BlockDirection.下).y + Vector3.down.y * Delta_Pro);
                        float _blockY = GetTargetBlockHeightAndWidth(BlockDirection.下);
                        transform.position = new Vector3(selfPos.x, _selyY + _blockY + hitBoxHeight / 2, selfPos.z);
                        //print($"位置修正, _selfY: {_selyY},  _blockY:{_blockY}");

                        // 调整位置后锁定避免重复调用
                        AdjustLock_Collsion_Down = true;

                        //print("Down");
                    }

                   
                    return true;
                }
                else
                {
                    
                    // 若未碰撞到地面，解锁位置调整
                    if (AdjustLock_Collsion_Down)
                    {
                        //print("解锁");
                        AdjustLock_Collsion_Down = false;
                    }
                    
                    return false;
                }
            }
        }


        //周围
        public bool Collider_Surround
        {
            get
            {
                return !isCollisionLocked && (collider_Front || collider_Back || collider_Left || collider_Right);
            }
        }


        //动态碰撞细分
        private bool DynamicSubdivision_CollisionCheck(BlockDirection _DIRECT)
        {
            //检测矩形
            float _DynamicWidth = 1f;
            float _DynamicHeight = 1f;

            //中心点
            Vector3 _Center = Vector3.zero; 

            //Block
            //byte _targetBlockType = 0;
            //CollosionRange _targetBlockRange;

            //先确定Width
            switch (_DIRECT)
            {
                case BlockDirection.前:
                    break;
                case BlockDirection.后:
                    break;
                case BlockDirection.左:
                    break;
                case BlockDirection.上:
                    break;
                case BlockDirection.下:





                    //根据width进行碰撞点检测-Front
                    float _stepX = 1f / _DynamicWidth;
                    float _stepZ = 1f / _DynamicHeight;

                    //遍历每个矩形
                    for (float _z = 0; _z < _stepZ; _z += _DynamicHeight)
                    {
                        for (float _x = 0; _x < _stepX; _x += _DynamicWidth)
                        {
                            //managerhub.world.CollisionCheckForVoxel(_左下 + new Vector3(_x, _y, 1) * _DynamicWidth);
                        }
                    }

                    break;
            }

            

            return false;
        }



        #endregion


        #region 位置修正

        //位置修正
        private bool AdjustLock_Collsion_Front = false;
        private bool AdjustLock_Collsion_Back = false;
        private bool AdjustLock_Collsion_Left = false;
        private bool AdjustLock_Collsion_Right = false;
        private bool AdjustLock_Collsion_Up = false;
        private bool AdjustLock_Collsion_Down = false;

        /// <summary>
        /// 获取目标方向方块的高度或者宽度
        /// </summary>
        /// <param name="_DIRECT">方向，枚举</param>
        float GetTargetBlockHeightAndWidth(BlockDirection _DIRECT)
        {
            //比较值
            float _value = 0f;

            //边界变量
            Vector3 _00 = Vector3.zero;
            Vector3 _11 = Vector3.zero;

            switch (_DIRECT)
            {
                case BlockDirection.前:

                    //确定范围
                    _00 = 前_左下;
                    _11 = 前_右上;

                    //print($"_00: {_00}, _11: {_11}");

                    //从左下角进行遍历，步长为1f
                    for (float _y = _00.y; _y <= _11.y; _y++)
                    {
                        for (float _x = _00.x; _x <= _11.x; _x++)
                        {
                            //获取坐标
                            Vector3 _pos = new Vector3(_x, _y, _00.z + Delta);

                            //获取方块类型
                            byte _targetType = managerhub.world.GetBlockType(_pos);

                            //提前返回-空气不需要计算
                            if (_targetType == VoxelData.Air)
                            {
                                continue;
                            }

                            //获取目标最大高度
                            float _blockY = managerhub.world.blocktypes[_targetType].CollosionRange.zRange.x;

                            //print($"遍历了{_pos}, 当前方块类型:{_targetType}, 当前方块高度: {_blockY}");

                            //比较Yrange.Y取最大值
                            if (_blockY < _value)
                            {
                                _value = _blockY;
                            }
                        }
                    }
                    break;
                case BlockDirection.后:
                    //确定范围
                    _00 = 后_左下;
                    _11 = 后_右上;

                    //print($"_00: {_00}, _11: {_11}");

                    //从左下角进行遍历，步长为1f
                    for (float _y = _00.y; _y <= _11.y; _y++)
                    {
                        for (float _x = _00.x; _x <= _11.x; _x++)
                        {   
                            //获取坐标
                            Vector3 _pos = new Vector3(_x, _y, _00.z - Delta);

                            //获取方块类型
                            byte _targetType = managerhub.world.GetBlockType(_pos);

                            //提前返回-空气不需要计算
                            if (_targetType == VoxelData.Air)
                            {
                                continue;
                            }

                            //获取目标最大高度
                            float _blockY = managerhub.world.blocktypes[_targetType].CollosionRange.zRange.y;

                            //print($"遍历了{_pos}, 当前方块类型:{_targetType}, 当前方块高度: {_blockY}");

                            //比较Yrange.Y取最大值
                            if (_blockY > _value)
                            {
                                _value = _blockY;
                            }
                        }
                    }
                    break;
                case BlockDirection.左:

                    //确定范围
                    _00 = 后_左下;
                    _11 = 前_左上;

                    //print($"_00: {_00}, _11: {_11}");

                    //从左下角进行遍历，步长为1f
                    for (float _z = _00.z; _z <= _11.z; _z++)
                    {
                        for (float _y = _00.y; _y <= _11.y; _y++)
                        {
                            //获取坐标
                            Vector3 _pos = new Vector3(_00.x - Delta, _y, _z);

                            //获取方块类型
                            byte _targetType = managerhub.world.GetBlockType(_pos);

                            //提前返回-空气不需要计算
                            if (_targetType == VoxelData.Air)
                            {
                                continue;
                            }

                            //获取目标最大高度
                            float _blockY = managerhub.world.blocktypes[_targetType].CollosionRange.xRange.y;

                            //print($"遍历了{_pos}, 当前方块类型:{_targetType}, 当前方块高度: {_blockY}");

                            //比较Yrange.Y取最大值
                            if (_blockY > _value)
                            {
                                _value = _blockY;
                            }
                        }
                    }
                    break;
                case BlockDirection.右:

                    //确定范围
                    _00 = 后_右下;
                    _11 = 前_右上;

                    //从左下角进行遍历，步长为1f
                    for (float _z = _00.z; _z <= _11.z; _z++)
                    {
                        for (float _y = _00.y; _y <= _11.y; _y++)
                        {
                            //获取坐标
                            Vector3 _pos = new Vector3(_00.x + Delta, _y, _z);

                            //获取方块类型
                            byte _targetType = managerhub.world.GetBlockType(_pos);

                            //提前返回-空气不需要计算
                            if (_targetType == VoxelData.Air)
                            {
                                continue;
                            }

                            //获取目标最大高度
                            float _blockY = managerhub.world.blocktypes[_targetType].CollosionRange.xRange.x;

                            //print($"遍历了{_pos}, 当前方块类型:{_targetType}, 当前方块高度: {_blockY}");

                            //比较Yrange.Y取最大值
                            if (_blockY < _value)
                            {
                                _value = _blockY;
                            }
                        }
                    }
                    break;
                case BlockDirection.上:

                    //确定范围
                    _00 = 后_左上;
                    _11 = 前_右上;

                    //从左下角进行遍历，步长为1f
                    for (float _z = _00.z; _z <= _11.z; _z++)
                    {
                        for (float _x = _00.x; _x <= _11.x; _x++)
                        {
                            //获取坐标
                            Vector3 _pos = new Vector3(_x, _00.y + Delta, _z);

                            //获取方块类型
                            byte _targetType = managerhub.world.GetBlockType(_pos);

                            //提前返回-空气不需要计算
                            if (_targetType == VoxelData.Air)
                            {
                                continue;
                            }

                            //获取目标最低高度
                            float _blockY = managerhub.world.blocktypes[_targetType].CollosionRange.yRange.x;

                            print($"遍历了{_pos}, 当前方块类型:{_targetType}, 当前方块高度: {_blockY}");

                            //比较Yrange.Y取最大值
                            if (_blockY < _value)
                            {
                                _value = _blockY;
                            }
                        }
                    }

                    break;
                case BlockDirection.下:

                    //确定范围
                    _00 = 后_左下;
                    _11 = 前_右下;

                    //从左下角进行遍历，步长为1f
                    for (float _z = _00.z; _z <= _11.z; _z++)
                    {
                        for (float _x = _00.x; _x <= _11.x; _x++)
                        {
                            //获取坐标
                            Vector3 _pos = new Vector3(_x, _00.y - Delta, _z);

                            //获取方块类型
                            byte _targetType = managerhub.world.GetBlockType(_pos);

                            //提前返回-空气不需要计算
                            if (_targetType == VoxelData.Air)
                            {
                                continue;
                            }

                            //获取目标最大高度
                            float _blockY = managerhub.world.blocktypes[_targetType].CollosionRange.yRange.y;

                            //print($"遍历了{_pos}, 当前方块类型:{_targetType}, 当前方块高度: {_blockY}");

                            //比较Yrange.Y取最大值
                            if (_blockY > _value)
                            {
                                _value = _blockY;
                            }
                        }
                    }

                    break;

                default:
                    print("GetTargetBlockHeightAndWidth函数出现了问题");
                    break;
            }


            return _value;
        }


        #endregion


        #region 实体身体结构

        [Foldout("(可隐藏)实体身体结构", true)]
        [Header("模型(头+身的父类)")] public GameObject Model; //只用于死亡动画
        [Header("头部")] public GameObject Head;
        [Header("身体(除了头都是身体)")] public GameObject Body;


        //自动获取身体结构
        void _ReferAwake_AutoGetModel()
        {
            Model = transform.Find("Model").gameObject;
            Head = transform.Find("Model/Head").gameObject;
            Body = transform.Find("Model/Body").gameObject;
           


            //如果Model找不到才会显示Info
            if(Model == null || Head == null || Body == null)
            {
                MC_Component_Registration Component_Registration = GetComponent<MC_Component_Registration>();

                //提前返回-没有注册组件
                if (Component_Registration == null)
                {
                    print("实体未挂载注册组件且找不到Model");
                    return;
                }

                EntityInfo _info = Component_Registration.GetEntityId();
                if (Model == null)
                    print($"Model搜索不到, id:{_info._id}, name:{_info._name}");

                if (Head == null)
                    print($"Head搜索不到, id:{_info._id}, name:{_info._name}");

                if (Body == null)
                    print($"Body搜索不到, id:{_info._id}, name:{_info._name}");

            }


            
        }


        #endregion


        #region 暂时关闭碰撞检测


        // 暂时关闭所有点的碰撞检测
        public void CloseCollisionForAWhile(float _time)
        {
            isCollisionLocked = true;
            collisionLockTimer = _time;
            StartCoroutine(CollisionLockTimerCoroutine());
        }


        // 关于暂时停止碰撞检测的代码
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


        #region 判定箱

        // 判定箱设置
        [Foldout("判定箱", true)]
        [Header("<!警告!>：判定箱尺寸最好不要超过1.2m")]
        [Header("- 不然可能会有意料之外的Bug")]
        [Space]
        [Header("锁定绘制判定箱")] public bool LockDrawHitBox;
        [Header("绘制判定箱")] public bool isDrawHitBox; private bool hasExec_isDrawHitBox = true;
        [Header("判定箱眼睛相对高度")] public float hitBoxEyes = 0.8f;
        [Header("判定箱宽度")] public float hitBoxWidth = 1f;
        [Header("判定箱高度")] public float hitBoxHeight = 1f;
        private GameObject BodyObject;
        private GameObject EyesObject;
        private Mesh mesh;

       

        //引用Update
        void _ReferUpdate_HitBox()
        {
            //绘制判定箱
            DrawHitBox();
        }


        


        //绘制判定箱
        void DrawHitBox()
        {

            //提前返回-锁定绘制碰撞盒
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

        //绘制判定箱-Mesh逻辑
        void _DrawHitBox_DrawBox()
        {
            if (hasExec_isDrawHitBox)
            {
                // 创建线框的 GameObject 和 Mesh
                BodyObject = new GameObject("HitBoxLine");
                BodyObject.transform.parent = transform;
                BodyObject.transform.position = transform.position;

                Mesh whiteMesh = new Mesh();
                Mesh redMesh = new Mesh();

                // 添加白色部分的 MeshRenderer 和材质
                MeshFilter whiteMeshFilter = BodyObject.AddComponent<MeshFilter>();
                whiteMeshFilter.mesh = whiteMesh;
                MeshRenderer whiteMeshRenderer = BodyObject.AddComponent<MeshRenderer>();
                whiteMeshRenderer.material = new Material(Shader.Find("Unlit/Color")) { color = Color.white };

                // 创建“眼睛”部分的 GameObject 和 MeshRenderer
                EyesObject = new GameObject("HitBoxEyeLine");
                EyesObject.transform.parent = transform;
                EyesObject.transform.position = transform.position;

                MeshFilter redMeshFilter = EyesObject.AddComponent<MeshFilter>();
                redMeshFilter.mesh = redMesh;
                MeshRenderer redMeshRenderer = EyesObject.AddComponent<MeshRenderer>();
                redMeshRenderer.material = new Material(Shader.Find("Unlit/Color")) { color = Color.red };

                Vector3 center = BodyObject.transform.localPosition;

                // 顶点
                Vector3[] positions = new Vector3[8];
                Vector3[] eyePositions = new Vector3[4];

                // 底面四个角
                positions[0] = center + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2, hitBoxWidth / 2);
                positions[1] = center + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2, hitBoxWidth / 2);
                positions[2] = center + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2, -hitBoxWidth / 2);
                positions[3] = center + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2, -hitBoxWidth / 2);

                // 顶面四个角
                positions[4] = positions[0] + Vector3.up * hitBoxHeight;
                positions[5] = positions[1] + Vector3.up * hitBoxHeight;
                positions[6] = positions[2] + Vector3.up * hitBoxHeight;
                positions[7] = positions[3] + Vector3.up * hitBoxHeight;

                // 眼睛部分的四个角
                eyePositions[0] = positions[0] + Vector3.up * hitBoxHeight * hitBoxEyes;
                eyePositions[1] = positions[1] + Vector3.up * hitBoxHeight * hitBoxEyes;
                eyePositions[2] = positions[2] + Vector3.up * hitBoxHeight * hitBoxEyes;
                eyePositions[3] = positions[3] + Vector3.up * hitBoxHeight * hitBoxEyes;

                // 白色部分的索引
                int[] indices = new int[]
                {
            // 底面
            0, 1,
            1, 2,
            2, 3,
            3, 0,
            // 顶面
            4, 5,
            5, 6,
            6, 7,
            7, 4,
            // 侧面
            0, 4,
            1, 5,
            2, 6,
            3, 7
                };

                // 红色部分（眼睛）的索引
                int[] eyeIndices = new int[]
                {
            0, 1,
            1, 2,
            2, 3,
            3, 0
                };

                // 更新 Mesh 数据
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


        #region 实体挤压

        [Foldout("实体挤压", true)]
        [Header("是否会被动挤压")] public bool canBeBounced;
        [Header("分离速度")] public float BounceSpeed = 2f;

        //该函数在Update中
        void _ReferUpdate_EntityBounceCheck()
        {
            // 提前返回 - 如果自己不会被动挤压
            if (!canBeBounced)
                return;

            // 获取范围内的实体，如果没有则提前返回
            float _maxR = Mathf.Max(hitBoxWidth, hitBoxHeight);
            if (!managerhub.Service_Entity.GetOverlapSphereEntity(transform.position, _maxR, GetComponent<MC_Component_Registration>().GetEntityId()._id, out List<EntityInfo> _entities))
                return;

            // 进一步过滤，剔除没有和自己重叠的实体
            List<EntityInfo> overlappingEntities = new List<EntityInfo>();
            foreach (var item in _entities)
            {
                bool a = CheckHitBox(this, item._obj.GetComponent<MC_Component_Physics>());
                if (a) // 如果有重叠则保留
                {
                    overlappingEntities.Add(item);
                }
            }

            // 如果没有重叠的实体，直接返回
            if (overlappingEntities.Count == 0)
                return;

            // 计算所有重叠实体的反方向
            Vector3 _backDirect = Vector3.zero;
            foreach (var item in overlappingEntities)
            {
                // 计算与当前实体的反向向量（包括X、Y和Z方向）
                Vector3 direction = transform.position - item._obj.transform.position;
                _backDirect += direction;  // 加上所有轴向量（X, Y, Z）

            }

            // 将总的反方向标准化并乘上反速度
            _backDirect = _backDirect.normalized * BounceSpeed;

            // 将反方向应用到实体的速度组件，X、Y、Z方向
            Component_Velocity.SetVelocity("x", _backDirect.x);
            //Component_Velocity.SetVelocity("y", _backDirect.y);
            Component_Velocity.SetVelocity("z", _backDirect.z);

        }



        #endregion


        #region 获取点


        /// <summary>
        /// 获取基于Block的目标方向点,长度为1m
        /// </summary>
        /// <param name="_direct">方向向量</param>
        /// <returns>目标点</returns>
        public Vector3 GetPoint_Direct_1m(BlockDirection _DIRECT)
        {

            switch (_DIRECT)
            {
                case BlockDirection.前:
                    return selfPos + Vector3.forward;
                case BlockDirection.后:
                    return selfPos + Vector3.back;
                case BlockDirection.左:
                    return selfPos + Vector3.left;
                case BlockDirection.右:
                    return selfPos + Vector3.right;
                case BlockDirection.上:
                    return selfPos + Vector3.up;
                case BlockDirection.下:
                    return selfPos + Vector3.down;
                default:
                    print("GetPoint_Direct_1m函数出现了问题");
                    return Vector3.zero;
            }


        }


        /// <summary>
        /// 获取基于HitBox上的边缘点
        /// </summary>
        /// <param name="_direct">方向向量</param>
        /// <returns>目标点</returns>
        public Vector3 GetPoint_HitBoxEdge(BlockDirection _DIRECT)
        {

            switch (_DIRECT)
            {
                case BlockDirection.前:
                    return selfPos + Vector3.forward * hitBoxWidth / 2f;
                case BlockDirection.后:
                    return selfPos + Vector3.back * hitBoxWidth / 2f;
                case BlockDirection.左:
                    return selfPos + Vector3.left * hitBoxWidth / 2f;
                case BlockDirection.右:
                    return selfPos + Vector3.right * hitBoxWidth / 2f;
                case BlockDirection.上:
                    return selfPos + Vector3.up * hitBoxHeight / 2f;
                case BlockDirection.下:
                    return selfPos + Vector3.down * hitBoxHeight / 2f;                   
                default:
                    print("GetPoint_HitBoxEdge函数出现了问题");
                    return Vector3.zero;
            }


        }


        //自身坐标
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

        //头顶
        public Vector3 HeadPoint
        {
            get
            {
                return selfPos + Vector3.up * hitBoxHeight / 2f;
            }
        }



        //脚下点
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

        // 前左下
        public Vector3 前_左下
        {
            get
            {
                return selfPos + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2, hitBoxWidth / 2);
            }
        }

        // 前右下
        public Vector3 前_右下
        {
            get
            {
                return selfPos + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2, hitBoxWidth / 2);
            }
        }

        // 前左上
        public Vector3 前_左上
        {
            get
            {
                return selfPos + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2 + hitBoxHeight, hitBoxWidth / 2);
            }
        }

        // 前右上
        public Vector3 前_右上
        {
            get
            {
                return selfPos + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2 + hitBoxHeight, hitBoxWidth / 2);
            }
        }

        // 后左下 
        public Vector3 后_左下
        {
            get
            {
                return selfPos + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2, -hitBoxWidth / 2);
            }
        }

        // 后右下
        public Vector3 后_右下
        {
            get
            {
                return selfPos + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2, -hitBoxWidth / 2);
            }
        }

        // 后左上
        public Vector3 后_左上
        {
            get
            {
                return selfPos + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2 + hitBoxHeight, -hitBoxWidth / 2);
            }
        }

        // 后右上
        public Vector3 后_右上
        {
            get
            {
                return selfPos + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2 + hitBoxHeight, -hitBoxWidth / 2);
            }
        }








        #endregion


        #region 获取方向

        //实体面朝方向
        public Vector3 EntityFaceForward
        {
            get
            {
                return Head.transform.forward;
            }
        }

        #endregion


        #region 特殊工具

        /// <summary>
        /// 是否在水中
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