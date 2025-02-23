using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//ͷ��ȡ��β���������
public class FixedList<T>
{
    private List<T> _list;
    public int Capacity { get; private set; }

    public FixedList(int capacity)
    {
        Capacity = capacity;
        _list = new List<T>(capacity);
    }

    // ���Ԫ�ص�ͷ��
    public void Add(T item)
    {
        // ����б��������Ƴ���ɵ�Ԫ��
        if (_list.Count >= Capacity)
        {
            _list.RemoveAt(_list.Count - 1);
        }

        // ����Ԫ����ӵ�ͷ��
        _list.Insert(0, item);
    }

    // �����б��е�Ԫ��
    public IEnumerable<T> Items => _list.AsEnumerable();

    // ��ȡ�б�ĵ�ǰԪ������
    public int Count => _list.Count;

    // ����б�
    public void Clear()
    {
        _list.Clear();
    }

    // ��������β��Ԫ��
    public T GetTail()
    {
        if (_list.Count == 0)
        {
            Debug.Log("empty");
            return default(T); // ����Ĭ��ֵ
        }
        return _list[_list.Count - 1];
    }

    // �Ƴ�����β��Ԫ��
    public void RemoveTail()
    {
        // ����б�Ϊ�գ��׳��쳣����Ϊ�����
        if (_list.Count == 0)
        {
            throw new InvalidOperationException("List is empty.");
        }

        // �Ƴ��б�β����Ԫ��
        _list.RemoveAt(_list.Count - 1);
    }

    // ����������
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
