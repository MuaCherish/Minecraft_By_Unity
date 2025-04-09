using Cloud;
using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��Ŀ�ܴ�����������ʽ: b*[^:b#/]+.*$
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
    public Player player;

    [Foldout("Others", true)]
    public Weather weather;
    public TextureTo3D textureTo3D;
    public CloudManager cloudManager;
    public Crepuscular crepuscularScript;
    public SunMoving sunMoving;
    public ChatManager chatManager;


    [Foldout("Services", true)]
    public MC_Service_Entity Service_Entity;
    public MC_Service_World Service_World;
    public MC_Service_Saving Service_Saving;

    [Foldout("������������(��Ϸ��;�޸���Ч)", true)]
    public bool ������ģʽ; 
    public bool �޺�ҹģʽ;
    public bool ������Ȼ���� = true;
    public bool �Ƿ�����Chunk���� = false;




}
