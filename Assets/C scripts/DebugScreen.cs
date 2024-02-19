using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DebugScreen : MonoBehaviour
{
    //获取组件
    public TextMeshProUGUI textMeshPro;
    //public PlayerController playercontroller;
    public Player player;
    public World world;
    string Block_Direction;

    //计算FPS
    private int count;
    private float deltaTime;
    private float fps;


    void Update()
    {
        //GetBlockDirection();
        countFPS();
        ShowText();
    }

    //void GetBlockDirection()
    //{
    //    switch (playercontroller.Face_flag)
    //    {
    //        case -1: Block_Direction = "null"; break;
    //        case 0: Block_Direction = "ForWard"; break;
    //        case 1: Block_Direction = "Back"; break;
    //        case 2: Block_Direction = "Left"; break;
    //        case 3: Block_Direction = "Right"; break;
    //        case 4: Block_Direction = "Top"; break;
    //        //case 5:isGround不处理
    //        case 6: Block_Direction = "ForWard+Left"; break;
    //        case 7: Block_Direction = "ForWard+Right"; break;
    //        case 8: Block_Direction = "Back+Left"; break;
    //        case 9: Block_Direction = "Back+Right"; break;
    //    }
    //}

    void countFPS()
    {
        count++;
        deltaTime += Time.deltaTime;

        if (count % 60 == 0)
        {
            count = 1;
            fps = 60f / deltaTime;
            deltaTime = 0;
        }
    }


    void ShowText()
    {
        //textMeshPro.text += $"\n";
        textMeshPro.text = $"FPS:{Mathf.Ceil(fps):F2}\n";
        textMeshPro.text += $"PlayerChunkPosition:{world.GetChunkLocation(world.PlayerFoot.position)}\n";
        textMeshPro.text += $"PlayerRelaPosition:{world.GetRelalocation(world.PlayerFoot.position)}\n";
        //textMeshPro.text += $"PlayerInput:<{playercontroller.verticalInput},{playercontroller.horizontalInput}>\n";
        textMeshPro.text += $"PlayerInput:<{player.verticalInput},{player.horizontalInput}>\n";
        textMeshPro.text += $"BlockDirection:{Block_Direction}\n";
        //textMeshPro.text += $"Ray.Length: {playercontroller.ray_length:F2}\n";
        textMeshPro.text += $"Ground:{world.foot_BlockType}\n";
        textMeshPro.text += $"isGround:{world.isBlock}\n";
        textMeshPro.text += $"isSwiming:{world.isSwiming}\n";
    }

}
