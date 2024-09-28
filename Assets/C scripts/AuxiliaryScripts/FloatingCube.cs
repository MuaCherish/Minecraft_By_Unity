using System.Collections;
using Unity.VisualScripting;
//using UnityEditor.SceneManagement;
using UnityEngine;

public class FloatingCube : MonoBehaviour
{
    public ManagerHub managerhub;
    public GameObject eyes;

    //参数
    public float destroyTime;
    public float rotationSpeed = 30f; // 旋转速度
    public float gravity; // 重力大小
    public float absorbDistance;
    public Coroutine MoveToPlayerCoroutine;
    public float moveDuration;
    public byte point_Block_type;
    public float ColdTimeToAbsorb = 1f;

    //材质
    public bool isGround; // 是否在地面上

    public void InitWorld(ManagerHub _managerhub,byte _point_Block_type)
    {
        managerhub = _managerhub;
        destroyTime = _managerhub.backpackManager.dropblock_destroyTime;
        absorbDistance = _managerhub.backpackManager.absorb_Distance;
        gravity = _managerhub.backpackManager.drop_gravity;
        moveDuration = _managerhub.backpackManager.moveToplayer_duation;

        point_Block_type = _point_Block_type;

        managerhub.backpackManager = _managerhub.backpackManager;
        managerhub.musicManager = _managerhub.musicManager;
        
    }


    private void FixedUpdate()
    {
        //isGround
        CheckIsGround();

        // 顺时针旋转
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 应用重力
        AchieveGravity();



       

    }

    private void Start()
    {
        Destroy(gameObject, destroyTime);
        StartCoroutine(absorb());
        eyes = managerhub.player.eyesObject;
    }



    void CheckIsGround()
    {
        if (managerhub.world.GetBlockType(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z)) != VoxelData.Air)
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
            // 计算重力下落量
            float gravityDelta = gravity * Time.deltaTime;

            // 执行重力下坠
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
            // 每一帧更新玩家的位置
            Vector3 targetPosition = new Vector3(eyes.transform.position.x, eyes.transform.position.y - 0.3f, eyes.transform.position.z);

            // 在移动持续时间内逐渐移动到目标位置
            transform.position = Vector3.Lerp(startingPosition, targetPosition, (elapsedTime / moveDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        //完成移动


        //播放音效
        managerhub.musicManager.PlaySound_Absorb();

        

        //背包系统计数
        if (point_Block_type != VoxelData.BedRock)
        {
            byte _point_Block_type = point_Block_type;

            //草块变泥土
            if (_point_Block_type == VoxelData.Grass)
            {
                _point_Block_type = VoxelData.Soil;
            }

            managerhub.backpackManager.update_slots(0, _point_Block_type);
        }

        //切换手中物品动画
        managerhub.backpackManager.ChangeBlockInHand();

        // 移动完成后销毁自身
        Destroy(gameObject);
    }


    //吸收检测
    IEnumerator absorb()
    {
        yield return new WaitForSeconds(ColdTimeToAbsorb);

        //是否可被吸收
        while (true)
        {
            if (((transform.position - eyes.transform.position).magnitude < absorbDistance) && managerhub.backpackManager.isfull == false)
            {
                Absorbable();
            }

            yield return new WaitForFixedUpdate();
        }
        
    }


}
