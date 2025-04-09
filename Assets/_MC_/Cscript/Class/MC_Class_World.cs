using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalTerrainAssets", menuName = "GameData/Terrain Assets")]
public class TerrainVisualAssets : ScriptableObject
{
    [Header("���β���")] public Material material_Terrain;
    [Header("ˮ�����")] public Material material_Water;
    [Header("BlockTexture(���ڵ�����)")] public Texture2D atlasTexture;
}


//ȫ����Ϸ״̬
public enum Game_State
{

    Start, Loading, Playing, Pause, Ending,

}

public enum GameMode
{

    Creative, Survival,

}

/// <summary>
/// ��Ϸʵʱ��������
/// </summary>
public class CurrentGameState
{
    [Header("��Ϸģʽ")] public GameMode gameMode = GameMode.Survival;
    [Header("��Ϸ״̬")] public Game_State gameState = Game_State.Start;
     
}


