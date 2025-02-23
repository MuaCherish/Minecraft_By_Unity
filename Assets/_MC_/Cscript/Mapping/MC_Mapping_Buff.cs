using System;
using System.Collections.Generic;

public static class BuffData
{
    public static readonly int Blink = 0;
    public static readonly int SwellandExplore = 1;

    // Buff���� -> Buff�� ����ӳ��
    private static readonly Dictionary<int, Type> BuffTypeMap = new Dictionary<int, Type>
    {
        { Blink, typeof(MC_Buff_Blink) },
        { SwellandExplore, typeof(MC_Buff_SwellandExplore) }
    };

    /// <summary>
    /// ͨ�� Buff ID ��ȡ Buff ��
    /// </summary>
    public static Type GetBuffType(int buffType)
    {
        return BuffTypeMap.TryGetValue(buffType, out Type buffClass) ? buffClass : null;
    }
}
