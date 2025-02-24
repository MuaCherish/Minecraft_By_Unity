using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MC_Static_Game
{

    #region 光标

    /// <summary>
    /// 如果为 true，则隐藏并固定鼠标；如果为 false，则显示并解锁鼠标
    /// </summary>
    /// <param name="isLocked">如果为 true，则隐藏并固定鼠标；如果为 false，则显示并解锁鼠标</param>
    public static void LockMouse(bool isLocked)
    {
        if (isLocked)
        {
            //print("隐藏鼠标");
            // 隐藏鼠标光标并将其锁定在屏幕中心
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            //print("显示鼠标");
            // 显示鼠标光标并解除锁定
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }


    #endregion


}
