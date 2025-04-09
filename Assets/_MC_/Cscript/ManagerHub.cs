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
    [Foldout("MC_Service", true)]
    public MC_Service_Entity Service_Entity;
    public MC_Service_World Service_World;
    public MC_Service_Saving Service_Saving;
    public MC_Service_Time Service_Time;
    public MC_Service_Weather Service_Weather;
    public MC_Service_Music Service_Music;

    [Foldout("δ�Ż�", true)]
    public BackPackManager backpackManager;
    public CanvasManager canvasManager;
    public CommandManager commandManager;
    public DebugManager debugManager;
    public LifeManager lifeManager;
    public Player player;

    [Foldout("��������", true)]
    public OldMusicManager OldMusicManager;

    [Foldout("Others", true)]
    public TextureTo3D textureTo3D;
    public CloudManager cloudManager;
    public Crepuscular crepuscularScript;
    public SunMoving sunMoving;
    public ChatManager chatManager;

    [Foldout("������������(��Ϸ��;�޸���Ч)", true)]
    public bool ������ģʽ; 
    public bool �޺�ҹģʽ;
    public bool ������Ȼ���� = true;
    public bool �Ƿ�����Chunk���� = false;




}
