using UnityEngine;

public class HandShake : MonoBehaviour
{
    private Animation animationComponent;
    public World world;
    public Player player;

    void Start()
    {
        animationComponent = GetComponent<Animation>();
    }

    void Update()
    {
        if (world.game_state == Game_State.Playing)
        {
            // 左键按下时播放一次动画
            if (Input.GetKey(KeyCode.Mouse0))
            {
                PlayFirstAnimation();
            }else if (Input.GetMouseButtonDown(1))
            {
                PlayFirstAnimation();
            }
            else
            {
                if (player.isMoving)
                {
                    PlaySecondAnimation();
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
