using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLightMoving : MonoBehaviour
{
    public Transform player;
    public Transform eyes;
    public float rotationSpeed = 5f; // 旋转速度

    private Quaternion targetRotation;

    private void OnEnable()
    {
        //Debug.Log(eyes.localRotation.eulerAngles);
        transform.localRotation = Quaternion.Euler(eyes.localRotation.eulerAngles + player.localRotation.eulerAngles);
    }

    private void FixedUpdate()
    {
        // 获取目标旋转
        targetRotation = Quaternion.LookRotation(eyes.forward, eyes.up);

        // 缓慢旋转自身到目标旋转
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        //保持坐标一致
        transform.position = eyes.position;
    }


}
