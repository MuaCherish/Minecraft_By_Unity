using UnityEngine;

public class HandShake : MonoBehaviour
{
    public BackPackManager BackPackManager;
    private Animation animationComponent;
    public World world;
    public Player player;
    private float timer = 0f;
    public CanvasManager canvasmanager;

    void Start()
    {
        animationComponent = GetComponent<Animation>();
    }

    void FixedUpdate()
    {
        if (world.game_state == Game_State.Playing)
        {

            if (!canvasmanager.isPausing)
            {
                // 左键按下时播放一次动画
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    //Debug.Log("HandShake Mouse0");
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
                    if (player.isMoving && (timer > 0.2f) && !BackPackManager.isChanging)
                    {
                        PlaySecondAnimation();
                    }

                    timer += Time.deltaTime;
                }
            }
        }
    }

    //第一个动画
    void PlayFirstAnimation()
    {
        // 如果 Animation 组件不为空并且第一个动画存在
        if (animationComponent != null && animationComponent.GetClip("HandWave") != null)
        {
            // 播放第一个动画
            animationComponent.Play("HandWave");
        }
        else
        {
            Debug.LogError("First Animation not found or Animation component is missing.");
        }
    }

    //第二个动画
    void PlaySecondAnimation()
    {
        // 如果 Animation 组件不为空并且第二个动画存在
        if (animationComponent != null && animationComponent.GetClip("HandMoving") != null)
        {
            // 播放第二个动画
            animationComponent.Play("HandMoving");
        }
        else
        {
            Debug.LogError("Second Animation not found or Animation component is missing.");
        }
    }

}
