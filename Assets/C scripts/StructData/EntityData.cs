using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EntityBase : MonoBehaviour, IEntityBrain
{
    public virtual void OnStartEntity() { }

    public virtual void OnEndEntity() { }

}


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
    public static readonly int Slime_Big = 0;
    public static readonly int TNT = 1;
    public static readonly int Pig = 2;
    public static readonly int Slime_Medium = 3;
    public static readonly int TestPig = 4;
    public static readonly int Slime_Small = 5;


    //8个指定方向
    public static readonly Vector3[] RandomWalkFace = new Vector3[8]
    {
        new Vector3(0.0f, 0.0f, 1.0f),  //North
        new Vector3(0.0f, 0.0f, -1.0f), //South
        new Vector3(-1.0f, 0.0f, 0.0f), //West
        new Vector3(1.0f, 0.0f, 0.0f),  //East

        new Vector3(1.0f, 0.0f, 1.0f),  //NorthEast
        new Vector3(1.0f, 0.0f, -1.0f), //SouthEast
        new Vector3(-1.0f, 0.0f, -1.0f),  //SouthWest
        new Vector3(-1.0f, 0.0f, 1.0f),  //NorthWest
    };

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
