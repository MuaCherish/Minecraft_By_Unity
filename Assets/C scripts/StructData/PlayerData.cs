using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class PlayerData
{
    

    //���β���
    /*
     * ƽԭ��soil[10,30],sealevel[17],tree[1]
     * ���꣺soil[20,50],sealevel[30],tree[5]
    */

    //�������
    /*
     * 253��δ�ҵ�Chunk
     * 254������
     * 255������δ����
    */
    public static readonly Byte notChunk = 25;
    public static readonly Byte Solid = 254;
    public static readonly Byte notHit = 255;

    //��·����
    /*
     * walkSpeed����·�����ӳ�
     * sprintSpeed����̲����ӳ�
    */
    public static readonly float walkSpeed = 0.5f;
    public static readonly float sprintSpeed = 0.3f;
}