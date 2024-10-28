using System.Collections;
using UnityEngine;
using MCEntity;
using Homebrew;

namespace MCEntity
{
    [RequireComponent(typeof(MC_Velocity_Component))]
    public class MC_Collider_Component : MonoBehaviour
    {

        #region 状态
        [Foldout("状态", true)]
        [ReadOnly] public bool isGround = false;

        void ReferUpdate()
        {
            isGround = collider_Down;
        }


        #endregion



        #region 生命周期函数

        MC_Velocity_Component Velocity_Component;
        ManagerHub managerhub;

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

            ReferUpdate();
            ReferUpdateHitBox();
        }




        #endregion


        #region 判定箱

        // 判定箱设置
        [Foldout("判定箱", true)]
        [Header("绘制判定箱")] public bool isDrawHitBox; private bool hasExec_isDrawHitBox = true;
        [Header("判定箱眼睛相对高度")] public float hitBoxEyes = 0.8f;
        [Header("判定箱宽度")] public float hitBoxWidth = 1f;
        [Header("判定箱高度")] public float hitBoxHeight = 1f;
        private GameObject lineObject;
        private Mesh mesh;

        // 前方碰撞检测
        public bool collider_Front
        {
            get
            {
                if (isCollisionLocked) return false;

                return managerhub.world.CollisionCheckForVoxel(前_左上 + new Vector3(0f, 0f, 0.01f)) ||
                       managerhub.world.CollisionCheckForVoxel(前_右上 + new Vector3(0f, 0f, 0.01f)) ||
                       managerhub.world.CollisionCheckForVoxel(前_左下 + new Vector3(0f, 0f, 0.01f)) ||
                       managerhub.world.CollisionCheckForVoxel(前_右下 + new Vector3(0f, 0f, 0.01f)) ||
                       TryCheckMore_SubdivisionCollition(new Vector3(0f, 0f, 1f));
            }
        }

        // 后方碰撞检测
        public bool collider_Back
        {
            get
            {
                if (isCollisionLocked) return false;

                return managerhub.world.CollisionCheckForVoxel(后_左上 + new Vector3(0f, 0f, -0.01f)) ||
                       managerhub.world.CollisionCheckForVoxel(后_右上 + new Vector3(0f, 0f, -0.01f)) ||
                       managerhub.world.CollisionCheckForVoxel(后_左下 + new Vector3(0f, 0f, -0.01f)) ||
                       managerhub.world.CollisionCheckForVoxel(后_右下 + new Vector3(0f, 0f, -0.01f)) ||
                       TryCheckMore_SubdivisionCollition(new Vector3(0f, 0f, -1f));
            }
        }

