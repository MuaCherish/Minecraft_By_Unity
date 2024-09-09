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
    //获取组件
    [Header("Transforms")]
    public ManagerHub managerHub;
    public GameObject DebugScreen;
    public Transform Content;
    public GameObject blockitem;
    public TextMeshProUGUI LeftText;
    //public Player player;
    //public World world;

    [Header("状态")]
    public bool isDebug = false;

    private void Start()
    {
        UpdateBlockItem();
    }

    void Update()
    {

        if (managerHub.worldManager.game_state == Game_State.Start)
        {
            if (DebugScreen.activeSelf)
            {
                DebugScreen.SetActive(false);
            }
            

        }


        if (managerHub.worldManager.game_state == Game_State.Playing)
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
        Vector3 footlocation = managerHub.worldManager.PlayerFoot.position;

        //FPS
        CalculateFPS();


        //update
        //LeftText.text += $"\n";
        LeftText.text = $"帧数: {Mathf.Ceil(fps):F2}\n";
        LeftText.text += $"\n";
        LeftText.text += $"[Player]\n";
        LeftText.text += $"朝向: {CalculateFacing()}\n";
        LeftText.text += $"实际朝向: {managerHub.playerManager.FactFacing}\n";
        LeftText.text += $"实际运动方向: {managerHub.playerManager.ActualMoveDirection}\n";
        LeftText.text += $"输入: <{managerHub.playerManager.keyInput}>\n";
        LeftText.text += $"实时重力: {managerHub.playerManager.verticalMomentum}\n";
        LeftText.text += $"绝对坐标: {(new Vector3((int)footlocation.x, (int)footlocation.y, (int)footlocation.z))}\n";
        LeftText.text += $"相对坐标: {managerHub.worldManager.GetRelalocation(footlocation)}\n";
        LeftText.text += $"已保存方块数量: {managerHub.worldManager.EditNumber.Count}\n";
        LeftText.text += $"碰撞点检测个数:{managerHub.playerManager.CollisionNumber}\n";
        LeftText.text += $"\n";
        LeftText.text += $"[Chunk]\n";
        LeftText.text += $"区块坐标: {managerHub.worldManager.GetChunkLocation(footlocation)}\n";
        LeftText.text += $"\n";
        //LeftText.text += $"[Noise]\n";


    }


    public void UpdateBlockItem()
    {
        

        for (int index = 0;index < managerHub.worldManager.blocktypes.Length; index++)
        {
            //初始化item
            GameObject instance = Instantiate(blockitem);
            instance.transform.SetParent(Content, false);
            instance.transform.Find("TMP_index").GetComponent<TextMeshProUGUI>().text = $"{index}";
            instance.transform.Find("Image").GetComponent<Image>().color = new Color(1, 1, 1, 200f / 255);

            if (managerHub.worldManager.blocktypes[index].sprite != null)
            {
                instance.transform.Find("Image").GetComponent<Image>().sprite = managerHub.worldManager.blocktypes[index].sprite;
            }else if (managerHub.worldManager.blocktypes[index].sprite != null)
            {
                instance.transform.Find("Image").GetComponent<Image>().sprite = managerHub.worldManager.blocktypes[index].top_sprit;
            }
            else
            {
                instance.transform.Find("Image").GetComponent<Image>().color = new Color(0, 0, 0, 0);
            }
            
        }

    }



    //calculate FPS
    private int count;
    private float deltaTime;
    private float fps;

    void CalculateFPS()
    {
        count++;
        deltaTime += Time.deltaTime;

        if (count >= 60)
        {
            fps = 60f / deltaTime;
            //Debug.Log($"FPS: {fps:F1}"); // 输出FPS值并精确到小数点后一位
            deltaTime = 0;
            count = 0;
        }
    }




    //facing
    string CalculateFacing()
    {
        Vector3 forward = managerHub.playerManager.transform.forward;
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
