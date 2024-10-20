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
    public MusicManager musicManager;
    public TimeManager timeManager;
    public World world;
    public Player player;
    public TextureTo3D textureTo3D;
    public CloudManager cloudManager;
    public Crepuscular crepuscularScript;
    public SunMoving sunMoving;

    [Foldout("������ģʽ", true)]
    public bool ������ģʽ; [HideInInspector] public bool hasExec_������ģʽ = true;
    public bool �޺�ҹģʽ; [HideInInspector] public bool hasExec_�޺�ҹģʽ = true;
    public bool �Ƿ�����Chunk���� = false;




}
