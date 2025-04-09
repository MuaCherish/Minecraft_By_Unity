using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLightMoving : MonoBehaviour
{
    public Transform player;
    public Transform eyes;
    public float rotationSpeed = 5f; // ��ת�ٶ�

    private Quaternion targetRotation;

    private void OnEnable()
    {
        //Debug.Log(eyes.localRotation.eulerAngles);
        transform.localRotation = Quaternion.Euler(eyes.localRotation.eulerAngles + player.localRotation.eulerAngles);
    }

    private void FixedUpdate()
    {
        // ��ȡĿ����ת
        targetRotation = Quaternion.LookRotation(eyes.forward, eyes.up);

        // ������ת����Ŀ����ת
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        //��������һ��
        transform.position = eyes.position;
    }


}
