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

    private bool movingForward = true; // ���������ƶ��ķ���

    private float moveTimer = 0f;
    private float moveDuration = 0.1f; // �ƶ��ĳ���ʱ�䣬��λ��

    public Transform Center; // Ŀ��λ�õ� Transform

    private void Start()
    {
        playerController = FirstCamera.GetComponent<PlayerController>();
    }

    private void Update()
    {
        //����Ƿ��ƶ�
        if (playerController.HandShake)
        {
            angle += delta;
            lastangle = Mathf.Lerp(-35f, -5f, Mathf.Abs(Mathf.Sin(angle)));
            transform.localRotation = Quaternion.Euler(lastangle, 0f, 0f);
        }

        //��ⰴť
        if (playerController.isPlacing)
        {
            if (start_distance == 0f)
            {
                start_distance = 0.01f;
            }

            // ����������ƶ�
            if (movingForward)
            {
                start_distance += delta;
            }
            else
            {
                start_distance -= delta;
            }

            // �ж��Ƿ�ﵽ�����룬�ı��ƶ�����
            if (start_distance >= max_distance)
            {
                movingForward = false;
            }
            else if (start_distance <= 0f)
            {
                movingForward = true;
            }

            // ���㵱ǰ�ƶ��ľ���
            float moveDelta = moveDistance * (movingForward ? 1f : -1f);
            transform.Translate(Vector3.forward * moveDelta, Space.Self);

            // ��ʱ
            moveTimer += Time.deltaTime;

            // �ж��Ƿ�ﵽָ��ʱ�䣬�Զ�ֹͣ�ƶ�
            if (moveTimer >= moveDuration)
            {
                playerController.isPlacing = false;
                start_distance = 0f;
                movingForward = true;
                moveTimer = 0f; // ���ü�ʱ��

                // ��λ���ƶ�������ڵ�ǰλ�õ�����
                transform.position = Center.position;
            }
        }
        else
        { 
            // ���ò���
            start_distance = 0f;
            movingForward = true;
            moveTimer = 0f;
        }

        //print($"����Ϊ��{transform.position}��������Ϊ��{start_distance},�Ƿ��Ҽ���{playerController.isPlacing}");
    }
}
