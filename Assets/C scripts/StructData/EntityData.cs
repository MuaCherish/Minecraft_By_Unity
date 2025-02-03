using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static readonly int Slime = 0;
    public static readonly int TNT = 1;
    public static readonly int Pig = 2;
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
