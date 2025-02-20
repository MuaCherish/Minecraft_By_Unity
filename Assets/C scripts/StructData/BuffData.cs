using System;
using System.Collections.Generic;

public static class BuffData
{
    public static readonly int Blink = 0;

    // Buff���� -> Buff�� ����ӳ��
    private static readonly Dictionary<int, Type> BuffTypeMap = new Dictionary<int, Type>
    {
        { Blink, typeof(MC_Buff_Blink) }
    };

    /// <summary>
    /// ͨ�� Buff ID ��ȡ Buff ��
    /// </summary>
    public static Type GetBuffType(int buffType)
    {
        return BuffTypeMap.TryGetValue(buffType, out Type buffClass) ? buffClass : null;
    }
}
