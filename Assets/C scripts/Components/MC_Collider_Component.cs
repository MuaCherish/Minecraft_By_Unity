using System.Collections;
using UnityEngine;
using MCEntity;
using Homebrew;
using static UsefulFunction;
using static UnityEngine.Rendering.DebugUI;

namespace MCEntity
{
    [RequireComponent(typeof(MC_Velocity_Component))]
    public class MC_Collider_Component : MonoBehaviour
    {

        #region 状态

        [Foldout("状态", true)]
        [ReadOnly] public bool isGround = false;

        void _ReferUpdate_State()
        {
            isGround = collider_Down;
        }


        #endregion


        #region 生命周期函数

        MC_Velocity_Component Velocity_Component;
        public ManagerHub managerhub;

        private void Awake()
        {
            Velocity_Component = GetComponent<MC_Velocity_Component>();

            if (managerhub == null)
            {
                managerhub = GlobalData.GetManagerhub();
            }
        }

        private void Update()
        {
            if (managerhub.world.game_state == Game_State.Playing)
            {
                _ReferUpdate_State();
                _ReferUpdate_HitBox();
            }
           
        }




        #endregion


        #region 公开函数

        /// <summary>
        /// 判定该点是否在判定向内
        /// </summary>
        public bool CheckHitBox(Vector3 _targetPos)
        {
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

        // 暂时关闭所有点的碰撞检测
        public void CloseCollisionForAWhile(float _time)
        {
            isCollisionLocked = true;
            collisionLockTimer = _time;
            StartCoroutine(CollisionLockTimerCoroutine());
        }


        #endregion


        #region 碰撞检测

        // 前方
        public bool collider_Front
        {
            get
            {
                if (isCollisionLocked) return false;

                if (managerhub.world.CollisionCheckForVoxel(前_左上 + new Vector3(Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(前_右上 + new Vector3(-Delta, -Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(前_左下 + new Vector3(Delta, Delta, Delta)) ||
                    managerhub.world.CollisionCheckForVoxel(前_右下 + new Vector3(-Delta, Delta, Delta)) ||
                    TryCheckMore_SubdivisionCollition(BlockDirection.前))
                {

                    //进行一次微调
                    if (!AdjustLock_Collsion_Front && Velocity_Component.GetVelocity().z > 0)
                    {
                        // 计算 X 坐标调整
                        int _selfZ = (int)(GetPoint_HitBoxEdge(BlockDirection.前).z + Vector3.forward.z * Delta_Pro);
                        float _blockZ = GetTargetBlockHeightAndWidth(BlockDirection.前);
                        transform.position = new Vector3(selfPos.x, selfPos.y, _selfZ + _blockZ - hitBoxWidth / 2);
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
                    managerhub.world.CollisionCheckForVoxel(后_右下 + new Vector3(-Delta, Delta, -Delta)) ||
                    TryCheckMore_SubdivisionCollition(BlockDirection.后))
                {

                    //进行一次微调
                    if (!AdjustLock_Collsion_Back && Velocity_Component.GetVelocity().z < 0)
                    {
                        // 计算 X 坐标调整
                        int _selfZ = (int)(GetPoint_HitBoxEdge(BlockDirection.后).z + Vector3.back.z * Delta_Pro);
                        float _blockZ = GetTargetBlockHeightAndWidth(BlockDirection.后);
                        transform.position = new Vector3(selfPos.x, selfPos.y, _selfZ + _blockZ + hitBoxWidth / 2);
                        //print($"进行一次位置修正,_selyX: {_selfZ},  _blockX:{_blockZ}");

                        // 调整位置后锁定避免重复调用
                        AdjustLock_Collsion_Back = true;

                        //print("Left");
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
                    managerhub.world.CollisionCheckForVoxel(后_左下 + new Vector3(-Delta, Delta, Delta)) ||
                    TryCheckMore_SubdivisionCollition(BlockDirection.左))
                {

                    //进行一次微调
                    if (!AdjustLock_Collsion_Left && Velocity_Component.GetVelocity().x < 0)
                    {
                        // 计算 X 坐标调整
                        int _selyX = (int)(GetPoint_HitBoxEdge(BlockDirection.左).x + Vector3.left.x * Delta_Pro);
                        float _blockX = GetTargetBlockHeightAndWidth(BlockDirection.左);
                        transform.position = new Vector3(_selyX + _blockX + hitBoxWidth / 2, selfPos.y, selfPos.z);
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
                    managerhub.world.CollisionCheckForVoxel(后_右下 + new Vector3(Delta, Delta, Delta)) ||
                    TryCheckMore_SubdivisionCollition(BlockDirection.右))
                {

                    


                    //进行一次微调
                    if (!AdjustLock_Collsion_Right && Velocity_Component.GetVelocity().x > 0)
                    {

                        // 计算 X 坐标调整
                        int _selyX = (int)(GetPoint_HitBoxEdge(BlockDirection.右).x + Vector3.right.x * Delta_Pro);
                        float _blockX = GetTargetBlockHeightAndWidth(BlockDirection.右);
                        transform.position = new Vector3(_selyX + _blockX - hitBoxWidth / 2, selfPos.y, selfPos.z);
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
                    managerhub.world.CollisionCheckForVoxel(前_右上 + new Vector3(-Delta, Delta, -Delta)) ||
                    TryCheckMore_SubdivisionCollition(BlockDirection.上))
                {

                    //进行一次微调
                    if (!AdjustLock_Collsion_Up && Velocity_Component.GetVelocity().y > 0)
                    {
                        // 计算 Y 坐标调整
                        int _selyY = (int)(GetPoint_HitBoxEdge(BlockDirection.上).y + Vector3.up.y * Delta_Pro);
                        float _blockY = GetTargetBlockHeightAndWidth(BlockDirection.上);
                        transform.position = new Vector3(selfPos.x, _selyY + _blockY - hitBoxHeight / 2, selfPos.z);
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
                    managerhub.world.CollisionCheckForVoxel(前_右下 + new Vector3(-Delta, -Delta, -Delta)) ||
                    TryCheckMore_SubdivisionCollition(BlockDirection.下))
                {

                    //进行一次微调
                    if (!AdjustLock_Collsion_Down && Velocity_Component.GetVelocity().y < 0)
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


        #region 暂时关闭碰撞检测

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


        #region 获取点

        //碰撞细分
        //根据指定方向的点的方块的长度
        //判断自身长度是否足够
        //否则将进行碰撞细分
        bool TryCheckMore_SubdivisionCollition(BlockDirection _DIRECT)
        {
            return false;
        }

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
                return transform.position + Vector3.up * hitBoxHeight * hitBoxEyes;
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