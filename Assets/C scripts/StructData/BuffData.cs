using System;
using System.Collections.Generic;

public static class BuffData
{
    public static readonly int Blink = 0;

    // Buff类型 -> Buff类 类型映射
    private static readonly Dictionary<int, Type> BuffTypeMap = new Dictionary<int, Type>
    {
        { Blink, typeof(MC_Buff_Blink) }
    };

    /// <summary>
    /// 通过 Buff ID 获取 Buff 类
    /// </summary>
    public static Type GetBuffType(int buffType)
    {
        return BuffTypeMap.TryGetValue(buffType, out Type buffClass) ? buffClass : null;
    }
}
