using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public static readonly int Slime_Small = 0;
    public static readonly int Slime_Medium = 1;
    public static readonly int Slime_Big = 2;
    public static readonly int TNT = 3;
    public static readonly int Pig = 4;

    //实体寻路的邻接节点
    public static readonly Vector3[] NearNodes = new Vector3[24]
    {
        //上一层
        new Vector3(0.0f, 1f, 1.0f),  //North
        new Vector3(0.0f, 1f, -1.0f), //South
        new Vector3(-1.0f, 1f, 0.0f), //West
        new Vector3(1.0f, 1f, 0.0f),  //East
        new Vector3(1.0f, 1f, 1.0f),  //NorthEast
        new Vector3(1.0f, 1f, -1.0f), //SouthEast
        new Vector3(-1.0f, 1f, -1.0f),  //SouthWest
        new Vector3(-1.0f, 1f, 1.0f),  //NorthWest

        //中间层
        new Vector3(0.0f, 0.0f, 1.0f),  //North
        new Vector3(0.0f, 0.0f, -1.0f), //South
        new Vector3(-1.0f, 0.0f, 0.0f), //West
        new Vector3(1.0f, 0.0f, 0.0f),  //East
        new Vector3(1.0f, 0.0f, 1.0f),  //NorthEast
        new Vector3(1.0f, 0.0f, -1.0f), //SouthEast
        new Vector3(-1.0f, 0.0f, -1.0f),  //SouthWest
        new Vector3(-1.0f, 0.0f, 1.0f),  //NorthWest

        //下一层
        new Vector3(0.0f, -1f, 1.0f),  //North
        new Vector3(0.0f, -1f, -1.0f), //South
        new Vector3(-1.0f, -1f, 0.0f), //West
        new Vector3(1.0f, -1f, 0.0f),  //East
        new Vector3(1.0f, -1f, 1.0f),  //NorthEast
        new Vector3(1.0f, -1f, -1.0f), //SouthEast
        new Vector3(-1.0f, -1f, -1.0f),  //SouthWest
        new Vector3(-1.0f, -1f, 1.0f),  //NorthWest
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


#region Astar算法所需数据结构


[System.Serializable]
public class AstarNode
{
    public Vector3 P;
    public float G; // 路径消耗
    public float H; // 预期消耗
    public float F; // 总消耗
    public AstarNode parentNode;

    public AstarNode(Vector3 _currentPos, float _G, float _H, AstarNode _parentNode)
    {
        P = _currentPos;
        G = _G;
        H = _H;
        F = G + H;
        parentNode = _parentNode;
    }


}


#endregion