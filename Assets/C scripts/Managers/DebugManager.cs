using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Animations.AimConstraint;

public class DebugManager : MonoBehaviour
{
    [Header("状态")]
    [ReadOnly] public bool isDebug = false;

    //获取组件
    [Header("引用")]
    public ManagerHub managerHub;
    public GameObject DebugScreen;
    public Transform Content;
    public GameObject blockitem;
    public TextMeshProUGUI LeftText;
    //public Player player;
    //public World world;

    

    private void Start()
    {
        UpdateBlockItem();
    }

    void Update()
    {

        if (managerHub.world.game_state == Game_State.Start)
        {
            if (DebugScreen.activeSelf)
            {
                DebugScreen.SetActive(false);
            }
            

        }


        if (managerHub.world.game_state == Game_State.Playing)
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                isDebug = !isDebug;
                DebugScreen.SetActive(!DebugScreen.activeSelf);
            }


        }

        

    }

    private void FixedUpdate()
    {
        if (isDebug)
        {
            UpdateScreen();
        }
    }



    void UpdateScreen()
    {
        Vector3 footlocation = managerHub.world.PlayerFoot.position;

        //FPS
        CalculateFPS();


        //update
        //LeftText.text += $"\n";
        LeftText.text = $"帧数: {managerHub.fpsmManaer.fps:F2}\n";
        LeftText.text += $"当前时间: {managerHub.timeManager.GetCurrentTime():F2}时\n";
        LeftText.text += $"\n";
        LeftText.text += $"[Player]\n";
        LeftText.text += $"朝向: {CalculateFacing()}\n";
        LeftText.text += $"实际朝向: {managerHub.player.FactFacing}\n";
        LeftText.text += $"实际运动方向: {managerHub.player.ActualMoveDirection}\n";
        LeftText.text += $"输入: {managerHub.player.keyInput}\n";
        LeftText.text += $"眼睛坐标: {managerHub.player.cam.position}\n";
        LeftText.text += $"实时重力: {managerHub.player.verticalMomentum}\n";
        LeftText.text += $"绝对坐标: {(new Vector3((int)footlocation.x, (int)footlocation.y, (int)footlocation.z))}\n";
        LeftText.text += $"相对坐标: {managerHub.world.GetRelalocation(footlocation)}\n";
        LeftText.text += $"已保存方块数量: {managerHub.world.EditNumber.Count}\n";
        LeftText.text += $"碰撞点检测个数:{managerHub.player.CollisionNumber}\n";
        LeftText.text += $"\n";
        LeftText.text += $"[Chunk]\n";
        LeftText.text += $"区块坐标: {managerHub.world.GetChunkLocation(footlocation)}\n";
        LeftText.text += $"\n";
        //LeftText.text += $"[Noise]\n";


    }


    public void UpdateBlockItem()
    {
        

        for (int index = 0;index < managerHub.world.blocktypes.Length; index++)
        {
            //初始化item
            GameObject instance = Instantiate(blockitem);
            instance.transform.SetParent(Content, false);
            instance.transform.Find("TMP_index").GetComponent<TextMeshProUGUI>().text = $"{index}";
            instance.transform.Find("Image").GetComponent<Image>().color = new Color(1, 1, 1, 200f / 255);

            if (managerHub.world.blocktypes[index].sprite != null)
            {
                instance.transform.Find("Image").GetComponent<Image>().sprite = managerHub.world.blocktypes[index].sprite;
            }else if (managerHub.world.blocktypes[index].sprite != null)
            {
                instance.transform.Find("Image").GetComponent<Image>().sprite = managerHub.world.blocktypes[index].top_sprit;
            }
            else
            {
                instance.transform.Find("Image").GetComponent<Image>().color = new Color(0, 0, 0, 0);
            }
            
        }

    }



    // FPS计数器
    private int frameCount;
    private float elapsedTime;
    private float fps;

    // 更新并计算FPS
    void CalculateFPS()
    {
        // 每帧累加时间
        elapsedTime += Time.deltaTime;
        frameCount++;

        // 每秒计算一次FPS
        if (elapsedTime >= 1.0f)
        {
            // 计算FPS
            fps = frameCount / elapsedTime;

            // 重置计数器和 elapsedTime
            frameCount = 0;
            elapsedTime = 0f;
        }
    }







    //facing
    string CalculateFacing()
    {
        Vector3 forward = managerHub.player.transform.forward;
        float angle = Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        string facingDirection;

        if (Mathf.Abs(angle - 0) <= 30 || Mathf.Abs(angle - 360) <= 30)
        {
            facingDirection = "East";
        }
        else if (Mathf.Abs(angle - 90) <= 30)
        {
            facingDirection = "North";
        }
        else if (Mathf.Abs(angle - 180) <= 30)
        {
            facingDirection = "West";
        }
        else if (Mathf.Abs(angle - 270) <= 30)
        {
            facingDirection = "South";
        }
        else if (Mathf.Abs(angle - 45) <= 30)
        {
            facingDirection = "NorthEast";
        }
        else if (Mathf.Abs(angle - 135) <= 30)
        {
            facingDirection = "SouthEast";
        }
        else if (Mathf.Abs(angle - 225) <= 30)
        {
            facingDirection = "SouthWest";
        }
        else if (Mathf.Abs(angle - 315) <= 30)
        {
            facingDirection = "NorthWest";
        }
        else
        {
            facingDirection = "Unknown";
        }

        return facingDirection;
    }

}
