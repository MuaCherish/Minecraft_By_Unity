using Cloud;
using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 项目总代码量正则表达式: b*[^:b#/]+.*$
/// </summary>
public class ManagerHub : MonoBehaviour
{
    [Foldout("Managers", true)]
    public BackPackManager backpackManager;
    public CanvasManager canvasManager;
    public CommandManager commandManager;
    public DebugManager debugManager;
    public LifeManager lifeManager;
    //public MusicManager musicManager;
    public NewMusicManager NewmusicManager;
    public MusicManager OldMusicManager;
    public TimeManager timeManager;
    public World world;
    public Player player;

    [Foldout("Others", true)]
    public Weather weather;
    public TextureTo3D textureTo3D;
    public CloudManager cloudManager;
    public Crepuscular crepuscularScript;
    public SunMoving sunMoving;
    public ChatManager chatManager;

    [Foldout("开发者模式", true)]
    public bool 低区块模式; [HideInInspector] public bool hasExec_低区块模式 = true;
    public bool 无黑夜模式; [HideInInspector] public bool hasExec_无黑夜模式 = true;
    public bool 是否生成Chunk侧面 = false;




}
