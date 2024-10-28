using Homebrew;
using MCEntity;
using UnityEngine;
//using UnityEngine.XR;
using System.Collections;
using UnityEngine.XR;


public class DropBlock: MonoBehaviour
{

    #region 周期函数

    [Foldout("初始化参数", true)]
    [Header("初始化随机跳跃力度")] public float JumpValue = 40f;
    BlockItem myItem = new BlockItem(VoxelData.Air, 1);
    ManagerHub managerhub;
    MC_Collider_Component Collider_Component;
    MC_Velocity_Component Velocity_Component;

    private void Awake()
    {
        Collider_Component = GetComponent<MC_Collider_Component>();
        Velocity_Component = GetComponent<MC_Velocity_Component>();
    }

    private void Start()
    {
        
    }

    public bool Toggle_Start;
    private void Update()
    {
        if (Toggle_Start)
        {
            OnStartEntity(transform.position, new BlockItem(VoxelData.Stone, 1), true);
            Toggle_Start = false;
        }
        
        ReferUpdateFloating();

        ReferUpdateBeBuried();
    }

    public void OnStartEntity(Vector3 _CenterPos, BlockItem _Item, bool _isRandomJump)
    {
        Destroy(this, destroyTime);

        transform.position = _CenterPos;
        myItem = _Item;

        if (_isRandomJump)
        {
            Velocity_Component.EntityRandomJump(JumpValue);
        }
        

        if (FloatingCube == null)
        {
            FloatingCube = transform.Find("Body").gameObject;
        }

        if (managerhub == null)
        {
            managerhub = GlobalData.GetManagerhub();
        }

        GenMesh();

        StartCoroutine(AbsorbCheck());
    }

    public void OnEndEntity()
    {
        //播放音效
        managerhub.musicManager.PlaySound_Absorb();



        //背包系统计数
        if (myItem._blocktype != VoxelData.BedRock)
        {
            byte _point_Block_type = myItem._blocktype;

            //草块变泥土
            if (_point_Block_type == VoxelData.Grass)
            {
                _point_Block_type = VoxelData.Soil;
            }

            managerhub.backpackManager.update_slots(0, _point_Block_type, myItem._number);
        }


        // 移动完成后销毁自身
        Destroy(gameObject);
    }

    #endregion


    #region 材质部分

    GameObject[] Faces = new GameObject[6];
    private float Offset = 1f / 32f;

    //为每个面创建材质
    void GenMesh()
    {
        if (managerhub == null)
        {
            managerhub = GlobalData.GetManagerhub();
        }


        if (managerhub.world.blocktypes[myItem._blocktype].is2d)
        {
            GenMesh_2D();

        }
        else
        {
            GenMesh_3D();
        }


    }

    void GenMesh_2D()
    {
        managerhub.textureTo3D.ProcessSprite(managerhub.world.blocktypes[myItem._blocktype].sprite, FloatingCube.transform, 9.5f, false);
    }

    void GenMesh_3D()
    {
        FindFaces();

        // 假设 Faces 的顺序为 front-back-left-right-up-down
        Texture2D atlasTexture = managerhub.world.BlocksatlasTexture;
        Rect frontRect = managerhub.world.blocktypes[myItem._blocktype].front_sprite.rect; // 获取前面纹理的矩形区域
        Rect upRect = managerhub.world.blocktypes[myItem._blocktype].top_sprit.rect; // 获取上面纹理的矩形区域
        Rect bottomRect = managerhub.world.blocktypes[myItem._blocktype].buttom_sprit.rect; // 获取下面纹理的矩形区域
        Rect surroundRect = managerhub.world.blocktypes[myItem._blocktype].sprite.rect; // 获取周围的矩形区域

        // 创建材质实例，将材质附着到 Faces 下对应的 MeshRenderer 上
        for (int i = 0; i < 6; i++)
        {
            var meshRenderer = Faces[i].GetComponent<MeshRenderer>();

            // 为每个面创建一个材质实例
            Material faceMaterial = new Material(meshRenderer.material);

            // 设置不同的纹理
            if (i == 0) // Front
            {
                faceMaterial.mainTexture = atlasTexture;
                faceMaterial.SetTextureScale("_MainTex", new Vector2(frontRect.width / atlasTexture.width, frontRect.height / atlasTexture.height));
                faceMaterial.SetTextureOffset("_MainTex", new Vector2(frontRect.x / atlasTexture.width + Offset, frontRect.y / atlasTexture.height + Offset));
            }
            else if (i == 4) // Up
            {
                faceMaterial.mainTexture = atlasTexture;
                faceMaterial.SetTextureScale("_MainTex", new Vector2(upRect.width / atlasTexture.width, upRect.height / atlasTexture.height));
                faceMaterial.SetTextureOffset("_MainTex", new Vector2(upRect.x / atlasTexture.width + Offset, upRect.y / atlasTexture.height + Offset));
            }
            else if (i == 5) // Down
            {
                faceMaterial.mainTexture = atlasTexture;
                faceMaterial.SetTextureScale("_MainTex", new Vector2(bottomRect.width / atlasTexture.width, bottomRect.height / atlasTexture.height));
                faceMaterial.SetTextureOffset("_MainTex", new Vector2(bottomRect.x / atlasTexture.width + Offset, bottomRect.y / atlasTexture.height + Offset));
            }
            else // Surrounding faces
            {
                faceMaterial.mainTexture = atlasTexture;
                faceMaterial.SetTextureScale("_MainTex", new Vector2(surroundRect.width / atlasTexture.width, surroundRect.height / atlasTexture.height));
                faceMaterial.SetTextureOffset("_MainTex", new Vector2(surroundRect.x / atlasTexture.width + Offset, surroundRect.y / atlasTexture.height + Offset));
            }

            // 将新创建的材质实例分配给当前面
            meshRenderer.material = faceMaterial;
            Faces[i].SetActive(true);
        }
    }

