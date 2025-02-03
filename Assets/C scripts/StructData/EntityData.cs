using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntityBrain
{
    void OnStartEntity();  // 实体启动时调用
    void OnEndEntity();    // 实体销毁时调用
}

public static class EntityData
{
    //实体设置
    public static readonly float MinYtoRemoveEntity = -20f;


    //生物类型
    public static readonly int Slime = 0;
    public static readonly int TNT = 1;
    public static readonly int Pig = 2;
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
