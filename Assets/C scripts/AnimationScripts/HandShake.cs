using UnityEngine;
using System.Collections;

public class HandShake : MonoBehaviour
{
    public BackPackManager BackPackManager;
    private Animation animationComponent;
    public World world;
    public Player player;
    public CanvasManager canvasmanager;

    // 新增的变量
    private float timer = 0f;
    [Header("动画开始播放的冷却")] public float MovingColdTime = 0.2f;
    [Header("回到初始位置的持续时间")] public float InitHandDuration = 0.1f;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isReturningToIdle = false;

    ManagerHub managerhub;
    private void Awake()
    {
        managerhub = SceneData.GetManagerhub(); 
    }


    void Start()
    {
        animationComponent = GetComponent<Animation>();
        // 保存初始位置和旋转
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    bool hasExec_isMoving = true;
    void FixedUpdate()
    {
        if (world.game_state == Game_State.Playing)
        {
            if (canvasmanager.isPausing)
            {
                return;
            }

            if (player.isInputing)
            {
                hasExec_isMoving = true;
            }

            // 左键按下时播放一次动画
            if (Input.GetKey(KeyCode.Mouse0) && !managerhub.chatManager.isInputing)
            {
                PlayFirstAnimation();
            }
            //右键
            else if (Input.GetMouseButtonDown(1))
            {
                timer = 0f;
                PlayFirstAnimation();
            }
            //如果玩家移动 && 没有切换方块 就播放移动动画
            else
            {
                if (player.isFlying)
                {
                    return;
                }

                if (player.isInputing)
                {
                    if ((timer > MovingColdTime) && !BackPackManager.isChanging)
                    {
                        PlaySecondAnimation();
                    }
                }
                //停止移动时手臂归位
                else
                {
                    if (hasExec_isMoving && !isReturningToIdle)
                    {
                        StopSecondAnimation();
                        StartCoroutine(ReturnToIdle());
                        hasExec_isMoving = false;
                    }
                }

                timer += Time.deltaTime;
            }
        }
    }

    // 第一个动画
    void PlayFirstAnimation()
    {
        if (animationComponent != null && animationComponent.GetClip("HandWave") != null)
        {
            animationComponent.Play("HandWave");
        }
        else
        {
            Debug.LogError("First Animation not found or Animation component is missing.");
        }
    }

    // 第二个动画
    void PlaySecondAnimation()
    {
        if (animationComponent != null && animationComponent.GetClip("HandMoving") != null)
        {
            animationComponent.Play("HandMoving");
        }
        else
        {
            Debug.LogError("Second Animation not found or Animation component is missing.");
        }
    }

    // 停止第二个动画
    void StopSecondAnimation()
    {
        if (animationComponent != null && animationComponent.IsPlaying("HandMoving"))
        {
            animationComponent.Stop("HandMoving");
        }
    }

    // 协程：返回到初始位置
    private IEnumerator ReturnToIdle()
    {
        isReturningToIdle = true;
        float elapsedTime = 0f;
        float duration = InitHandDuration;

        Vector3 startPosition = transform.localPosition;
        Quaternion startRotation = transform.localRotation;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.localPosition = Vector3.Lerp(startPosition, initialPosition, t);
            transform.localRotation = Quaternion.Slerp(startRotation, initialRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保最终位置和旋转是准确的
        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;

        isReturningToIdle = false;
    }
}
