using System;
using UnityEngine;


public static class EntityData
{
    //实体设置
    public static readonly float MinYtoRemoveEntity = -20f;


    //生物类型---添加生物的时候需要完成实体姓名列表
    public static readonly int Slime_Small = 0;
    public static readonly int Slime_Medium = 1;
    public static readonly int Slime_Big = 2;
    public static readonly int TNT = 3;
    public static readonly int Pig = 4;
    public static readonly int Sheep = 5;
    public static readonly int Zombie = 6;
    public static readonly int Alex = 7;
    public static readonly int Steve = 8;
    public static readonly int Creeper = 9;

    //实体姓名列表
    public static string GetEntityName(int entityId)
    {
        switch (entityId)
        {
            case 0:
                return "Slime_Small";
            case 1:
                return "Slime_Medium";
            case 2:
                return "Slime_Big";
            case 3:
                return "TNT";
            case 4:
                return "Pig";
            case 5:
                return "Sheep";
            case 6:
                return "Zombie";
            case 7:
                return "Alex";
            case 8:
                return "Steve";
            case 9:
                return "Creeper";
            default:
                return "Unknown Entity"; // 默认返回值，处理非法输入
        }
    }


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



//实体类
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
/// 方向
/// </summary>
public enum BlockDirection
{
    前,
    后,
    左,
    右,
    上,
    下
}
