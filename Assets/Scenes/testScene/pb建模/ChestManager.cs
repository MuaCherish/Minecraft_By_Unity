using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestManager : MonoBehaviour
{
    public bool isOpen;
    private Animator headAnimator;

    private void Awake()
    {
        // ��ȡAnimator���
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

    // ��ʼ��
    public void InitChest()
    {
        // ����д
    }

    // ������
    public void OpenChest()
    {
        if (!isOpen)
        {
            isOpen = true;
            PlayAnimationForward();
        }
    }

    // �ر�����
    public void CloseChest()
    {
        if (isOpen)
        {
            isOpen = false;
            PlayAnimationBackward();
        }
    }

    // ���򲥷Ŷ���
    private void PlayAnimationForward()
    {
        headAnimator.SetFloat("Speed", 1f); // ���ٶ���Ϊ��
        headAnimator.Play("OpenChest", 0, 0f); // ��ͷ��ʼ���ţ�ȷ��������ȷ
    }

    // ���Ų��Ŷ���
    private void PlayAnimationBackward()
    {
        headAnimator.SetFloat("Speed", -1f); // ���ٶ���Ϊ��
        headAnimator.Play("OpenChest", 0, 1f); // ��ĩβ��ʼ���ţ�ȷ��������ȷ
    }
}
