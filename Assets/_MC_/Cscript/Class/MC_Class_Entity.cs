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



//ʵ����
[Serializable]
public class EntityInfo
{
    public int _id = -1;
    public string _name;
    public GameObject _obj = null;

    public EntityInfo(int _id, string _name, GameObject _obj)
    {
        this._id = _id;
        this._name = _name;
        this._obj = _obj;
    }
}




/// <summary>
/// ����
/// </summary>
public enum BlockDirection
{
    ǰ,
    ��,
    ��,
    ��,
    ��,
    ��
}
