using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MC_Static_Name
{
    /// <summary>
    /// ����int�������������͵�����
    /// </summary>
    /// <param name="WorldType"></param>
    /// <returns></returns>
    public static String GetWorldTypeString(int WorldType)
    {
        switch (WorldType)
        {
            case 0:
                return "��ԭȺϵ";
            case 1:
                return "��ԭȺϵ";
            case 2:
                return "ɳĮȺϵ";
            case 3:
                return "����Ⱥϵ";
            case 4:
                return "����Ⱥϵ";
            case 5:
                return "Ĭ��Ⱥϵ";
            case 6:
                return "��ƽ̹����";
            default:
                return "����������������GetWorldTypeChinese";
        }
    }

    /// <summary>
    /// ������Ϸģʽ������
    /// </summary>
    /// <param name="gamemode"></param>
    /// <returns></returns>
    public static String GetGameModeString(GameMode gamemode)
    {
        if (gamemode == GameMode.Survival)
        {
            return "����ģʽ";
        }
        else
        {
            return "����ģʽ";
        }
    }



}
