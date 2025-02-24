using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MC_Static_Name
{
    /// <summary>
    /// 给定int，返回世界类型的中文
    /// </summary>
    /// <param name="WorldType"></param>
    /// <returns></returns>
    public static String GetWorldTypeString(int WorldType)
    {
        switch (WorldType)
        {
            case 0:
                return "草原群系";
            case 1:
                return "高原群系";
            case 2:
                return "沙漠群系";
            case 3:
                return "沼泽群系";
            case 4:
                return "密林群系";
            case 5:
                return "默认群系";
            case 6:
                return "超平坦世界";
            default:
                return "给定世界类型有误GetWorldTypeChinese";
        }
    }

    /// <summary>
    /// 给定游戏模式的中文
    /// </summary>
    /// <param name="gamemode"></param>
    /// <returns></returns>
    public static String GetGameModeString(GameMode gamemode)
    {
        if (gamemode == GameMode.Survival)
        {
            return "生存模式";
        }
        else
        {
            return "创造模式";
        }
    }



}
