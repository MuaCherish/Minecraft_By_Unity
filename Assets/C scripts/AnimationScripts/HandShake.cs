using UnityEngine;
using System.Collections;

public class HandShake : MonoBehaviour
{
    public BackPackManager BackPackManager;
    private Animation animationComponent;
    public World world;
    public Player player;
    public CanvasManager canvasmanager;

    // �����ı���
    private float timer = 0f;
    [Header("������ʼ���ŵ���ȴ")] public float MovingColdTime = 0.2f;
    [Header("�ص���ʼλ�õĳ���ʱ��")] public float InitHandDuration = 0.1f;
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
        // �����ʼλ�ú���ת
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

            // �������ʱ����һ�ζ���
            if (Input.GetKey(KeyCode.Mouse0) && !managerhub.chatManager.isInputing)
            {
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
                //ֹͣ�ƶ�ʱ�ֱ۹�λ
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

    // ��һ������
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

    // �ڶ�������
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

    // ֹͣ�ڶ�������
    void StopSecondAnimation()
    {
        if (animationComponent != null && animationComponent.IsPlaying("HandMoving"))
        {
            animationComponent.Stop("HandMoving");
        }
    }

    // Э�̣����ص���ʼλ��
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

        // ȷ������λ�ú���ת��׼ȷ��
        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;

        isReturningToIdle = false;
    }
}
