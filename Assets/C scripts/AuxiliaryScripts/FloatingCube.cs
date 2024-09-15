using System.Collections;
using Unity.VisualScripting;
//using UnityEditor.SceneManagement;
using UnityEngine;

public class FloatingCube : MonoBehaviour
{
    public World world;
    public GameObject Eyes;
    public BackPackManager backpackmanager;
    public MusicManager musicmanager;

    public float destroyTime;
    public float rotationSpeed = 30f; // ��ת�ٶ�
    public float gravity; // ������С
    public float absorbDistance;
    public Coroutine MoveToPlayerCoroutine;
    public float moveDuration;
    public byte point_Block_type;
    public float ColdTimeToAbsorb;

    //����
    public bool isGround; // �Ƿ��ڵ�����

    public void InitWorld(ManagerHub _managerhub,byte _point_Block_type, float _ColdTimeToAbsorb)
    {
        world = _managerhub.world;
        destroyTime = _managerhub.backpackManager.dropblock_destroyTime;
        absorbDistance = _managerhub.backpackManager.absorb_Distance;
        gravity = _managerhub.backpackManager.drop_gravity;
        moveDuration = _managerhub.backpackManager.moveToplayer_duation;

        point_Block_type = _point_Block_type;
        ColdTimeToAbsorb = _ColdTimeToAbsorb;

        backpackmanager = _managerhub.backpackManager;
        musicmanager = _managerhub.musicManager;
        
    }


    private void FixedUpdate()
    {
        //isGround
        CheckIsGround();

        // ˳ʱ����ת
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Ӧ������
        AchieveGravity();



       

    }

    private void Start()
    {
        Destroy(gameObject, destroyTime);
        StartCoroutine(absorb());
        Eyes = GameObject.Find("Player/Eyes");
    }



    void CheckIsGround()
    {
        if (world.GetBlockType(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z)) != VoxelData.Air)
        {
            isGround = true;
        }
        else
        {
            isGround = false;
        }
    }

    void AchieveGravity()
    {
        if (!isGround)
        {
            // ��������������
            float gravityDelta = gravity * Time.deltaTime;

            // ִ��������׹
            transform.Translate(Vector3.down * gravityDelta, Space.World);
        }
        else
        {
            if (GetComponent<Rigidbody>())
            {
                Destroy(GetComponent<Rigidbody>());
            }
        }
    }


    void Absorbable()
    {
        if (MoveToPlayerCoroutine == null)
        {
            MoveToPlayerCoroutine = StartCoroutine(MoveToPlayerSmoothly());
        }
        
    }

    IEnumerator MoveToPlayerSmoothly()
    {
        float elapsedTime = 0.0f;
        Vector3 startingPosition = transform.position;

        while (elapsedTime < moveDuration)
        {
            // ÿһ֡������ҵ�λ��
            Vector3 targetPosition = new Vector3(Eyes.transform.position.x, Eyes.transform.position.y - 0.3f, Eyes.transform.position.z);

            // ���ƶ�����ʱ�������ƶ���Ŀ��λ��
            transform.position = Vector3.Lerp(startingPosition, targetPosition, (elapsedTime / moveDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        //����ƶ�


        //������Ч
        musicmanager.PlaySound_Absorb();

        

        //����ϵͳ����
        if (world.game_mode == GameMode.Survival && point_Block_type != VoxelData.BedRock)
        {
            byte _point_Block_type = point_Block_type;

            //�ݿ������
            if (_point_Block_type == VoxelData.Grass)
            {
                _point_Block_type = VoxelData.Soil;
            }

            backpackmanager.update_slots(0, _point_Block_type);
        }

        //�л�������Ʒ����
        backpackmanager.ChangeBlockInHand();

        // �ƶ���ɺ���������
        Destroy(gameObject);
    }


    //���ռ��
    IEnumerator absorb()
    {
        yield return new WaitForSeconds(ColdTimeToAbsorb);

        //�Ƿ�ɱ�����
        while (true)
        {
            if (((transform.position - Eyes.transform.position).magnitude < absorbDistance) && backpackmanager.isfull == false)
            {
                Absorbable();
            }

            yield return new WaitForFixedUpdate();
        }
        
    }


}
