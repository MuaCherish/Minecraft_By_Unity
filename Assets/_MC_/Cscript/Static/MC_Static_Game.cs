using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MC_Static_Game
{

    #region ���

    /// <summary>
    /// ���Ϊ true�������ز��̶���ꣻ���Ϊ false������ʾ���������
    /// </summary>
    /// <param name="isLocked">���Ϊ true�������ز��̶���ꣻ���Ϊ false������ʾ���������</param>
    public static void LockMouse(bool isLocked)
    {
        if (isLocked)
        {
            //print("�������");
            // ��������겢������������Ļ����
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            //print("��ʾ���");
            // ��ʾ����겢�������
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }


    #endregion


}