    //找到自己所有的面
    void FindFaces()
    {

        // 检查是否找到了 "Body" 对象
        if (FloatingCube == null)
        {
            Debug.LogWarning("未找到 'Body' 子对象");
            return;
        }

        // 获取“Body”下的所有子对象，并确保数量为6
        if (FloatingCube.transform.childCount != 6)
        {
            Debug.LogWarning("子对象数量不为6，无法正确分配到Faces数组");
            return;
        }

        // 将“Body”子对象放入Faces数组
        for (int i = 0; i < 6; i++)
        {
            Faces[i] = FloatingCube.transform.GetChild(i).gameObject;
        }
    }

    #endregion


    #region 漂浮部分

    //参数
    [Foldout("漂浮部分", true)]
    [Header("是否需要上下漂浮")][ReadOnly] public bool StopFloating;
    [Header("生命周期")] public float destroyTime = 100f;
    [Header("旋转速度")] public float rotationSpeed = 30f; 
    [Header("漂浮高度")] public float floatingHeight = 0.3f; 
    [Header("漂浮速度")] public float floatingSpeed = 1f;
    private GameObject FloatingCube;
    private float originalY;
    private float floatingOffset; // 漂浮偏移量

    bool hasExec_isGround = true;
    void ReferUpdateFloating()
    {
        if (FloatingCube == null)
        {
            FloatingCube = transform.Find("Body").gameObject;
        }

        // 顺时针旋转
        FloatingCube.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 落地才开始上下漂浮
        if (Collider_Component.isGround && !StopFloating)
        {
            if (hasExec_isGround)
            {
                originalY = transform.position.y;
                //print(originalY);
                hasExec_isGround = false;
            }

            // 计算上下漂浮的偏移量
            floatingOffset = Mathf.Sin(Time.time * floatingSpeed) * (floatingHeight / 2f); // 使用一半的高度来中心化漂浮

            // 更新物体的Y坐标，确保在原始高度的范围内
            FloatingCube.transform.position = new Vector3(
                transform.position.x,
                originalY + floatingOffset + (floatingHeight / 2f), // 加上中心点的高度
                transform.position.z
            );
        }
        else
        {
            hasExec_isGround = true;
        }
    }

    #endregion


    #region 吸收部分

    Coroutine MoveToPlayerCoroutine;
    [Foldout("吸收部分", true)]
    [Header("吸收时间")] public float moveDuration = 0.2f;
    [Header("最小吸收范围")] public float absorbDistance = 2.3f;
    [Header("刚创建的吸收冷却时间")] public float ColdTimeToAbsorb = 1f;
    Vector3 Eyes;

    //吸收检测
    IEnumerator AbsorbCheck()
    {
        yield return new WaitForSeconds(ColdTimeToAbsorb);
        Eyes = Collider_Component.EyesPoint;

        //是否可被吸收
        while (true)
        {
            Vector3 PlayerEyes = managerhub.player.eyesObject.transform.position;

            if (((FloatingCube.transform.position - PlayerEyes).magnitude < absorbDistance) && managerhub.backpackManager.CheckSlotsFull(myItem._blocktype) == false)
            {
                Absorbable();
            }

            yield return new WaitForFixedUpdate();
        }

    }


