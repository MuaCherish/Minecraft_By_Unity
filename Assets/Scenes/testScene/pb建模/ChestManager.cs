using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestManager : MonoBehaviour
{
    public bool isOpen;
    private Animator headAnimator;

    private void Awake()
    {
        // 获取Animator组件
        headAnimator = transform.Find("Head").gameObject.GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!isOpen)
            {
                OpenChest();
            }
            else
            {
                CloseChest();
            }
        }
    }

    // 初始化
    public void InitChest()
    {
        // 不用写
    }

    // 打开箱子
    public void OpenChest()
    {
        if (!isOpen)
        {
            isOpen = true;
            PlayAnimationForward();
        }
    }

    // 关闭箱子
    public void CloseChest()
    {
        if (isOpen)
        {
            isOpen = false;
            PlayAnimationBackward();
        }
    }

    // 正向播放动画
    private void PlayAnimationForward()
    {
        headAnimator.SetFloat("Speed", 1f); // 将速度设为正
        headAnimator.Play("OpenChest", 0, 0f); // 从头开始播放，确保名称正确
    }

    // 倒着播放动画
    private void PlayAnimationBackward()
    {
        headAnimator.SetFloat("Speed", -1f); // 将速度设为负
        headAnimator.Play("OpenChest", 0, 1f); // 从末尾开始播放，确保名称正确
    }
}
