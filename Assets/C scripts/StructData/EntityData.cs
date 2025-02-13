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
    void OnStartEntity();  // ʵ������ʱ����
    void OnEndEntity();    // ʵ������ʱ����
}

public static class EntityData
{
    //ʵ������
    public static readonly float MinYtoRemoveEntity = -20f;


    //��������
    public static readonly int Slime_Small = 0;
    public static readonly int Slime_Medium = 1;
    public static readonly int Slime_Big = 2;
    public static readonly int TNT = 3;
    public static readonly int Pig = 4;

    //ʵ��Ѱ·���ڽӽڵ�
    public static readonly Vector3[] NearNodes = new Vector3[24]
    {
        //��һ��
        new Vector3(0.0f, 1f, 1.0f),  //North
        new Vector3(0.0f, 1f, -1.0f), //South
        new Vector3(-1.0f, 1f, 0.0f), //West
        new Vector3(1.0f, 1f, 0.0f),  //East
        new Vector3(1.0f, 1f, 1.0f),  //NorthEast
        new Vector3(1.0f, 1f, -1.0f), //SouthEast
        new Vector3(-1.0f, 1f, -1.0f),  //SouthWest
        new Vector3(-1.0f, 1f, 1.0f),  //NorthWest

        //�м��
        new Vector3(0.0f, 0.0f, 1.0f),  //North
        new Vector3(0.0f, 0.0f, -1.0f), //South
        new Vector3(-1.0f, 0.0f, 0.0f), //West
        new Vector3(1.0f, 0.0f, 0.0f),  //East
        new Vector3(1.0f, 0.0f, 1.0f),  //NorthEast
        new Vector3(1.0f, 0.0f, -1.0f), //SouthEast
        new Vector3(-1.0f, 0.0f, -1.0f),  //SouthWest
        new Vector3(-1.0f, 0.0f, 1.0f),  //NorthWest

        //��һ��
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


//ʵ����
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


#region Astar�㷨�������ݽṹ


[System.Serializable]
public class AstarNode
{
    public Vector3 P;
    public float G; // ·������
    public float H; // Ԥ������
    public float F; // ������
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