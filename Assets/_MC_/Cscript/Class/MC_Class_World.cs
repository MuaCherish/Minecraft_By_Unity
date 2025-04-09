using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalTerrainAssets", menuName = "GameData/Terrain Assets")]
public class TerrainVisualAssets : ScriptableObject
{
    [Header("地形材质")] public Material material_Terrain;
    [Header("水面材质")] public Material material_Water;
    [Header("BlockTexture(用于掉落物)")] public Texture2D atlasTexture;
}


//全局游戏状态
public enum Game_State
{

    Start, Loading, Playing, Pause, Ending,

}

public enum GameMode
{

    Creative, Survival,

}

/// <summary>
/// 游戏实时运行数据
/// </summary>
public class CurrentGameState
{
    [Header("游戏模式")] public GameMode gameMode = GameMode.Survival;
    [Header("游戏状态")] public Game_State gameState = Game_State.Start;
     
}


