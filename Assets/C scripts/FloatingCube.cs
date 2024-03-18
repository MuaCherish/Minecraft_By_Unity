using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FloatingCube : MonoBehaviour
{
    public World world;
    public GameObject Eyes;
    public BackPackManager backpackmanager;
    public MusicManager musicmanager;

    public float destroyTime;
    public float rotationSpeed = 30f; // 旋转速度
    public float gravity; // 重力大小
    public float absorbDistance;
    public Coroutine MoveToPlayerCoroutine;
    public float moveDuration;
    public byte point_Block_type;

    //材质
    public bool isGround; // 是否在地面上

    public void InitWorld(World _world, float _destroytime, float _absorbDistance, float _gravity, float _moveDuration, byte _point_Block_type, BackPackManager _backpackmanager, MusicManager _musicmanager)
    {
        world = _world;
        destroyTime = _destroytime;
        absorbDistance = _absorbDistance;
        gravity = _gravity;
        moveDuration = _moveDuration;
        point_Block_type = _point_Block_type;
        backpackmanager = _backpackmanager;
        musicmanager = _musicmanager;
    }

    private void FixedUpdate()
    {
        //isGround
        CheckIsGround();

        // 顺时针旋转
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 应用重力
        AchieveGravity();

        //是否可被吸收
        if ((transform.position - Eyes.transform.position).magnitude < absorbDistance)
        {
            Absorbable();
        }

    }

    private void Start()
    {
        StartCoroutine(WaitToDestroy());
        Eyes = GameObject.Find("Player/Eyes");
    }

    IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(destroyTime);

        Destroy(gameObject);
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
            // 计算重力下落量
            float gravityDelta = gravity * Time.deltaTime;

            // 执行重力下坠
            transform.Translate(Vector3.down * gravityDelta, Space.World);
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
            Vector3 targetPosition = new Vector3(Eyes.transform.position.x, Eyes.transform.position.y - 0.3f, Eyes.transform.position.z);

            // 在移动持续时间内逐渐移动到目标位置
            transform.position = Vector3.Lerp(startingPosition, targetPosition, (elapsedTime / moveDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        //完成移动


        //播放音效
        musicmanager.PlaySound_Absorb();

        //背包系统计数
        if (world.game_mode == GameMode.Survival && point_Block_type != VoxelData.BedRock)
        {
            byte _point_Block_type = point_Block_type;

            //草块变泥土
            if (_point_Block_type == VoxelData.Grass)
            {
                _point_Block_type = VoxelData.Soil;
            }

            backpackmanager.update_slots(0, _point_Block_type);
        }


        // 移动完成后销毁自身
        Destroy(gameObject);
    }


}
