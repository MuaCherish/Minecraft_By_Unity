using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EntityData
{
    //生物类型
    public static readonly int TestSlim = 0;
}


//实体类
[Serializable]
public class EntityStruct
{
    public int _id = -1;
    public GameObject _obj = null;

    public EntityStruct(int _id, GameObject _obj)
    {
        this._id = _id;
        this._obj = _obj;
    }
}
