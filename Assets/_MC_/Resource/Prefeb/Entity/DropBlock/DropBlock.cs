using Homebrew;
using MCEntity;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class DropBlock: MonoBehaviour, IEntityBrain
{

    #region ���ں���

    [Foldout("��ʼ������", true)]
    [Header("��ʼ�������Ծ����")] public float JumpValue = 40f;
    BlockItem myItem = new BlockItem(VoxelData.Air, 1);

    ManagerHub managerhub;
    MC_Component_Physics Component_Physics;
    MC_Component_Velocity Component_Velocity; 
    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        Component_Physics = GetComponent<MC_Component_Physics>();
        Component_Velocity = GetComponent<MC_Component_Velocity>();
    }

    void Update()
    {

        //if (Toggle_Start)
        //{
        //    OnStartEntity(transform.position, new BlockItem(VoxelData.Stone, 1), true);
        //    Toggle_Start = false;
        //}
        
        ReferUpdateFloating();

        ReferUpdateBeBuried();
    }

    public void OnStartEntity()
    {
        //throw new System.NotImplementedException();
    }

    public void OnStartEntity(Vector3 _CenterPos, BlockItem _Item, bool _isRandomJump)
    {

        Destroy(this.gameObject, destroyTime);

        transform.position = _CenterPos;
        myItem = _Item;

        if (_isRandomJump)
        {
            Component_Velocity.EntityRandomJump(JumpValue);
        }


        if (FloatingCube == null)
        {
            FloatingCube = Component_Physics.Body;
        }

        GenMesh();

        StartCoroutine(AbsorbCheck());
    }


    public void OnEndEntity()
    {
        //������Ч
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            managerhub.NewmusicManager.PlayOneShot(MusicData.absorb_1);
        }
        else
        {
            managerhub.NewmusicManager.PlayOneShot(MusicData.absorb_2);
        }

        //����ϵͳ����
        byte _point_Block_type = myItem._blocktype;

        //�ݿ������
        if (_point_Block_type == VoxelData.Grass)
        {
            _point_Block_type = VoxelData.Soil;
        }

        managerhub.backpackManager.update_slots(0, _point_Block_type, myItem._number);


        // �ƶ���ɺ���������
        Destroy(gameObject);
    }



    #endregion


    #region ���ʲ���

    GameObject[] Faces = new GameObject[6];
    private float Offset = 1f / 32f;

    //Ϊÿ���洴������
    void GenMesh()
    {
        if (managerhub == null)
        {
            managerhub = SceneData.GetManagerhub();
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

        // ���� Faces ��˳��Ϊ front-back-left-right-up-down
        Texture2D atlasTexture = managerhub.world.atlasTexture;
        Rect frontRect = managerhub.world.blocktypes[myItem._blocktype].front_sprite.rect; // ��ȡǰ������ľ�������
        Rect upRect = managerhub.world.blocktypes[myItem._blocktype].top_sprit.rect; // ��ȡ��������ľ�������
        Rect bottomRect = managerhub.world.blocktypes[myItem._blocktype].buttom_sprit.rect; // ��ȡ��������ľ�������
        Rect surroundRect = managerhub.world.blocktypes[myItem._blocktype].sprite.rect; // ��ȡ��Χ�ľ�������

        // ��������ʵ���������ʸ��ŵ� Faces �¶�Ӧ�� MeshRenderer ��
        for (int i = 0; i < 6; i++)
        {
            var meshRenderer = Faces[i].GetComponent<MeshRenderer>();

            // Ϊÿ���洴��һ������ʵ��
            Material faceMaterial = new Material(meshRenderer.material);

            // ���ò�ͬ������
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

            // ���´����Ĳ���ʵ���������ǰ��
            meshRenderer.material = faceMaterial;
            Faces[i].SetActive(true);
        }
    }

    //�ҵ��Լ����е���
    void FindFaces()
    {

        // ����Ƿ��ҵ��� "Body" ����
        if (FloatingCube == null)
        {
            Debug.LogWarning("δ�ҵ� 'Body' �Ӷ���");
            return;
        }

        // ��ȡ��Body���µ������Ӷ��󣬲�ȷ������Ϊ6
        if (FloatingCube.transform.childCount != 6)
        {
            Debug.LogWarning("�Ӷ���������Ϊ6���޷���ȷ���䵽Faces����");
            return;
        }

        // ����Body���Ӷ������Faces����
        for (int i = 0; i < 6; i++)
        {
            Faces[i] = FloatingCube.transform.GetChild(i).gameObject;
        }
    }

    #endregion


    #region Ư������

    //����
    [Foldout("Ư������", true)]
    [Header("�Ƿ���Ҫ����Ư��")][ReadOnly] public bool StopFloating;
    [Header("��������")] public float destroyTime = 100f;
    [Header("��ת�ٶ�")] public float rotationSpeed = 30f; 
    [Header("Ư���߶�")] public float floatingHeight = 0.3f; 
    [Header("Ư���ٶ�")] public float floatingSpeed = 1f;
    private GameObject FloatingCube;
    private float originalY;
    private float floatingOffset; // Ư��ƫ����

    bool hasExec_isGround = true;
    void ReferUpdateFloating()
    {
        if (FloatingCube == null)
        {
            FloatingCube = Component_Physics.Body;
        }

        // ˳ʱ����ת
        FloatingCube.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // ��زſ�ʼ����Ư��
        if (Component_Physics.isGround && !StopFloating)
        {
            if (hasExec_isGround)
            {
                //��֤�����
                floatingHeight = Random.Range(0.2f, 0.4f);
                floatingSpeed = Random.Range(0.8f, 1.2f);

                originalY = transform.position.y;
                //print(originalY);
                hasExec_isGround = false;
                //print("��ʼƯ��");
            }

            // ��������Ư����ƫ����
            floatingOffset = Mathf.Sin(Time.time * floatingSpeed) * (floatingHeight / 2f); // ʹ��һ��ĸ߶������Ļ�Ư��

            // ���������Y���꣬ȷ����ԭʼ�߶ȵķ�Χ��
            FloatingCube.transform.position = new Vector3(
                transform.position.x,
                originalY + floatingOffset + (floatingHeight / 2f), // �������ĵ�ĸ߶�
                transform.position.z
            );
        }
        else
        {
            hasExec_isGround = true;
        }
    }

    #endregion


    #region ���ղ���

    Coroutine MoveToPlayerCoroutine;
    [Foldout("���ղ���", true)]
    [Header("����ʱ��")] public float moveDuration = 0.2f;
    [Header("��С���շ�Χ")] public float absorbDistance = 2.3f;
    [Header("�մ�����������ȴʱ��")] public float ColdTimeToAbsorb = 1f;
    Vector3 Eyes;

    //���ռ��
    IEnumerator AbsorbCheck()
    {
        yield return new WaitForSeconds(ColdTimeToAbsorb);
        Eyes = Component_Physics.EyesPoint;

        //�Ƿ�ɱ�����
        while (true)
        {

            if (managerhub.world.game_state == Game_State.Playing)
            {
                Vector3 PlayerEyes = managerhub.player.eyesObject.transform.position;

                if (((FloatingCube.transform.position - PlayerEyes).magnitude < absorbDistance) && managerhub.backpackManager.CheckSlotsFull(myItem._blocktype) == false)
                {
                    Absorbable();
                }
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

            // ÿһ֡������ҵ�λ��
            Vector3 targetPosition = new Vector3(PlayerEyes.x, PlayerEyes.y - 0.3f, PlayerEyes.z);

            // ���ƶ�����ʱ�������ƶ���Ŀ��λ��
            FloatingCube.transform.position = Vector3.Lerp(startingPosition, targetPosition, (elapsedTime / moveDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        //����ƶ�
        OnEndEntity();
    }



    #endregion


    #region ���񲿷�

    // �� Update ��
    void ReferUpdateBeBuried()
    {
        bool Front = Component_Physics.collider_Front;
        bool Back = Component_Physics.collider_Back;
        bool Left = Component_Physics.collider_Left;
        bool Right = Component_Physics.collider_Right;
        bool Up = Component_Physics.collider_Up;
        bool Down = Component_Physics.collider_Down;

        // �����������û
        if (Front && Back && Left && Right && Up && Down)
        {
            HandleBuried();
        }
    }

    // ������û���
    [Foldout("����û���" ,true)]
    [Header("�Ƿ���")] private bool isBuried = false; // ���ڱ����ظ�����
    [Header("���ʱ��")] public float HandleBuriedColdTime = 1f;  // ��ȴʱ��
    [Header("����������")] public float HandleBuriedForceValue = 60f;  // ���ȴ�С 
    [Header("���������ݹر���ײ��ʱ��")] public float CloseCollisionTime_Surrond = 0.2f;  // ���ȴ�С 

    [Header("����������")] public float HandleBuriedForceValue_Up = 77f;  // ���ȴ�С 
    [Header("�������ݹر���ײ��ʱ��")] public float CloseCollisionTime_Up = 0.5f;  // ���ȴ�С 
    
    void HandleBuried()
    {
        // ��ֹ�ظ�����
        if (isBuried)
            return;

        Vector3 _Force = Vector3.zero;

        isBuried = true; // ���Ϊ����û
                         // ��ʱ�ر���ײ



        // �����ĸ�������б�
        List<(BlockDirection direction, Vector3 force)> directions = new List<(BlockDirection, Vector3)>
        {
            (BlockDirection.ǰ, Vector3.forward),
            (BlockDirection.��, Vector3.back),
            (BlockDirection.��, Vector3.left),
            (BlockDirection.��, Vector3.right)
        };

        // ������ҷ����б�
        System.Random random = new System.Random();
        directions = directions.OrderBy(x => random.Next()).ToList();

        // �������Һ�ķ���
        bool foundAir = false;
        foreach (var (direction, force) in directions)
        {
            if (managerhub.world.GetBlockType(Component_Physics.GetPoint_Direct_1m(direction)) == VoxelData.Air)
            {
                Component_Physics.CloseCollisionForAWhile(CloseCollisionTime_Surrond);
                _Force = force;
                _Force.y = 0.5f;
                Component_Velocity.AddForce(_Force, HandleBuriedForceValue);
                foundAir = true;
                break;
            }
        }

        // ���û�ҵ������飬ִ��������Ծ
        if (!foundAir)
        {
            Component_Physics.CloseCollisionForAWhile(CloseCollisionTime_Up);
            _Force = Vector3.up;
            Component_Velocity.AddForce(_Force, HandleBuriedForceValue_Up);
        }



        // ������ȴʱ�䣬�ָ�״̬
        StartCoroutine(ResetBuriedState());
    }

    // �ָ���û״̬��Э��
    private IEnumerator ResetBuriedState()
    {
        yield return new WaitForSeconds(HandleBuriedColdTime);
        isBuried = false; // �����ٴδ���
    }


    #endregion
}
