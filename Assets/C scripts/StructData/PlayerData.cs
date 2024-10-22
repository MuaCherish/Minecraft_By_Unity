using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class PlayerData
{
    

    //地形参数
    /*
     * 平原：soil[10,30],sealevel[17],tree[1]
     * 丘陵：soil[20,50],sealevel[30],tree[5]
    */

    //特殊参数
    /*
     * 253：未找到Chunk
     * 254：固体
     * 255：射线未打中
    */
    public static readonly Byte notChunk = 25;
    public static readonly Byte Solid = 254;
    public static readonly Byte notHit = 255;

    //走路参数
    /*
     * walkSpeed：走路播放延迟
     * sprintSpeed：冲刺播放延迟
    */
    public static readonly float walkSpeed = 0.5f;
    public static readonly float sprintSpeed = 0.3f;
}