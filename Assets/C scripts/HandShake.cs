using UnityEngine;

public class HandShake : MonoBehaviour
{
    public GameObject FirstCamera;
    PlayerController playerController;
    private float angle = 0f;
    private float lastangle = 0f;
    private float delta = 0.01f;

    private float start_distance = 0f;
    private float max_distance = 0.1f;
    private float moveDistance = 0.1f;

    private bool movingForward = true; // 控制物体移动的方向

    private float moveTimer = 0f;
    private float moveDuration = 0.1f; // 移动的持续时间，单位秒

    public Transform Center; // 目标位置的 Transform

    private void Start()
    {
        playerController = FirstCamera.GetComponent<PlayerController>();
    }

    private void Update()
    {
        //检测是否移动
        if (playerController.HandShake)
        {
            angle += delta;
            lastangle = Mathf.Lerp(-35f, -5f, Mathf.Abs(Mathf.Sin(angle)));
            transform.localRotation = Quaternion.Euler(lastangle, 0f, 0f);
        }

        //检测按钮
        if (playerController.isPlacing)
        {
            if (start_distance == 0f)
            {
                start_distance = 0.01f;
            }

            // 控制物体的移动
            if (movingForward)
            {
                start_distance += delta;
            }
            else
            {
                start_distance -= delta;
            }

            // 判断是否达到最大距离，改变移动方向
            if (start_distance >= max_distance)
            {
                movingForward = false;
            }
            else if (start_distance <= 0f)
            {
                movingForward = true;
            }

            // 计算当前移动的距离
            float moveDelta = moveDistance * (movingForward ? 1f : -1f);
            transform.Translate(Vector3.forward * moveDelta, Space.Self);

            // 计时
            moveTimer += Time.deltaTime;

            // 判断是否达到指定时间，自动停止移动
            if (moveTimer >= moveDuration)
            {
                playerController.isPlacing = false;
                start_distance = 0f;
                movingForward = true;
                moveTimer = 0f; // 重置计时器

                // 将位置移动到相对于当前位置的坐标
                transform.position = Center.position;
            }
        }
        else
        { 
            // 重置参数
            start_distance = 0f;
            movingForward = true;
            moveTimer = 0f;
        }

        //print($"坐标为：{transform.position}，进度条为：{start_distance},是否右键：{playerController.isPlacing}");
    }
}