    void Absorbable()
    {
        if (MoveToPlayerCoroutine == null)
        {
            MoveToPlayerCoroutine = StartCoroutine(MoveToPlayer());
        }

    }

    IEnumerator MoveToPlayer()
    {
        float elapsedTime = 0.0f;
        Vector3 startingPosition = FloatingCube.transform.position;
        StopFloating = true;

        while (elapsedTime < moveDuration)
        {
            Vector3 PlayerEyes = managerhub.player.eyesObject.transform.position;

            // 每一帧更新玩家的位置
            Vector3 targetPosition = new Vector3(PlayerEyes.x, PlayerEyes.y - 0.3f, PlayerEyes.z);

            // 在移动持续时间内逐渐移动到目标位置
            FloatingCube.transform.position = Vector3.Lerp(startingPosition, targetPosition, (elapsedTime / moveDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        //完成移动
        OnEndEntity();
    }



    #endregion


    #region 被埋部分

    // 在 Update 中
    void ReferUpdateBeBuried()
    {
        bool Front = Collider_Component.collider_Front;
        bool Back = Collider_Component.collider_Back;
        bool Left = Collider_Component.collider_Left;
        bool Right = Collider_Component.collider_Right;
        bool Up = Collider_Component.collider_Up;
        bool Down = Collider_Component.collider_Down;

        // 如果被方块埋没
        if (Front && Back && Left && Right && Up && Down)
        {
            HandleBuried();
        }
    }

    // 处理埋没情况
    [Foldout("被埋没情况" ,true)]
    [Header("是否被埋")] private bool isBuried = false; // 用于避免重复处理
    [Header("检测时间")] public float HandleBuriedColdTime = 1f;  // 冷却时间
    [Header("逃逸力")] public float HandleBuriedForceValue = 60f;  // 力度大小 
    
    void HandleBuried()
    {
        // 防止重复触发
        if (isBuried)
            return;

        Vector3 _Force = Vector3.zero;

        isBuried = true; // 标记为已埋没
                         // 暂时关闭碰撞

        
        
        // 优先搜索四周是否有空气，搜到了就向那个方块进行移动
        if (managerhub.world.GetBlockType(Collider_Component.GetBlockDirectPoint(Vector3.forward)) == VoxelData.Air)
        {
            Collider_Component.CloseCollisionForAWhile(0.2f);
            _Force = Vector3.forward;
            _Force.y = 0.5f;
            Velocity_Component.AddForce(_Force, HandleBuriedForceValue);
        }
        else if (managerhub.world.GetBlockType(Collider_Component.GetBlockDirectPoint(Vector3.back)) == VoxelData.Air)
        {
            Collider_Component.CloseCollisionForAWhile(0.2f);
            _Force = Vector3.back;
            _Force.y = 0.5f;
            Velocity_Component.AddForce(_Force, HandleBuriedForceValue);
        }
        else if (managerhub.world.GetBlockType(Collider_Component.GetBlockDirectPoint(Vector3.left)) == VoxelData.Air)
        {
            Collider_Component.CloseCollisionForAWhile(0.2f);
            _Force = Vector3.left;
            _Force.y = 0.5f;
            Velocity_Component.AddForce(_Force, HandleBuriedForceValue);
        }
        else if (managerhub.world.GetBlockType(Collider_Component.GetBlockDirectPoint(Vector3.right)) == VoxelData.Air)
        {
            Collider_Component.CloseCollisionForAWhile(0.2f);
            _Force = Vector3.right;
            _Force.y = 0.5f;
            Velocity_Component.AddForce(_Force, HandleBuriedForceValue);
        }
        else
        {
            // 如果没搜到则向上方跳跃1f
            Collider_Component.CloseCollisionForAWhile(0.5f);
            _Force = Vector3.up;
            Velocity_Component.AddForce(_Force, 77f);
        }
        

        // 设置冷却时间，恢复状态
        StartCoroutine(ResetBuriedState());
    }

    // 恢复埋没状态的协程
    private IEnumerator ResetBuriedState()
    {
        yield return new WaitForSeconds(HandleBuriedColdTime);
        isBuried = false; // 允许再次处理
    }


    #endregion
}
