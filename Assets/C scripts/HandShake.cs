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
            // �������ʱ����һ�ζ���
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

    //��һ������
    void PlayFirstAnimation()
    {
        // ��� Animation �����Ϊ�ղ��ҵ�һ����������
        if (animationComponent != null && animationComponent.GetClip("HandWave") != null)
        {
            // ���ŵ�һ������
            animationComponent.Play("HandWave");
        }
        else
        {
            Debug.LogError("First Animation not found or Animation component is missing.");
        }
    }

    //�ڶ�������
    void PlaySecondAnimation()
    {
        // ��� Animation �����Ϊ�ղ��ҵڶ�����������
        if (animationComponent != null && animationComponent.GetClip("HandMoving") != null)
        {
            // ���ŵڶ�������
            animationComponent.Play("HandMoving");
        }
        else
        {
            Debug.LogError("Second Animation not found or Animation component is missing.");
        }
    }

}