        // 左方碰撞检测
        public bool collider_Left
        {
            get
            {
                if (isCollisionLocked) return false;

                return managerhub.world.CollisionCheckForVoxel(前_左上 + new Vector3(-0.01f, 0f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(前_左下 + new Vector3(-0.01f, 0f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(后_左上 + new Vector3(-0.01f, 0f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(后_左下 + new Vector3(-0.01f, 0f, 0f)) ||
                       TryCheckMore_SubdivisionCollition(new Vector3(-1f, 0f, 0f));
            }
        }

        // 右方碰撞检测
        public bool collider_Right
        {
            get
            {
                if (isCollisionLocked) return false;

                return managerhub.world.CollisionCheckForVoxel(前_右上 + new Vector3(0.01f, 0f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(前_右下 + new Vector3(0.01f, 0f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(后_右上 + new Vector3(0.01f, 0f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(后_右下 + new Vector3(0.01f, 0f, 0f)) ||
                       TryCheckMore_SubdivisionCollition(new Vector3(1f, 0f, 0f));
            }
        }

        // 上方碰撞检测
        public bool collider_Up
        {
            get
            {
                if (isCollisionLocked) return false;

                return managerhub.world.CollisionCheckForVoxel(前_左上 + new Vector3(0f, 0.01f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(前_右上 + new Vector3(0f, 0.01f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(后_左上 + new Vector3(0f, 0.01f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(后_右上 + new Vector3(0f, 0.01f, 0f)) ||
                       TryCheckMore_SubdivisionCollition(new Vector3(0f, 1f, 0f));
            }
        }

        // 下方碰撞检测
        public bool collider_Down
        {
            get
            {
                if (isCollisionLocked) return false;

                return managerhub.world.CollisionCheckForVoxel(前_左下 + new Vector3(0f, -0.01f, 0f)) ||
                    managerhub.world.CollisionCheckForVoxel(前_右下 + new Vector3(0f, -0.01f, 0f)) ||
                    managerhub.world.CollisionCheckForVoxel(后_左下 + new Vector3(0f, -0.01f, 0f)) ||
                    managerhub.world.CollisionCheckForVoxel(后_右下 + new Vector3(0f, -0.01f, 0f)) ||
                    TryCheckMore_SubdivisionCollition(new Vector3(0f, -1f, 0f));

            }
        }
        public bool Collider_Surround
        {
            get
            {
                return !isCollisionLocked && (collider_Front || collider_Back || collider_Left || collider_Right);
            }
        }




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


        //引用Update
        private void ReferUpdateHitBox()
        {
            //位置修正
            if (collider_Down)
            {
                // 计算新的 y 位置，避免物体陷入地下
                float newYPosition = (int)selfPos.y + hitBoxHeight / 2f;
                // 创建一个新的位置
                Vector3 newPosition = new Vector3(transform.position.x, newYPosition, transform.position.z);

                // 更新物体的位置
                transform.position = newPosition;
            }


            //绘制判定箱
            if (isDrawHitBox)
            {
                DrawHitBox();
                
            }
            else
            {
                if (lineObject != null)
                {
                    Destroy(lineObject);
                    lineObject = null;
                }

                if (hasExec_isDrawHitBox == false)
                {
                    hasExec_isDrawHitBox = true;
                }
            }

        }

        //绘制判定箱
        private void DrawHitBox()
        {

            if (hasExec_isDrawHitBox)
            {

                // 创建一个新的 GameObject 用于绘制线框
                lineObject = new GameObject("HitBoxLine");
                lineObject.transform.parent = transform; // 设定父物体为当前物体
                lineObject.transform.position = transform.position;

                // 创建 Mesh 组件
                mesh = new Mesh();
                MeshFilter meshFilter = lineObject.AddComponent<MeshFilter>();
                meshFilter.mesh = mesh;

                // 添加 MeshRenderer 组件
                MeshRenderer meshRenderer = lineObject.AddComponent<MeshRenderer>();
                meshRenderer.material = new Material(Shader.Find("Unlit/Color")) { color = Color.white }; // 使用无光照材质

                Vector3 center = lineObject.transform.localPosition;

                // 计算判定箱的八个角
                Vector3[] positions = new Vector3[12];

                // 底面四个角
                positions[0] = center + new Vector3(-hitBoxWidth / 2 - 0.01f, -hitBoxHeight / 2,  hitBoxWidth / 2 + 0.01f);  // 前左
                positions[1] = center + new Vector3( hitBoxWidth / 2 + 0.01f, -hitBoxHeight / 2,  hitBoxWidth / 2 + 0.01f);   // 前右
                positions[2] = center + new Vector3( hitBoxWidth / 2 + 0.01f, -hitBoxHeight / 2, -hitBoxWidth / 2 - 0.01f);  // 后右
                positions[3] = center + new Vector3(-hitBoxWidth / 2 - 0.01f, -hitBoxHeight / 2, -hitBoxWidth / 2 - 0.01f); // 后左

                // 顶面四个角
                positions[4] = positions[0] + Vector3.up * hitBoxHeight; // 前左上
                positions[5] = positions[1] + Vector3.up * hitBoxHeight; // 前右上
                positions[6] = positions[2] + Vector3.up * hitBoxHeight; // 后右上
                positions[7] = positions[3] + Vector3.up * hitBoxHeight; // 后左上

                // 眼睛
                positions[8] = positions[0] + Vector3.up * hitBoxHeight * hitBoxEyes; // 前左上
                positions[9] = positions[1] + Vector3.up * hitBoxHeight * hitBoxEyes; // 前右上
                positions[10] = positions[2] + Vector3.up * hitBoxHeight * hitBoxEyes; // 后右上
                positions[11] = positions[3] + Vector3.up * hitBoxHeight * hitBoxEyes; // 后左上

                // 创建线段索引
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
                    3, 7,
                    // 侧面
                    8, 9,
                    9, 10,
                    10, 11,
                    11, 8
                };

                // 更新 Mesh 数据
                mesh.Clear();
                mesh.vertices = positions;
                mesh.SetIndices(indices, MeshTopology.Lines, 0);
                mesh.RecalculateBounds();

                hasExec_isDrawHitBox = false;
            }

        }


        #endregion


        #region 获取点

        //碰撞细分
        //根据指定方向的点的方块的长度
        //判断自身长度是否足够
        //否则将进行碰撞细分
        bool TryCheckMore_SubdivisionCollition(Vector3 _Direct)
        {
            return false;
        }

        /// <summary>
        /// 获取基于Block的目标方向点
        /// </summary>
        /// <param name="_direct">方向向量</param>
        /// <returns>目标点</returns>
        public Vector3 GetBlockDirectPoint(Vector3 _direct)
        {
            // 确保方向向量为单位向量
            _direct.Normalize();

            // 根据方向返回相应的目标点
            if (_direct == Vector3.forward) // Front
            {
                return selfPos + Vector3.forward;
            }
            else if (_direct == Vector3.back) // Back
            {
                return selfPos + Vector3.back;
            }
            else if (_direct == Vector3.left) // Left
            {
                return selfPos + Vector3.left;
            }
            else if (_direct == Vector3.right) // Right
            {
                return selfPos + Vector3.right;
            }
            else if (_direct == Vector3.up) // Up
            {
                return selfPos + Vector3.up;
            }
            else if (_direct == Vector3.down) // Down
            {
                return selfPos + Vector3.down;
            }
            else
            {
                Debug.LogError("GetDirectionPoint输入不正确");
                return Vector3.zero; // 返回零向量表示错误
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
                return selfPos - Vector3.down * hitBoxHeight - new Vector3(0f, 0.01f, 0f);
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


        private bool isCollisionLocked = false;
        private float collisionLockTimer = 0f;

        // 暂时关闭碰撞检测
        //isGround除外
        public void CloseCollisionForAWhile(float _time)
        {
            isCollisionLocked = true;
            collisionLockTimer = _time;
            StartCoroutine(CollisionLockTimerCoroutine());
        }

        // 碰撞检测的协程，用于解锁
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


    }
}