using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//头读取，尾插入的链表
public class FixedList<T>
{
    private List<T> _list;
    public int Capacity { get; private set; }

    public FixedList(int capacity)
    {
        Capacity = capacity;
        _list = new List<T>(capacity);
    }

    // 添加元素到头部
    public void Add(T item)
    {
        // 如果列表已满，移除最旧的元素
        if (_list.Count >= Capacity)
        {
            _list.RemoveAt(_list.Count - 1);
        }

        // 将新元素添加到头部
        _list.Insert(0, item);
    }

    // 访问列表中的元素
    public IEnumerable<T> Items => _list.AsEnumerable();

    // 获取列表的当前元素数量
    public int Count => _list.Count;

    // 清空列表
    public void Clear()
    {
        _list.Clear();
    }

    // 返回链表尾部元素
    public T GetTail()
    {
        if (_list.Count == 0)
        {
            Debug.Log("empty");
            return default(T); // 返回默认值
        }
        return _list[_list.Count - 1];
    }

    // 移除链表尾部元素
    public void RemoveTail()
    {
        // 如果列表为空，抛出异常或处理为空情况
        if (_list.Count == 0)
        {
            throw new InvalidOperationException("List is empty.");
        }

        // 移除列表尾部的元素
        _list.RemoveAt(_list.Count - 1);
    }

    // 索引访问器
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _list.Count)
            {
                throw new ArgumentOutOfRangeException("Index is out of range.");
            }
            return _list[index];
        }
    }
}
