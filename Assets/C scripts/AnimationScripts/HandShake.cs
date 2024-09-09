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
                // �������ʱ����һ�ζ���
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    //Debug.Log("HandShake Mouse0");
                    PlayFirstAnimation();
                }
                //�Ҽ�
                else if (Input.GetMouseButtonDown(1))
                {
                    timer = 0f;
                    PlayFirstAnimation();
                }
                //�������ƶ� && û���л����� �Ͳ����ƶ�����
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
